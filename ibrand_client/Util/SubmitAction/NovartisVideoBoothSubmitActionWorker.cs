using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.XmlTransformation.image;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    class NovartisVideoBoothSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly NovartisVideoBoothSubmitAction _action;
        private readonly int _respIdD;

        public NovartisVideoBoothSubmitActionWorker(NovartisVideoBoothSubmitAction action, Respondent resp, int respId)
        {
            _respondent = resp;
            _action = action;
            _respIdD = respId;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();
                var picture = _respondent.GetAnswerText(1);

                if (picture != null)
                {
                    var controller = new TransformationsController();
                    //controller.Initialize(_action.Project.ProjectDir + _respondent.QuestionnaireId + Path.DirectorySeparatorChar + "picture_transformation_config.xml");
                    var transformation = controller.GetTransformations(_respondent, false);

                    var fileName = StringUtil.RemoveExtension(StringUtil.GetFileName(picture)) + "." +
                                   transformation[0].OutputFormat;
                    string transformedPicture = null; // TODO à refactoriser
                    // var transformedPicture = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_respondent.ProjectId,
                    //                              _respondent.QuestionnaireId,
                    //                              DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                    //                              DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + fileName;

                    

                    // On fait uniquement la transformation si le fichier n'existe pas dans le répertoire d'images transformées.
                    if (!File.Exists(transformedPicture))
                    {
                        var transformedPictureBitmap = controller.TransformPictures(_respondent, false);
                        if (transformation[0].OutputFormat.ToLower().Equals("png"))
                        {
                            transformedPictureBitmap[0].Save(ImageFormat.Png, 9, transformedPicture);
                        }
                        else
                        {
                            transformedPictureBitmap[0].Save(ImageFormat.Jpeg, 90, transformedPicture);
                        }

                        var answerPicture = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = int.Parse(transformation[0].OutputNameQuestionNo), Response = transformedPicture};
                        db.DeleteAnswer(_respIdD, answerPicture);
                        db.InsertAnswer(_respIdD, answerPicture);
                    }
                }

                var conf = ServicesContainer.GetService<NomadConfiguration>();
                var videoFile = _respondent.GetAnswerText(2); // Vidéo original

                if (!videoFile.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
                {
                    var videoMod = // Vidéo modifié
                        DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.ProjectId, _action.QuestionnaireId, _action.TerminalId,
                            DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                            DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) +
                        StringUtil.RemoveExtension(StringUtil.GetFileName(videoFile)) + ".mp4";

                    // On fait la réduction de fichier.
                    FfmpegUtil.CompressVideo(conf.FfmpegLocation, videoFile, videoMod, 24);

                    if (!File.Exists(videoMod))
                    {
                        throw new System.Exception("Could not compress video file.");
                    }

                    var answerPicture = new Answer{QuestionType = QuestionTypeEnum.Video, QuestionNo = 2, Response = videoMod};
                    db.DeleteAnswer(_respIdD, answerPicture);
                    db.InsertAnswer(_respIdD, answerPicture);

                    var tmp = videoFile;
                    videoFile = videoMod;
                    try
                    {
                        File.Delete(tmp);
                    }
                    catch(System.Exception e)
                    {
                        log.Error(e, "Could not delete original movie file. Does not block rest of action.");
                    }
                }
                
                var dirBkp = DirectoryUtil.GetBackupDir(conf);
                var bkpFile = dirBkp + StringUtil.GetFileName(videoFile);

                if (File.Exists(bkpFile))
                {
                    File.Delete(bkpFile);
                }

                File.Copy(videoFile, bkpFile);

                if (!File.Exists(bkpFile))
                {
                    throw new System.Exception("Backup file does not exists after copy: " + bkpFile);
                }
               
                db.SetRespondentReadyToSync(_respIdD);
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while doing Novartis SubmitAction");

                _action.UiSynchro?.AddUiMessage("Error while doing Novartis SubmitAction", 4);
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        public int GetRespId()
        {
            return _respIdD;
        }
    }
}