using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Synchro;
using NomadLogic.Common.Ui.Configuration.Eshots;
using NomadLogic.Common.Util.Networking;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.PrintStation.Ui
{
    public partial class ConfigurationDialog : Form
    {
        private readonly int _sspdPort;
        private readonly IEnumerator _cameras;

        public ConfigurationDialog(int ssdpPort, IEnumerator cameras)
        {
            InitializeComponent();

            _sspdPort = ssdpPort;
            _cameras = cameras;

            ok_button.Click += OnOk;
            btnPrinterSettings.Click += OnPrinterSettingsClick;
            btnEshotsApi.Click += BtnEshotsApiOnClick;

            Init();

            Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - Width)/2,
                (Screen.PrimaryScreen.WorkingArea.Height - Height)/2);
        }

        private void BtnEshotsApiOnClick(object sender, EventArgs e)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var diag = new EshotApiSetup(db);
            diag.ShowDialog();
        }

        private static void OnPrinterSettingsClick(object sender, EventArgs e)
        {
            var diag = new PrinterSettingsDialog();
            diag.ShowDialog();
        }

        private void OnOk(object sender, EventArgs e)
        {
            SaveConfiguration();
            Close();
        }

        private void SaveConfiguration()
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            //configuration.ServerIbrandAdr = serverAdrTextBox.Text;
            //configuration.ServerIbrandPort = serverPortTextBox.Text;
            
            configuration.Printstation.StartLookingFromLastPost = lastPostCheckBox.Checked;

            configuration.Printstation.ListenForPicturesFromIBrand = listenForFilesCheckBox.Checked;

            configuration.Printstation.DontLookForPost = dontLookForPost.Checked;

            configuration.Printstation.AutomaticPrint = automaticPrint.Checked;

            int.TryParse(printer_x_offset_textbox.Text, out var tmp);
            configuration.PrintPictureOffsetX = tmp;

            NomadConfiguration.SaveConfigurationInstanceToDisk(configuration);
        }

        private void Init()
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            
            //serverAdrTextBox.Text = configuration.ServerIbrandAdr;
            //serverPortTextBox.Text = configuration.ServerIbrandPort;
            
            if (_cameras != null)
            {
                while (_cameras.MoveNext())
                {
                    if (_cameras.Current != null)
                        camerasComboBox.Items.Add(_cameras.Current);
                }
            }

            if (camerasComboBox.Items.Count == 0)
                camerasComboBox.Items.Add("No Camera Connected");

            camerasComboBox.SelectedIndex = 0;
            
            if (configuration.Printstation.StartLookingFromLastPost)
                lastPostCheckBox.Checked = true;

            if (configuration.Printstation.ListenForPicturesFromIBrand)
                listenForFilesCheckBox.Checked = true;

            if (configuration.Printstation.DontLookForPost)
                dontLookForPost.Checked = true;

            if (configuration.Printstation.AutomaticPrint)
                automaticPrint.Checked = true;

            ipAddressLabel.Text = @"Local IP Address: " + NetUtil.GetLocalIpAddress();

            if (_sspdPort != -1)
            {
                ssdpPortLabel.Text = @"Ssdp Port : " + _sspdPort;
            }

            printer_x_offset_textbox.Text = configuration.PrintPictureOffsetX.ToString();

            var g = CreateGraphics();
            Size = new Size((int) (Size.Width*g.DpiX/96), (int) (Size.Height*g.DpiY/96));
        }

        private void BtnSynchroMonitor_Click(object sender, EventArgs e)
        {
            new SynchronizationMonitorUi().Show();
        }
    }
}