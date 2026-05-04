using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.iBrand.Common;
using NomadLogic.iBrand.Common.Services;
using NomadLogic.iBrand.Common.Util;
using NomadLogic.iBrand.Common.Util.Print;
using NomadLogic.iBrand.Services;
using NomadLogic.iBrand.Transformation.Layer;
using NomadLogic.iBrand.Ui.Question;
using NomadLogic.iBrand.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrand.Util.SubmitAction
{
    /// <summary>
    /// Fait une transformation blue-screen pour créer une image de front, puis créer une autre image pour le dos d'une carte de hockey.
    /// On leur ajoute également du texte avec un font particulier.
    /// Une fois fini, on lance l'impression, puis on termine.
    /// </summary>
    public class ChevroletHockeyCardSubmitAction : AbstractSubmitAction
    {
        private const string FACE_X = "face_x";
        private const string FACE_Y = "face_y";
        private const string FACE_SIZE_X = "face_size_x";
        private const string FACE_SIZE_Y = "face_size_y";
        private const string LEFT_COL_X = "left_col_x";
        private const string RIGHT_COL_X = "right_col_x";
        private const string TOP_ROW_Y = "top_row_y";
        private const string BOTTOM_ROW_Y = "bottom_row_y";
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private PrintJob _printJob;

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transform", 1);
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp)
        {
            return new ChevroletHockeyCardSubmitActionWorker(this, resp, int.Parse(resp.Id));
        }

        protected override void ValidateConfigData()
        {
            if (!_config_data.ContainsKey(FACE_X))
            {
                _config_data.Add(FACE_X, "340");
            }

            if (!_config_data.ContainsKey(FACE_Y))
            {
                _config_data.Add(FACE_Y, "200");
            }

            if (!_config_data.ContainsKey(FACE_SIZE_X))
            {
                _config_data.Add(FACE_SIZE_X, "350");
            }

            if (!_config_data.ContainsKey(FACE_SIZE_Y))
            {
                _config_data.Add(FACE_SIZE_Y, "350");
            }

            /* Colonnes */

            if (!_config_data.ContainsKey(LEFT_COL_X))
            {
                _config_data.Add(LEFT_COL_X, "201");
            }

            if (!_config_data.ContainsKey(RIGHT_COL_X))
            {
                _config_data.Add(RIGHT_COL_X, "1247");
            }

            if (!_config_data.ContainsKey(TOP_ROW_Y))
            {
                _config_data.Add(TOP_ROW_Y, "240");
            }

            if (!_config_data.ContainsKey(BOTTOM_ROW_Y))
            {
                _config_data.Add(BOTTOM_ROW_Y, "1520");
            }
        }

        public int FaceX
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(FACE_X))
                {
                    return int.Parse(_config_data[FACE_X]);
                }

                return 0;
            }
        }

        public int FaceY
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(FACE_Y))
                {
                    return int.Parse(_config_data[FACE_Y]);
                }

                return 0;
            }
        }

        public int FaceSizeX
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(FACE_SIZE_X))
                {
                    return int.Parse(_config_data[FACE_SIZE_X]);
                }

                return 0;
            }
        }

        public int FaceSizeY
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(FACE_SIZE_Y))
                {
                    return int.Parse(_config_data[FACE_SIZE_Y]);
                }

                return 0;
            }
        }

        public int LeftCol
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(LEFT_COL_X))
                {
                    return int.Parse(_config_data[LEFT_COL_X]);
                }

                return 0;
            }
        }

        public int RightCol
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(RIGHT_COL_X))
                {
                    return int.Parse(_config_data[RIGHT_COL_X]);
                }

                return 0;
            }
        }

        public int TopRow
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(TOP_ROW_Y))
                {
                    return int.Parse(_config_data[TOP_ROW_Y]);
                }

                return 0;
            }
        }

        public int BottomRow
        {
            get
            {
                if (_config_data != null && _config_data.ContainsKey(BOTTOM_ROW_Y))
                {
                    return int.Parse(_config_data[BOTTOM_ROW_Y]);
                }

                return 0;
            }
        }

        public PrivateFontCollection CustomFontCollection { get; } = new PrivateFontCollection();

        class ChevroletHockeyCardSubmitActionWorker : ISubmitActionWorker
        {
            private static readonly Logger logWorker = LogManager.GetCurrentClassLogger();
            private readonly Respondent _respondent;
            private readonly ChevroletHockeyCardSubmitAction _action;
            private readonly int _respId;

            public ChevroletHockeyCardSubmitActionWorker(ChevroletHockeyCardSubmitAction action, Respondent resp, int respId)
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
                    var baseDir = DirectoryUtil.GetLocalQuestionnaireDir(_action.ProjectId,
                        int.Parse(_action.QuestionnaireId));
                    
                    using (var frontBasePicture = ImageUtil.LoadBitmapFromFile(baseDir + "Hockey_front_bleed.png"))
                    {
                        frontBasePicture.SetResolution(300,300);

                        using (var backBasePicture = ImageUtil.LoadBitmapFromFile(baseDir + "Hockey_Back_bleed.png"))
                        {
                            backBasePicture.SetResolution(300, 300);

                            // 1- Chargement des fichiers

                            var facePicture = _respondent.GetResponseText(1);
                            var facePictureFileId = StringUtil.RemoveExtension(StringUtil.GetFileName(facePicture));
                            
                            var outputFilePrint = DirectoryUtil.GetLocalQuestionnaireDataDir(_action.ProjectId,
                                int.Parse(_action.QuestionnaireId),
                                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.print) + facePictureFileId + "_print.jpg";

                            var outputFileDisplay = DirectoryUtil.GetLocalQuestionnaireDataDir(_action.ProjectId,
                                int.Parse(_action.QuestionnaireId),
                                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + facePictureFileId + "_display.jpg";
                            
                            if (!_action.CustomFontCollection.Families.Any()) // Si vide
                            {
                                var fontBoldItalicFileName = baseDir + "Louis-BoldItalic.otf";
                                var fontRegularFileName = baseDir + "Louis-Regular.otf";
                                _action.CustomFontCollection.AddFontFile(fontRegularFileName);
                                _action.CustomFontCollection.AddFontFile(fontBoldItalicFileName);
                            }

                            // Variables pour le texte
                            var fontFamilly = _action.CustomFontCollection.Families.First(fontCheck => fontCheck.Name.Contains("Louis"));
                            const float scale = 4f;
                            const int rotateOffset = -250;
                            var fontPrenomFront = new Font(fontFamilly, 10.92f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                            var fontNomFront = new Font(fontFamilly, 19.94f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                            var fontPrenomBack = new Font(fontFamilly, 12.33f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                            var fontNomBack = new Font(fontFamilly, 18.49f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                            var couleurPrenom = new SolidBrush(Color.White);
                            var couleurNom = new SolidBrush(Color.FromArgb(255, 228, 160, 38));
                            var prenom = _respondent.GetResponseText(4).ToUpper();
                            var nom = _respondent.GetResponseText(5).ToUpper();

                            // 2- Modification de l'image de front
                            
                            _action.UiSynchro?.AddUiMessage("Creating front image.", 4);

                            var photoLayer = new PhotoLayer(_action.FaceX, _action.FaceY, new [] {_action.FaceSizeX, _action.FaceSizeY})
                            {
                                FaceDetection = true,
                                GreenScreen = true,
                                QuestionNo = 1,
                                Exif = true,
                                Square = true
                            };

                            photoLayer.Draw(frontBasePicture, _respondent); // Met l'image de la face dans l'image
                            
                            ImageUtil.AddTextToBitmap(frontBasePicture, prenom, 0 + rotateOffset, 943, false, couleurPrenom, fontPrenomFront, -22.45f, null, null);
                            ImageUtil.AddTextToBitmap(frontBasePicture, nom, 42 + rotateOffset, 974, false, couleurNom, fontNomFront, -22.45f, null, null);

                            // On sauvegarde cette image comme étant celle à afficher sur le display
                            if (File.Exists(outputFileDisplay))
                            {
                                File.Delete(outputFileDisplay);
                            }
                            ImageUtil.SaveToJpegWithOptions(frontBasePicture, 90, outputFileDisplay);

                            var ansDisplay = new Answer(QuestionPhotoText.Type, 3, 1, outputFileDisplay);
                            db.DeleteAnswer(_respId, ansDisplay);
                            db.InsertAnswer(_respId, ansDisplay);

                            // 2b- Déplacer (copier) l'image de présentation vers le répertoire d'affichage sur Télé
                            
                            var tvDisplayCopyFile = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() +
                                                    StringUtil.GetFileName(outputFileDisplay);
                            if (File.Exists(tvDisplayCopyFile))
                            {
                                File.Delete(tvDisplayCopyFile);
                            }
                            File.Copy(outputFileDisplay, tvDisplayCopyFile);

                            // 3- Modification de l'image arrière

                            _action.UiSynchro?.AddUiMessage("Creating back image.", 4);

                            ImageUtil.AddTextToBitmap(backBasePicture, prenom, 70, 80, false, couleurPrenom, fontPrenomBack, null, null, null);
                            ImageUtil.AddTextToBitmap(backBasePicture, nom, 112, 119, false, couleurNom, fontNomBack, null, null, null);

                            backBasePicture.RotateFlip(RotateFlipType.Rotate180FlipNone); // Il faut l'imprimer à l'envers

                            // 4- Création de l'image pour l'impression

                            _action.UiSynchro?.AddUiMessage("Creating print image.", 4);

                            // Faut créer un bitmap de 2550x3300, soit 8.5x11 pouces en 300 DPI
                            using (var baseBmp = new Bitmap(2550, 3300, PixelFormat.Format32bppArgb))
                            {
                                baseBmp.SetResolution(300, 300);

                                using (var g = Graphics.FromImage(baseBmp))
                                {
                                    // Fond blanc
                                    g.FillRectangle(new SolidBrush(Color.White), 0, 0, baseBmp.Width, baseBmp.Height);

                                    g.DrawImageUnscaled(frontBasePicture, _action.LeftCol, _action.TopRow);
                                    g.DrawImageUnscaled(frontBasePicture, _action.RightCol, _action.TopRow);

                                    g.DrawImageUnscaled(backBasePicture, _action.LeftCol, _action.BottomRow);
                                    g.DrawImageUnscaled(backBasePicture, _action.RightCol, _action.BottomRow);
                                }

                                // On sauvegarde cette image comme étant celle à imprimer
                                if (File.Exists(outputFilePrint))
                                {
                                    File.Delete(outputFilePrint);
                                }
                                ImageUtil.SaveToJpegWithOptions(baseBmp, 90, outputFilePrint);

                                var ansPrint = new Answer(QuestionPhotoText.Type, 2, 1, outputFilePrint);
                                db.DeleteAnswer(_respId, ansPrint);
                                db.InsertAnswer(_respId, ansPrint);

                                // 5- On imprime le fichier

                                if (_action._printJob == null)
                                {
                                    var printerName = PrintUtil.GetPrinterNamesList().First(printer =>
                                    {
                                        var ps = new PrinterSettings {PrinterName = printer};
                                        return printer.Contains("6600") && ps.IsValid;
                                    });

                                    if (printerName != null)
                                    {
                                        _action._printJob = PrintUtil.CreatePrintJob(printerName,
                                            null, null, null, null, null, null, false, "mac-2016-01:" + _respId);
                                    }
                                }
                                
                                _action.UiSynchro?.AddUiMessage("Sending Print job.", 4);
                                
                                if (_action._printJob != null)
                                {
                                    _action._printJob.CleanPrintActions();
                                    _action._printJob.AddActionDrawBitmap(baseBmp);
                                    
                                    if (_action._printJob.IsReadyToPrint && _action._printJob.DoWork(null))
                                    {
                                        // 6- Tout est beau, on officialize le répondant comme prêt.
                                        db.SetRespondentReadyToSync(_respId);
                                        _action.UiSynchro?.AddUiMessage("Respondent hockey card action finished.", 4);
                                    }
                                    else
                                    {
                                        _action.UiSynchro?.AddUiMessage("Could not print respondent hockey card.", 4);
                                        log.Error("Did not print final file correctly: " + outputFilePrint);
                                    }
                                }
                                else
                                {
                                    _action.UiSynchro?.AddUiMessage("Could not print respondent hockey card.", 4);
                                    log.Error("Did not print final file correctly: " + outputFilePrint);
                                }
                            }
                        }
                    }
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