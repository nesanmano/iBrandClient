using System;
using System.Windows.Forms;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Net;

namespace NomadLogic.iBrand.CameraRingClient
{
    /// <summary>
    /// Forme dialogue pour configurer (on enlever) une adresse spécifique comme server pour le client de Camera Ring.
    /// </summary>
    public partial class CustomServerConfigurationDialog : Form
    {
        private readonly CameraRingNetworkManager _networkManager;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="manager">Le gestionnaire de réseau à reconfigurer.</param>
        public CustomServerConfigurationDialog(CameraRingNetworkManager manager)
        {
            InitializeComponent();

            _networkManager = manager;
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = Resources.Text.CustomServerConfigurationDialog_Title;
            lblEnterAddress.Text = Resources.Text.CustomServerConfigurationDialog_Label_EnterAdress;
            cmdCancel.Text = Resources.Text.CustomServerConfigurationDialog_Button_Cancel;
            cmdClear.Text = Resources.Text.CustomServerConfigurationDialog_Button_Clear;
            cmdValidate.Text = Resources.Text.CustomServerConfigurationDialog_Button_Validate;

            var custom = SsdpUtil.GetLocalCameraRingServerAddress();
            if (custom != null)
            {
                txtAddress.Text = custom.Value.IpAddress + @":" + custom.Value.Port;
            }
        }

        private void cmdValidate_Click(object sender, EventArgs e)
        {
            if (!StringUtil.IsValidIPAddressWithPort(txtAddress.Text, true))
            {
                MessageBox.Show(this, Resources.Text.CustomServerConfigurationDialog_InvalidIpAddress, null, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // On enregistre l'addresse, et on ré-initialise le gestionnaire du réseau.
            var tcpAddress = SsdpUtil.SetLocalCameraRingServerAddress(txtAddress.Text);

            _networkManager.SetCustomAddress(tcpAddress);

            DialogResult = DialogResult.OK;
            Dispose();
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            var tcpAddress = SsdpUtil.SetLocalCameraRingServerAddress(null);

            _networkManager.SetCustomAddress(tcpAddress);

            DialogResult = DialogResult.OK;
            Dispose();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Dispose();
        }
    }
}
