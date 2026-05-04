using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Modules.ManualProjectInstaller;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.Gpu.OpenGL;

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local

namespace NomadLogic.iBrandDesktop
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread] //Obligatoire quand on utilise la librairie Canon FSF
        //[STAThread]
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
                    MessageBox.Show(@"Could not initialize basic services. Stopping application.", @"Kapture client Error", MessageBoxButtons.OK);
                    return;
                }

                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                var verifiers = new List<ILocalCapabilityVerifier>
                {
                    new LocalCapabilityVerifierAssets(),
                    new LocalCapabilityVerifierVlc(),
                    new LocalCapabilityVerifierFfmpeg(),
                    new LocalCapabilityVerifierOpenGl(),
                    new LocalCapabilityVerifierOpenCv()
                };

                Form startForm = null;
                var db = ServicesContainer.GetService<IbrandDatabase>();

                // Manual project installer
                var mpi = new ManualProjectInstallManager(db, configuration);
                mpi.Init();
                var pakFound = mpi.Search();
                if (pakFound > 1)
                {
                    log.Info("Processing {0} local installation packages", pakFound);
                    var processResult = mpi.Process(log);
                }
                
                var actTerminal = db.GetActive();
                var startFullScreen = db.GetStartFullScreen();

                if (actTerminal != null && startFullScreen)
                {
                    var project = db.GetProject(actTerminal.ProjectId);
                    var questInfo = project?.QuestionnaireInfos?.FirstOrDefault(qi => qi.Id == actTerminal.QuestionnaireId);
                    
                    // Si tout charge correctement.
                    if (project != null && questInfo != null)
                    {
                        var mainForm = new MainForm(actTerminal)
                        {
                            CapabilityVerifiers = verifiers
                        };
                        var mainFormUtil = MainFormUtil.GetInstance();
                        mainFormUtil.MainForm = mainForm;
                        //main_form_util.Restore();
                        mainFormUtil.Maximize();
                        mainForm.ResizeElementsForMaximize();

                        startForm = mainForm;
                    }
                    else
                    {
                        log.Fatal("Bad metadata. Cannot load projet/questionnaire/terminal.");
                    }
                }
                
                if (startForm == null)
                {
                    startForm = new KioskForm
                    {
                        CapabilityVerifiers = verifiers
                    };
                }

                Application.Run(startForm);

                var t2 = ServicesContainer.StopServices();
                t2.Wait();
                ServicesContainer.Dispose();
            }
            catch (System.Exception ex)
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
            var config = NomadConfiguration.GetConfigurationInstanceFromDisk(DirectoryUtil.ApplicationNameEnum.iBrand);
            if (!await ServicesContainer.AddService(config))
            {
                return false;
            }

            Resources.iBrand.Culture = config.PreferedLanguage.Equals("en") ? new CultureInfo("en") : new CultureInfo("fr-CA");

            LogUtil.InitNLog();

            var db = new IbrandDatabase(DirectoryUtil.GetLocalApplicationDir() + config.Ibrand.DbIbrandFileName);
            if (!await ServicesContainer.AddService(db))
            {
                log.Fatal("Could not add client database service.");
                return false;
            }

            var activityDbFilePath = DirectoryUtil.GetLocalApplicationDir() + config.DbActivityFileName;
            var activityLoggerServ = new ActivityLoggerService(activityDbFilePath);
            if (!await ServicesContainer.AddService(activityLoggerServ))
            {
                log.Fatal("Could not add activity logger service.");
                return false;
            }
            activityLoggerServ.LogActivity(-1, ActivityLoggerActionType.Debug, "Start of application.");

            var syncServ = new SynchronizationService(db);
            if (!await ServicesContainer.AddService(syncServ, config.LiveSynchro))
            {
                log.Fatal("Could not add synchronisation service.");
                return false;
            }

            if (config.LiveSynchro)
            {
                syncServ.SignalNewRespondent();
            }

            return true;
        }
    }
}
