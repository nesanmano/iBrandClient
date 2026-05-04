using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation
{
    public class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [MTAThread]
        private static void Main()
        {
            if (SingleApplication.IsAlreadyRunning())
            {
                SingleApplication.SwitchToCurrentInstance();
                return;
            }
            Application.SetCompatibleTextRenderingDefault(false);

            new Program().Run();
        }

        private void Run()
        {
            try
            {
                var t = AddServices();
                t.Wait();
                if (!t.Result)
                {
                    return;
                }
                
                var mainForm = MainForm.GetInstance();
                var mainFormUtil = MainFormUtil.GetInstance();
                mainFormUtil.MainForm = mainForm;
                //main_form_util.Restore();
                mainFormUtil.Maximize();
                var initTask = mainForm.Initialize();
                initTask.Wait();
                if (initTask.Result)
                {
                    Application.Run(mainForm);
                }
                mainForm.Dispose();

                LogUtil.StopNLog();

                var t2 = ServicesContainer.StopServices();
                t2.Wait();
                ServicesContainer.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task<bool> AddServices()
        {
            try
            {
                var conf = NomadConfiguration.GetConfigurationInstanceFromDisk(DirectoryUtil.ApplicationNameEnum.Printstation);
                if (!await ServicesContainer.AddService(conf))
                {
                    return false;
                }

                LogUtil.InitNLog();

                var dbPrint = new PrintStationDatabase(DirectoryUtil.GetLocalApplicationDir() + conf.Printstation.DbPrintStationFileName);
                if (!await ServicesContainer.AddService(dbPrint))
                {
                    log.Fatal("Could not add print database service.");
                    return false;
                }

                var dbIbrand = new IbrandDatabase(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.iBrand) + conf.Ibrand.DbIbrandFileName);
                if (!await ServicesContainer.AddService(dbIbrand))
                {
                    log.Fatal("Could not add iBrand database service.");
                    return false;
                }

                var syncServ = new SynchronizationService(dbIbrand);

                if (!await ServicesContainer.AddService(syncServ))
                {
                    log.Fatal("Could not add synchronization service.");
                    return false;
                }

                if (conf.LiveSynchro)
                {
                    if (!await syncServ.StartLongRunning())
                    {
                        log.Fatal("Could start automatic sync service as requested.");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message);
                log.Fatal(ex.StackTrace);
                throw;
            }
        }
    }
}