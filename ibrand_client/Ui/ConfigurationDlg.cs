using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using NomadLogic.Common.Resources;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Configuration.Eshots;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Ui.Localization;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Ui
{
    /// <summary>
    /// Classe de configuration pour l'application iBrand.
    /// </summary>
    public partial class ConfigurationDlg : LocalizedForm
    {
        /// <summary>
        /// Quand on change la langue via ce dialogue.
        /// </summary>
        public event EventHandler LanguageChanged;

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            //base.OnLoad(e);

            okButton.Click += OnOk;
            cancelButton.Click += OnCancel;
            btnPrinterSettings.Click += OnPrinterSettingsClick;
            btnEshotsSettings.Click += OnEshotsSettingsClick;

            bkDirButton.Click += (sender, args) => { ShowDirectoryPicker(bkDeviceTextBox); };
            db_button.Click += (sender, args) => { ShowDirectoryPicker(dbTextBox); };
            pictureDirButton.Click += (sender, args) => { ShowDirectoryPicker(pictureDirTextBox); };
            videoDirButton.Click += (sender, args) => { ShowDirectoryPicker(videoDirTextBox); };
            videoSelectionButton.Click += (sender, args) => { ShowDirectoryPicker(bkDeviceTextBox); };

            // configuration des valeurs
            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            //serverAdrTextBox.Text = configuration.ServerIbrandAdr;
            //serverPortTextBox.Text = configuration.ServerIbrandPort;
          
            //streamPortTextBox.Text = configuration.StreamPort;
            //securePortTextBox.Text = configuration.SecureStreamPort;
            deviceTextBox.Text = configuration.DeviceName;
            dbTextBox.Text = configuration.Ibrand.DbIbrandFileName;
            bkDeviceTextBox.Text = configuration.BackupDrive;
            pictureDirTextBox.Text = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures);
            videoDirTextBox.Text = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);
            videoSelectiontextBox.Text = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);
            ppcTextBox.Text = configuration.PcName;
            PrintOffsetXTextBox.Text = configuration.PrintPictureOffsetX.ToString();

            //if (configuration.SecureSynchro)
            //    secureCheckBox.Checked = true;
            if (configuration.DeletePictureAfterSynchro)
                deletePictureCheckBox.Checked = true;
            if (configuration.ShowKeyboard)
                showKeyboardCheckBox.Checked = true;

            languageDropDown.SelectedIndex = configuration.PreferedLanguage.Equals("en") ? 0 : 1;
            languageDropDown.SelectedIndexChanged += languageDropDown_SelectedIndexChanged;

            var g = CreateGraphics();
            Size = new Size((int)(Size.Width * g.DpiX / 96), (int)(Size.Height * g.DpiY / 96));

            OnChangeLanguage(this, null);
        }

        private void ShowDirectoryPicker(Control ctrl)
        {
            var newThread = new Thread(() =>
            {
                var diag = new FolderBrowserDialog();
                if (diag.ShowDialog() != DialogResult.OK) return;

                this.SafeCall(() =>
                {
                    ctrl.Text = diag.SelectedPath;
                });

            });
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }

        private void OnPrinterSettingsClick(object sender, EventArgs e)
        {
            var diag = new PrinterSettingsDialog();
            diag.ShowDialog();
        }

        private void OnEshotsSettingsClick(object sender, EventArgs e)
        {
            this.SafeCall(() =>
            {
                var database = ServicesContainer.GetService<IbrandDatabase>();
                var diag = new EshotApiSetup(database);
                diag.ShowDialog();
            });
        }

        private void languageDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {

            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            if (languageDropDown.SelectedIndex == 0) // english
            {
                CultureInfo.CurrentCulture = EnCulture;
                CultureInfo.CurrentUICulture = EnCulture;
                Resources.iBrand.Culture = EnCulture;
                common.Culture = EnCulture; // On propage le changement de langue à Common.
                configuration.PreferedLanguage = "en";
            }
            else // french
            {
                CultureInfo.CurrentCulture = FrCulture;
                CultureInfo.CurrentUICulture = FrCulture;
                Resources.iBrand.Culture = FrCulture;
                common.Culture = FrCulture; // On propage le changement de langue à Common.
                configuration.PreferedLanguage = "fr";
            }
            LanguageChanged?.Invoke(this, e);

            //OnChangeLanguage(this, e);

            /* TODO Ne fonctionne pas, même si la plupart de la documentation sur le net indique que ça devrait. À vérifier plus tard.
            Console.WriteLine(@"Common (current): " + Common.Resources.common.ResourceManager.GetString("UI_Cancel", CultureInfo.CurrentUICulture));
            Console.WriteLine(@"Common (en)     : " + Common.Resources.common.ResourceManager.GetString("UI_Cancel", EnCulture));
            Console.WriteLine(@"Common (fr-CA)  : " + Common.Resources.common.ResourceManager.GetString("UI_Cancel", FrCulture));
            Console.WriteLine(@"Common (ref)    : " + Common.Resources.common.UI_Cancel);
            Console.WriteLine(@"iBrand (current): " + Resources.iBrand.ResourceManager.GetString("Cancel", CultureInfo.CurrentUICulture));
            Console.WriteLine(@"iBrand (en)     : " + Resources.iBrand.ResourceManager.GetString("Cancel", EnCulture));
            Console.WriteLine(@"iBrand (fr-CA)  : " + Resources.iBrand.ResourceManager.GetString("Cancel", FrCulture));
            Console.WriteLine(@"iBrand (ref)    : " + Resources.iBrand.Cancel);
            */
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (!ValidateEntry()) return;
            SaveConfiguration();
            Close();
        }

        private void SaveConfiguration()
        {
            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();

            //nomadConfig.ServerIbrandAdr = serverAdrTextBox.Text;
            //nomadConfig.ServerIbrandPort = serverPortTextBox.Text;
            //nomadConfig.StreamPort = streamPortTextBox.Text;
            //nomadConfig.SecureStreamPort = securePortTextBox.Text;
            nomadConfig.Ibrand.DbIbrandFileName = dbTextBox.Text;
            nomadConfig.BackupDrive = bkDeviceTextBox.Text;
            //nomadConfig.SecureSynchro = secureCheckBox.Checked;
            nomadConfig.DeletePictureAfterSynchro = deletePictureCheckBox.Checked;
            nomadConfig.ShowKeyboard = showKeyboardCheckBox.Checked;
            nomadConfig.Ibrand.ShowPreviewOnTv = showPreviewOnTVCheckBox.Checked;

            int.TryParse(PrintOffsetXTextBox.Text, out int tmp);
            nomadConfig.PrintPictureOffsetX = tmp;

            NomadConfiguration.SaveConfigurationInstanceToDisk(nomadConfig);
        }

        private bool ValidateEntry()
        {
            if (serverAdrTextBox.Text.Length == 0 || !StringUtil.IsValidIPAddress(serverAdrTextBox.Text))
            {
                MsgBox.Show(this, Resources.iBrand.NotValidIP, Resources.iBrand.AppName);
                return false;
            }

            if (serverPortTextBox.Text.Length == 0 || !StringUtil.IsValidServerPort(serverPortTextBox.Text))
            {
                MsgBox.Show(this, Resources.iBrand.NotValidServerPort, Resources.iBrand.AppName);
                return false;
            }

            if (streamPortTextBox.Text.Length == 0 || !StringUtil.IsValidStreamPort(streamPortTextBox.Text))
            {
                MsgBox.Show(this, Resources.iBrand.NotValidStreamPort, Resources.iBrand.AppName);
                return false;
            }

            if (securePortTextBox.Text.Length == 0 || !StringUtil.IsValidStreamPort(securePortTextBox.Text))
            {
                MsgBox.Show(this, Resources.iBrand.NotValidSecureStreamPort, Resources.iBrand.AppName);
                return false;
            }

            if (deviceTextBox.Text.Length == 0)
            {
                MsgBox.Show(this, Resources.iBrand.NotValidDeviceName, Resources.iBrand.AppName);
                return false;
            }

            if (ppcTextBox.Text.Length == 0)
            {
                MsgBox.Show(this, "You must enter PPC Name.", Resources.iBrand.AppName);
                return false;
            }

            if (PrintOffsetXTextBox.Text.Length != 0 && int.TryParse(PrintOffsetXTextBox.Text, out _)) return true;
            MsgBox.Show(this, "A valid Print offset is an integer number.", Resources.iBrand.AppName);
            return false;
        }

        private void OnCancel(object sender, EventArgs e)
        {
            Close();
        }

        private void btnAddCustomPrintStation_Click(object sender, EventArgs e)
        {
            new CustomPrintstationAddressDialog().Show();
        }

        /// <inheritdoc />
        protected override void InitializeControlTags()
        {
            // condif des étiquette localisées.
            // Attention: vous fiez uniquement au fichier "Ressource\iBrand.resx"
            // Attention: Les tags sont sensibles à la case!
            server_adr_label.Tag = "ServerAdr";
            server_port_label.Tag = "ServerPort";
            stream_port_label.Tag = "StreamPort";
            db_file_name_label.Tag = "DatabaseSrc";
            picture_label.Tag = "picturesSrc";
            video_label.Tag = "VideosSrc";
            bk_folder_label.Tag = "BkDirSrc";
            video_selection_label.Tag = "VideoSelectionDir";
            device_label.Tag = "DeviceName";
            secure_port_label.Tag = "SecureStreamPort";
            secureCheckBox.Tag = "UseEncryption";
            language_label.Tag = "AppLanguage";
            deletePictureCheckBox.Tag = "DeletePicturesAfterSynchroLabel";
            showKeyboardCheckBox.Tag = "ShowKeyboardLabel";
            PrintOffsetX_label.Tag = "PrintOffsetXLabel";
            ppc_label.Tag = "PpcLabel";
            showPreviewOnTVCheckBox.Tag = "ShowPreviewOnTvLabel";
            okButton.Tag = "Ok";
            cancelButton.Tag = "Cancel";
            btnAddCustomPrintStation.Tag = "ui_setPrintstationsAddresses";
            btnPrinterSettings.Tag = "UI_PrinterSettings";
        }

        /// <inheritdoc />
        protected override string GetLocalizedTextFromManager(string tag)
        {
            return Resources.iBrand.ResourceManager.GetString(tag, Resources.iBrand.Culture);
        }

    }
}