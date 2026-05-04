using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    class CardSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger logWorker = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly CardSubmitAction _action;
        private readonly int _respId;

        public CardSubmitActionWorker(CardSubmitAction action, Respondent resp, int respId)
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
                var size = ImageUtil.CARD_PRINT_DEFAULT_INTERNAL_IMAGE_SIZE;
                using (var frontBasePicture = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
                {
                    frontBasePicture.SetResolution(300, 300);

                    using (var backBasePicture = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
                    {
                        backBasePicture.SetResolution(300, 300);

                        // 1- Chargement des fichiers
                        var filesName = GenerateUniqueId.GetId();

                        var outputFilePrint = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                                  DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                  DirectoryUtil.ProjectDataDirectoriesModEnum.print) + filesName + "_print.jpg";

                        var outputFileDisplay = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                    DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + filesName + "_display.jpg";


                        _action.DrawFrontImage(_respondent, _action.CustomFontCollection, frontBasePicture);

                        // On sauvegarde cette image comme étant celle à afficher sur le display
                        if (File.Exists(outputFileDisplay))
                        {
                            File.Delete(outputFileDisplay);
                        }
                        frontBasePicture.Save(ImageFormat.Jpeg, 90, outputFileDisplay);

                        if (_action.QuestionNoToSaveFrontImage != null)
                        {
                            var ansDisplay = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = _action.QuestionNoToSaveFrontImage.Value, Response = outputFileDisplay};
                            db.DeleteAnswer(_respId, ansDisplay);
                            db.InsertAnswer(_respId, ansDisplay);
                        }

                        // 2b- Déplacer (copier) l'image de présentation vers le répertoire d'affichage sur Télé

                        if (_action.ShowOnDisplayTv)
                        {
                            var tvDisplayCopyFile = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() +
                                                    StringUtil.GetFileName(outputFileDisplay);
                            if (File.Exists(tvDisplayCopyFile))
                            {
                                File.Delete(tvDisplayCopyFile);
                            }
                            File.Copy(outputFileDisplay, tvDisplayCopyFile);
                        }

                        // 3- Modification de l'image arrière

                        _action.UiSynchro?.AddUiMessage("Creating back image.", 4);

                        _action.DrawBackImage(_respondent, _action.CustomFontCollection, backBasePicture);

                        if (_action.QuestionNoToSaveBackImage != null)
                        {
                            
                            string backImageNameOutputFile = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed)+"backImage.jpg";
                            /*string[] fics = FileUtil.GetFiles(DirectoryUtil.GetLocalIbrandQuestionnaireDir(_action.ProjectId, _action.QuestionnaireId), "*.jpg");

                            foreach (string s in fics)
                            {
                                if (s.Contains("Back"))
                                {
                                    backImageNameOutputFile += StringUtil.GetFileName(s);
                                }
                            }
                            */
                         
                            //DirectoryUtil.GetLocalIbrandQuestionnaireDir(_action.ProjectId, _action.QuestionnaireId) + filesName + "TradingCard_Back.jpg";
                            backBasePicture.Save(ImageFormat.Jpeg, 90, backImageNameOutputFile);
                            var ansDisplay = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = _action.QuestionNoToSaveBackImage.Value, Response = backImageNameOutputFile};
                            db.DeleteAnswer(_respId, ansDisplay);
                            db.InsertAnswer(_respId, ansDisplay);
                        }
                        
                        // 4- Création de l'image pour l'impression

                        _action.UiSynchro?.AddUiMessage("Creating print image.", 4);

                        var coords = new[]
                        {
                            int.Parse(_action._configData[CardSubmitAction.LEFT_COL_X]),
                            int.Parse(_action._configData[CardSubmitAction.TOP_ROW_Y]),
                            int.Parse(_action._configData[CardSubmitAction.RIGHT_COL_X]),
                            int.Parse(_action._configData[CardSubmitAction.BOTTOM_ROW_Y])
                        };

                        using (var baseBmp = ImageUtil.CreateCardPrintBitmap(frontBasePicture,backBasePicture, coords[0], coords[1], coords[2], coords[3]))
                        {
                            // On sauvegarde cette image comme étant celle à imprimer
                            if (File.Exists(outputFilePrint))
                            {
                                File.Delete(outputFilePrint);
                            }
                            baseBmp.Save(90, outputFilePrint);

                            if (_action.QuestionNoToSavePrintImage != null)
                            {
                                var ansPrint = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = _action.QuestionNoToSavePrintImage.Value, Response = outputFilePrint};
                                db.DeleteAnswer(_respId, ansPrint);
                                db.InsertAnswer(_respId, ansPrint);
                            }

                            // 5- On imprime le fichier
                            var printResult =_action.PrintFinalImage(baseBmp, outputFilePrint, _respId, db);

                            // SI l'impression n'est pas active, ou si l'impression est active et que l'action d'impression à réussie.
                            if ((printResult && _action.PrintAuto) || !_action.PrintAuto)
                            {
                                // 6- Tout est beau, on officialize le répondant comme prêt.
                                db.SetRespondentReadyToSync(_respId);
                                _action.UiSynchro?.AddUiMessage("Respondent Card action finished.", 4);
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