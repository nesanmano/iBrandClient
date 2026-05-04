using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.App.EventVideoCapture.config;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.App.EventVideoCapture.Video.Workers
{
    /// <summary>
    /// Classe d'instanciation d'un travailleur pour l'application de capture vidéos.
    /// Project interne de prototype pour ConnectAndGo: Trois sources vidéos, pour envoi vers AWS S3 et vers les serveurs de
    /// ConnectAndGo.
    /// </summary>
    [PublicAPI]
    public class VideoTreatmentCngPrototype : VideoTreatmentAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static CngPrototypeConfig _workerConfig;
        private WebRequestConnectAndGo _webExecutorCnG;
        private AmazonS3Client _awsS3Client;

        public override bool CustomInit(VideoCaptureConfig config, NomadConfiguration nomadConfig)
        {
            var success = base.CustomInit(config, nomadConfig);

            if (!success) return false;

            if (_workerConfig != null && _awsS3Client != null) return true;

            var appDir = DirectoryUtil.GetLocalApplicationDir();
            _workerConfig = new CngPrototypeConfig();
            var file = new FileInfo(_workerConfig.ConfigFileName ?? appDir + _workerConfig.DefaultConfigFileName);
            if (!file.Exists)
            {
                log.Warn("No previous configuration file detected. Will create a new one.");
                if (!JsonUtil.WriteJsonFile(file, _workerConfig, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                }))
                {
                    throw new Exception("Could not create initial config file for Connect&Go workers.");
                }
                file.Refresh();
            }

            // Initialize une seule fois la config de l'ensemble de workers de ce type.
            log.Trace("Lazy Initialization of worker type finished. Connect&Go Prototype 2021.");
            if (!JsonUtil.UpdateFromJson(file, _workerConfig))
            {
                throw new Exception("Could not load config file for Connect&Go Prototype workers: " + file.FullName);
            }

            // Vérification des scripts
            if (!File.Exists(appDir + _workerConfig.MainVideoFfmpegScriptFileName) ||
                !File.Exists(appDir + _workerConfig.PreviewVideoFfmpegScriptFileName))
            {
                log.Error("The scripts file for worker do not exist.");
                return false;
            }

            // Validation de la connexion vers C&G
            _webExecutorCnG = new WebRequestConnectAndGo
            {
                BaseUrl = _workerConfig.BaseUrl
            };

            if (!_webExecutorCnG.InitializeExecutor().GetAwaiter().GetResult())
            {
                log.Error("Could not initialize executor for ConnectAndGo.");
                return false;
            }

            // Validation des configurations AWS

            // À REFACTORISER OPC POUR UNE CRYPTE SÉCURITAIRE!
            var region = _workerConfig.AwsRegionEndpoint;

            var s3Config = new AmazonS3Config
            {
                ReadWriteTimeout = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(10),
                RetryMode = RequestRetryMode.Standard,
                MaxErrorRetry = 0,
                RegionEndpoint = region
            };

            var sharedFile = new SharedCredentialsFile();
            if (!sharedFile.TryGetProfile(_workerConfig.AwsProfileName, out var profile))
            {
                var options = new CredentialProfileOptions
                {
                    AccessKey = _workerConfig.AwsAccessKey,
                    SecretKey = _workerConfig.AwsSecretKey
                };
                profile = new CredentialProfile(_workerConfig.AwsProfileName, options) {Region = region};
                sharedFile.RegisterProfile(profile);

                sharedFile.TryGetProfile(_workerConfig.AwsProfileName, out profile); //  Écrase l'ancienne instance de profile, au besoin
            }

            if (!AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var awsCreds))
            {
                return false;
            }

            _awsS3Client = new AmazonS3Client(awsCreds, s3Config);

            if (_awsS3Client == null)
            {
                log.Error("Could not access AWS S3 client from local configurations.");
                return false;
            }

            return true;
        }

        protected override async Task<bool> Launch(CancellationToken ct)
        {
            // Vérifications de base
            if (_awsS3Client ==  null || _webExecutorCnG == null)
            {
                throw new Exception("Worker class not properly initialized. Check code.");
            }

            log.Debug("Worker started for event: " + Parameter.WorkDirectory.Name);
            WorkerResult = new VideoTreatmentResult(); // Résultat par défaut est faux, jusqu'à complétion.

            var appDir = DirectoryUtil.GetLocalApplicationDir();
            var workDir = Parameter.WorkDirectory;
            var exileDir = new DirectoryInfo(DirectoryUtil.GetLocalApplicationSuspectMediaDir());
            var appConfig = ServicesContainer.GetService<NomadConfiguration>();

            // Vérification des fichiers vidéos originaux.

            #region Vérif et metadonnées des fichiers originaux

            var inFilesNbr = _workerConfig.InputFileNames.Count;
            var inFileCheckTasks = new Task[inFilesNbr];
            var fileCheckToken = new CancellationTokenSource(TimeSpan.FromSeconds(4)).Token;
            for (var index = 0; index < inFilesNbr; index++)
            {
                // Empêche les variables d'être modifié hors contexte.
                var vidFilePath = _workerConfig.InputFileNames[index];
                var exec = appConfig.FfprobeLocation;
                inFileCheckTasks[index] = Task.Run(async () => { await ExtractVideoMetadata(vidFilePath, workDir, fileCheckToken, exec); }, fileCheckToken);
            }

            Task.WaitAll(inFileCheckTasks);

            if (WorkerResult.State == VideoTreatmentResultEnum.InvalidFile)
            {
                await InvalidateVideoEvent(workDir, exileDir);
            }

            // Vérifie que le RespondentID contient une valeur entière valide: qui est le Wallet_ID de Connect And Go, obtenu lors de la vérification
            // de la lecture RFID.
            if (!int.TryParse(Parameter.RespondentUniqueId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var walletId))
            {
                log.Warn("Problem during request of Procees Result. Re-queuing.");
                WorkerResult.State = VideoTreatmentResultEnum.RejectedByThirdParty;
                await InvalidateVideoEvent(workDir, exileDir);
                return false;
            }

            #endregion

            /* // TODO Rendu inutile grâce au Reader de ConnectAndGo qui va valider le RFID en trouvant le WALLET_ID, qui contenu dans le code du RespondentID.
            #region Connect&Go: ProcessScan
            if (ct.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Process Scan call.");

            AccessControlLog processResult;
            var processResultFile = new FileInfo(Path.Combine(workDir.FullName, "process_access_scan.json"));
            

            // SKIP CONDITION
            if (processResultFile.Exists)
            {
                // Si lu correctement, on saute cette étape.
                processResult = JsonUtil.ReadJsonFile<AccessControlLog>(processResultFile);

                if (processResult == null)
                {
                    log.Warn("Unreadable process scan result for unfinished CnG worker:{0}. Will not re-queue, but will not destroy.", Parameter.RespondentUniqueId);
                    WorkerResult.State = VideoTreatmentResultEnum.VideoTransformationFailed;
                    return false;
                }
            }
            else // On exécute le script et on stocke les métadonnées comme preuve de réussite.
            {
                cngLogin = await GetCnGLoginInfo(ct);

                if (cngLogin == null)
                {
                    log.Warn("Could not have access to a valid CnG auth token. Re-queuing.");
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyAuthFailed;
                    RequeueWorker();
                    return false;
                }

                var pars = new ProcessAccessScanPayload
                {
                    UniqueId = Parameter.RespondentUniqueId,
                    AccessPointId = _workerConfig.AccessPointId,
                    Direction = "IN",
                    GateName = _workerConfig.GateName,
                    XtenantId = _workerConfig.TenantId
                };

                processResult = await _webExecutorCnG.ProcessAccessScan(cngLogin, pars);
                if (processResult == null)
                {
                    log.Warn("Problem during request of Procees Result. Re-queuing.");
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyAuthFailed;
                    RequeueWorker();
                    return false;
                }

                // Si l'identifiant est complétement inconnu
                // TODO Faire cette vérification bien avant. Lors de la lecture du RFID idéalement.
                if (processResult.WalletId == 0)
                {
                    log.Warn("Could not have access to a valid wallet id with following identifier:{0}. Deleting.", Parameter.RespondentUniqueId);
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyAuthFailed;
                    await InvalidateVideoEvent(workDir, exileDir);
                    return false;
                }

                JsonUtil.WriteJsonFile(processResultFile, processResult);
            }

            // Base pour les noms de fichiers à créer.
            
            #endregion
            */

            var dateStr = Parameter.AdditionalData[DATE_TIME];
            var dateFormatFile = dateStr.Replace(':', '-');
            var baseFileName = $"{ProjConfig.LocationKeyName}_{_workerConfig.AccessPointId}_{walletId}_{dateFormatFile}_0_";

            #region Script Principal
            if (ct.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Main video script");

            log.Trace("Script principal pourt CNG lancé.");
            var videoPrincipalFileName = baseFileName + "full.mp4";
            var vpFilePath = Path.Combine(workDir.FullName, videoPrincipalFileName);
            var videoPrincipalFile = new FileInfo(vpFilePath);
            var videoPrincipalMetadataFile = new FileInfo(Path.Combine(workDir.FullName, videoPrincipalFileName + ".metadata"));
            VideoMetadata videoPrincipalMetadata;

            // SKIP CONDITION
            if (videoPrincipalMetadataFile.Exists)
            {
                // Si lu correctement, on saute cette étape.
                videoPrincipalMetadata = JsonUtil.ReadJsonFile<VideoMetadata>(videoPrincipalMetadataFile);

                if (videoPrincipalMetadata == null)
                {
                    log.Warn("Unreadable metadata file for existing main script result or unfinished CnG worker:{0}. Will not re-queue, but will not destroy.", Parameter.RespondentUniqueId);
                    WorkerResult.State = VideoTreatmentResultEnum.VideoTransformationFailed;
                    return false;
                }
            }
            else // On exécute le script et on stocke les métadonnées comme preuve de réussite.
            {
                var scriptPrincipalDico = new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(1), new Answer {Response = Path.Combine(workDir.FullName, _workerConfig.InputFileNames[0])}},
                    {new QuestionIdentifier(2), new Answer {Response = Path.Combine(workDir.FullName, _workerConfig.InputFileNames[1])}},
                    {new QuestionIdentifier(3), new Answer {Response = Path.Combine(workDir.FullName, _workerConfig.InputFileNames[2])}}
                };

                var scriptPrincipalPath = Path.Combine(appDir, _workerConfig.MainVideoFfmpegScriptFileName);
                
                var tokenCommand = FileUtil.ReadFileToStringSingleLine(scriptPrincipalPath);
                var modifiedCommandLine = StringUtil.CreateStringFromTokenizedPatternRespondent(tokenCommand, scriptPrincipalDico,
                    null, null, appDir, videoPrincipalFile.FullName);

                if (string.IsNullOrWhiteSpace(modifiedCommandLine))
                {
                    log.Error("Modified arguments for commands line are invalid. Main script file is invalid. Will not re-queue, but will not destroy.");
                    WorkerResult.State = VideoTreatmentResultEnum.VideoTransformationFailed;
                    return false;
                }

                await LaunchProcessUtil.LaunchProcessAsync(ct, appConfig.FfmpegLocation, modifiedCommandLine);
                videoPrincipalFile.Refresh();

                if (!videoPrincipalFile.Exists)
                {
                    log.Error("FFMPEG execution for main script failed. Main script file is invalid. Will not re-queue, but will not destroy.");
                    WorkerResult.State = VideoTreatmentResultEnum.VideoTransformationFailed;
                    return false;
                }

                videoPrincipalMetadata = await FfmpegUtil.GetVideoFileMetadata(ct, appConfig.FfprobeLocation, videoPrincipalFile.FullName);

                JsonUtil.WriteJsonFile(videoPrincipalMetadataFile, videoPrincipalMetadata);
            }

            #endregion

            #region Script Preview
            if (ct.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Preview video script");

            log.Trace("Script pour le preview pourt CNG lancé.");
            var videoPreviewFileName = baseFileName + "preview.mp4";
            var videoPreviewFile = new FileInfo(Path.Combine(workDir.FullName, videoPreviewFileName));
            var videoPreviewMetadataFile = new FileInfo(Path.Combine(workDir.FullName, videoPreviewFileName + ".metadata"));
            VideoMetadata videoPreviewMetadata;

            // SKIP CONDITION
            if (videoPreviewMetadataFile.Exists)
            {
                // Si lu correctement, on saute cette étape.
                videoPreviewMetadata = JsonUtil.ReadJsonFile<VideoMetadata>(videoPreviewMetadataFile);

                if (videoPreviewMetadata == null)
                {
                    log.Warn("Unreadable metadata file for existing preview script result or unfinished CnG worker:{0}. Will not re-queue, but will not destroy.", Parameter.RespondentUniqueId);
                    WorkerResult.State = VideoTreatmentResultEnum.VideoTransformationFailed;
                    return false;
                }
            }
            else // On exécute le script et on stocke les métadonnées comme preuve de réussite.
            {
                // Au besoin, on donne le fichier vidéo principal en plus. Comme ça on peut avoir un script qui agit sur le vidéo principal à la place, au besoin.
                var scriptPreviewDico = new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(1), new Answer {Response = Path.Combine(workDir.FullName, _workerConfig.InputFileNames[0])}},
                    {new QuestionIdentifier(2), new Answer {Response = Path.Combine(workDir.FullName, _workerConfig.InputFileNames[1])}},
                    {new QuestionIdentifier(3), new Answer {Response = Path.Combine(workDir.FullName, _workerConfig.InputFileNames[2])}},
                    {new QuestionIdentifier(4), new Answer {Response = Path.Combine(workDir.FullName, videoPrincipalFileName)}} 
                };

                var scriptPreviewPath = Path.Combine(appDir, _workerConfig.PreviewVideoFfmpegScriptFileName);
                
                var tokenCommand = FileUtil.ReadFileToStringSingleLine(scriptPreviewPath);
                var modifiedCommandLine = StringUtil.CreateStringFromTokenizedPatternRespondent(tokenCommand, scriptPreviewDico,
                    null, null, appDir, videoPreviewFile.FullName);

                if (string.IsNullOrWhiteSpace(modifiedCommandLine))
                {
                    log.Error("Modified arguments for commands line are invalid. Preview script file is invalid. Will not re-queue, but will not destroy.");
                    WorkerResult.State = VideoTreatmentResultEnum.VideoTransformationFailed;
                    return false;
                }

                await LaunchProcessUtil.LaunchProcessAsync(ct, appConfig.FfmpegLocation, modifiedCommandLine);
                videoPreviewFile.Refresh();

                if (!videoPreviewFile.Exists)
                {
                    log.Error("FFMPEG execution for preview script failed. Preview script file is invalid. Will not re-queue, but will not destroy.");
                    WorkerResult.State = VideoTreatmentResultEnum.VideoTransformationFailed;
                    return false;
                }

                videoPreviewMetadata = await FfmpegUtil.GetVideoFileMetadata(ct, appConfig.FfprobeLocation, videoPreviewFile.FullName);

                JsonUtil.WriteJsonFile(videoPreviewMetadataFile, videoPreviewMetadata);
            }

            #endregion
            
            #region Thumbnail

            if (ct.IsCancellationRequested) return false;
            StartIntermediateStopwatch("Frame thumbnails extraction");

            if (!await FileUtil.CheckAndWaitForFileLock(videoPrincipalFile, 5, 50))
            {
                log.Warn($"Could not have access to main video file (before thumbnail sequence). Re-queuing\n\t- Video source: {videoPrincipalFileName}");
                WorkerResult.State = VideoTreatmentResultEnum.FileSystemBusy;
                RequeueWorker();
                return false;
            }

            // Thumbnail du video
            var tmbImageFileName = Path.Combine(workDir.FullName, baseFileName + "thumbnail.jpg");
            if (!File.Exists(tmbImageFileName))
            {
                log.Trace("Thumbnail frames extraction begins.");
                await FfmpegUtil.ExtractSingleFrameAsync(ct, appConfig.FfmpegLocation, videoPrincipalFile.FullName, _workerConfig.ThumbnailTime,
                    tmbImageFileName, _workerConfig.DesiredThumbnailSize);
                if (!File.Exists(tmbImageFileName))
                {
                    log.Warn("Thumbnail extraction critical fail.");
                    await InvalidateVideoEvent(workDir, exileDir);
                    WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                    WorkerResult.UiMessage = "Invalid thumbnail extraction. Cancelling video event.";
                    return false;
                }

                // On diminie maintenant la taille de l'image.
            }
            else
            {
                log.Debug("Thumbnail image extraction skipped: already exists.");
            }

            #endregion

            #region Envoi AWS

            if (ct.IsCancellationRequested) return false;
            StartIntermediateStopwatch("AWS upload.");

            /*
            if (!await FileUtil.CheckAndWaitForFileLock(videoPrincipalFile, 5, 50))
            {
                log.Warn($"Could not have access to main video file (before AWS upload sequence). Re-queuing\n\t- Video source: {videoPrincipalFile.FullName}");
                WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyFileUploadFailed;
                WorkerResult.UiMessage = "Video file locked. Will retry later.";
                RequeueWorker();
                return false;
            }
            */

            // On charge un dictionnaire de données pour cette étape, pour ne pas envoyer deux fois le même fichier.
            var awsResultDictionaryFile = new FileInfo(Path.Combine(workDir.FullName, "was_step_dictionay.json"));
            var awsResultDico = awsResultDictionaryFile.Exists ? 
                JsonUtil.ReadJsonFile<Dictionary<string, VideoTreatmentCnGMediaFileInfo>>(awsResultDictionaryFile) : 
                new Dictionary<string, VideoTreatmentCnGMediaFileInfo>();

            // Envoi des fichiers vidéos.
            var awsFileTosend = new []
            {
                new VideoTreatmentCnGMediaFileInfo {LocalFile = videoPrincipalFile.FullName, CnGMediaFileType = "video/mp4", CnGMediaFormatType = MediaFormatEnum.full_size},
                new VideoTreatmentCnGMediaFileInfo {LocalFile = videoPreviewFile.FullName, CnGMediaFileType = "video/mp4", CnGMediaFormatType = MediaFormatEnum.preview},
                new VideoTreatmentCnGMediaFileInfo {LocalFile = tmbImageFileName, CnGMediaFileType = "image/jpg", CnGMediaFormatType = MediaFormatEnum.thumbnail}
            };

            foreach (var data in awsFileTosend)
            {
                if (awsResultDico.ContainsKey(data.LocalFile)) continue; // Déjà fait. Skip!

                var fileKey = data.LocalFile;
                data.AwsUrl = $"https://{_workerConfig.AwsBucketName}.{_workerConfig.AwsRegionEndpoint.SystemName}.amazonaws.com/{fileKey}";

                var awsPutRequest = new PutObjectRequest
                {
                    BucketName = _workerConfig.AwsBucketName,
                    ContentType = "application/octet-stream",
                    FilePath = data.LocalFile,
                    ReadWriteTimeout = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(10),
                    Key = fileKey
                };

                awsPutRequest.Metadata.Add("x-amz-meta-title", ProjConfig.ProjectDescription);
                var awsResponse = await _awsS3Client.PutObjectAsync(awsPutRequest, ct);

                if (awsResponse == null)
                {
                    log.Error("AWS upload failed for unknown reasons. Please check. File impacted: " + data.LocalFile);
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyFileUploadFailed;
                    RequeueWorker();
                    return false;
                }

                if (awsResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    log.Error($"AWS upload failed with code='{awsResponse.HttpStatusCode}'. Please check. File impacted: {data.LocalFile}");
                    WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyFileUploadFailed;
                    RequeueWorker();
                    return false;
                }

                awsResultDico.Add(data.LocalFile, data);
                if (JsonUtil.WriteJsonFile(awsResultDictionaryFile, awsResultDico))
                {
                    awsResultDictionaryFile.Refresh();
                    continue;
                }

                log.Error($"Could not write AWS upload result to file. Check JSON file: {awsResultDictionaryFile.FullName}");
                WorkerResult.State = VideoTreatmentResultEnum.FileSystemBusy;
                RequeueWorker();
                return false;
            }
            
            // Vérification post-hoc: le dictionnaire DOIT contenir trois fichiers
            if (awsResultDico.Count != 3)
            {
                log.Error($"Integrity check failed. We should have three files defined for AWS setup. Will not re-queue. Check JSON file: {awsResultDictionaryFile.FullName}");
                WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                return false;
            }

            #endregion
            
            #region Envoi des images vers le serveur de CnG.

            if (ct.IsCancellationRequested) return false;
            StartIntermediateStopwatch("File uploads to CnG servers.");

            var cngLogin = await GetCnGLoginInfo(ct);

            if (cngLogin == null)
            {
                log.Warn("Could not have access to a valid CnG auth token. Re-queuing.");
                WorkerResult.State = VideoTreatmentResultEnum.ThirdPartyAuthFailed;
                RequeueWorker();
                return false;
            }

            var mediaList = awsResultDico.Values.Select(
                awsFile => new Media
                {
                    Format = awsFile.CnGMediaFormatType,
                    MediaType = awsFile.CnGMediaFileType, 
                    MediaUrl = awsFile.AwsUrl
                }).ToList();

            var mediaPayload = new MediaPayload
            {
                AccessPointId = 1,
                XtenantId = 1000001,
                WalletId = walletId,
                SourceId = _workerConfig.MediaUploadSourceId,
                Medias = mediaList
            };

            var mediaResult = await _webExecutorCnG.SendMediaInfo(cngLogin, mediaPayload, ct);
            if (mediaResult == null)
            {
                log.Error($"Media sending to CnG serveur failed. Will retry.  Check JSON file: {awsResultDictionaryFile.FullName}");
                WorkerResult.State = VideoTreatmentResultEnum.ServerUnreachable;
                RequeueWorker();
                return false;
            }

            #endregion

            // Nettoyage
            await InvalidateVideoEvent(workDir);

            WorkerResult.Complete = true;
            WorkerResult.State = VideoTreatmentResultEnum.Complete;
            return true;
        }

        // TODO À mettre ailleurs, idéalement dans un classe spécifique pour gérer les appels vers CnG.
        private async Task<LoginResponse> GetCnGLoginInfo(CancellationToken ct)
        {
            var logResponse = await _webExecutorCnG.CnGLogin(ct, _workerConfig.CnGClientId, _workerConfig.CnGPassword, _workerConfig.CnGUsername, _workerConfig.CnGClientSecret);
            return logResponse?.Data;
        }

        private async Task ExtractVideoMetadata(string vidFilePath, DirectoryInfo workDir, CancellationToken token, string exec)
        {
            var metaFileName = new FileInfo(vidFilePath + ".metadata");
            var vidFile = Path.Combine(workDir.FullName, vidFilePath);
            // Skip check
            if (metaFileName.Exists)
            {
                return;
            }

            var vidData = await FfmpegUtil.GetVideoFileMetadata(token, exec, vidFile, metaFileName);
            metaFileName.Refresh();
            if (vidData == null || !metaFileName.Exists)
            {
                log.Warn("Video metadata cannot be extracted: {0}", vidFile);
                WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                WorkerResult.UiMessage = "Video metadata cannot be extracted.";
                return;
            }

            var duration = vidData.GetMainVideoStreamData().DurationSeconds;
            log.Trace("Video metadata extracted: {0}", metaFileName.FullName);

            if (duration < 1)
            {
                log.Warn($"Video lenght critical fail: too short! ({duration} < {1})");
                WorkerResult.State = VideoTreatmentResultEnum.InvalidFile;
                WorkerResult.UiMessage = "Video file is too short! Cancelling video event.";
            }
        }

        public override string CreateVideoPreview(DirectoryInfo baseDirectory, string tempName, string previewDirectory)
        {
            // TODO
            throw new NotImplementedException();
        }
    }

    public class VideoTreatmentCnGMediaFileInfo
    {
        public string AwsUrl { get; set; }
        public string LocalFile { get; set; }
        public string CnGMediaFileType { get; set; }
        public MediaFormatEnum CnGMediaFormatType { get; set; }
    }

    public class CngPrototypeConfig : IJsonConfig, IExternalFileConfig
    {
        public string DefaultConfigFileName => "cng_proto_config.json";

        public string ConfigFileName { get; set; }

        public Size? DesiredThumbnailSize { get; set; } = new Size(240, 240); // 1/8ième

        public string MainVideoFfmpegScriptFileName { get; set; } = "main_video.script";

        public string PreviewVideoFfmpegScriptFileName { get; set; } = "preview_video.script";

        public float ThumbnailTime { get; set; }

        public List<string> InputFileNames { get; set; }

        /// <summary>
        /// URL de base pour le serveur de Connect And Go.
        /// </summary>
        public string BaseUrl { get; set; } = "https://connectngo-demo.com";

        // REFACTORISER CECI OPC!
        #region To Refactor CnG
        // TODO Refactoriser dans une crypte
        public int CnGClientId { get; set; }
        // TODO Refactoriser dans une crypte
        public string CnGPassword { get; set; }
        // TODO Refactoriser dans une crypte
        public string CnGUsername { get; set; }
        // TODO Refactoriser dans une crypte
        public string CnGClientSecret { get; set; }
        // TODO Refactoriser dans une crypte
        public string AwsAccessKey { get; set; }
        // TODO
        public string AwsSecretKey { get; set; }

        [JsonIgnore]
        public RegionEndpoint AwsRegionEndpoint
        {
            get => RegionEndpoint.GetBySystemName(AwsRegionEndpointStr);
            set => AwsRegionEndpointStr = value.SystemName;
        }

        public string AwsRegionEndpointStr { get; set; } = "us-aws-2";
        #endregion

        public string GateName { get; set; }

        public long TenantId { get; set; }
        
        public string AwsProfileName { get; set; }

        public string AwsBucketName { get; set; }
        public int MediaUploadSourceId { get; set; }


        public int AccessPointId;
    }

}