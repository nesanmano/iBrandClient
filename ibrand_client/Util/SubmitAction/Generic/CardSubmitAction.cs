using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Linq;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    /// <summary>
    /// Classe extensible pour créer des SubmitAction qui transforment des photos en fichier d'images
    /// destinés à être imprimés sur du papier à Carte via l'imprimante Xerox Phaser 6600
    /// </summary>
    public abstract class CardSubmitAction : AbstractSubmitAction
    {
        internal const string LEFT_COL_X = "left_col_x";
        internal const string RIGHT_COL_X = "right_col_x";
        internal const string TOP_ROW_Y = "top_row_y";
        internal const string BOTTOM_ROW_Y = "bottom_row_y";
        private const string FONT_FILES = "font_files";
        private const string QUESTION_NO_SAVE_FRONT_IMAGE = "question_no_save_front_image";
        private const string QUESTION_NO_SAVE_BACK_IMAGE = "question_no_save_back_image";
        private const string QUESTION_NO_SAVE_PRINT_IMAGE = "question_no_save_print_image";
        private const string SHOW_ON_DISPLAY_TV = "show_on_display_tv";
        private const string PRINT_AUTO = "print_auto";
        private const string IMAGES_SIZE = "images_size";
        
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private PrintJob _printJob;
        public abstract void DrawFrontImage(Respondent resp, PrivateFontCollection fontCollection, Bitmap canvas);
        public abstract void DrawBackImage(Respondent resp, PrivateFontCollection fontCollection, Bitmap canvas);
        
        public PrivateFontCollection CustomFontCollection { get; } = new PrivateFontCollection();
        public int? QuestionNoToSaveFrontImage { get; private set; }
        public int? QuestionNoToSaveBackImage { get; private set; }
        public int? QuestionNoToSavePrintImage { get; private set; }
        public bool ShowOnDisplayTv { get; private set; }
        public bool PrintAuto { get; private set; }
        public Size? ImagesSize { get; private set; }

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformCard", 1);
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new CardSubmitActionWorker(this, resp, resp.Id);
        }

        protected override void ValidateConfigData()
        {
            base.ValidateConfigData();

            if (!_configData.ContainsKey(LEFT_COL_X))
            {
                _configData.Add(LEFT_COL_X, ImageUtil.CARD_PRINT_DEFAULT_LEFT_COLUMN.ToString());
            }

            if (!_configData.ContainsKey(RIGHT_COL_X))
            {
                _configData.Add(RIGHT_COL_X, ImageUtil.CARD_PRINT_DEFAULT_RIGHT_COLUMN.ToString());
            }

            if (!_configData.ContainsKey(TOP_ROW_Y))
            {
                _configData.Add(TOP_ROW_Y, ImageUtil.CARD_PRINT_DEFAULT_TOP_ROW.ToString());
            }

            if (!_configData.ContainsKey(BOTTOM_ROW_Y))
            {
                _configData.Add(BOTTOM_ROW_Y, ImageUtil.CARD_PRINT_DEFAULT_BOTTOM_ROW.ToString());
            }

            if (_configData.ContainsKey(FONT_FILES))
            {
                var baseDir = GetTerminalDirectory;

                var fontlist = _configData[FONT_FILES].Split(StringUtil.STRING_DELIMITER_CHARS);
                foreach (var fontFile in fontlist)
                {
                    CustomFontCollection.AddFontFile(baseDir + fontFile);
                }
            }

            if (_configData.ContainsKey(QUESTION_NO_SAVE_FRONT_IMAGE))
            {
                if (int.TryParse(_configData[QUESTION_NO_SAVE_FRONT_IMAGE], out var value))
                {
                    QuestionNoToSaveFrontImage = value;
                }
            }

            if (_configData.ContainsKey(QUESTION_NO_SAVE_BACK_IMAGE))
            {
                if (int.TryParse(_configData[QUESTION_NO_SAVE_BACK_IMAGE], out var value))
                {
                    QuestionNoToSaveBackImage = value;
                }
            }

            if (_configData.ContainsKey(QUESTION_NO_SAVE_PRINT_IMAGE))
            {
                if (int.TryParse(_configData[QUESTION_NO_SAVE_PRINT_IMAGE], out var value))
                {
                    QuestionNoToSavePrintImage = value;
                }
            }

            if (_configData.ContainsKey(SHOW_ON_DISPLAY_TV))
            {
                if (bool.TryParse(_configData[SHOW_ON_DISPLAY_TV], out var value))
                {
                    if (value)
                    {
                        ShowOnDisplayTv = true;
                    }
                }
            }

            if (_configData.ContainsKey(PRINT_AUTO))
            {
                if (bool.TryParse(_configData[PRINT_AUTO], out var value))
                {
                    if (value)
                    {
                        PrintAuto = true;
                    }
                }
            }

            if (_configData.ContainsKey(IMAGES_SIZE))
            {
                var vals = StringUtil.GetDataFromDelimitedString(_configData[IMAGES_SIZE], ',');
                if (vals != null && vals.Length >= 2)
                {
                    ImagesSize = new Size(vals[0], vals[1]);
                }
                else
                {
                    ImagesSize = ImageUtil.CARD_PRINT_DEFAULT_INTERNAL_IMAGE_SIZE;
                }
            }
        }
        
        public bool PrintFinalImage(Bitmap imageToPrint, string imageToPrintPath, int respId, IbrandDatabase db)
        {
            if (!PrintAuto) return false;

            if (_printJob == null)
            {
                var printerName = PrintUtil.GetPrinterNamesList().First(printer =>
                {
                    var ps = new PrinterSettings { PrinterName = printer };
                    return printer.Contains("6600") && ps.IsValid;
                });

                if (printerName != null)
                {
                    _printJob = PrintUtil.CreatePrintJob(printerName,
                        null, null, null, null, null, null, false, "card:" + respId);
                }
            }
                
            UiSynchro?.AddUiMessage("Sending Print job.", 4);

            if (_printJob != null)
            {
                _printJob.CleanPrintActions();
                _printJob.AddActionDrawBitmap(imageToPrint);


                if (_printJob.IsReadyToPrint && _printJob.DoWork(null)) return true;

                UiSynchro?.AddUiMessage("Could not print respondent card.", 4);
                log.Error("Did not print final file correctly: " + imageToPrintPath);
            }
            else
            {
                UiSynchro?.AddUiMessage("Could not print respondent card.", 4);
                log.Error("Did not print final file correctly: " + imageToPrintPath);
            }

            return false;
        }
    }
}