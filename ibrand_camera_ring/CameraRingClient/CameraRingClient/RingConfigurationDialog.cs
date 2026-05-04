using System;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrand.CameraRingClient
{
    /// <summary>
    /// Dialogue pour modifier la configuration du logiciel de CameraRing
    /// </summary>
    public partial class RingConfigurationDialog : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly CameraRingNetworkManager _networkManager;
        private readonly CameraRingConfiguration _config;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="manager">Le gestionnaire de réseau.</param>
        /// <param name="config">L'instance de configuration.</param>
        public RingConfigurationDialog(CameraRingNetworkManager manager, CameraRingConfiguration config)
        {
            _networkManager = manager;
            _config = config;
            InitializeComponent();

            nudNbrCameraTolearance.Value = config.MissingCameraToleranceNumber;
        }

        private void nudNbrCameraTolearance_ValueChanged(object sender, EventArgs e)
        {
            if (!(sender is NumericUpDown nud)) return;

            _config.MissingCameraToleranceNumber = (int)nud.Value;
            WriteConfigToFile(_config);
        }

        private static void WriteConfigToFile(CameraRingConfiguration config)
        {
            try
            {
                FileUtil.WriteJsonFile(new FileInfo(CameraRingConfiguration.DefaultFileLocation), config);
            }
            catch (Exception e)
            {
                log.Error(e, "Could not write configuration file to disk.");
            }
        }

        private void modifyCustomAddressButton_Click(object sender, EventArgs e)
        {
            var diag = new CustomServerConfigurationDialog(_networkManager);
            diag.ShowDialog(this);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
