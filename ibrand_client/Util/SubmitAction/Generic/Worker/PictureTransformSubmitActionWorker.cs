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
using NomadLogic.Multimedia.XmlTransformation.image;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    class PictureTransformSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly PictureTransformSubmitAction _action;
        private readonly int _respIdD;

        public PictureTransformSubmitActionWorker(PictureTransformSubmitAction action, Respondent resp, int respId)
        {
            _respondent = resp;
            _action = action;
            _respIdD = respId;
        }

        /// <inheritdoc />
        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                var controller = new TransformationsController();
                var configFile = DirectoryUtil.GetLocalIbrandTerminalDir(_action.TerminalInfo) + "picture_transformation_config.xml";
                controller.Initialize(configFile);
                var transformations = controller.GetTransformations(_respondent, false);
                var db = ServicesContainer.GetService<IbrandDatabase>();
                var transformedDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(
                    _action.TerminalInfo, 
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, 
                    DirectoryUtil.ProjectDataDirectoriesModEnum.transformed);

                foreach(var t in transformations)
                {
                    var picture = _respondent.GetAnswerText(int.Parse(t.OutputNameQuestionNo));
                    if (string.IsNullOrEmpty(picture)) picture = GenerateUniqueId.GetId();

                    var fileName = StringUtil.RemoveExtension(StringUtil.GetFileName(picture)) + "." + t.OutputFormat;
                    var transformedPicture = transformedDir + fileName;

                    // On fait uniquement la transformation si le fichier n'existe pas dans le répertoire d'images transformées.
                    if (!File.Exists(transformedPicture))
                    {
                        t.Respondent = _respondent;
                        var transformedPictureBitmap = t.TransformImage();
                        if (t.OutputFormat.ToLower().Equals("png"))
                        {
                            transformedPictureBitmap.Save(ImageFormat.Png, 9, transformedPicture);
                        }
                        else
                        {
                            transformedPictureBitmap.Save(ImageFormat.Jpeg, 90, transformedPicture);
                        }

                        var answerPicture = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = int.Parse(t.OutputNameQuestionNo), Response = transformedPicture};
                        db.DeleteAnswer(_respIdD, answerPicture);
                        db.InsertAnswer(_respIdD, answerPicture);
                    }

                    log.Debug("action.DestinationFolder = " + _action.DestinationFolder);

                    if (_action.DestinationFolder != null)
                    {
                        var destinations = _action.DestinationFolder.Split(';');

                        foreach (var s in destinations)
                        {
                            DirectoryInfo di;
                            try
                            {
                                di = !Directory.Exists(s) ? Directory.CreateDirectory(s) : new DirectoryInfo(s);
                            }
                            catch (IOException e)
                            {
                                log.Error("Could not create directory: " + e.Message);
                                di = null;
                            }

                            if (di == null) continue;

                            var newpath = s + Path.DirectorySeparatorChar + fileName;
                            log.Debug(transformedPicture + " -- > " + newpath);
                            File.Copy(transformedPicture, newpath);

                            if (!File.Exists(newpath))
                            {
                                log.Error("Could not copy file to following directory: " + newpath);
                            }
                        }
                    }

                    if (!_action.CopyToBackUpDirectory) continue;
                    var conf = ServicesContainer.GetService<NomadConfiguration>();
                    var dir = DirectoryUtil.GetBackupDir(conf);
                    var bkpFile = dir + transformedPicture;

                    if (File.Exists(bkpFile))
                    {
                        File.Delete(bkpFile);
                    }

                    File.Copy(transformedPicture, bkpFile);

                    if (!File.Exists(bkpFile))
                    {
                        throw new System.Exception("Backup file does not exists after copy: " + bkpFile);
                    }
                }

                db.SetRespondentReadyToSync(_respIdD);
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while Transforming pictures");

                _action.UiSynchro?.AddUiMessage("Error while Transforming pictures", 4);
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