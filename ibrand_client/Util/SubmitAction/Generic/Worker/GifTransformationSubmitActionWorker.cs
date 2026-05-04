using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    class GifTransformationSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly GifTransformationSubmitAction _action;
        private readonly int _respId;

        public GifTransformationSubmitActionWorker(GifTransformationSubmitAction action, int respId)
        {
            _action = action;
            _respId = respId;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();

                var respondent = db.GetRespondent(_respId);
                respondent.Answers = db.GetAnswers(_respId);
                respondent.DynamicHeaders = db.GetDynamicHeaders(_respId);

                var pId = respondent.ProjectId;
                var qId = respondent.QuestionnaireId;

                var bundleQuestionResponse = respondent.GetAnswerText(int.Parse(_action.NoQuestionPhotoBundle));

                // Création du bundle avec normalization
                if (!bundleQuestionResponse.EndsWith(".zip")) // Le fichier ZIP est déjà créé, on saute cette étape.
                {
                    #region Préparation répertoire d'images
                    if (!Directory.Exists(bundleQuestionResponse))
                    {
                        log.Warn("Bundle directory does not exists: " + bundleQuestionResponse);
                        return;
                    }

                    var di = new DirectoryInfo(bundleQuestionResponse);

                    var orderedPictureList = di.EnumerateFiles("*.jpg").ToList();

                    // Erreur fatale. Pas d'images à traiter.
                    if (!orderedPictureList.Any())
                    {
                        log.Error(
                            "Respondent has no pictures in directory. Impossible to use this respondent. Discarding it.");
                        db.SetRespondentReadyToSync(_respId);
                        _action.NewRespondentEventHandle?.Set();
                        _action.OnWorkFinished(_respId);
                        return;
                    }

                    FileUtil.SortFileCollection(orderedPictureList);
                    #endregion

                    #region Normalisation
                    List<FileInfo> picturesToUse;
                    var zippedFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                             DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                             DirectoryUtil.ProjectDataDirectoriesModEnum.bundle) +
                                         GenerateUniqueId.GetId() + ".zip";

                    var canBeUsed = true;
                    if (_action.Normalize)
                    {
                        // Utilise un répertoire  temporaire custom pour prévenir un bogue avec la commande FFMPEG, plus loin.
                        var transformDir =
                            new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId());
                        Directory.CreateDirectory(transformDir.FullName);

                        var normalizationFile = new FileInfo(FileBundleNormalizationTransforms.DefaultTransformsJsonFileLocation);

                        if (!File.Exists(normalizationFile.FullName))
                        {
                            log.Warn("No normalization file found. Please create a normalization file. Suspending worker.");
                            _action.OnWorkFinished(_respId);
                            return;
                        }
                        var transformInfo = JsonUtil.ReadJsonFile<FileBundleNormalizationTransforms>(normalizationFile);
                        
                        canBeUsed = OpenCvUtil.NormalizeFileBundleWithTransforms(orderedPictureList, transformInfo, transformDir, _action.NormalizeSize, ct);
                        picturesToUse = transformDir.GetFiles().ToList();
                    }
                    else
                    {
                        picturesToUse = new DirectoryInfo(bundleQuestionResponse).GetFiles().ToList();
                    }

                    FileUtil.SortFileCollection(picturesToUse);
                    #endregion
                    
                    #region Modification des images avec overlay
                    if (_action.Overlay != null)
                    {
                        var overlayFile = _action.GetTerminalDirectory + _action.Overlay;
                        if (!File.Exists(overlayFile))
                        {
                            log.Warn(
                                $"Overlay file file found. Please check file name ({overlayFile}). Suspending worker.");
                            _action.OnWorkFinished(_respId);
                            return;
                        }

                        using (var overlayBitmap = FileUtil.LoadBitmap(overlayFile))
                        {
                            foreach (var pictureFile in picturesToUse)
                            {
                                Bitmap newImage;
                                using (var image = new Bitmap(pictureFile.FullName))
                                {
                                    newImage = new Bitmap(image);
                                }
                                newImage.AddOverlay(overlayBitmap);
                                newImage.Save(ImageFormat.Jpeg, 90, pictureFile.FullName);
                                newImage.Dispose();
                            }
                        }
                    }

                    #endregion

                    #region GIF
                    var gifFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo, DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                          DirectoryUtil.ProjectDataDirectoriesModEnum.gif) +
                                      GenerateUniqueId.GetId() + ".gif";

                    // Création du GIF
                    ImageUtil.CreateGif(picturesToUse, gifFileName, true, _action.RewindGif, 1, _action.Delay, null, _action.GifSize);

                    var gifQuestionAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = int.Parse(_action.NoQuestionGif), Response = gifFileName};
                    db.DeleteAnswer(_respId, gifQuestionAnswer);
                    db.InsertAnswer(_respId, gifQuestionAnswer);
                    #endregion

                    #region Finalize ZIP
                    if (canBeUsed)
                    {
                        if (!File.Exists(zippedFileName))
                        {
                            using (var zip = new ZipFile())
                            {
                                zip.AddFiles(picturesToUse.Select(file => file.FullName), "");
                                zip.Save(zippedFileName);
                            }
                        }

                        var photoBundleAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoBundle, QuestionNo = int.Parse(_action.NoQuestionPhotoBundle), Response = zippedFileName};
                        db.DeleteAnswer(_respId, photoBundleAnswer);
                        db.InsertAnswer(_respId, photoBundleAnswer);
                        
                        db.SetRespondentReadyToSync(_respId);
                    }
                    else
                    {
                        throw new System.Exception("Could not do normalization while it being necessary. Cancelling user submit action.");
                    }
                    #endregion

                    _action.NewRespondentEventHandle?.Set();
                    _action.OnWorkFinished(_respId);
                }
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while making gif");
                _action.UiSynchro?.AddUiMessage("Error while making gif", 4);
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }
        
        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        public int GetRespId()
        {
            return _respId;
        }
    }
}