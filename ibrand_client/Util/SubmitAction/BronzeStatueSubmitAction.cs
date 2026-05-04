using System;
using System.Drawing;
using System.IO;
using NLog;
using NomadLogic.iBrand.Common;
using NomadLogic.iBrand.Common.Services;
using NomadLogic.iBrand.Common.Util;
using NomadLogic.iBrand.Services;
using NomadLogic.iBrand.Synchro;
using NomadLogic.iBrand.Transformation.Video.Ffmpeg;
using NomadLogic.iBrand.Ui.Question;
using NomadLogic.iBrand.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrand.Util.SubmitAction
{
    /// <summary>
    /// Transformation écran vert de la tête au pied, puis intégration dans Photoshop via commande directe pour
    /// transformer le répondant chormatisé en "statue" via une série de filtres photoshop. On prend l'image résultante pour
    /// l'intégrer dans une image du parlement.
    /// </summary>
    public class BronzeStatueSubmitAction : AbstractSubmitAction
    {
        private const string POS_X = "pos_x";
        private const string POS_Y = "pos_y";
        private const string SIZE_X = "size_x";
        private const string SIZE_Y = "size_y";
        private const string ACTION_CATEGORY = "action_category";
        private const string ACTION_NAME = "action_name";
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transform", 1);
        }
        
        protected override ISubmitActionWorker CreateWorker(Respondent resp)
        {
            return new BronzeStatueSubmitActionWorker(this, resp, int.Parse(resp.Id));
        }

        protected override void ValidateConfigData()
        {
            if (!_config_data.ContainsKey(POS_X))
            {
                _config_data.Add(POS_X, "492");
            }

            if (!_config_data.ContainsKey(POS_Y))
            {
                _config_data.Add(POS_Y, "232");
            }

            if (!_config_data.ContainsKey(SIZE_X))
            {
                _config_data.Add(SIZE_X, "773");
            }

            if (!_config_data.ContainsKey(SIZE_Y))
            {
                _config_data.Add(SIZE_Y, "1160");
            }

            if (!_config_data.ContainsKey(ACTION_CATEGORY))
            {
                _config_data.Add(ACTION_CATEGORY, "Statue v3a");
            }

            if (!_config_data.ContainsKey(ACTION_NAME))
            {
                _config_data.Add(ACTION_NAME, "statue v3a");
            }
        }

        public string PhotoshopActionCategory
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(ACTION_CATEGORY))
                {
                    return _config_data[ACTION_CATEGORY];
                }

                return null;
            }
        }

        public string PhotoshopActionName
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(ACTION_NAME))
                {
                    return _config_data[ACTION_NAME];
                }

                return null;
            }
        }

        public int PosX
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(POS_X))
                {
                    return int.Parse(_config_data[POS_X]);
                }

                return 0;
            }
        }

        public int PosY
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(POS_Y))
                {
                    return int.Parse(_config_data[POS_Y]);
                }

                return 0;
            }
        }

        public int SizeX
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(SIZE_X))
                {
                    return int.Parse(_config_data[SIZE_X]);
                }

                return 0;
            }
        }

        public int SizeY
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(SIZE_Y))
                {
                    return int.Parse(_config_data[SIZE_Y]);
                }

                return 0;
            }
        }

        public static bool TransformPicture(string backgroundFile, string foregroundFile, string respondentBasePictureFile, string outputFilePrint,
            int posX, int posY, int sizeX, int sizeY, string actionCategory, string actionName, IUiSynchro uicallback, string tmpFileOutputKeyed = null,
            string tmpFileOutputPhotoshop = null)
        {
            var keyedPictureFile = tmpFileOutputKeyed == null ? 
                new FileInfo(DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png")) : 
                new FileInfo(tmpFileOutputKeyed);
            var tmpPictureResized = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png"));
            var photoshoppedPictureFile = tmpFileOutputPhotoshop == null ? 
                new FileInfo(DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png")) :
                new FileInfo(tmpFileOutputPhotoshop);
            
            if (tmpPictureResized.Exists)
            {
                tmpPictureResized.Delete();
            }

            if (!File.Exists(keyedPictureFile.FullName))
            {
                // 1- Redimensionnement de l'image

                using (var baseImage = ImageUtil.LoadBitmapFromFile(respondentBasePictureFile))
                {
                    using (var resized = ImageUtil.ResizeImage(baseImage, sizeX, sizeY))
                    {
                        ImageUtil.SaveToJpegWithOptions(resized, 90, tmpPictureResized.FullName);
                    }
                }

                // 2- Chroma-keying

                uicallback?.AddUiMessage("Keying picture.", 4);

                var conf = (Configuration) ServicesContainer.GetService(ServicesNames.CONFIGURATION);

                var filter = File.Exists(FfmpegChromaKeyInfo.DefaultConfigFileLocation)
                    ? FileUtil.ReadJsonFile<FfmpegChromaKeyInfo>(
                        FfmpegChromaKeyInfo.DefaultConfigFileLocation)
                    : FfmpegChromaKeyInfo.NewInstanceChromaKeyBlue();

                FfmpegUtil.ChromakeyImage(conf.FfmpegLocation, tmpPictureResized.FullName, keyedPictureFile.FullName,
                    filter);
            }

            // 3- On applique la transformation Photoshop sur le fichier résultant
            // TODO Bug avec photoshop. L'exécution de l'action donne un résultat différent quand invoquée par code.
            /*
             * uicallback?.AddUiMessage("Applying photoshop filters.", 4);
            var err = PhotoshopUtils.ExecuteActionOnFile(tmpPictureKeyed, tmpPictureStatue, actionCategory, actionName);
            if (err != null) throw err;
            */
            if (!File.Exists(photoshoppedPictureFile.FullName))
            {
                log.Info("Must manually do Photoshop transformation for respondent.");
                uicallback?.AddUiMessage("Manual Photoshop manipulation required.", 4);
                return false;
            }
            
            // 4- On ajoute le background et le foreground autour du fichier statue
            uicallback?.AddUiMessage("Adding statue to background.", 4);

            using (var bkgPict = ImageUtil.LoadBitmapFromFile(backgroundFile))
            {
                bkgPict.SetResolution(300, 300);

                using (var g = Graphics.FromImage(bkgPict))
                {
                    using (var statuePicture = ImageUtil.LoadBitmapFromFile(photoshoppedPictureFile.FullName))
                    {
                        statuePicture.SetResolution(300, 300);
                        g.DrawImageUnscaled(statuePicture, posX, posY);
                    }

                    using (var forePict = ImageUtil.LoadBitmapFromFile(foregroundFile))
                    {
                        forePict.SetResolution(300, 300);
                        g.DrawImageUnscaled(forePict, 0, 0);
                    }
                }

                ImageUtil.SaveToJpegWithOptions(bkgPict, 90, outputFilePrint);
            }

            return true;
        }

        class BronzeStatueSubmitActionWorker : ISubmitActionWorker
        {
            private static readonly Logger logWorker = LogManager.GetCurrentClassLogger();
            private readonly Respondent _respondent;
            private readonly BronzeStatueSubmitAction _action;
            private readonly int _respId;

            public BronzeStatueSubmitActionWorker(BronzeStatueSubmitAction action, Respondent resp, int respId)
            {
                _respondent = resp;
                _action = action;
                _respId = respId;
            }

            public void DoWork()
            {
                var db = (IBrandDatabase)ServicesContainer.GetService(ServicesNames.SQL_DB_IBRAND);

                try
                {
                    var baseDir = DirectoryUtil.GetLocalQuestionnaireDir(_action.ProjectId, int.Parse(_action.QuestionnaireId));

                    // Chargement des fichiers

                    var respondentBasePictureFile = _respondent.GetResponseText(1);
                    var respondentBasePictureFileId = StringUtil.RemoveExtension(StringUtil.GetFileName(respondentBasePictureFile));

                    var baseDirImage = DirectoryUtil.GetLocalQuestionnaireDataDir(_action.ProjectId,
                        int.Parse(_action.QuestionnaireId),
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                        DirectoryUtil.ProjectDataDirectoriesModEnum.print);
                    var outputFilePrint = baseDirImage + respondentBasePictureFileId + "_statue.jpg";
                    var outputFileKeyed = baseDirImage + respondentBasePictureFileId + "_photoshop.png";
                    var outputFilePhotoshop = baseDirImage + @"manuel\" + respondentBasePictureFileId + "_photoshop copy.png";

                    var displayFile = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() + respondentBasePictureFileId + "_statue.jpg";

                    var bkgFile = baseDir + "background.jpg";
                    var foreFile = baseDir + "foreground.png";

                    ImageUtil.RotateJpegToMatchExifData(respondentBasePictureFile);

                    // Appel de la transformation.

                    var result = TransformPicture(bkgFile, foreFile, respondentBasePictureFile, outputFilePrint,
                        _action.PosX, _action.PosY, _action.SizeX, _action.SizeY, _action.PhotoshopActionCategory, _action.PhotoshopActionName,
                        _action.UiSynchro, outputFileKeyed, outputFilePhotoshop);

                    if (!result)
                    {
                        /* TODO
                         * Hack pour contourner le problème que l'invocation de l'action de "statuification" de photoshop.
                         * Il faut compléter l'action manuellement à l'extérieur de l'application.
                         */
                        _action.UiSynchro?.AddUiMessage($"External photoshop action is required for photoshop for respondant {_respondent.Id}.", 4);
                        return;
                    }

                    // Sauvegarde du fichier final, et enregistrer sa location comme réponse 2
                    var ansDisplay = new Answer(QuestionPhotoText.TYPE, 2, 1, outputFilePrint);
                    db.DeleteAnswer(_respId, ansDisplay);
                    db.InsertAnswer(_respId, ansDisplay);

                    // Copie vers display on tv.
                    File.Copy(outputFilePrint, displayFile);

                    _action.UiSynchro?.AddUiMessage("Respondent is now a statue.", 4);

                    db.SetRespondentReadyToSync(_respId);

                    try { File.Delete(outputFileKeyed); } catch (System.Exception) { /* Die Nice*/ }
                    try { File.Delete(outputFilePhotoshop); } catch (System.Exception) { /* Die Nice*/ }
                }
                catch (System.Exception ex)
                {
                    logWorker.Fatal(ex, "Error while executing treatment of respondent.");
                    _action.UiSynchro?.AddUiMessage("Error while treating respondent.", 4);
                }
            }

            public AbstractSubmitAction GetSubmitAction()
            {
                return _action;
            }

            public int GetRespId()
            {
                return _respId;
            }
        }
    }
}