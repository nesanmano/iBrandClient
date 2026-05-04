using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.App.EventVideoCapture
{
    static class Program
    {
        //private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var t = AddServices();
            t.Wait();
            if (!t.Result)
            {
                // TODO Splash screen avec erreur spécifique.
                MessageBox.Show(@"Could not initialize basic services. Stopping application.", @"iBrand Error", MessageBoxButtons.OK);
                return;
            }
            
            var verifiers = new List<ILocalCapabilityVerifier>
            {
                new LocalCapabilityVerifierAssets(),
                new LocalCapabilityVerifierVlc(),
                new LocalCapabilityVerifierFfmpeg()
            };
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm
            {
                CapabilityVerifiers = verifiers
            });

            var t2 = ServicesContainer.StopServices();
            t2.Wait();
            ServicesContainer.Dispose();
        }

        private static async Task<bool> AddServices()
        {
            var config = NomadConfiguration.GetConfigurationInstanceFromDisk(DirectoryUtil.ApplicationNameEnum.VideoEventCapture);
            if (!await ServicesContainer.AddService(config))
            {
                return false;
            }

            LogUtil.InitNLog();

            return true;
        }
    }
}
