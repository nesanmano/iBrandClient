using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.VimeoApi;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;

namespace NomadLogic.App.EventVideoCapture.Video.Workers
{
    /// <summary>
    /// Classe d'instanciation d'un travailleur pour l'application de capture vidéos.
    /// Projet AQU-2021-01: aquaparc tchèque. On extrait X nombres de trames, puis on en fait des thumbnails.
    /// On envoi vers Vimeo, puis tous les fichiers vers le serveur.
    /// On assume que on ne traite qu'un seul vidéo, et que ce vidéo est nommé "original.mp4".
    /// </summary>
    [PublicAPI]
    public class VideoTreatmentAquaparc2021 : VideoTreatmentAbstract
    {
        private const string VIDEO_NAME = "original.mp4";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private VimeoApi _vimeo;
        private static AquaparcConfig _aquaConfig;
        
        protected override async Task<bool> Launch(CancellationToken token)
        {
            log.Debug("Worker started for event: " + Parameter.WorkDirectory.Name);
            WorkerResult = new VideoTreatmentResult(); // Résultat par défaut est faux, jusqu'à complétion.

            var tmpDir = Parameter.WorkDirectory;
            var exileDir = new DirectoryInfo(DirectoryUtil.GetLocalApplicationSuspectMediaDir());
            var originalVideo = new FileInfo(Path.Combine(tmpDir.FullName, VIDEO_NAME));

            // Initialisation globale, et "lazy"
            if (_aquaConfig == null)
            {
                _aquaConfig = new AquaparcConfig();
                var file = new FileInfo(_aquaConfig.ConfigFileName ?? DirectoryUtil.GetLocalApplicationDir() + _aquaConfig.DefaultConfigFileName);
                if (!file.Exists)
                {
                    log.Warn("No previous configuration file detected. Will create a new one.");
                    if (!JsonUtil.WriteJsonFile(file, _aquaConfig, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include
                    }))
                    {
                        throw new Exception("Could not create initial config file for Aqualand workers.");
                    }
                    file.Refresh();
                }
                // Initialize une seule fois la config de l'ensemble de workers de ce type.
                log.Trace("Lazy Initialization of worker type finished. Aqualand 2021.");
                if (!JsonUtil.UpdateFromJson(file, _aquaConfig))
                {
                    throw new Exception("Could not load config file for Aqualand workers: " + file.FullName);
                }
            }

            var appConfig = ServicesContainer.GetService<NomadConfiguration>();

            // Vérification
            var vidData = await FfmpegUtil.GetVideoFileMetadata(token, appConfig.FfprobeLocation, originalVideo.FullName);
            var duration = vidData?.GetMainVideoStreamData()?.DurationSeconds;
            log.Trace("Video data extracted.");

            if (duration == null || duration.Value < _aquaConfig.MinimalVideoDuration)
            {
                log.Warn($"Video lenght critical fail: too short! ({duration} < {_aquaConfig.MinimalVideoDuration})");
                InvalidateVideoFile(tmpDir, originalVideo, exileDir);
                WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                WorkerResult.UiMessage = "Video file is too short! Cancelling video event.";
                return false;
            }
            
            #region Extraction des trames d'image.

            if (token.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Main frames extraction");

            var mainImages = _aquaConfig.GetMainFileNames(tmpDir, originalVideo);
            if (CheckImages(token, mainImages))
            {
                log.Trace("Main frames extraction begins.");
                if (!await ExtractMainImages(token, mainImages, originalVideo, vidData, appConfig))
                {
                    log.Warn("Frame extraction critical fail.");
                    InvalidateVideoFile(tmpDir, originalVideo, exileDir);
                    WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                    WorkerResult.UiMessage = "Invalid video file. Discarding video event.";
                    return false;
                }
            }
            else
            {
                log.Debug("Main images extraction skipped: they already exists.");
            }
            #endregion

            #region Thumbnails

            if (token.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Frame thumbnails extraction");

            if (!await FileUtil.CheckAndWaitForFileLock(originalVideo, 5, 50))
            {
                log.Warn($"Could not have access to main video file (before thumbnail sequence). Re-queuing\n\t- Video source: {originalVideo.FullName}");
                WorkerResult.State = VideoTreatmentResultEnum.FileSystemBusy;
                RequeueWorker();
                return false;
            }

            // Thumbnails des images
            var tmbImages = _aquaConfig.GetThumbFileNames(tmpDir, mainImages);
            if (CheckImages(token, tmbImages))
            {
                log.Trace("Thumbnail frames extraction begins.");
                if (!await ExtractThumbImages(token, mainImages, tmbImages, _aquaConfig.DesiredThumnailSize, tmpDir))
                {
                    log.Warn("Thumbnail resize critical fail.");
                    InvalidateVideoFile(tmpDir, originalVideo, exileDir);
                    WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                    WorkerResult.UiMessage = "Invalid frame to resize. Cancelling video event.";
                    return false;
                }
            }
            else
            {
                log.Debug("Thumbnail images extraction skipped: they already exists.");
            }

            #endregion

            #region Envoi Vimeo

            if (token.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Vimeo upload.");

            if (!await FileUtil.CheckAndWaitForFileLock(originalVideo, 5, 50))
            {
                log.Warn($"Could not have access to main video file (before Vimeo sequence). Re-queuing\n\t- Video source: {originalVideo.FullName}");
                WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyFileUploadFailed;
                WorkerResult.UiMessage = "Video file locked. Will retry later.";
                RequeueWorker();
                return false;
            }

            // Envoi du fichier vidéo.
            var vimeoResultFile = _aquaConfig.GetVimeoConfigFileName(tmpDir, originalVideo);
            var uploadVimeoResult = new UploadVideoResult
            {
                ConfigFileName = vimeoResultFile.FullName
            };
            if (!vimeoResultFile.Exists)
            {
                log.Trace("Sending file to Vimeo service.");
                var vimeoApi = GetMainVimeoInstance();
                uploadVimeoResult = VimeoStaticUtil.UploadVideoToVimeo(vimeoApi, originalVideo, _aquaConfig.VimeoAlbumName);
                // Nouvelle instance, on ré-assigne son nom de fichier.
                uploadVimeoResult.ConfigFileName = vimeoResultFile.FullName;

                if (!uploadVimeoResult.Success)
                {
                    log.Warn("Video failed to upload to Vimeo. Re-queuing worker for file: " + originalVideo.FullName);
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyFileUploadFailed;
                    WorkerResult.UiMessage = "Vimeo upload failed. Will retry later.";
                    RequeueWorker();
                    return false;
                }

                var check = JsonUtil.WriteJsonFile(vimeoResultFile, uploadVimeoResult);
                if (!check)
                {
                    log.Warn($"Could not write JSON result of Vimeo upload. Please verify.\n\t- Video source: {originalVideo.FullName}\n\t- Json file: {vimeoResultFile.FullName}");
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyFileUploadFailed;
                    WorkerResult.UiMessage = "Vimeo result file cannot be written to disk. Will retry later.";
                    RequeueWorker();
                    return false;
                }
            }
            else
            {
                var loadSuccess = JsonUtil.UpdateInstanceFromFile(uploadVimeoResult, tmpDir);
                if (!loadSuccess)
                {
                    log.Warn("Failed to read vimeo result file. Deleting file and trying anew: " + uploadVimeoResult.ConfigFileName);
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyFileUploadFailed;
                    WorkerResult.UiMessage = "Vimeo result file corrupt. Destrying invalid file. Will retry upload later.";
                    RequeueWorker();
                    return false;
                }
                log.Debug("Vimeo result file detected. Skipping sending.");
            }

            #endregion
            
            #region Envoi des fichiers d'images vers le serveur

            if (token.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Main server data and file uploads");

            log.Trace("Sending data to Nomad Main server.");
            var login = await ServerRestRequestsUtil.Login(ServerConf, token);
            if (login == null)
            {
                var message = "Communications with server failed.";
                LoggerReference.LogEvent(message);
                log.Warn(message);
                WorkerResult.State = VideoTreatmentResultEnum.ServerUnreachable;
                WorkerResult.UiMessage = "Failure to connect to main server. Will retry later.";
                RequeueWorker();
                return false;
            }
            if (login.Data.Length < 10)
            {
                var message = "Could not get authentification token. Will retry later." + login.MessageUi;
                LoggerReference.LogEvent(message);
                log.Warn(message);
                WorkerResult.State = VideoTreatmentResultEnum.ServerUnreachable;
                WorkerResult.UiMessage = "Failure to authenticate to main server. Will retry later.";
                RequeueWorker();
                return false;
            }

            // Création des paramètres
            
            /*
             * Index des valeurs dynamiques
             * 00 :             (X) Nombre d'images. Même valeur pour les thumbnails. Integer.
             * 01 :             ID Vimeo
             * 02 - 02+X :      Images
             * 03+X - 03+X+X :  Thumbanils
             *
             * Exemple avec 4 Images:
             * 0 -> 4
             * 1 -> 12345678
             * 2 -> Img 1
             * 3 -> Img 2
             * 4 -> Img 3
             * 5 -> Img 4
             * 6 -> Tbn Img 1
             * 7 -> Tbn Img 2
             * 8 -> Thn Img 3
             * 9 -> Thn Img 4
             */

            var dico = new Dictionary<string, FileInfo>();
            var dynamicValues = new List<string>
            {
                mainImages.Length.ToString(),   // Index 00
                "vimeo:" + uploadVimeoResult.VideoInfo.Id  // Index 01, avec ajout du préfixe Viméo.
            };
            
            var i = 1;
            foreach (var img in mainImages)
            {
                var refVal = $"file_main_{i:00}";
                dico.Add(refVal, img);
                dynamicValues.Add("ref:" + refVal);
                i++;
            }
            i = 1;
            foreach (var img in tmbImages)
            {
                var refThnVal = $"file_tmb_{i:00}";
                dico.Add(refThnVal, img);
                dynamicValues.Add("ref:" + refThnVal);
                i++;
            }

            var jujuRequestParam = new RequestSendRespondentDataPayload
            {
                RequestFiles = dico,
                RequestConfig = new RequestSendRespondentDataConfig
                {
                    SendRespondentIdentification = new RequestSendRespondentDataIdentification
                    {
                        SearchStrategy = new RequestSendRespondentDataIdentificationStrategy
                        {
                            SearchStrategy = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                            ServerIdQuestionnaire = ProjConfig.NomadQuestionnaireId,
                            ServerIdTerminal = ProjConfig.NomadTerminalId,
                            TextValue = Parameter.RespondentUniqueId
                        }
                    },
                    DynamicValuesToSend = new List<RequestSendRespondentDataDynamicValue>
                    {
                        new RequestSendRespondentDataDynamicValue
                        {
                            DateCreated = DateTimeOffset.Now,
                            DataKey = ProjConfig.LocationKeyName,
                            DataValues = dynamicValues
                        }
                    },
                    AnswersToSend = new List<RequestSendRespondentDataAnswer>
                    {
                        new RequestSendRespondentDataAnswer
                        {
                            QuestionNumber = 1,
                            ChoiceNumber = 1,
                            DontOverrideData = true,
                            TextValue = Parameter.RespondentUniqueId
                        }
                    }
                }
            };

            if (log.IsTraceEnabled)
            {
                log.Trace("Main server parameters: \r\n" + jujuRequestParam.ToStringPublicProperties(2));
            }

            var sendResult = ServerRestRequestsUtil.SendRespondentData(token, ServerConf, login.Data, jujuRequestParam);
            if (sendResult == null || sendResult.Result != ServerRestRequestResultType.Success)
            {
                WorkerResult.State = VideoTreatmentResultEnum.ServerDidNotReceive;
                WorkerResult.UiMessage = "Server rejected data. Will retry later. Message: " + sendResult?.MessageUi;
                log.Warn("Major error during data sening to server. Will retry. Code={0}, Message={1}", sendResult?.Result, sendResult?.MessageUi);
                RequeueWorker();
                return false;
            }
            if (sendResult.Result != ServerRestRequestResultType.Success)
            {
                var killWorker = false;
                switch (sendResult.Result)
                {
                    case ServerRestRequestResultType.Unauthorized:
                        log.Warn("Unauthorized. Check server credential. Will retry. Message: " + sendResult.MessageUi);
                        break;
                    case ServerRestRequestResultType.Failed:
                        log.Warn("Failed. Server is unable to process. Will retry. Message: " + sendResult.MessageUi);
                        break;
                    default:
                        log.Warn("Major error during sending to Nomad server. Will not retry. Vimeo upload still executed. Message: " + sendResult.MessageUi);
                        killWorker = true;
                        break;
                }

                if (killWorker)
                {
                    InvalidateVideoFile(tmpDir, originalVideo, exileDir);
                    WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                    WorkerResult.UiMessage = "Major error detected. Invalidating video event.";
                }
                else
                {
                    WorkerResult.State = VideoTreatmentResultEnum.ServerDidNotReceive;
                    WorkerResult.UiMessage = "Failure to send data to main server. Will retry later.";
                    RequeueWorker();
                }
                return false;
            }

            log.Trace("Data sent to server. Worker job is complete.");

            #endregion

            // Nettoyage
            InvalidateVideoFile(tmpDir, originalVideo);

            WorkerResult.Complete = true;
            WorkerResult.State = VideoTreatmentResultEnum.Complete;
            return true;
        }

        public override string CreateVideoPreview(DirectoryInfo baseDirectory, string tempName, string previewDirectory)
        {
            // TODO
            throw new NotImplementedException();
        }

        private VimeoApi GetMainVimeoInstance()
        {
            // TODO Test keepalive?

            if (_vimeo != null) return _vimeo;
            _vimeo = VimeoApi.GetInstance(_aquaConfig.VimeoAccount);
            return _vimeo;
        }

        private static async Task<bool> ExtractThumbImages(CancellationToken token, FileInfo[] mainImages, FileInfo[] tmbImages, Size desiredSize, DirectoryInfo tmpDir)
        {
            if (mainImages.Length != tmbImages.Length) return false;
            
            for (var i = 0; i < mainImages.Length; i++)
            {
                if (token.IsCancellationRequested) return true; // Pas d'erreur, mais pas complet.
                
                var result = await ImageMagickCliUtil.CreateThumbnailAsync(mainImages[i], tmpDir, desiredSize, false, null, tmbImages[i]);
                if (tmbImages[i] != result)
                {
                    log.Error("Could not create thumbnail for image: " + mainImages[i].FullName);
                    return false;
                }

                log.Trace("Extracted thumbnail file: " + mainImages[i].FullName);
            }

            return true;
        }

        /// <summary>
        /// Enlève les fichiers temporaires et le fichier vidéo source du disque dur, pour nettoyer.
        /// Si on donne un répertoire d'exil, on déplace le fichier source ou lieu de simplement le détruire.
        /// </summary>
        /// <param name="dirToDestroy">Le répertoire temporaire à détruire.</param>
        /// <param name="exileDirectory">Optionnel. Le répertoire pour contenir le fichier problématique.</param>
        /// <param name="fileToExile">Le fichier principal.</param>
        private void InvalidateVideoFile(DirectoryInfo dirToDestroy, FileInfo fileToExile, DirectoryInfo exileDirectory = null)
        {
            try
            {
                if (!FileUtil.CheckAndWaitForFileLock(fileToExile, 5, 50).GetAwaiter().GetResult())
                {
                    log.Warn($"Could not have access to main video file during destruction. Will still try to destroy.\n\t- Video source: {fileToExile.FullName}");
                }
                
                if (exileDirectory != null && exileDirectory.Exists)
                {
                    File.Move(fileToExile.FullName, Path.Combine(exileDirectory.FullName, fileToExile.Name));
                }
                
                FileUtil.DeleteFolderContent(dirToDestroy.FullName);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            fileToExile.Refresh();
            if (!fileToExile.Exists) return;

            log.Fatal("Could not destroy file! Errors will compound. Stopping execution. File: " + fileToExile.FullName);
            throw new Exception("Critical file could not be destroyed: " + fileToExile.FullName);
        }

        private async Task<bool> ExtractMainImages(CancellationToken token, FileInfo[] mainImages, FileInfo sourceVideo, VideoMetadata vidData, NomadConfiguration config)
        {
            // On fait l'extraction à intervalle régulier, en sautant la prmière et dernière trame. En gros, si on extrait quatre trames, on le fera
            // à 20%, 40%, 60%, 80%. On ne touche pas à la première trame et à la dernière trame.

            var timeStep = (float)(vidData.GetMainVideoStreamData().DurationSeconds / (mainImages.Length + 1));

            var accTime = 0f; // Ne commence pas à zéro
            foreach (var img in mainImages)
            {
                if (token.IsCancellationRequested) return true; //  Pas d'erreur, mais pas complet.

                accTime += timeStep;
                await FfmpegUtil.ExtractSingleFrameAsync(token, config.FfmpegLocation, sourceVideo.FullName, accTime, img.FullName);
                if (File.Exists(img.FullName))
                {
                    log.Trace("Image frame extracted: Time={0}, file={1}", accTime, img.FullName);
                    continue;
                }

                log.Warn($"Could not extract frame at {StringUtil.FormatTime(accTime)} for video file '{sourceVideo.FullName}'");
                return false;
            }

            return true;
        }

        private bool CheckImages(CancellationToken token, IEnumerable<FileInfo> imageFiles)
        {
            var mustAct = false;
            foreach (var img in imageFiles)
            {
                if (token.IsCancellationRequested) return false;

                img.Refresh();
                if (!File.Exists(img.FullName))
                {
                    mustAct = true;
                } else if (img.Length == 0)
                {
                    try
                    {
                        File.Delete(img.FullName);
                    }
                    finally
                    {
                        mustAct = true;
                    }
                }
            }

            return mustAct;
        }
    }
    
    public class AquaparcConfig : IJsonConfig, IExternalFileConfig
    {
        public string DefaultConfigFileName => "aquaparc_config.json";
        public string ConfigFileName { get; set; }
        public Size DesiredThumnailSize { get; set; } = new Size(240, 135); // 1/8ième

        [JsonConverter(typeof(StringEnumConverter))] 
        public VimeoAccountLogin VimeoAccount { get; set; } = VimeoAccountLogin.MainAccount;

        public string VimeoAlbumName { get; set; } = "Aqualand 2021";
        
        public uint StillImagesToExtract { get; set; } = 4;

        /// <summary>
        /// Temps minimal pour qu'un fichier vidéo soit considéré valide.
        /// </summary>
        public double MinimalVideoDuration { get; set; } = 3;

        /// <summary>
        /// Obtient les noms de fichiers de principaux fichiers d'image à extraire du vidéo. Dépend de la configuration.
        /// </summary>
        /// <param name="workDir">Le répertoire de base d'où faire le traitement vidéo.</param>
        /// <param name="sourceVideo">Le fichier vidéo brut sur lequel faire le traitement.</param>
        /// <returns>Une liste de noms fichiers à extraire.</returns>
        public FileInfo[] GetMainFileNames(DirectoryInfo workDir, FileInfo sourceVideo)
        {
            var videoName = StringUtil.GetFileName(sourceVideo.FullName, true);
            var nbrImages = StillImagesToExtract;

            var res = new FileInfo[nbrImages];

            for (var i = 0; i < nbrImages; i++)
            {
                res[i] = new FileInfo(Path.Combine(workDir.FullName, $"frame_{i:0000}_{videoName}.jpg"));
            }

            return res;
        }
        
        public FileInfo[] GetThumbFileNames(DirectoryInfo tmpDir, FileInfo[] mainImages)
        {
            var nbrImages = mainImages.Length;

            var res = new FileInfo[nbrImages];
            
            for (var i = 0; i < nbrImages; i++)
            {
                var ori = mainImages[i].FullName;
                var baseName = StringUtil.GetFileName(ori, true);
                var baseExt = StringUtil.GetFileExtension(ori);
                res[i] = new FileInfo(Path.Combine(tmpDir.FullName, $"{baseName}_{StringUtil.FormatSize(DesiredThumnailSize)}.{baseExt}"));
            }

            return res;
        }

        public FileInfo GetVimeoConfigFileName(DirectoryInfo workDir, FileInfo sourceVideo)
        {
            var videoName = StringUtil.GetFileName(sourceVideo.FullName, true);
            
            return new FileInfo(Path.Combine(workDir.FullName, $"vimeo_{videoName}.json"));
        }
    }

}