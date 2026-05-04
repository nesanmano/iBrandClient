using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Classe d'exécution pour le traitement d'un groupe d'image.
    /// </summary>
    public class BundleCameraRingFilesWorker : SubmitActionWorkerAbstract<CreateFileBundleSubmitAction>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly SubmitActionWorkerPool _pool;

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="action">L'action qui appelle ce travailleur.</param>
        /// <param name="resp">Le répondant à traiter.</param>
        /// <param name="respId">L'identifiant du répondant à traiter.</param>
        /// <param name="secondPool">La référence vers la deuxième partie possible du traitement, l'envoi vers le printstation.</param>
        public BundleCameraRingFilesWorker(CreateFileBundleSubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool)
            : base(action, resp, respId)
        {
            _pool = secondPool;
        }

        /// <summary>
        /// Référence vers le deuxième worker une fois la première étape complétée.
        /// </summary>
        public SendFilesToPrintstationWorker SecondWorkerStep { get; private set; }

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "InvertIf")]
        public override void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            if (_action._stopped) return;

            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();

                _respondent.Answers = db.GetAnswers(_respondent.Id);
                _respondent.DynamicHeaders = db.GetDynamicHeaders(_respondent.Id);

                var config = ServicesContainer.GetService<NomadConfiguration>();

                var questionBundleNo =
                    int.Parse(_action._configData[CreateFileBundleSubmitAction.QUESTION_BUNDLE_NUMBER]);
                // Si -1, il n'y a pas de création vidéo.
                var questionVideoNo = int.Parse(_action._configData[CreateFileBundleSubmitAction.QUESTION_VIDEO_NUMBER]);
                var questionFirstPictureNo = int.Parse(_action._configData[CreateFileBundleSubmitAction.QUESTION_FIRST_PICTURE_NUMBER]);
                // Si -1, pas de question thumbnail (first picture)
                var questionThumbnailNo = int.Parse(_action._configData[CreateFileBundleSubmitAction.QUESTION_THUMBNAIL_NUMBER]);

                {
                    var bundleQuestionResponse = _respondent.GetAnswerText(questionBundleNo, 1);
                    //Question photo bundle

                    // Étape #1 Normalisation des fichiers d'image et création du fichier ZIP
                    if (!bundleQuestionResponse.EndsWith(".zip")) // Le fichier ZIP est déjà créé, on saute cette étape.
                    {
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

                        if (questionFirstPictureNo != -1)
                        {
                            var firstPictFileName =
                                DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                    DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail) +
                                    GenerateUniqueId.GetId() + ".jpg";

                            File.Copy(orderedPictureList.First().FullName, firstPictFileName);

                            var fistQuestionAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = questionFirstPictureNo, Response = firstPictFileName};
                            db.DeleteAnswer(_respId, fistQuestionAnswer);
                            db.InsertAnswer(_respId, fistQuestionAnswer);
                        }

                        FileInfo[] picturesToUse;
                        var zippedFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                                 DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                 DirectoryUtil.ProjectDataDirectoriesModEnum.bundle) +
                                             GenerateUniqueId.GetId() + ".zip";

                        var canBeUsed = true;
                        if (bool.Parse(_action._configData[CreateFileBundleSubmitAction.NORMALIZE_PICTURES]))
                        {
                            // Utilise un répertoire  temporaire custom pour prévenir un bogue avec la commande FFMPEG, plus loin.
                            var transformDir =
                                new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId());
                            Directory.CreateDirectory(transformDir.FullName);

                            var normalizationFile = new FileInfo(FileBundleNormalizationTransforms.DefaultTransformsJsonFileLocation);

                            if (!File.Exists(normalizationFile.FullName))
                            {
                                log.Warn(
                                    "No normalization file found. Please create a normalization file. Suspending worker.");
                                _action.OnWorkFinished(_respId);
                                return;
                            }
                            var transformInfo = JsonUtil.ReadJsonFile<FileBundleNormalizationTransforms>(normalizationFile);

                            string cropSize = null;
                            if (_action._configData.ContainsKey(CreateFileBundleSubmitAction.NORMALIZATION_CROP_SIZE))
                                cropSize = _action._configData[CreateFileBundleSubmitAction.NORMALIZATION_CROP_SIZE];

                            Size? newSize = null;
                            if (cropSize != null)
                            {
                                newSize = StringUtil.SizeFromStringDefinition(cropSize);
                            }

                            canBeUsed = OpenCvUtil.NormalizeFileBundleWithTransforms(orderedPictureList, transformInfo, transformDir, newSize);
                            picturesToUse = transformDir.GetFiles();
                        }
                        else
                        {
                            picturesToUse = new DirectoryInfo(bundleQuestionResponse).GetFiles();
                        }

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

                            var photoBundleAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoBundle, QuestionNo = questionBundleNo, Response = zippedFileName};
                            db.DeleteAnswer(_respId, photoBundleAnswer);
                            db.InsertAnswer(_respId, photoBundleAnswer);

                            FileUtil.DeleteFolderContent(bundleQuestionResponse);
                            _respondent.Answers.First(a => a.QuestionNo.Equals(questionBundleNo.ToString())).Response =
                                zippedFileName;
                        }
                        else
                        {
                            throw new System.Exception("Could not do normalization while it being necessary. Cancelling user submit action.");
                        }
                    }
                }

                if (questionVideoNo != -1)
                {
                    var videoFilePath = _respondent.GetAnswerText(questionVideoNo, 1); //Question video

                    if (!File.Exists(videoFilePath))
                    {
                        var id = GenerateUniqueId.GetId();

                        videoFilePath =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.none) + id + ".mp4";

                        DirectoryInfo dirTmp = null;
                        try
                        {
                            // On va chercher les images à partir du fichier zip (car le dossier temporaire n'existe p-e pas.
                            // Utilise un répertoire temporaire custom (que l'on peut changer avec les variables systèmes) pour empêcher
                            // un bogue avec la commande FFMPEG
                            dirTmp = new DirectoryInfo(Path.GetTempPath() + id + Path.DirectorySeparatorChar);
                            dirTmp.Create();

                            var zipFile = _respondent.GetAnswerText(questionBundleNo, 1);

                            using (var zip = ZipFile.Read(zipFile))
                            {
                                foreach (var e in zip)
                                {
                                    e.Extract(dirTmp.FullName);
                                }
                            }

                            // Il faut maintenant renommer les fichiers selon pattern précis car la commande FFMPEG DOIT avoir besoin d'une séquence commençant par 1 et sans 
                            // numéro manquants dans tous les noms de fichiers.
                            var filesToRename = dirTmp.GetFiles().ToArray();
                            for (var i = 0; i < filesToRename.Length; i++)
                            {
                                var ori = filesToRename[i].FullName;
                                var renamed = dirTmp.FullName + (1000 + i) + ".jpg";

                                File.Move(ori, renamed);
                            }

                            FfmpegUtil.CreateVideoFromPictureFiles(config.FfmpegLocation, dirTmp, "%04d.jpg", videoFilePath, 6, 10, 1000);

                            if (!File.Exists(videoFilePath))
                            {
                                log.Error("Could not create video from directory. Make sure it is ASCII compatible with TMP and TEMP environment variables, and reboot: {0}", dirTmp);
                                _action.OnWorkFinished(_respId);
                                return;
                            }

                            var videoAnswer = new Answer{QuestionType = QuestionTypeEnum.Video, QuestionNo = questionVideoNo, Response = videoFilePath};
                            db.InsertAnswer(_respId, videoAnswer);
                        }
                        catch (System.Exception e)
                        {
                            log.Error(e, "Could not create video from directory. Make sure it is ASCII compatible with TMP and TEMP environment variables, and reboot: {0}", dirTmp);
                            _action.OnWorkFinished(_respId);
                            return;
                        }
                        finally
                        {
                            FileUtil.DeleteFolderContent(dirTmp?.FullName);
                        }
                    }

                    // Vérification d'une question thumbnail
                    if (questionThumbnailNo != -1)
                    {
                        var thumbnailFilePath = _respondent.GetAnswerText(questionThumbnailNo, 1); //Question thumbnail

                        if (!File.Exists(thumbnailFilePath))
                        {
                            var thumbnailTime = float.Parse(_action._configData[CreateFileBundleSubmitAction.THUMBNAIL_TIME]);

                            FfmpegUtil.ExtractSingleFrame(config.FfmpegLocation, videoFilePath, thumbnailTime, thumbnailFilePath);

                            var thumbnailAnswer = new Answer{QuestionType = QuestionTypeEnum.Video, QuestionNo = questionThumbnailNo, Response = thumbnailFilePath};
                            db.InsertAnswer(_respId, thumbnailAnswer);
                        }
                    }
                }

                if (bool.Parse(_action._configData[CreateFileBundleSubmitAction.SEND_TO_PRINTSTATION]))
                {
                    // Si le bassin est nul, alors on fait des transformations synchrones, et on aura besoin d'avoir accès au
                    // deuxième travailleur de l'extérieur.
                    SecondWorkerStep = new SendFilesToPrintstationWorker(_action, _respondent, _respId);

                    // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                    _pool?.AddWorker(SecondWorkerStep);
                }
                else
                {
                    db.SetRespondentReadyToSync(_respId);
                    _action.NewRespondentEventHandle?.Set();
                    _action.OnWorkFinished(_respId);
                }
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while executing worker.");
                _action.UiSynchro?.AddUiMessage("Error while executing worker.", 4);
            }
        }

        protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
        {
            throw new System.NotImplementedException();
        }

        protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
        {
            throw new System.NotImplementedException();
        }
    }
}
