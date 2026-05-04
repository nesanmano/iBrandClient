using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.CameraRing;
using NomadLogic.Common.Ui.Configuration.Eshots;
using NomadLogic.Common.Ui.Localization;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Gpu.Ui.BackgroundPictureDialog;
using NomadLogic.Multimedia.Gpu.Ui.ChromaKeyDialogs;

#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public partial class TouchModeConfigurationDlg : LocalizedForm
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private readonly ScreenConfig _currentConfig;
        public static event EventHandler LanguageChanged;

        public TouchModeConfigurationDlg(ScreenConfig screenConfig = null)
        {
            _currentConfig = screenConfig;
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            okButton.Click += OnOk;
            closeAppButton.Click += OnCloseApp;
            UnloadAndCloseButton.Click += OnUnloadAndCloseApp;
            cameraSettingsButton.Click += OnCameraSettings;
            buttonChromaKeySettings.Click += OnChromaKeySettingsClick;
            btnLocalImageTransform.Click += OnCustomImageTransformClick;
            btnPrinterSettings.Click += OnPrinterSettingsClick;
            btnBasePictureSettings.Click += OnBasePictureSettingsClick;
            buttonEshotsAPI.Click += OnEshotsApiSettingsClick;
            button_export_activity_data.Click += OnExportActivityDataCLick;

            languageComboBox.Items.AddRange(new object[] {
            "English",
            "Français"});

            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();

            printOffsetXTextBox.Text = nomadConfig.PrintPictureOffsetX.ToString();
            
            languageComboBox.SelectedIndex = nomadConfig.PreferedLanguage.Equals("en") ? 0 : 1;

            var g = CreateGraphics();
            Size = new Size((int)(Size.Width * g.DpiX / 96), (int)(Size.Height * g.DpiY / 96));

            Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - Width) / 2, 5);

            languageComboBox.SelectedIndexChanged += LanguageDropDownSelectedIndexChanged;

            OnChangeLanguage(this, null);
        }

        private void OnPrinterSettingsClick(object sender, EventArgs e)
        {
            var diag = new PrinterSettingsDialog();
            diag.ShowDialog();
        }

        private void OnCloseApp(object sender, EventArgs e)
        {
            log.Trace("OnCloseApp Invoked");
            if (!SaveConfiguration()) return;

            var acti = ServicesContainer.GetService<ActivityLoggerService>();
            acti?.LogActivity(-1, ActivityLoggerActionType.Debug, "Application closing.");

            Close();
            log.Trace("Calling Contained Close");
            WorkingArea.Container.Close();
        }

        private void OnUnloadAndCloseApp(object sender, EventArgs e)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            db?.ClearActive();
            db?.SetStartFullScreen(false);
            log.Debug("Questionnaire par défaut déchargé.");
            OnCloseApp(sender, e);
        }

        private void OnCameraSettings(object sender, EventArgs e)
        {
            var camera = CanonCameraManager.GetInstance().GetCamera();

            if (camera != null)
            {
                using (var dlg = new CameraSettingsDialog(camera))
                {
                    dlg.ShowDialog(WorkingArea.Container);
                }
            }
            else
            {
                MessageBox.Show(this, Resources.iBrand.UI_NoCameraFound, Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void LanguageDropDownSelectedIndexChanged(object sender, EventArgs e)
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            if (languageComboBox.SelectedIndex == 0) // english
            {
                Resources.iBrand.Culture = EnCulture;
                configuration.PreferedLanguage = "en";
            }
            else // french
            {
                Resources.iBrand.Culture = FrCulture;
                configuration.PreferedLanguage = "fr";
            }

            LanguageChanged?.Invoke(this, e);
            OnChangeLanguage(this, null);
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (SaveConfiguration())
            {
                Close();
            }
        }

        private bool SaveConfiguration()
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            
            switch (languageComboBox.SelectedIndex)
            {
                case 0:
                    configuration.PreferedLanguage = "en";
                    break;
                case 1:
                    configuration.PreferedLanguage = "fr";
                    break;
                default:
                    configuration.PreferedLanguage = "en";
                    break;
            }

            if (int.TryParse(printOffsetXTextBox.Text, out var tmp))
            {
                configuration.PrintPictureOffsetX = tmp;
            }
            else
            {
                var msg = GetLocalizedTextFromManager("PrintOffsetXInvalidText");
                MessageBox.Show(this, msg, Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                printOffsetXTextBox.Text = @"0";
                return false;
            }

            NomadConfiguration.SaveConfigurationInstanceToDisk(configuration);
            log.Trace("TouchMode Configuration saved.");
            return true;
        }

        private void OnChromaKeySettingsClick(object sender, EventArgs e)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();

            if (config.ForceUsageOfFfmpeg || !config.IsCapabilityReportGroupValid(CapabilityStatusGroupEnum.OpenCv))
            {
                var chromaDialog = new PreviewDialogChromaKeyFfmpeg
                {
                    Owner = this
                };
                chromaDialog.ShowDialog();
            }
            else
            {
                var chromaDialog = new PreviewDialogChromaKeyOpenCv
                {
                    Owner = this
                };
                chromaDialog.ShowDialog();
            }
        }

        private void OnExportActivityDataCLick(object sender, EventArgs e)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();

            var terminalInfo = db.GetActive();

            var bpmd = new ExportActivityDataDialog(terminalInfo)
            {
                Owner = this
            };
            bpmd.ShowDialog();
        }

        private void OnBasePictureSettingsClick(object sender, EventArgs e)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();

            if (config.ForceUsageOfFfmpeg || !config.IsCapabilityReportGroupValid(CapabilityStatusGroupEnum.OpenCv))
            {
                MessageBox.Show(@"No OpenCv settings detected. This feature requires a working OpenCv Environement to work.", @"iBrand", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var bpmd = new BackgroundPictureManagerDialog
                {
                    Owner = this
                };
                bpmd.ShowDialog();
            }
        }

        private void OnEshotsApiSettingsClick(object sender, EventArgs e)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            
            var bpmd = new EshotApiSetup(db)
            {
                Owner = this
            };
            bpmd.ShowDialog();
        }

        private void OnCustomImageTransformClick(object sender, EventArgs e)
        {
            var iBrandDb = ServicesContainer.GetService<IbrandDatabase>();

            ImageListTransformConfigDialog diag;
            if (_currentConfig != null)
            {
                var qInfo = iBrandDb.GetProject(_currentConfig.ProjectId.ToString()).QuestionnaireInfos.First();
                diag = new ImageListTransformConfigDialog(qInfo, _currentConfig)
                {
                    Owner = this
                };
            }
            else
            {
                var projects = iBrandDb.GetProjects();
                var qInfos = projects.SelectMany(p => p.QuestionnaireInfos).ToList();
                diag = new ImageListTransformConfigDialog(qInfos)
                {
                    Owner = this
                };
            }
            
            diag.ShowDialog();
        }

        private void _btnSynchroMonitor_Click(object sender, EventArgs e)
        {
            new SynchronizationMonitorUi().Show();
        }

        /// <inheritdoc />
        protected override void InitializeControlTags()
        {
            // configuration des étiquette localisées.
            // Attention: vous fiez uniquement au fichier "Ressource\iBrand.resx"
            // Attention: Les tags sont sensibles à la case!
            languageLabel.Tag = "LanguageLabel";
            cameraSettingsButton.Tag = "CameraSettingsButtonLabel";
            closeAppButton.Tag = "CloseApplicationButtonLabel";
            okButton.Tag = "Ok";
            printOffsetXLabel.Tag = "PrintOffsetXLabel";
            btnPrinterSettings.Tag = "UI_PrinterSettings";
            btnBasePictureSettings.Tag = "UI_BasePictureSettingsButton";
        }

        /// <inheritdoc />
        protected override string GetLocalizedTextFromManager(string tag)
        {
            return Resources.iBrand.ResourceManager.GetString(tag, Resources.iBrand.Culture);
        }
    }
}
