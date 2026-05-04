using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Cette classe gère la méthode principale pour la création et la gestion du répondant lors d'une création vidéo dans un contexte de SubmitAction.
    /// </summary>
    [PublicAPI]
    public static class SubmitActionCreateVideoUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public delegate bool ExecVideoCreationCommand(NomadConfiguration config, FileInfo scriptFile, FileInfo resultFile, string baseDir, IRespondent resp, IbrandDatabase db);
        
        public const string TRANSFORMED_NAME_MARKER = "_t.";

        public static string GenerateVideoFileName(IRespondent resp, string format, Dictionary<QuestionIdentifier, Answer> answerMap, QuestionIdentifier currentQuestion)
        {
            if (string.IsNullOrWhiteSpace(format)) return GenerateUniqueId.GetId() + TRANSFORMED_NAME_MARKER + "mp4"; // Aucun jeton ou format
            var result = StringUtil.CreateStringFromTokenizedPatternRespondent(format, answerMap, currentQuestion, resp);

            if (result.Equals(format) || !result.Contains(TRANSFORMED_NAME_MARKER))
            {
                return GenerateUniqueId.GetId() + TRANSFORMED_NAME_MARKER + "mp4"; // Jeton ou format invalide
            }

            return result;
        }

        public static bool StandardFfmpegScriptCommand(NomadConfiguration config, FileInfo scriptFile, FileInfo resultFile, string baseDir, IRespondent resp, IbrandDatabase db)
        {
            var answerMap = db.GetAnswersMap(resp.Id);
            return StandardFfmpegScriptCommand(config, scriptFile, resultFile, baseDir, resp, answerMap);
        }

        public static bool StandardFfmpegScriptCommand(NomadConfiguration config, FileInfo scriptFile, FileInfo resultFile, string baseDir, IRespondent resp, Dictionary<QuestionIdentifier, Answer> answerMap)
        {
            var exec = config.FfmpegLocation;
            
            var tokenCommand = FileUtil.ReadFileToStringSingleLine(scriptFile.FullName);
            var modifiedCommandLine = StringUtil.CreateStringFromTokenizedPatternRespondent(tokenCommand, answerMap, null, resp, baseDir, resultFile.FullName);

            if (string.IsNullOrWhiteSpace(modifiedCommandLine))
            {
                log.Error("Modified arguments for commands line are invalid.");
                return false;
            }

            LaunchProcessUtil.LaunchProcess(exec, modifiedCommandLine);

            var fileTest = new FileInfo(resultFile.FullName);
            if (fileTest.Exists && fileTest.Length >= 100) return true;
            log.Error("Could not create video file with following command: " + modifiedCommandLine);
            return false;
        }

        /// <summary>
        /// Méthode d'exécution complète pour générer un vidéo et modifier la BD ainsi que le répondant selon les résultats des transformations.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="respondent"></param>
        /// <param name="db"></param>
        /// <param name="config"></param>
        /// <param name="execCommand"></param>
        public static bool ExecuteTransformation(SubmitVideoTransformationSetting settings, Respondent respondent, 
            IbrandDatabase db, NomadConfiguration config, ExecVideoCreationCommand execCommand)
        {
            var respId = respondent.IdInt;
            var answerMap = db.GetAnswersMap(respId);
            
            FileInfo scriptFile = null;
            if (settings.ScriptFileName != null)
            {
                scriptFile = new FileInfo(settings.ScriptFileName);
                if (!scriptFile.Exists)
                {
                    scriptFile = new FileInfo(Path.Combine(settings.BaseDirectory.FullName, settings.ScriptFileName));
                    scriptFile.Refresh();
                }
            }

            for (var i = 0; i < settings.VideoQuestions.Count; i++)
            {
                var video = settings.VideoQuestions[i].Item1;
                var thumb = settings.VideoQuestions[i].Item2;

                var originalQi = new QuestionIdentifier(video.OriginalVideoFileQuestionNo);

                var originalVideoFile = answerMap.ContainsKey(originalQi) ? answerMap[originalQi] : null;

                if (originalVideoFile == null)
                {
                    // Problème majeur: il n'y a pas de fichier vidéo original à transformer...
                    throw new System.Exception("No original video file found at this location in questionnaire: question=" + video.OriginalVideoFileQuestionNo);
                }

                if (!video.NoVideoEdition)
                {
                    // Vérification que les vidéos ont déjà été transformés.
                    if (!video.OutputVideoFileQuestionNo.HasValue)
                    {
                        if (originalVideoFile.Response.Contains(TRANSFORMED_NAME_MARKER)) continue;
                    }
                    else
                    {
                        var resultQi = new QuestionIdentifier(video.OutputVideoFileQuestionNo.Value);
                        if ((answerMap.ContainsKey(resultQi)
                                ? answerMap[resultQi]
                                : null) != null)
                        {
                            continue; // La question qui reçoit le vidéo transformé contient une réponse, donc on saute cette étape
                        }
                    }
                    
                    // On est capable de générer un nom valide même avec un format null, mais si le format est non nul il doit être valide
                    if (settings.ResultFileNamePattern != null)
                    {
                        if (video.OutputVideoFileQuestionNo == null && !settings.ResultFileNamePattern.Contains(TRANSFORMED_NAME_MARKER))
                        {
                            throw new System.Exception("File name token is invalid. Must contain the transformation marker: " + TRANSFORMED_NAME_MARKER);
                        }
                    }
                    var tmp = GenerateVideoFileName(respondent, settings.ResultFileNamePattern, answerMap, null);
                    var videoFile = new FileInfo(Path.Combine(settings.OutputVideoDirectory.FullName, tmp));

                    if (!execCommand.Invoke(config, scriptFile, videoFile, settings.BaseDirectory.FullName, respondent, db))
                    {
                        throw new System.Exception("Video creation delegate did not work. Worker is aborting.");
                    }

                    videoFile.Refresh();
                    if (!videoFile.Exists)
                    {
                        throw new System.Exception($"Worker could not create video file for respondant {respId}");
                    }

                    #region thumbnail creation

                    if (thumb != null && settings.OutputThumbnailDirectory != null && settings.OutputThumbnailDirectory.Exists)
                    {
                        /* Extraction de la trame de présentation. */
                        var frameFileName = Path.Combine(settings.OutputThumbnailDirectory.FullName, GenerateUniqueId.GetId() + ".jpg");

                        // Création de l'image de présentation.
                        FfmpegUtil.ExtractSingleFrame(config.FfmpegLocation, videoFile.FullName, settings.ThumbnailTime, frameFileName);

                        if (!File.Exists(frameFileName))
                        {
                            throw new System.Exception($"Frame File not created for respondent ({respId}) at time ({settings.ThumbnailTime}) from video file: {videoFile.FullName}");
                        }

                        var photoAnswer = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = thumb.ThumbnailQuestionNo, Response = frameFileName};
                        db.DeleteAnswer(respId, photoAnswer);
                        db.InsertAnswer(respId, photoAnswer);

                        respondent.Answers.Add(photoAnswer);
                    }

                    #endregion

                    var saveTo = video.OutputVideoFileQuestionNo ?? video.OriginalVideoFileQuestionNo;

                    /* Les fichiers sont ok, on met les résultats dans la BD. */
                    var videoAnswer = new Answer{QuestionType = QuestionTypeEnum.Vimeo, QuestionNo = saveTo, Response = videoFile.FullName};
                    db.DeleteAnswer(respId, videoAnswer);
                    db.InsertAnswer(respId, videoAnswer);

                    respondent.Answers.RemoveAll(a => a.QuestionNo == saveTo);
                    respondent.Answers.Add(videoAnswer);

                    if (video.DestroyOriginalMultimediaFile)
                    {

                        FileInfo file = new FileInfo(originalVideoFile.Response); // l'instance fait référence à la réponse avant modification.
                        if (file.Exists)
                        {
                            try
                            {
                                file.Delete();
                            }
                            catch (System.Exception e)
                            {
                                log.Error(e);
                            }
                        }

                        DirectoryInfo photoBundleDir = new DirectoryInfo(originalVideoFile.Response); // l'instance fait référence à la réponse avant modification.

                        if (photoBundleDir.Exists)
                        {
                            try
                            {
                                photoBundleDir.Delete(true);
                            }
                            catch (System.Exception e)
                            {
                                log.Error(e);
                            }
                        }
                    }
                }

                var videoFileName = originalVideoFile.Response;

                #region Copy To ShowPictureOnTv

                if (settings.CopyToShowOnTv)
                {
                    var copyFile = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() + StringUtil.GetFileName(videoFileName);
                    if (File.Exists(copyFile))
                    {
                        File.Delete(copyFile);
                    }
                    File.Copy(videoFileName, copyFile);
                }

                #endregion

            }

            return true;
        }

    }

    public class SubmitVideoTransformationSetting
    {
        [JsonRequired]
        public IList<ValueTuple<VideoSetting, ThumbnailSetting>> VideoQuestions { get; set; } = new List<(VideoSetting, ThumbnailSetting)>();

        /// <summary>
        /// Le fichier qui contient le script FFMPEG. Si n'est pas absolu, on tente de localsier le fichier via le répertoire de travail de l'action.
        /// </summary>
        public string ScriptFileName { get; set; }

        /// <summary>
        /// Répertoire de base de travail, où se trouvent les fichiers nécessaire à la transformation.
        /// Doit être généré automatiquement, et donc ne fait pas partie d'une configuration JSON sur disque
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo BaseDirectory { get; set; }

        public bool CopyToShowOnTv { get; set; }
        
        public string ResultFileNamePattern { get; set; }
        
        /// <summary>
        /// Répertoire extrant, où vont aller les résultats de la transformation.
        /// Doit être généré automatiquement, et donc ne fait pas partie d'une configuration JSON sur disque
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo OutputVideoDirectory { get; set; }
        
        /// <summary>
        /// Répertoire extrant, où vont aller les thumbnails de vidéo.
        /// Doit être généré automatiquement, et donc ne fait pas partie d'une configuration JSON sur disque
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo OutputThumbnailDirectory { get; set; }
        
        /// <summary>
        /// Le temps en secondes fractionnaire où doit être prit le thumbnail du vidéo final.
        /// </summary>
        public float ThumbnailTime { get; set; } = 1.0f;
    }

    public class VideoSetting
    {
        /// <summary>
        /// Fichier vidéo original
        /// </summary>
        [JsonRequired]
        public int OriginalVideoFileQuestionNo { get; set; }


        /// <summary>
        /// Si présent, on sauvegarde le vidéo transformé dans une autre question. Si absent, la réponse du vidéo original sera écrasée
        /// par une nouvelle réponse pointant vers le vidéo transformé, avec une pseudo extension pour identifier si la transformation a bel et bien eu lieu.
        /// </summary>
        public int? OutputVideoFileQuestionNo { get; set; }

        /// <summary>
        /// Désactive la transformation vidéo. Plus utilisé dans un contexte de débogage et pour gÉrer les ficheirs originaux.
        /// </summary>
        public bool NoVideoEdition { get; set; }

        /// <summary>
        ///  Indique, si on détruit le vidéo original après la transformation. Pour des raisons d'espace.
        /// </summary>
        public bool DestroyOriginalMultimediaFile { get; set; }
    }

    

    public class ThumbnailSetting
    {
        [JsonRequired]
        public int ThumbnailQuestionNo { get; set; }
    }
}