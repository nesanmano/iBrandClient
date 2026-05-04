using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.App.EventVideoCapture.Capture;
using NomadLogic.App.EventVideoCapture.config;
using NomadLogic.App.EventVideoCapture.Reader;
using NomadLogic.App.EventVideoCapture.Video;
using NomadLogic.Common.Phidgets;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking;
using NomadLogic.Common.Util.ThreadPool;
using NomadLogic.Common.Util.Trigger;
using NomadLogic.Common.Util.Win32;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Video;
using static NomadLogic.App.EventVideoCapture.VideoCaptureAppGlobalStatus;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.App.EventVideoCapture
{
    public enum VideoCaptureAppGlobalStatus
    {
        Waiting,
        Capturing,
        CannotRun,
        Stopped,
        Initializing
    }

    public partial class MainForm : Form, IProgress<ExternalStateReport>, IEventLogger, IProgress<VideoCaptureReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public const string METADATA_FILE_NAME = "metadata.json";

        private NomadConfiguration _nomadConfig;
        private string _baseDir;
        private VideoCaptureConfig _config;
        private IVideoCaptureManager<VideoCaptureFfmpeg> _capture;
        private ILightsManager _ligths;
        private CustomWorkerPool<VideoTreatmentParameter, VideoTreatmentResult> _workPool;
        private Type _workerType;
        private ReaderManagerBase _reader;
        private TriggerManagerBase _trigger;
        private ServiceStateWatcher _watcherService;
        private IReadOnlyList<IWatcherStatus> _lastReport;
        private bool _serviceStateGood;
        private VideoCaptureAppGlobalStatus _globalStatus;
        private bool _poolStopped;
        private bool _triggerStopped;
        private bool _captureStopped;
        private bool _readerStopped;
        private Task _captureTask;
        private CancellationTokenSource _captureCancelToken;
        private DirectoryInfo _mediaDir;
        private int _workerId;
        private string _configFilePath;
        private List<string> _textLogLines = new List<string>(75);
        private Timer _refocusTimer;
        private const string UNKNOWN = "UNKNOWN";
        private const string NA = "N/A";
        
        public IList<ILocalCapabilityVerifier> CapabilityVerifiers { get; set; }


        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            VerifyCapabilities();

            // On charge le fichier de configuration
            _nomadConfig = ServicesContainer.GetService<NomadConfiguration>();

            _baseDir = DirectoryUtil.GetLocalApplicationDir();
            _mediaDir = new DirectoryInfo(DirectoryUtil.GetLocalApplicationMediaDir());
            _config = new VideoCaptureConfig();

            // Vidange des fichiers temporaires.
            FileUtil.DeleteFolderContent(DirectoryUtil.GetLocalApplicationWorkDir());

            _configFilePath = Path.Combine(_baseDir, _config.ConfigFileName ?? _config.DefaultConfigFileName);
            if (!File.Exists(_configFilePath))
            {
                var initialWrite = JsonUtil.WriteJsonFile(new FileInfo(_configFilePath), _config, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,

                });
                if (!initialWrite)
                {
                    log.Fatal("Could not write base config file to disk: " + _configFilePath);
                    this.SafeCall(() =>
                    {
                        var diagResult = MessageBox.Show(this, $@"The config file did not exist, and could not be created. Correct this.\r\nPath: {_configFilePath}",
                            @"Impossible to write config file.", MessageBoxButtons.OK);
                        if (diagResult == DialogResult.OK)
                        {
                            Close();
                        }
                    });
                }
                else
                {
                    log.Warn("Incomplete configuration file detected. You must configure app with this file: " + _configFilePath);
                    this.SafeCall(() =>
                    {
                        var diagResult = MessageBox.Show(this, $@"The config file did not exist, and was created. It is however incomplete. Correct this.\r\nPath: {_configFilePath}",
                            @"Basic and incomplete configuration file.", MessageBoxButtons.OK);
                        if (diagResult == DialogResult.OK)
                        {
                            Close();
                        }
                    });
                }
            }

            var loadConfig = JsonUtil.UpdateInstanceFromFile(_config, _baseDir);

            if (!loadConfig)
            {
                log.Warn("Could not load config file from disk: " + _configFilePath);
                this.SafeCall(() =>
                {
                    var diagResult = MessageBox.Show(this, $@"The config file to load did not contain correct or complete information. Please revise.\r\nPath: {_configFilePath}",
                        @"Incomplete config file.", MessageBoxButtons.YesNo);
                    if (diagResult == DialogResult.Yes)
                    {
                        Close();
                    }
                });
            }

            // On initialize maintenant les modules
            _capture = InitModule<CaptureVideoBase>(_config.CaptureClassFullName);
            _ligths = InitModule<ILightsManager>(_config.LightClassFullName);
            _trigger = InitModule<TriggerManagerBase>(_config.TriggerClassFullName);
            _reader = InitModule<ReaderManagerBase>(_config.ReaderClassFullName);
            
            // Service de surveillance système.
            _watcherService = ServicesContainer.GetService<ServiceStateWatcher>();
            if (_watcherService == null)
            {
                // Si pas déjà initialisé, on le créer maintenant.
                var watchService = new ServiceStateWatcher();
                // Ajoute la description des services requis.
                var watchers = new List<ExternalStateWatcher>
                {
                    new InternetConnectionWatcher
                    {
                        DelayBetweenChecks = TimeSpan.FromSeconds(5)
                    }
                };

                _capture?.UpdateWatcherList(watchers);
                _ligths?.UpdateWatcherList(watchers);
                _trigger?.UpdateWatcherList(watchers);
                _reader?.UpdateWatcherList(watchers);

                watchService.AddStateWatchers(watchers);

                ServicesContainer.AddService(watchService).GetAwaiter().GetResult();
                _watcherService = ServicesContainer.GetService<ServiceStateWatcher>();
            }

            if (_watcherService == null)
            {
                this.SafeCall(() =>
                {
                    var diagResult = MessageBox.Show(this, @"The critical service 'State watcher' cannot be started. It is impossible to use the application.",
                        @"Critical service missing.", MessageBoxButtons.YesNo);
                    if (diagResult == DialogResult.Yes)
                    {
                        Close();
                    }
                });
            }
            else
            {
                _watcherService.AddObserver(this);
                _watcherService.SendLastReport();
            }

            // Service de manipulation du vidéo.
            _workPool = new CustomWorkerPool<VideoTreatmentParameter, VideoTreatmentResult>();
            _workerType = _workPool.ClassTypeCheckFromName( _config.WorkerClassFullName); // On DOIT absolument utiliser le AssemblyQualifiedName ici!
            if (_workerType == null)
            {
                // Test pour assigner une classe via code.
                // Pour obtenir le nom d'assemblage complet. Nécessaire si on cherche à partir d'autres "assemblies" de C#.
                // var t = typeof(VideoTreatmentAquaparc2021);
                // var vta = new VideoTreatmentAquaparc2021
                // {
                //     Id = 1
                // };
                // _workPool.AddWorker(vta);
                
                _workPool.Stop();
            }
            else
            {
                CustomWorkerPool<VideoTreatmentParameter, VideoTreatmentResult>.StartPoolInstance(_workPool, "Video Capture worker pool thread.");
                _workPool.WorkerFinished += OnPoolWorkerFinished;
                _workPool.StateUpdate += SafeUpdate;
                log.Debug("Started worker pool.");

                // On scan s'il existe des fichiers existants déjà, ce qui implique que il y a des travailleurs qui n'ont pas terminés leur travail.

                var eventDirectories = _mediaDir.GetDirectories();
                if (eventDirectories.Any())
                {
                    AddLogMessage($"Detected old events not completed by workers. Creating {eventDirectories.Length} new workers immediately.");

                    foreach (var f in eventDirectories)
                    {
                        var isEmpty = f.CheckDirectoryEmpty_Fast();
                        if (!isEmpty.HasValue || isEmpty.Value)
                        {
                            log.Warn("Empty directory detected. Destroying: " + f.FullName);
                            FileUtil.DeleteFolderContent(f.FullName);
                            continue;
                        }

                        var metadataFile = new FileInfo(Path.Combine(f.FullName, METADATA_FILE_NAME));
                        var metadata = JsonUtil.ReadJsonFile<Dictionary<string, string>>(metadataFile);

                        if (metadata == null)
                        {
                            log.Error("Could not read metadata from video file. Corrupt file. Deleting folder: {0}", f.FullName);
                            FileUtil.DeleteFolderContent(f.FullName);
                            continue;
                        }

                        var parameters = new VideoTreatmentParameter
                        {
                            WorkDirectory = f,
                            RespondentUniqueId = metadata[VideoTreatmentAbstract.RESPONDENT_ID_KEY],
                            AdditionalData = metadata
                        };
                        var ex = _workPool.AddCustomWorker(++_workerId, _workerType, parameters, this, worker =>
                        {
                            if (!(worker is VideoTreatmentAbstract w)) return false;
                            w.WorkerName = worker.GetType().Name;
                            return w.CustomInit(_config, _nomadConfig);
                        });
                        if (ex != null)
                        {
                            log.Error(ex, "Could not launch worker for following file: " + f.FullName);
                        }
                    }
                }
            }

            _globalStatus = Initializing;

            trk_delay_valeur.MouseUp += Trk_delay_valeurOnMouseUp;

            CheckForStartupProblems();

            CheckGlobalState();

            if (_globalStatus == CannotRun) UpdateForm();
        }

        private void SafeUpdate()
        {
            this.SafeCall(UpdateForm);
        }

        private void Trk_delay_valeurOnMouseUp(object sender, MouseEventArgs e)
        {
            if (!(sender is TrackBar tb)) return;
            if (tb.Value == _config.DelayForCaptureAfterTrigger) return;
            _config.DelayForCaptureAfterTrigger = tb.Value;
            AddLogMessage($"Capture delay changed to {tb.Value} ms.");
            if (!JsonUtil.WriteJsonFile(new FileInfo(_configFilePath), _config))
            {
                log.Error("Could not save delay configuration to file.");
            }
            UpdateForm();
        }

        /// <summary>
        /// Cette méthode est appellée uniquement au démarrage de l'Application, car tous les problème détectés sont non-recouvrable.
        /// </summary>
        private void CheckForStartupProblems()
        {
            // Pas de check de Invoke car uniquement appellé dans l'initialisation.

            var pList = new List<string> {"Cannot run application. The following problems have been encountered at startup:"};

            if (_workerType == null)
            {
                log.Error("No valid worker type detected. Please check configuration: " + _config.WorkerClassFullName);
                pList.Add("Fatal error detected at startup: no valid video worker is defined. Application cannot be used.");
                _globalStatus = CannotRun;
            }

            if (_reader == null)
            {
                log.Error("No valid reader class given. Please check configuration: " + _config.ReaderClassFullName);
                pList.Add("Fatal error detected at startup: no valid reader system is defined. Application cannot be used.");
                _globalStatus = CannotRun;
            }
            else
            {
                _reader.ScanDetectedAndValidated += ScannerCheckEvent;
                _reader.UiMessageEvent += AddLogMessage;

                if (_reader.NeedTextBoxSource)
                {
                    var message = "WARNING: Non event-based reader detected. Must use textbox to capture card reads.";
                    log.Warn(message);
                    AddLogMessage(message);
                    grb_rfid_text.Visible = true;
                    grb_rfid_text.Enabled = true;
                    _reader.InitializeTextBoxEvents(grb_rfid_text_box);

                    // Force un refocus constant (ou presque) sur cette boîte pour saisir des entrée clavier d'un lecteur RFID.
                    _refocusTimer = new Timer {Interval = 500};
                    _refocusTimer.Tick += (sender, args) =>
                    {
                        ActiveControl = grb_rfid_text_box;
                    };
                    _refocusTimer.Start();
                }
            }

            if (_capture == null)
            {
                log.Error("No valid capture class given. Please check configuration: " + _config.CaptureClassFullName);
                pList.Add("Fatal error detected at startup: no valid capture system is defined. Application cannot be used.");
                _globalStatus = CannotRun;
            }

            if (_trigger == null)
            {
                log.Error("No valid trigger class given. Please check configuration: " + _config.TriggerClassFullName);
                pList.Add("Fatal error detected at startup: no valid trigger system is defined. Application cannot be used.");
                _globalStatus = CannotRun;
            }
            else
            {
                if (_capture != null)
                {
                    _trigger.Triggered += _capture.TriggerEvent;
                }
                _trigger.Start();
            }
            
            if (_ligths == null && !string.IsNullOrWhiteSpace(_config.LightClassFullName))
            {
                log.Warn("No valid light class given. Please check configuration: " + _config.LightClassFullName);
                pList.Add("Error detected at startup: no valid light system is defined. If you do not want lights, clear this configuration.");
            }

            if (pList.Count > 1)
            {
                AddLogMessage(pList);
            }

            if (_nomadConfig.CurrentServerConfigurationKey != ServerConfigurationSwitch.Prod)
            {
                AddLogMessage("Warning: current Nomad server configuration is not official. Current setting is => " + _nomadConfig.CurrentServerConfigurationKey);
            }
        }

        /// <summary>
        /// Cette méthode est le point de contact pour lancer la capture. C'est le système de lecture RFID qui lance cet événement.
        /// </summary>
        private void ScannerCheckEvent(string respondantUniqueId)
        {
            if (_globalStatus != Waiting)
            {
                log.Debug("Trigger event detected. Skipped due to wrong global state: " + _globalStatus);
                return;
            }

            if (_captureTask != null)
            {
                log.Debug("Trigger event detected. Skipped due to previous capture task being processed.");
                return;
            }

            // Création de la tâche de capture.
            if (log.IsDebugEnabled)
            {
                AddLogMessage("Scan detected -> Creating capture sequence.");
            }
            _captureCancelToken = new CancellationTokenSource(_config.MaxCaptureTimeAllowed);
            
            _captureTask = Task.Run(async () =>
            {
                this.SafeCall(CheckGlobalState);
                await CaptureSequence(_captureCancelToken.Token, respondantUniqueId);
            }, _captureCancelToken.Token).ContinueWith(task =>
            {
                if (_captureCancelToken.Token.IsCancellationRequested)
                {
                    var message = "Capture operation was cancelled because it overall took too much time. Time allowed: " + _config.MaxCaptureTimeAllowed.ToMinimumReadableTime();
                    log.Warn(message);
                    AddLogMessage(message);
                }
                // Nettoyage des valeur de tâche. Va s'exécuter même si cancellé à cause qu'on ne passe pas un jeton cancellable.
                // Donc, indépendent de la tâche parent.
                _captureTask = null;
                _captureCancelToken = null;
                log.Trace("Capture sequence task cleared.");
                if (log.IsDebugEnabled)
                {
                    AddLogMessage("Capture sequence as ended.");
                }
                this.SafeCall(CheckGlobalState);

            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
            
            log.Debug("Capture sequence task created.");
        }
        
        private async Task CaptureSequence(CancellationToken ct, string respondantUniqueId)
        {
            // Vérification rapide que rien n'est bizarre dans l'état
            if (_capture == null)
            {
                log.Fatal("Fatal unforeseen state: capture device is NULL. Cannot proceed with capture task.");
                return;
            }

            var currentEventDateTime = DateTime.Now;
            var dirToCreate = new DirectoryInfo(Path.Combine(_mediaDir.FullName, currentEventDateTime.ToString("yyyy'_'MM'_'dd'_'hh'_'mm'_'ss")));
            dirToCreate.Create();
            _capture.DelayAfterTrigger = new TimeSpan(0,0,0,0,_config.DelayForCaptureAfterTrigger);
            await _capture.StartCapture(ct, dirToCreate);
            
            dirToCreate.Refresh();
            // On regarde le résultat de la capture.
            if (!_capture.Validate(dirToCreate))
            {
                FileUtil.DeleteFolderContent(dirToCreate.FullName);

                var mess = "Video capture failed. Check logs for more details.";
                AddLogMessage(mess);
                log.Warn(mess);
                return;
            }

            log.Debug("After capture - video event created => {0}", dirToCreate.FullName);

            // Si le répertoire est correct, on part un nouveau travailleur, et on ajoute les métadonnées.
            var metadata = new Dictionary<string, string>
            {
                {VideoTreatmentAbstract.RESPONDENT_ID_KEY, respondantUniqueId},
                {VideoTreatmentAbstract.DATE_TIME, currentEventDateTime.ToString("O", CultureInfo.InvariantCulture)}

            };
            var metadataFile = new FileInfo(Path.Combine(dirToCreate.FullName, METADATA_FILE_NAME));

            JsonUtil.WriteJsonFile(metadataFile, metadata);
            metadataFile.Refresh();

            if (!metadataFile.Exists)
            {
                FileUtil.DeleteFolderContent(dirToCreate.FullName);

                var mess = "Video capture failed. Could not create metadata file. Check logs for more details.";
                AddLogMessage(mess);
                log.Warn(mess);
                return;
            }

            var parameters = new VideoTreatmentParameter
            {
                WorkDirectory = dirToCreate,
                RespondentUniqueId = respondantUniqueId,
                AdditionalData = metadata
            };
            var ex = _workPool.AddCustomWorker(++_workerId, _workerType, parameters, this,  worker =>
            {
                if (!(worker is VideoTreatmentAbstract w)) return false;
                w.WorkerName = worker.GetType().Name;
                return w.CustomInit(_config, _nomadConfig);
            });

            if (ex == null) return;

            // Travailleur n'a pas pu être ajouté.
            var mess2 = $"Worker ID '{_workerId}' could not be added for event '{dirToCreate.Name}'.";
            AddLogMessage(mess2);
            log.Warn(mess2);
        }

        private void CheckGlobalState()
        {
            if (_globalStatus == CannotRun)
            {
                // Inutile de faire de vérifications supplémentaires. L'application ne peut pas rouler.
                return;
            }

            var lastGlobalStatus = _globalStatus;

            var mustStop = false;
            var pList = new List<string> {"The following problems have been encountered:"};

            // Workpool
            mustStop |= CheckGlobalStateItem(ref _poolStopped, _workPool.IsRunning(), pList,
                "Worker pool has stopped running.", 
                "Worker pool has restarted.");
            
            // Trigger
            mustStop |= CheckGlobalStateItem(ref _triggerStopped, _trigger.IsRunning, pList,
                "Trigger has stopped running.", 
                "Trigger has restarted.");

            // Capture
            mustStop |= CheckGlobalStateItem(ref _captureStopped, _capture.IsRunning, pList,
                "Capture has stopped running.", 
                "Capture has restarted.");

            // Reader
            mustStop |= CheckGlobalStateItem(ref _readerStopped, _reader.IsRunning, pList,
                "Reader has stopped running.", 
                "Reader has restarted.");

            if (pList.Count > 1)
            {
                // Ajoute messages de log uniquement si un changement d'état est survenu. EN bien ou en mal.
                AddLogMessage(pList);
            }

            if (mustStop)
            {
                _globalStatus = Stopped;
            }
            else
            {
                _globalStatus = _captureTask != null ? Capturing : Waiting;
            }

            if (_globalStatus == lastGlobalStatus) return;
            log.Trace("Status changed to : " + _globalStatus);
            UpdateForm();
        }

        private static bool CheckGlobalStateItem(ref bool lastStateErrorDetected, bool currentState, ICollection<string> messageLog, 
            string messageStopped, string messageRestarted)
        {
            // Si un problème existe
            if (!currentState)
            {
                if (lastStateErrorDetected) return true; // Pas besoin de ré-écrire dans le log.
                
                // Notification du changement d'état
                lastStateErrorDetected = true;
                messageLog.Add(messageStopped);
                return true;
            }

            // Si on roule
            if (!lastStateErrorDetected) return false; // Tout allait déjà bien, pas de message à écrire
            
            // Notification du changement d'état
            lastStateErrorDetected = false;
            messageLog.Add(messageRestarted);
            return false;
        }

        private void OnPoolWorkerFinished(bool success, VideoTreatmentResult result)
        {
            if (!log.IsDebugEnabled) return;
            var message = $"Worker finished: success={result.Complete}, state={result.State}";
            log.Debug(message);
            if (log.IsDebugEnabled)
            {
                if (result.UiMessage != null)
                {
                    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    AddLogMessage(message, result.UiMessage);
                } else AddLogMessage(message);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            log.Debug("Form closing. Disposing of all components.");

            _refocusTimer?.Stop();

            _captureCancelToken?.Cancel();
            _captureCancelToken = null;
            _captureTask = null;

            if (_workPool != null)
            {
                _workPool.WorkerFinished -= OnPoolWorkerFinished;
                _workPool.StateUpdate -= SafeUpdate;
                _workPool.Stop();
            }

            _watcherService?.RemoveObserver(this);

            if (_reader != null)
            {
                _reader.ScanDetectedAndValidated -= ScannerCheckEvent;
                _reader.UiMessageEvent -= AddLogMessage;
                _reader.InitializeTextBoxEvents(grb_rfid_text_box, true); // Même si on en a pas besoin.
                _reader.Dispose();
            }

            if (_trigger != null)
            {
                if (_capture != null)
                {
                    _trigger.Triggered -= _capture.TriggerEvent;
                }
                _trigger.Dispose();
            }
            _capture?.Dispose();
            _ligths?.Dispose();

            base.OnClosing(e);
        }

        private void UpdateForm()
        {
            // Boîtes Info
            AssignLabelText(lbl_info_capture_class_box, _capture, _capture?.GetType().Name, true);
            AssignLabelText(lbl_info_led_class_box, _ligths, _config.LightClassFullName, false);
            AssignLabelText(lbl_info_project_name, _config, _config.ProjectDescription, false);
            AssignLabelText(lbl_info_nomad_ids_box, _config, _config.GetNomadIdsStr, false, false);
            AssignLabelText(lbl_info_terminal_datakey_box, _config, _config.LocationKeyName, false);
            AssignLabelText(lbl_info_worker_class_box, _workerType, _workerType?.Name, true);
            AssignLabelText(lbl_info_rfid_class_box, _reader, _reader?.GetType().Name, true, false);
            AssignLabelText(lbl_info_trigger_class_box, _trigger, _trigger?.GetType().Name, true, false);

            // Valeur du délai trigger
            AssignLabelText(lbl_delay_value_box, _config, _config.DelayForCaptureAfterTrigger + " ms", false, false);
            trk_delay_valeur.Value = _config.DelayForCaptureAfterTrigger;

            if (_workPool?.IsRunning() ?? false)
            {
                var nbrWork = _workPool.GetCurrentWorkerCount();
                lbl_bkg_worker_service_state_box.Text = nbrWork > 0 ? "Working" : "Idle";
                lbl_workers_current_box.Text = nbrWork.ToString();
                lbl_workers_waiting_box.Text = _workPool.GetQueueSize().ToString();
                btn_pause_workers.Enabled = true;
            }
            else
            {
                lbl_bkg_worker_service_state_box.Text = @"Stopped";
                lbl_workers_current_box.Text = NA;
                lbl_workers_waiting_box.Text = NA;
                btn_pause_workers.Enabled = false;
            }

            // Label de l'état principal.
            var color = Color.Black; // défaut
            string text;
            switch (_globalStatus)
            {
                case Waiting:
                    text = "Waiting for scan";
                    break;
                case Capturing:
                    text = "Capturing";
                    break;
                case CannotRun:
                    text = "Application cannot run";
                    color = Color.Crimson;
                    break;
                case Stopped:
                    text = "Stopped";
                    color = Color.OrangeRed;
                    break;
                case Initializing:
                    text = "Initializing";
                    color = Color.OrangeRed;
                    break;
                default:
                    text = "UNKNOWN";
                    color = Color.OrangeRed;
                    break;
            }
            lbl_info_status.ForeColor = color;
            lbl_info_status.Text = text;

            // Si on utilise un textbox pour capturer un scan RFID, on tente de mettre le focus sur ce textbox à chaque update.
            if (grb_rfid_text.Visible)
            {
                grb_rfid_text_box.Focus();
            }
        }

        private void AssignLabelText(Control lbl, object baseObject, string className, bool isCritical, bool extractClassName = true)
        {
            string text;
            var color = Color.White;

            if (baseObject == null)
            {
                text = UNKNOWN;
                if (isCritical) color = Color.Beige;
            }
            else
            {
                text = extractClassName ? StringUtil.GetClassName(className) : className;
            }

            lbl.Text = text;
            lbl.BackColor = color;
        }

        /// <summary>
        /// Instancie, configure via fichier puis initialise un module.
        /// Le module n'est pas encore démarré.
        /// </summary>
        /// <typeparam name="T">Le type du module.</typeparam>
        /// <param name="moduleClassName">Le nom complet de la classe du module.</param>
        /// <returns>L'instance créée.</returns>
        private T InitModule<T>(string moduleClassName) where T : IJsonConfig, IExternalFileConfig, IInitialisable
        {
            if (string.IsNullOrWhiteSpace(moduleClassName)) return default;
            var instance = ClassUtil.InstanciateClassFromName<T>(moduleClassName);
            if (instance == null)
            {
                log.Error("Could not instanciate class: " + moduleClassName);
                return default;
            }

            // Instance créée
            var f = FileUtil.FindFirstFilePath(_baseDir, instance.ConfigFileName, instance.DefaultConfigFileName);
            if (f == null || !f.Exists)
            {
                log.Warn($"Missing config file for module ('{moduleClassName}'), will try to generate one.");

                // Ontente de créer un fichier par défaut, puis de le recharger.
                f = new FileInfo(Path.Combine(_baseDir, instance.ConfigFileName ?? instance.DefaultConfigFileName));
                if (!JsonUtil.WriteJsonFile(f, instance, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                }))
                {
                    log.Error("Could not create config file for module: " + moduleClassName);
                    return default;
                }
            }

            // Ajout d'un hook pour IEventLogger
            if (instance is IReportObserver<VideoCaptureReport> el)
            {
                el.AddObserver(this);
            }

            // Fichier existant
            if (JsonUtil.UpdateInstanceFromFile(instance, _baseDir))
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (instance is INeedWinformParentControl child)
                {
                    child.WinformParentControl = this;
                }

                if (instance.Init()) // Initialisation réussie
                {
                    return instance;
                }

                log.Error("Could not initialize instance: " + moduleClassName);
                return default;
            }

            // Fichier existe mais invalide (probablement)
            log.Error($"Could not populate module ('{moduleClassName}') instance with following config file. Destroy or modify it: " + f.FullName);
            return default;
        }

        public void VerifyCapabilities()
        {
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
                sb.AppendLine("Could not initialize application. Missing capabilities.");
                foreach (var gr in capResult.Groups)
                {
                    foreach (var message in gr.ErrorMessages)
                    {
                        sb.AppendLine(message);
                    }
                }

                Invoke(new Action(() =>
                {
                    var diagResult = MessageBox.Show(this, sb.ToString(),
                        @"Critical components missing.", MessageBoxButtons.YesNo);
                    if (diagResult == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                }));
            });
        }

        private void Btn_quit_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        public void Report(ExternalStateReport value)
        {
            var s = value.GetWatcherStatusList;
            var lastServiceState = _serviceStateGood;

            var populate = false;
            if (_lastReport == null)
            {
                _lastReport = s;
                populate = true;
            }
            else if (!s.SequenceEqual(_lastReport))
            {
                populate = true;
            }

            // Vérifie si un état critique à changé.
            _serviceStateGood = s.All(w => w.Status == WatcherStatusEnum.LastCheckSuccessfull);
            if (!_serviceStateGood)
            {
                if (lastServiceState)
                {
                    AddLogMessage("Invalid Service state detected.");
                }

                this.SafeCall(UpdateForm);
            }
            else if (_serviceStateGood != lastServiceState)
            {
                // On revient d'un mauvais état à un bon.
                AddLogMessage("Service state now valid.");
                this.SafeCall(UpdateForm);
            }

            if (!populate) return; // Pas de changements.
            StringBuilder sb = null;
            if (log.IsDebugEnabled)
            {
                sb = new StringBuilder();
                sb.AppendLine("Populating service watcher list:");
            }
            this.SafeCall(() => {
                var iter = flp_services.Controls.GetEnumerator();
                if (iter != null)
                {
                    while (iter.MoveNext())
                    {
                        if (iter.Current is IDisposable d)
                        {
                            d.Dispose();
                        }
                    }
                }

                flp_services.Controls.Clear();
                foreach (var watch in _lastReport)
                {
                    sb?.AppendLine("\t\t- Base service: " + watch.GetDescription());
                    flp_services.Controls.Add(watch.GetWinFormStatusControl(flp_services));
                        
                    // TODO Refaire cette logique si jamais on a des sous-services à plus de deux niveaux.
                    foreach (var sw in watch.SubWatchers)
                    {
                        sb?.AppendLine("\t\t\t╠ Sub service: " + watch.GetDescription());
                        flp_services.Controls.Add(sw.GetWinFormStatusControl(flp_services));    
                    }
                }
            }, (e) =>
            {
                log.Error(e, "Could not populate service display correctly. List emptied.");
                flp_services?.Controls.Clear();
            });

            if (sb != null)
            {
                log.Debug(sb.ToString);
            }
        }

        private delegate void SafeCallSingleMessage(string text);
        private void AddLogMessage([NotNull] string message)
        {
            if (txt_log.IsDisposed) return;

            if (txt_log.InvokeRequired)
            {
                var d = new SafeCallSingleMessage(AddLogMessage);
                txt_log.Invoke(d, message);
                return;
            }

            _textLogLines.Add($"{DateTime.Now:T} - {message}");
            if (_textLogLines.Count > 50)
            {
                _textLogLines = _textLogLines.Skip(30).ToList();
                log.Trace("Log lines exceed. Purging last 30.");
            }

            txt_log.Lines = _textLogLines.ToArray();
            txt_log.Refresh();
        }

        private void AddLogMessage(object sender, [NotNull] string message)
        {
            AddLogMessage(message);
        }

        private delegate void SafeCallMultipleMessage(IList<string> text);

        private void AddLogMessage(params string[] messages)
        {
            AddLogMessage(messages.ToList());
        }

        private void AddLogMessage([NotNull] IList<string> pList)
        {
            if (!pList.Any()) return;
            if (txt_log.IsDisposed) return;

            if (txt_log.InvokeRequired)
            {
                var d = new SafeCallMultipleMessage(AddLogMessage);
                txt_log.Invoke(d, pList);
                return;
            }

            // On ajoute l'heure au premier message de la liste.
            _textLogLines.Add($"{DateTime.Now:T} - {pList[0]}");
            _textLogLines.AddRange(pList);
            
            if (_textLogLines.Count > 50)
            {
                _textLogLines = _textLogLines.Skip(30).ToList();
                log.Trace("Log lines exceed. Purging last 30.");
            }

            txt_log.Lines = _textLogLines.ToArray();
            txt_log.Refresh();
        }

        private void Btn_pause_workers_Click_1(object sender, EventArgs e)
        {
            if (_workPool == null)
            {
                btn_pause_workers.Enabled = false;
                log.Warn("Worker pool was null. Disabling button.");
                return;
            }

            if (_workPool.IsPaused())
            {
                _workPool.UnPause();
                btn_pause_workers.Text = @"Pause";
            }
            else
            {
                _workPool.Pause();
                btn_pause_workers.Text = @"Unpause";
            }
        }

        public void LogEvent(string message)
        {
            AddLogMessage(message);
        }

        public void LogEvent(string message, Exception e)
        {
            AddLogMessage(message);
            log.Error(e, message);
        }

        public void Report(VideoCaptureReport report)
        {
            AddLogMessage(report.StatusMessage);
        }
    }
}
