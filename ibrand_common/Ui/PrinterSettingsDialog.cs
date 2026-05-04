using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui.Localization;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Print;

namespace NomadLogic.Common.Ui
{
    /// <summary>
    /// Forme pour éditer les configurations de l'impression.
    /// </summary>
    public partial class PrinterSettingsDialog : LocalizedForm
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private PrintJobInfo _config;
        private PrintCountInfo _countInfo;

        private void OnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OnComboChange(object sender, EventArgs e)
        {
            if (sender == comboPrinterName)
            {
                var newPrinterName = ((ComboBox)sender).Text;

                if (!string.IsNullOrWhiteSpace(newPrinterName))
                {
                    comboPaper.BeginUpdate();
                    comboPaper.Items.Clear();
                    comboPaper.SelectedIndex = -1;

                    var currentIndex = -1;
                    var selectIndex = -1;
                    var tmp = new PrinterSettings { PrinterName = _config.PrinterName };

                    foreach (PaperSize paper in tmp.PaperSizes)
                    {
                        currentIndex++;
                        comboPaper.Items.Add(paper.PaperName);
                        if (PrintUtil.Equals(paper, _config.PaperSize)) selectIndex = currentIndex;
                    }
                    comboPaper.SelectedIndex = selectIndex;

                    comboPaper.EndUpdate();
                }
                else
                {
                    comboPaper.BeginUpdate();
                    comboPaper.Items.Clear();
                    comboPaper.SelectedIndex = -1;
                    comboPaper.EndUpdate();
                }
            }

            SaveConfiguration(1);
        }

        private void OnRadioChange(object sender, EventArgs e)
        {
            if (!(sender is RadioButton rb)) return; // Empêche de traiter un événement sur deux (uncheck et check sont lancés).
            if (rb.Checked)
            {
                SaveConfiguration(2);
            }
        }

        private void OnChkChange(object sender, EventArgs e)
        {
            if (sender == chkScaled)
            {
                chkMaxSurfaceScale.Enabled = chkScaled.CheckState == CheckState.Checked;
                chkMaxSurfaceScale.CheckState = CheckState.Unchecked;
            }
            else if (sender == chkDetectOrientation)
            {
                chkLandscape.Enabled = chkDetectOrientation.CheckState != CheckState.Checked;
                chkLandscape.CheckState = CheckState.Unchecked;
            }
            SaveConfiguration(3);
        }
        
        private void OnNudChange(object sender, EventArgs e)
        {
            SaveConfiguration(4);
        }
        
        /// <inheritdoc />
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Met à jour la configuration
                _config = File.Exists(PrintUtil.DEFAULT_PRINTER_CONFIG_FILE.FullName) ?
                    JsonUtil.ReadJsonFile<PrintJobInfo>(PrintUtil.DEFAULT_PRINTER_CONFIG_FILE) : new PrintJobInfo();

                _countInfo = File.Exists(PrintUtil.DEFAULT_PRINTER_COUNT_FILE.FullName) ?
                    JsonUtil.ReadJsonFile<PrintCountInfo>(PrintUtil.DEFAULT_PRINTER_COUNT_FILE) : new PrintCountInfo();

                if (_config.DrawOptions == null)
                {
                    _config.DrawOptions = new PrintJobImageOption();
                }

                // CheckBox
                chkMaxSurfaceScale.Checked = _config.DrawOptions.CropMaximizedScale;
                chkDetectOrientation.Checked = _config.DrawOptions.DetectOrientation;
                chkScaled.Checked = _config.DrawOptions.Scaled;
                chkPageResolution.Checked = _config.DrawOptions.UsePageResolution;
                chkIgnoreMargins.Checked = _config.DrawOptions.IgnoreMargins;
                chkLandscape.Checked = _config.DrawOptions.LandScape;
                chkShowPrintCountWarning.Checked = _countInfo.PrintActionCounterShowWarning;

                // Radio
                switch (_config.DrawOptions.HorizontalAlignment)
                {
                    case PrintJobImageHorizontalAlignment.Left:
                        radioHLeft.Checked = true;
                        break;
                    case PrintJobImageHorizontalAlignment.Center:
                        radioHCenter.Checked = true;
                        break;
                    case PrintJobImageHorizontalAlignment.Right:
                        radioHRight.Checked = true;
                        break;
                }
                switch (_config.DrawOptions.VerticalAlignment)
                {
                    case PrintJobImageVerticalAlignment.Bottom:
                        radioVBottom.Checked = true;
                        break;
                    case PrintJobImageVerticalAlignment.Center:
                        radioVCenter.Checked = true;
                        break;
                    case PrintJobImageVerticalAlignment.Top:
                        radioVTop.Checked = true;
                        break;
                }

                // Combo
                var currentIndex = -1;
                var selectIndex = -1;
                foreach (var printer in PrintUtil.GetPrinterNamesList())
                {
                    currentIndex++;
                    comboPrinterName.Items.Add(printer);
                    if (printer.Equals(_config.PrinterName)) selectIndex = currentIndex;
                }
                comboPrinterName.SelectedIndex = selectIndex;

                if (!string.IsNullOrWhiteSpace(_config.PrinterName))
                {
                    currentIndex = -1;
                    selectIndex = -1;
                    var tmp = new PrinterSettings { PrinterName = _config.PrinterName };

                    foreach (PaperSize paper in tmp.PaperSizes)
                    {
                        currentIndex++;
                        comboPaper.Items.Add(paper.PaperName);
                        if (PrintUtil.Equals(paper, _config.PaperSize)) selectIndex = currentIndex;
                    }
                    comboPaper.SelectedIndex = selectIndex;
                }

                lblPrintCounterValue.Text = _countInfo.PrintActionCounter.ToString();
                nudPrintRoll.Value = _countInfo.PrintActionCounterThreshold;

                // À la fin, on ajoute les événements, comme ça ils ne se lancent pas lors de l'initialisation.
                chkScaled.CheckStateChanged += OnChkChange;
                chkPageResolution.CheckStateChanged += OnChkChange;
                chkIgnoreMargins.CheckStateChanged += OnChkChange;
                chkDetectOrientation.CheckStateChanged += OnChkChange;
                chkLandscape.CheckStateChanged += OnChkChange;
                chkMaxSurfaceScale.CheckStateChanged += OnChkChange;

                radioHCenter.CheckedChanged += OnRadioChange;
                radioHLeft.CheckedChanged += OnRadioChange;
                radioHRight.CheckedChanged += OnRadioChange;
                radioVCenter.CheckedChanged += OnRadioChange;
                radioVTop.CheckedChanged += OnRadioChange;
                radioVBottom.CheckedChanged += OnRadioChange;

                comboPrinterName.SelectedIndexChanged += OnComboChange;
                comboPaper.SelectedIndexChanged += OnComboChange;

                btnClose.Click += OnCloseClick;
                btnTestPrintSettings.Click += OnPrintTestClick;

                nudPrintRoll.ValueChanged += OnNudChange;
                btnResetPrintCounter.Click += OnResetPrintCountClick;
                chkShowPrintCountWarning.CheckedChanged += OnShowPrintCountWarningChange;

                OnChangeLanguage(this, null);
            }
            catch (Exception exception)
            {
                log.Error(exception, "Could not initialize Printer settings dialog. Check logs for details.");
                throw;
            }
        }

        private void OnResetPrintCountClick(object sender, EventArgs e) {
            _countInfo.PrintActionCounter = 0;
            lblPrintCounterValue.Text = _countInfo.PrintActionCounter.ToString();
            JsonUtil.WriteJsonFile(PrintUtil.DEFAULT_PRINTER_COUNT_FILE, _countInfo);
        }

        private void OnShowPrintCountWarningChange(object sender, EventArgs e)
        {
            _countInfo.PrintActionCounterShowWarning = chkShowPrintCountWarning.Checked;
            JsonUtil.WriteJsonFile(PrintUtil.DEFAULT_PRINTER_COUNT_FILE, _countInfo);
        }

        private void OnPrintTestClick(object sender, EventArgs e)
        {
            var diag = new PrintTestDialog();
            diag.ShowDialog();

            // Réinitialise le compteur
            _countInfo = File.Exists(PrintUtil.DEFAULT_PRINTER_COUNT_FILE.FullName) ?
                             JsonUtil.ReadJsonFile<PrintCountInfo>(PrintUtil.DEFAULT_PRINTER_COUNT_FILE) : new PrintCountInfo();
            lblPrintCounterValue.Text = _countInfo.PrintActionCounter.ToString();
        }

        private void SaveConfiguration(int group)
        {
            
            switch (@group)
            {
                case 1:
                    // Combo
                    _config.PrinterName = string.IsNullOrWhiteSpace(comboPrinterName.Text) ? null : comboPrinterName.Text;
                    var paperName = string.IsNullOrWhiteSpace(comboPaper.Text) ? null : comboPaper.Text;
                    if (paperName != null && _config.PrinterName != null)
                    {
                        var tmp = new PrinterSettings { PrinterName = _config.PrinterName };
                        _config.PaperSize = PrintUtil.FindPaperSizeFromPrinterSettings(tmp, paperName, true);
                    }
                    else
                    {
                        _config.PaperSize = null;
                    }
                    break;
                case 2:
                    // Radio
                    if (radioVTop.Checked)
                    {
                        _config.DrawOptions.VerticalAlignment = PrintJobImageVerticalAlignment.Top;
                    }
                    else if (radioVCenter.Checked)
                    {
                        _config.DrawOptions.VerticalAlignment = PrintJobImageVerticalAlignment.Center;
                    }
                    else if (radioVBottom.Checked)
                    {
                        _config.DrawOptions.VerticalAlignment = PrintJobImageVerticalAlignment.Bottom;
                    }

                    if (radioHLeft.Checked)
                    {
                        _config.DrawOptions.HorizontalAlignment = PrintJobImageHorizontalAlignment.Left;
                    }
                    else if (radioHCenter.Checked)
                    {
                        _config.DrawOptions.HorizontalAlignment = PrintJobImageHorizontalAlignment.Center;
                    }
                    else if (radioHRight.Checked)
                    {
                        _config.DrawOptions.HorizontalAlignment = PrintJobImageHorizontalAlignment.Right;
                    }
                    break;
                case 3:
                    // Check
                    _config.DrawOptions.CropMaximizedScale = chkMaxSurfaceScale.Checked;
                    _config.DrawOptions.DetectOrientation = chkDetectOrientation.Checked;
                    _config.DrawOptions.IgnoreMargins = chkIgnoreMargins.Checked;
                    _config.DrawOptions.LandScape = chkLandscape.Checked;
                    _config.DrawOptions.Scaled = chkScaled.Checked;
                    _config.DrawOptions.UsePageResolution = chkPageResolution.Checked;
                    break;
                case 4:
                    // NumericUpDown
                    _countInfo.PrintActionCounterThreshold = (int)nudPrintRoll.Value;
                    JsonUtil.WriteJsonFile(PrintUtil.DEFAULT_PRINTER_COUNT_FILE, _countInfo);
                    break;
            }

            if (_config == null) return;
            JsonUtil.WriteJsonFile(PrintUtil.DEFAULT_PRINTER_CONFIG_FILE, _config);
        }
        
        /// <inheritdoc />
        protected override void InitializeControlTags()
        {
            lblPrinterName.Tag = "UI_PrinterSettings_PrinterName";
            lblPaper.Tag = "UI_PrinterSettings_PaperType";
            btnClose.Tag = "UI_PrinterSettings_Close";
            radioVTop.Tag = "UI_PrinterSettings_Top";
            lblVerticalAlignment.Tag = "UI_PrinterSettings_VerticalAlignment";
            radioVBottom.Tag = "UI_PrinterSettings_Bottom";
            radioVCenter.Tag = "UI_PrinterSettings_Center";
            radioHRight.Tag = "UI_PrinterSettings_Right";
            radioHCenter.Tag = "UI_PrinterSettings_Center";
            lblHorizontalAlignment.Tag = "UI_PrinterSettings_HorizontalAlignment";
            radioHLeft.Tag = "UI_PrinterSettings_Left";
            chkScaled.Tag = "UI_PrinterSettings_Scaled";
            chkPageResolution.Tag = "UI_PrinterSettings_PageResolution";
            chkIgnoreMargins.Tag = "UI_PrinterSettings_IgnoreMargins";
            chkDetectOrientation.Tag = "UI_PrinterSettings_DetectOrientation";
            chkLandscape.Tag = "UI_PrinterSettings_Landscape";
            chkMaxSurfaceScale.Tag = "UI_PrinterSettings_MaxSurfaceScale";

            lblPrintCounterText.Tag = "UI_PrinterSettings_PrintCounter";
            lblPrintRoll.Tag = "UI_PrinterSettings_PrintRoll";
            btnResetPrintCounter.Tag = "UI_PrinterSettings_Reset";
            chkShowPrintCountWarning.Tag = "UI_PrinterSettings_ShowPrintCountWarnings";
        }

        /// <inheritdoc />
        protected override string GetLocalizedTextFromManager(string tag)
        {
            return Resources.common.ResourceManager.GetString(tag, Resources.common.Culture);
        }
        
    }
}
