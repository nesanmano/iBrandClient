using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.DatabaseExporter
{
    internal static class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (SingleApplication.IsAlreadyRunning())
            {
                SingleApplication.SwitchToCurrentInstance();
                return;
            }
            Application.SetCompatibleTextRenderingDefault(false);

            Run();
        }

        private static void Run()
        {
            try
            {
                var t = AddServices();
                t.Wait();
                if (!t.Result)
                {
                    // TODO Splash screen avec erreur spécifique.
                    MessageBox.Show(@"Could not initialize basic services. Stopping application.", @"iBrand Error", MessageBoxButtons.OK);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ExtractorUi());

                var t2 = ServicesContainer.StopServices();
                t2.Wait();
                ServicesContainer.Dispose();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            finally
            {
                LogUtil.StopNLog();
            }
        }

        private static async Task<bool> AddServices()
        {
            var config = NomadConfiguration.GetConfigurationInstanceFromDisk(DirectoryUtil.ApplicationNameEnum.DatabaseExtractor);
            if (!await ServicesContainer.AddService(config))
            {
                return false;
            }

            LogUtil.InitNLog();

            var db = new IbrandDatabase(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.iBrand) + config.Ibrand.DbIbrandFileName);
            if (!await ServicesContainer.AddService(db))
            {
                log.Fatal("Could not add iBrand database service.");
                return false;
            }

            return true;
        }
    }
}
