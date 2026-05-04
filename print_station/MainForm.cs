using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Services;
using NomadLogic.PrintStation.Ui;
using static NomadLogic.PrintStation.FeatureConfigEnum;
using System.Collections;
using System.Linq;
using System.Text;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Camera.Canon;
using NomadLogic.Common.Util.Networking;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.Gpu.OpenGL;

namespace NomadLogic.PrintStation
{
    public partial class MainForm : Form, ILocalCapabilityTester
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly MainForm _mainForm = new MainForm
        {
            CapabilityVerifiers = new List<ILocalCapabilityVerifier>
            {
                new LocalCapabilityVerifierAssets(),
                new LocalCapabilityVerifierVlc(),
                new LocalCapabilityVerifierFfmpeg(),
                new LocalCapabilityVerifierOpenGl(),
                new LocalCapabilityVerifierOpenCv()
            }

        };

        private readonly List<Task> _longRunningTasks = new List<Task>();
        private CancellationTokenSource _cancelTokenSource;
        private LoginControl _loginControl;
        private GalleryControl _picturesControl;
        private GetPostsTask _postTask;
        private QuestionnaireControl _questionnaireControl;
        private QuestionnaireUtil _questionnaireUtil;
        private TaskFactory _taskFactory;
        private SsdpUtil.CompleteFileReceptionProvider? _listenToPicturesFromIbrand;

        private CreateRespondentFromPicture _createRespondentFromPicture;
        private volatile  bool _waitForExit;

        private MainForm()
        {
            InitializeComponent();

            exit_button.Click += OnExitButtonClick;
            logout_button.Click += OnLogOutButtonClick;
            configuration_button.Click += OnConfigurationButtonClick;
            full_screen_button.Click += OnFullScreenButtonClick;
            clear_button.Click += OnClearButtonClick;

            Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            VerifyCapabilities();
        }

        public Questionnaire Questionnaire => _questionnaireUtil.Questionnaire;

        public PicturePostInfo SelectedPicture
        {
            get => _questionnaireUtil.SelectedPicture;

            set => _questionnaireUtil.SelectedPicture = value;
        }

        public static MainForm GetInstance()
        {
            if(_mainForm!=null)
            {
                return _mainForm;
            }
            else
            {
                throw new Exception();
            }
   
        }

        private void ShowLoginControl()
        {
            if (_loginControl != null) return;
            _loginControl = new LoginControl(main_panel.Width, main_panel.Height) { Location = new Point(0, 0) };
            _loginControl.UserInserted += OnUserInserted;

            main_panel.Controls.Add(_loginControl);
        }

        private void OnNewPosts(object sender, EventArgs e)
        {
            _picturesControl?.RefreshLastScreen();
        }

        public async Task<bool> Initialize()
        {
            main_panel.Location = new Point(0, 0);
            main_panel.Size = new Size(Width, Height);
            _taskFactory = new TaskFactory(
                TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning,
                TaskContinuationOptions.None);

            var db = ServicesContainer.GetService<PrintStationDatabase>();
            
            if (db.Disposed)
            {
                // Si la BD est invalide, on corrige la situation ici tout en affichant un message.
                var result = MsgBox.Show(this, Resources.printstation.MainForm_InvalidDatabase, Resources.printstation.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (result == DialogResult.No)
                {
                    MsgBox.Show(this, Resources.printstation.MainForm_InvalidDatabase_ManualRemove, Resources.printstation.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }

                var filePath = db.DataseFileName;

                try
                {
                    await ServicesContainer.RemoveService(db);

                    // Étape 1: on renomme le fichier
                    var newName = DateTime.Now.ToShortDateString() + "_" + StringUtil.GetFileName(filePath);
                    var newPathName = StringUtil.GetDirName(filePath) + Path.DirectorySeparatorChar + newName;

                    File.Move(filePath, newPathName);
                
                    if (File.Exists(filePath))
                    {
                        MsgBox.Show(this, Resources.printstation.MainForm_InvalidDatabase_ErrorRecovery, Resources.printstation.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    db = new PrintStationDatabase(filePath);
                    await ServicesContainer.AddService(db);
                }
                catch (Exception err)
                {
                    log.Error(err);
                    return false;
                }
            }

            if (db.HasActiveUser())
            {
                var user = db.GetActiveUser();
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                var runningConfig = RunningFeatureConfig.GetInstance();
                runningConfig.Initialize(user.FeatureId);

                ShowPictures(user);

                StartLongRunningTasks(db, runningConfig, configuration, user);
            }
            else
            {
                ShowLoginControl();
                tool_strip.Visible = true;
            }

            return true;
        }

        /// <summary>
        ///     Démarre chacune des tâches background qui doivent être initialisé et démarrées lors de l'exucution d'un
        ///     PrintStation.
        ///     Démarre également le questionnaire, s'il est nécessaire.
        ///     Ces tâches sont mise dans une structure pour facilement les monitorer, et chacune se voie donner le même jeton de
        ///     cancellation.
        ///     Pour stoppper ces tâches de façon gracieuse, il faut utiliser la méthode "StopLongRunningTasks()".
        /// </summary>
        /// <param name="database">La base de donnée PrintStation</param>
        /// <param name="runningConfig">Les paramètres actifs du projet.</param>
        /// <param name="config">La abstractConfiguration globale du PrintStation.</param>
        /// <param name="user">L'usager PrintStation</param>
        private void StartLongRunningTasks(PrintStationDatabase database, RunningFeatureConfig runningConfig,
            NomadConfiguration config, PrintStationUser user)
        {
            runningConfig.SetConfiguration(IsLookForMostRecentPostOnServer, config.Printstation.StartLookingFromLastPost);

            // Initialisation du questionnaire.
            if (runningConfig.GetConfiguration<bool>(HasQuestionnaire) && user != null)
            {
                if (_questionnaireControl != null)
                {
                    main_panel.Controls.Remove(_questionnaireControl);
                    _questionnaireControl.Dispose();
                    _questionnaireControl = null;
                }

                if (_questionnaireUtil != null)
                {
                    _questionnaireUtil.QuestionnaireController.OnCloseControl -= OnQuestionnaireControlClose;
                    _questionnaireUtil.CloseQuestionnaire();
                    _questionnaireUtil = null;
                }

                _questionnaireControl = new QuestionnaireControl(main_panel.Width, main_panel.Height)
                {
                    Location = new Point(0, 0)
                };

                if (_questionnaireUtil == null)
                    _questionnaireUtil = new QuestionnaireUtil(_questionnaireControl.ScreenPanel, user);
                _questionnaireUtil.QuestionnaireController.OnCloseControl += OnQuestionnaireControlClose;


                _questionnaireUtil.AddScreens();

                main_panel.Controls.Add(_questionnaireControl);
            }

            // Initialisation des tâches longues.
            _cancelTokenSource = new CancellationTokenSource();

            if (runningConfig.GetConfiguration<bool>(FeatureConfigEnum.CreateRespondentFromPicture) && user != null)
            {
                if (_questionnaireUtil == null)
                    _questionnaireUtil = new QuestionnaireUtil(null, user);

                _createRespondentFromPicture =  new CreateRespondentFromPicture(_questionnaireUtil);
            }

            _postTask = new GetPostsTask(database, config);
            _postTask.NewPosts += OnNewPosts;
            _longRunningTasks.Add(_taskFactory.StartNew(() => { _postTask.DoWork(_cancelTokenSource.Token, this); },
                _cancelTokenSource.Token));

            _longRunningTasks.Add(
                _taskFactory.StartNew(
                    () => { new SendPrintCountTask(database, config).DoWork(_cancelTokenSource.Token); },
                    _cancelTokenSource.Token));

            if (config.Printstation.ListenForPicturesFromIBrand)
            {
                _listenToPicturesFromIbrand = SsdpUtil.PrepareAndLaunchPrintstationImageListener(new DirectoryInfo(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.listen_from_ibrand)), null);
            }
        }

        private void OnUserInserted(PrintStationUser user)
        {
            if (_loginControl != null)
            {
                main_panel.Controls.Remove(_loginControl);
                _loginControl.Dispose();
                _loginControl = null;
            }

            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            var database = ServicesContainer.GetService<PrintStationDatabase>();
            var runningConfig = RunningFeatureConfig.GetInstance();
            runningConfig.Initialize(user.FeatureId);

            ShowPictures(user);

            StartLongRunningTasks(database, runningConfig, configuration, user);
        }

        private void OnExitButtonClick(object sender, EventArgs e)
        {
            if (_waitForExit) return;
            _waitForExit = true;
            StopLongRunningTasks(true);
        }

        /// <summary>
        ///     Désactive toutes les tâches longues, et désactive aussi l'utilitaire de questionnaire en même temps si actif.
        ///     <br />Il est possible de réactiver ces tâches avec la méthode "StartLongRunningTasks()".
        /// </summary>
        private void StopLongRunningTasks(bool totalExit = false)
        {
            if (_questionnaireUtil != null)
            {
                _questionnaireUtil.QuestionnaireController.OnCloseControl -= OnQuestionnaireControlClose;
                _questionnaireUtil.CloseQuestionnaire();
                _questionnaireUtil.QuestionnaireController.QuestionnaireClosed +=
                    (s, e) => { StopLongRunningTasksDo(totalExit); };
            }
            else
            {
                StopLongRunningTasksDo(totalExit);
            }
        }

        private void StopLongRunningTasksDo(bool totalExit = false) {
            _createRespondentFromPicture?.Dispose();

            if (_postTask != null)
            {
                _postTask.NewPosts -= OnNewPosts;
                _postTask = null;
            }

            _listenToPicturesFromIbrand?.StopAllService();

            log.Trace("Sending cancellation order for all long-running tasks.");
            _cancelTokenSource?.Cancel();

            try
            {
                Task.WaitAll(_longRunningTasks.ToArray(), 3000);
            }
            catch (AggregateException e)
            {
                log.Error(e, "Error during stopping long running tasks.");
            }
            finally
            {
                _longRunningTasks.Clear();
                _cancelTokenSource = null;
            }
            log.Trace("Long-Running tasks all stopped.");

            _questionnaireUtil = null;
            if (totalExit) { this.SafeCall(Close); }
        }

        private void OnFullScreenButtonClick(object sender, EventArgs e)
        {
            if (_picturesControl != null)
                tool_strip.Visible = false;
        }

        private void OnClearButtonClick(object sender, EventArgs e)
        {
            var database = ServicesContainer.GetService<PrintStationDatabase>();

            var user = database.GetActiveUser();
            if (user == null) return;
            database.ClearAll(user);

            _picturesControl?.RefreshLastScreen();
        }


        private void OnShowToolStrip(object sender, EventArgs e)
        {
            tool_strip.Visible = true;
        }

        private void OnConfigurationButtonClick(object sender, EventArgs e)
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            var startLookingFromLastPost = configuration.Printstation.StartLookingFromLastPost;

            var ssdpPort = -1;

            if(_listenToPicturesFromIbrand?.TcpFileListenerService != null)
            {
                ssdpPort = _listenToPicturesFromIbrand.Value.TcpFileListenerService.Port;
            }

            IEnumerator cameras = null;
            if (_createRespondentFromPicture != null)
            {
                cameras = CameraControllerCanon.GetInstance().EosCameras;
            }

            new ConfigurationDialog(ssdpPort, cameras).ShowDialog();

            if (!startLookingFromLastPost && configuration.Printstation.StartLookingFromLastPost)
                RunningFeatureConfig.GetInstance().SetConfiguration(IsLookForMostRecentPostOnServer, configuration.Printstation.StartLookingFromLastPost);
        }

        private void OnLogOutButtonClick(object sender, EventArgs e)
        {
            var sw = new Stopwatch();
            if (log.IsTraceEnabled)
            {
                sw.Start();
                log.Trace("Logout started.");
            }

            if (_picturesControl != null)
            {
                var database = ServicesContainer.GetService<PrintStationDatabase>();

                database.SetUserStatus(_picturesControl.User, false);

                Controls.Remove(_picturesControl);

                _picturesControl.DisposeUiElements();
                _picturesControl.Dispose();
                _picturesControl = null;

                if (log.IsTraceEnabled)
                {
                    sw.Stop();
                    log.Trace("Removed pictures controls. Elapsed time: " + sw.ElapsedMilliseconds / 1000.0f + " seconds.");
                    sw.Restart();
                }
            }

            if (_questionnaireControl != null)
            {
                main_panel.Controls.Remove(_questionnaireControl);
                _questionnaireControl.Dispose();
                _questionnaireControl = null;

                if (log.IsTraceEnabled)
                {
                    sw.Stop();
                    log.Trace("Removed questionnaire controls. Elapsed time: " + sw.ElapsedMilliseconds / 1000.0f + " seconds.");
                    sw.Restart();
                }
            }

            StopLongRunningTasks();

            if (log.IsTraceEnabled)
            {
                sw.Stop();
                log.Trace("Stopped long running tasks controls. Elapsed time: " + sw.ElapsedMilliseconds / 1000.0f + " seconds.");
                sw.Restart();
            }

            ShowLoginControl();

            if (log.IsTraceEnabled)
            {
                sw.Stop();
                log.Trace("Login controls shown. Elapsed time: " + sw.ElapsedMilliseconds / 1000.0f + " seconds.");
                sw.Restart();
            }
        }

        private void ShowPictures(PrintStationUser userSession)
        {
            if (_picturesControl != null)
            {
                main_panel.Controls.Remove(_picturesControl);
                _picturesControl._showToolStrip -= OnShowToolStrip;
                _picturesControl.Dispose();
                _picturesControl = null;
            }

            _picturesControl = new GalleryControl(userSession, Width, Height) { Location = new Point(0, 0) };
            _picturesControl._showToolStrip += OnShowToolStrip;
            _picturesControl._onShowPicturesControl += OnShowSearchPicturesControl;
            _picturesControl._onRemovePicturesControl += OnRemoveSearchPicturesControl;
            _picturesControl.ShowPictures();


            main_panel.Controls.Add(_picturesControl);
        }

        private void OnQuestionnaireControlClose(object sender, EventArgs e)
        {
            if (_questionnaireUtil == null) return;

            _questionnaireControl.SendToBack();
            _questionnaireControl.Invalidate();
            //_questionnaireControl.Enabled = false;
            _picturesControl.Enabled = true;
            main_panel.Invalidate();
            _picturesControl.OnPictureControlClosed(this, EventArgs.Empty);
        }

        public void ShowQuestionnaire(PicturePostInfo picture)
        {
            if (_questionnaireUtil == null) return;

            _questionnaireUtil.ShowQuestionnaire(picture);
            _questionnaireControl.BringToFront();
            //_questionnaireControl.Enabled = true;
            _questionnaireControl.Invalidate();
            _picturesControl.Enabled = false;
            main_panel.Invalidate();
        }

        private void OnShowSearchPicturesControl(SearchPicturesControl picturesControl)
        {
            main_panel.Controls.Add(picturesControl);
            picturesControl.BringToFront();
            main_panel.Refresh();
        }

        private void OnRemoveSearchPicturesControl(SearchPicturesControl searchControl)
        {
            main_panel.Controls.Remove(searchControl);
            searchControl.Dispose();

            _picturesControl?.ShowLastScreen();
            main_panel.Refresh();
        }
        
        public IList<ILocalCapabilityVerifier> CapabilityVerifiers { get; set; }
        
        public void VerifyCapabilities()
        {
            if (CapabilityVerifiers == null)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(this, @"No capabilities are checked. This is a fatal code error. Please check class initialisation.", @"Missing capabilities.");
                    Application.Exit();
                }));
                return;
            }
            
            var config = ServicesContainer.GetService<NomadConfiguration>();

            // Assigne les vérificateurs de forme avec la référence de cet object
            foreach (var cv in CapabilityVerifiers)
            {
                if (!(cv is ILocalCapabilityVerifierForm cvf)) continue;
                cvf.ParentForm = this;
            }
            
            LocalCapabilitiesManager.AnalyseCapabilities(CapabilityVerifiers, config, capResult =>
            {
                if (capResult.Groups.All(g => g.Status == StatusEnum.Valid)) return;
                var sb = new StringBuilder();
                sb.AppendLine(@"Some capabilities were detected as invalid or missing, some critical functions may not work as intended. Do you wish to stop the execution of the application?");
                foreach (var gr in capResult.Groups)
                {
                    foreach (var message in gr.ErrorMessages)
                    {
                        sb.AppendLine(message);
                    }
                }

                Invoke(new Action(() =>
                {
                    var diagResult = MessageBox.Show(this, sb.ToString(), @"Missing capabilities", MessageBoxButtons.YesNo);
                    if (diagResult == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                }));
            });
        }
    }
}