using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.XmlTransformation.Layer;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Fait une transformation blue-screen pour créer une image de front, puis créer une autre image pour le dos d'une carte de hockey.
    /// On leur ajoute également du texte avec un font particulier.
    /// Une fois fini, on lance l'impression, puis on termine.
    /// </summary>
    public class SofinaHockeyCardSubmitAction : AbstractSubmitAction
    {
        private const string CROUSE_MC = "crouse_mc";
        private const string HERBST_MC = "herbst_mc";
        private const string SPENCER_MC = "spencer_mc";
        private const string CROUSE_PW = "crouse_pw";
        private const string HERBST_PW = "herbst_pw";
        private const string SPENCER_PW = "spencer_pw";
        private const string LEFT_COL_X = "left_col_x";
        private const string RIGHT_COL_X = "right_col_x";
        private const string TOP_ROW_Y = "top_row_y";
        private const string BOTTOM_ROW_Y = "bottom_row_y";
        private const string DEBUG_FACE_DETECTION = "debug";
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private PrintJob _printJob;

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transform", 1);
        }
        
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new SofinaHockeyCardSubmitActionWorker(this, resp, resp.Id);
        }

        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey(CROUSE_MC))
            {
                _configData.Add(CROUSE_MC, "371,223,500,500");
            }

            if (!_configData.ContainsKey(HERBST_MC))
            {
                _configData.Add(HERBST_MC, "371,223,500,500");
            }

            if (!_configData.ContainsKey(SPENCER_MC))
            {
                _configData.Add(SPENCER_MC, "371,223,500,500");
            }

            if (!_configData.ContainsKey(CROUSE_PW))
            {
                _configData.Add(CROUSE_PW, "371,223,500,500");
            }

            if (!_configData.ContainsKey(HERBST_PW))
            {
                _configData.Add(HERBST_PW, "371,223,500,500");
            }

            if (!_configData.ContainsKey(SPENCER_PW))
            {
                _configData.Add(SPENCER_PW, "371,223,500,500");
            }

            /* Colonnes */

            if (!_configData.ContainsKey(LEFT_COL_X))
            {
                _configData.Add(LEFT_COL_X, "201");
            }

            if (!_configData.ContainsKey(RIGHT_COL_X))
            {
                _configData.Add(RIGHT_COL_X, "1247");
            }

            if (!_configData.ContainsKey(TOP_ROW_Y))
            {
                _configData.Add(TOP_ROW_Y, "240");
            }

            if (!_configData.ContainsKey(BOTTOM_ROW_Y))
            {
                _configData.Add(BOTTOM_ROW_Y, "1520");
            }

            if (!_configData.ContainsKey(DEBUG_FACE_DETECTION))
            {
                _configData.Add(DEBUG_FACE_DETECTION, "false");
            }
        }
        
        public PrivateFontCollection CustomFontCollection { get; } = new PrivateFontCollection();

        class SofinaHockeyCardSubmitActionWorker : ISubmitActionWorker
        {
            private static readonly Logger logWorker = LogManager.GetCurrentClassLogger();
            private readonly Respondent _respondent;
            private readonly SofinaHockeyCardSubmitAction _action;
            private readonly int _respId;

            public SofinaHockeyCardSubmitActionWorker(SofinaHockeyCardSubmitAction action, Respondent resp, int respId)
            {
                _respondent = resp;
                _action = action;
                _respId = respId;
            }

            public void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                var db = ServicesContainer.GetService<IbrandDatabase>();

                try
                {
                    var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(_action.ProjectId, _action.QuestionnaireId, _action.TerminalId);

                    var frontImage = baseDir + "Hockey_front_bleed.png";
                    var backImage = baseDir + "Hockey_Back_bleed.png";

                    var strSubchoice = _respondent.GetAnswerText(9); // donne '1' ou '2' (anglais ou français)
                    if (strSubchoice.Equals("2"))
                    {
                        backImage = baseDir + "Hockey_Back_bleed_fr.png";
                    }

                    using (var frontBasePicture = FileUtil.LoadBitmap(frontImage))
                    {
                        frontBasePicture.SetResolution(300, 300);

                        using (var backBasePicture = FileUtil.LoadBitmap(backImage))
                        {
                            backBasePicture.SetResolution(300, 300);

                            // 1- Chargement des fichiers

                            var facePicture = _respondent.GetAnswerText(1);
                            var facePictureFileId = StringUtil.RemoveExtension(StringUtil.GetFileName(facePicture));

                            var outputFilePrint = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.ProjectId, _action.QuestionnaireId, _action.TerminalId,
                                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.print) + facePictureFileId + "_print.jpg";

                            var outputFileDisplay = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.ProjectId, _action.QuestionnaireId, _action.TerminalId,
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
                            var prenom = _respondent.GetAnswerText(4).ToUpper();
                            var nom = _respondent.GetAnswerText(5).ToUpper();

                            // 2- Modification de l'image de front

                            _action.UiSynchro?.AddUiMessage("Creating front image.", 4);
                            
                            var photoLayer = new PhotoLayer(0,0, new[] { 0, 0 })
                            {
                                FaceDetection = true,
                                GreenScreen = true,
                                QuestionNo = 1,
                                Exif = true,
                                Square = true
                            };
                            
                            photoLayer.Draw(frontBasePicture, _respondent); // Met l'image de la face dans l'image

                            frontBasePicture.AddTextToBitmap(prenom, 0 + rotateOffset, 943, false, couleurPrenom, fontPrenomFront, -22.45f);
                            frontBasePicture.AddTextToBitmap(nom, 42 + rotateOffset, 974, false, couleurNom, fontNomFront, -22.45f);

                            // On sauvegarde cette image comme étant celle à afficher sur le display
                            if (File.Exists(outputFileDisplay))
                            {
                                File.Delete(outputFileDisplay);
                            }
                            frontBasePicture.Save(90, outputFileDisplay);

                            var ansDisplay = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = 3, Response = outputFileDisplay};
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

                            backBasePicture.AddTextToBitmap(prenom, 70, 80, false, couleurPrenom, fontPrenomBack);
                            backBasePicture.AddTextToBitmap(nom, 112, 119, false, couleurNom, fontNomBack);

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

                                    g.DrawImageUnscaled(frontBasePicture, int.Parse(_action._configData[LEFT_COL_X]), int.Parse(_action._configData[TOP_ROW_Y]));
                                    g.DrawImageUnscaled(frontBasePicture, int.Parse(_action._configData[RIGHT_COL_X]), int.Parse(_action._configData[TOP_ROW_Y]));

                                    g.DrawImageUnscaled(backBasePicture, int.Parse(_action._configData[LEFT_COL_X]), int.Parse(_action._configData[BOTTOM_ROW_Y]));
                                    g.DrawImageUnscaled(backBasePicture, int.Parse(_action._configData[RIGHT_COL_X]), int.Parse(_action._configData[BOTTOM_ROW_Y]));
                                }

                                // On sauvegarde cette image comme étant celle à imprimer
                                if (File.Exists(outputFilePrint))
                                {
                                    File.Delete(outputFilePrint);
                                }
                                baseBmp.Save(90, outputFilePrint);

                                var ansPrint = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = 2, Response = outputFilePrint};
                                db.DeleteAnswer(_respId, ansPrint);
                                db.InsertAnswer(_respId, ansPrint);

                                // 5- On imprime le fichier
                                if (_action._printJob == null)
                                {
                                    var printerName = PrintUtil.GetPrinterNamesList().First(printer =>
                                    {
                                        var ps = new PrinterSettings { PrinterName = printer };
                                        return printer.Contains("6600") && ps.IsValid;
                                    });

                                    if (printerName != null)
                                    {
                                        _action._printJob = PrintUtil.CreatePrintJob(printerName,
                                            null, null, null, null, null, null, false, "sof-2016-02:" + _respId);
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
}