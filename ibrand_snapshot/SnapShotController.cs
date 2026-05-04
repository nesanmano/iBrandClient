using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using iBrand_SnapShot.CustomWorker;
using iBrand_SnapShot.Trigger;
using iBrand_SnapShot.UI;
using Newtonsoft.Json;
using NLog;
using NomadLogic.iBrand.Common.Transformation.Video.Ffmpeg;
using NomadLogic.iBrand.Common.Util;

namespace iBrand_SnapShot
{
    internal enum SnapShotState
    {
        /// <summary>
        /// Aucune caméra n'est détectée.
        /// </summary>
        Absent,
        /// <summary>
        /// La caméra est prête à démarrer un film ou prendre une photo.
        /// </summary>
        Ready,
        /// <summary>
        /// La caméra est en train de filmer.
        /// </summary>
        Filming,
        /// <summary>
        /// La caméra à terminer son opération et est en train de gérer son fichier interne. Dans cet
        /// état, on devrait bientôt recevoir un événement de OnFileCreatedOnCamera.
        /// Cette opération peut prendre plusieurs secondes dans le cas d'un vidéo.
        /// </summary>
        Stopped,
        /// <summary>
        /// La caméra à terminé de créé son fichier interne et est en train de le transférer sur la machine hôte.
        /// cette opération peut prendre quelques secondes, selon la taille du fichier.
        /// </summary>
        Transfering,
        /// <summary>
        /// Les opérations sur la caméra sont présentement bloquées, probablement par un traitement de masse des
        /// fichiers pris par la caméra.
        /// </summary>
        Blocked,
        /// <summary>
        /// Une erreur s'est produite sur la séquence et on doit stopper la machine tant que l'on a pas résolu le problème.
        /// </summary>
        ErrorOccured,
    };

    internal class SnapShotController : IEventLogger
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly Color NOT_READY = Color.Red;
        public static readonly Color READY = Color.BlueViolet;
        public static readonly Color ORIGINAL = Color.Black;
        public static readonly Color MODIFIED = Color.Olive;
        private static readonly string ftpVideosSubdir = @"videos";
        private static readonly string ftpPicturesSubdir = @"pictures";
        private static readonly string ftpThumbnailsSubdir = @"thumbnails";
        private static readonly string settingsFile = @".\snapshot_Settings.json";
        private static readonly string VIDEO_DIR = @"\videos\";
        private static readonly string PICTURE_DIR = @"\pictures\";
        private static readonly string pictureFileType = @".jpg";
        private static readonly string cameraVideoFileType = @".mov";
        private static readonly string videoFileType = @".avi";

        private readonly MainFormUi _refUi;
        private volatile bool _stop;
        //private static Object locker = new Object();
        private SnapShotState _state = SnapShotState.Absent;
        private Thread _mainLoop;
        private FtpPool _poolFtp;
        private CustomWorkerPool<SnapShotCustomWorkerParameter, FileInfo> _poolWorker;
        private readonly JsonSerializer _serializer = new JsonSerializer();
        private EosFrameworkManager _eosManager;
        private EosCamera _eosCamera;
        private int _workerNbr;
        private TriggerAbstract _triggerAbstractMechanism;
        private TimeSpan[] _activationWindow;
        private DateTime _lastStateChange = DateTime.Now;
        private readonly object _locker = new object();

        /// <summary>
        /// Indique si on est en test pour le trigger.
        /// </summary>
        public bool TriggerTestMode { get; set; }

        /// <summary>
        /// Indique si on est en test pour le trigger.
        /// </summary>
        public SnapShotSettings Settings { get; set; }

        /// <summary>
        /// Constructeur public.
        /// </summary>
        /// <param name="form">La forme principale.</param>
        public SnapShotController(MainFormUi form)
        {
            _refUi = form;
        }

        /// <summary>
        /// Démarre les principales fonction du contrôlleur, ainsi que son initialisation.
        /// </summary>
        public void Start()
        {
            if (_mainLoop == null)
            {
                _mainLoop = new Thread(MainLoop)
                {
                    IsBackground = true,
                    Name = "Main Thread",
                    Priority = ThreadPriority.Lowest
                };
                // Important, sinon l'application ne ferme pas.
                _mainLoop.Start();
            }
        }

        private void MainLoop()
        {
            ReadConfigFile();
            StartWorkerPool();
            InitFtpPool();
            InitTriggerClass();
            ValidateTriggerTime(Settings.TimeTriggerWindow);

            var index = 0;
            string[] symbol = { @"|", @"/", @"-", @"\" };
            while (!_stop)
            {
                /* Regarde les status des bassins de traitement. */
                SetFtpStatus();
                SetCustomWorkerStatus();
                _refUi.SetStatus(_refUi.DisplayAliveCtrl, ORIGINAL, symbol[index++], true);

                if (index >= symbol.Length)
                {
                    index = 0;
                }

                try
                {
                    Thread.Sleep(3000);
                    Thread.Yield();
                }
                catch (ThreadInterruptedException)
                {
                    // On ne fait rien.
                }

                /* Fait le monitoring de l'état principal */
                var ts = DateTime.Now - _lastStateChange;
                switch (_state)
                {
                    // Cas d'une durée de fil trop long.
                    case SnapShotState.Filming:
                        if (ts.Milliseconds > (Settings.VideoDuration * 1000) + 1000)
                        {
                            if (_eosCamera != null)
                            {
                                _eosCamera.StopRecord();
                                SetSnapShotState(SnapShotState.Stopped);
                                if (Settings.PoolVerboseLog) _refUi.LogEvent("-Sent STOP command to camera.");
                                _refUi.LogEvent("-Camera monitor detected an overly long filming duration.");
                            }
                            else
                            {
                                // Caméra non-disponible.
                                SetSnapShotState(SnapShotState.Absent);
                                if (Settings.PoolVerboseLog) _refUi.LogEvent("-Camera monitor detected an absent camera.");
                            }
                        }
                        break;

                    // Cas d'une durée de traitement vraiment trop long.
                    case SnapShotState.Stopped:
                        if (ts.Seconds > 10)
                        {
                            if (_eosCamera != null)
                            {
                                SetSnapShotState(SnapShotState.Ready);
                                _refUi.LogEvent("-Camera monitor detected an overly long camera internal file processing duration.");
                            }
                            else
                            {
                                // Caméra non-disponible.
                                SetSnapShotState(SnapShotState.Absent);
                                if (Settings.PoolVerboseLog) _refUi.LogEvent("-Camera monitor detected an absent camera.");
                            }
                        }
                        break;

                    // Cas d'une durée de transfert vraiment trop long.
                    case SnapShotState.Transfering:
                        if (ts.Seconds > 10)
                        {
                            if (_eosCamera != null)
                            {
                                SetSnapShotState(SnapShotState.Ready);
                                _refUi.LogEvent("-Camera monitor detected an overly long file transfert from camera.");
                            }
                            else
                            {
                                // Caméra non-disponible.
                                SetSnapShotState(SnapShotState.Absent);
                                if (Settings.PoolVerboseLog) _refUi.LogEvent("-Camera monitor detected an absent camera.");
                            }
                        }
                        break;
                    case SnapShotState.ErrorOccured:
                        if (_eosCamera != null)
                        {
                            // On tente d'abord de stopper la caméra. Au cas où.
                            try { _eosCamera.StopRecord(); }
                            catch (Exception) { /* rien */ }

                            /* On lâche la caméra, et on tente de la réinitialiser. */
                            OnCameraShutDown(null, null);
                            LoadCamera();
                            if (IsCameraInitialized)
                            {
                                if (Settings.PoolVerboseLog) _refUi.LogEvent("-Camera error state purged. Camera is now enabled anew.");
                                SetSnapShotState(SnapShotState.Ready);
                            }
                            else
                            {
                                if (Settings.PoolVerboseLog) _refUi.LogEvent("-Camera error state purged. Camera could not be re-attached.");
                                SetSnapShotState(SnapShotState.Absent);
                            }
                        }
                        break;
                }
            }
        }

        public void Stop()
        {
            _stop = true;

            RemoveCameraFramework();

            if (_poolFtp != null)
            {
                _poolFtp.Stop();
                _poolFtp.WorkerFinished -= TranfertFinished;
            }
            if (_poolWorker != null)
            {
                _poolWorker.Stop();
                _poolWorker.WorkerFinished -= CustomWorkerFinished;
            }
            if (_triggerAbstractMechanism != null)
            {
                _triggerAbstractMechanism.Stop();
                _triggerAbstractMechanism.Dispose();
            }
        }

        private void SetCustomWorkerStatus()
        {
            var sb = new StringBuilder();

            if (_poolWorker == null || Settings.CustomWorker == null || Settings.CustomWorker.Length == 0)
            {
                sb.Append("Pool not initialized");
                _refUi.SetStatus(_refUi.CustomWorkerStatusCtrl, NOT_READY, sb.ToString(), true);
                return;
            }

            sb.Append(_poolWorker.IsRunning() ? "Running: " : "Stopped: ");

            sb.Append(_poolWorker.GetCurrentWorkerCount());
            sb.Append(':');
            sb.Append(_poolWorker.GetQueueSize());

            _refUi.SetStatus(_refUi.CustomWorkerStatusCtrl, READY, sb.ToString(), true);
        }

        private void SetFtpStatus()
        {
            var sb = new StringBuilder();

            if (_poolFtp == null)
            {
                if (Settings.UseFtp)
                {
                    sb.Append("Pool not initialized");
                    _refUi.SetStatus(_refUi.FtpStatusCtrl, NOT_READY, sb.ToString(), true);
                }
                else
                {
                    sb.Append("Not required.");
                    _refUi.SetStatus(_refUi.FtpStatusCtrl, ORIGINAL, sb.ToString(), true);
                }
                return;
            }

            sb.Append(_poolFtp.IsRunning() ? "Running: " : "Stopped: ");

            sb.Append(_poolFtp.GetCurrentWorkerCount());
            sb.Append(':');
            sb.Append(_poolFtp.GetQueueSize());

            _refUi.SetStatus(_refUi.FtpStatusCtrl, READY, sb.ToString(), true);
        }

        private void SetSnapShotState(SnapShotState newState)
        {
            lock (_locker)
            {
                _state = newState;
                _lastStateChange = DateTime.Now;

                if (_eosCamera != null)
                {
                    var col = READY;

                    if (_state == SnapShotState.Absent || _state == SnapShotState.Blocked || _state == SnapShotState.ErrorOccured)
                    {
                        col = NOT_READY;
                    }

                    _refUi.SetStatus(_refUi.CamStatusCtrl, col, _state.ToString(), true);
                    if (Settings.PoolVerboseLog) _refUi.LogEvent("State changed to " + _state);
                }
            }
        }

        private void CreateThumbnail(string path)
        {
            // On extrait un thumbnail du video.
            if (!string.IsNullOrEmpty(Settings.LocationFfmpeg))
            {
                if (!(Settings.VideoThumbnail > Settings.VideoDuration))
                {
                    var thumb = new ThumbnailsGenerator {CustomFFmpegLocation = Settings.LocationFfmpeg};
                    thumb.GenerateThumbnail(path, path + pictureFileType, Settings.VideoThumbnail);
                }
                else
                {
                    _refUi.LogEvent("Could not create thumbnail because thumbnail time is greater than video duration.");
                }
            }
            else
            {
                _refUi.LogEvent("Could not create thumbnail because FFMPEG location of executable is not set.");
            }
        }

        /// <summary>
        /// Méthode pour partir l'enregistrement vidéo vers la caméra.
        /// <br/>Il y a un blocage thread dans la méthode alors n'appeller cette méthode que sur un fil
        /// d'exécution différent de celui du UI.
        /// </summary>
        private void StartVideo()
        {
            if (_eosCamera != null && _state == SnapShotState.Ready)
            {
                try
                {
                    var sw = new Stopwatch();
                    long maxTime = Settings.VideoDuration * 1000;

                    lock (_locker)
                    {
                        SetSnapShotState(SnapShotState.Filming);

                        if (Settings.UseFtp && _poolFtp != null && _poolFtp.IsRunning())
                        {
                            string[] files = { "filming" };
                            string[] subDirs = { ftpThumbnailsSubdir };
                            _poolFtp.SendFalseFile(subDirs, files);
                        }

                        sw.Start();

                        if (Settings.PoolVerboseLog) _refUi.LogEvent("Sent START command to camera.");
                        _eosCamera.StartRecord();
                    }

                    while (sw.ElapsedMilliseconds < maxTime)
                    {
                        var left = maxTime - sw.ElapsedMilliseconds;
                        if (left > 1000)
                        {
                            Thread.Yield();
                        }
                        else
                        {
                            Thread.Sleep(new TimeSpan(0, 0, 0, 0, (int)left));
                        }
                    }

                    sw.Stop();
                    // Au cas où l'enregistrement arrête avant (manque de mémoire, interférence d'un humain, etc...)
                    if (_state == SnapShotState.Filming)
                    {
                        lock (_locker)
                        {
                            SetSnapShotState(SnapShotState.Stopped);
                            if (Settings.PoolVerboseLog) _refUi.LogEvent("Sent STOP command to camera.");
                            _eosCamera.StopRecord();
                        }

                        if (Settings.UseFtp && _poolFtp != null && _poolFtp.IsRunning())
                        {
                            string[] files = { "processing" };
                            string[] subDirs = { ftpThumbnailsSubdir };
                            _poolFtp.SendFalseFile(subDirs, files);
                        }
                    }
                }
                catch (Exception e)
                {
                    _refUi.LogEvent("Problem occured during video capture." + "\r\n" +
                        "Is the camera mode on 'movie'?" + "\r\n" +
                        "Is the camera firmware version is '1.0.2-ml-v2.3'?");
                    log.Error(e.Message + "\r\n" + e.StackTrace);
                    SetSnapShotState(SnapShotState.ErrorOccured);
                }
            }
        }

        /// <summary>
        /// Lit le fichier de configuration du logiciel. En profite aussi pour initialiser les paramètres de
        /// la sérialisation JSON, qui seront utilisée dans le reste du logiciel.
        /// </summary>
        private void ReadConfigFile()
        {
            // Config du sérialisateur.
            _serializer.NullValueHandling = NullValueHandling.Ignore;

            if (File.Exists(settingsFile))
            {
                using (var file = File.OpenText(settingsFile))
                {
                    Settings = (SnapShotSettings)_serializer.Deserialize(file, typeof(SnapShotSettings));
                }
            }
            else
            {
                using (var file = File.CreateText(settingsFile))
                {
                    if (Settings == null)
                    {
                        Settings = new SnapShotSettings();
                    }
                    _serializer.Serialize(file, Settings);
                    _refUi.LogEvent("Settings file not found. New one created.");
                }
            }

            if (Settings.ConcurrentWorkersFtp == 0) Settings.ConcurrentWorkersFtp = SnapShotSettings.DEFAULT_CONCURRENT_WORKERS_FTP;
            if (Settings.ConcurrentWorkersCustom == 0) Settings.ConcurrentWorkersCustom = SnapShotSettings.DefaultConcurrentWorkersCustom;

            _refUi.ResetSettingsValues(Settings);
            _refUi.SetStatus(_refUi.SettingsTrigClassCtrl, ORIGINAL, null, true);
        }

        /// <summary>
        /// Démarre le bassin de travailleur personnalisé, s'il n'est pas déjà démarré.
        /// </summary>
        public void StartWorkerPool()
        {
            if (_poolWorker != null) return;
            if (string.IsNullOrEmpty(Settings.CustomWorker)) return;
            _poolWorker = new CustomWorkerPool<SnapShotCustomWorkerParameter, FileInfo> {ActiveLog = Settings.PoolVerboseLog};
            _poolWorker.WorkerFinished += CustomWorkerFinished;
            _poolWorker.MaxWorkers = Settings.ConcurrentWorkersCustom;
            CustomWorkerPool<SnapShotCustomWorkerParameter, FileInfo>.StartPoolInstance(_poolWorker, "Worker Pool");
        }

        /// <summary>
        /// Initialise l'état du mécanisme du déclencheur. Selon le résultat de l'initialisation, cela
        /// va aussi possiblement modifier l'état du bouton de test.
        /// </summary>
        private void InitTriggerClass()
        {

            Type trigger = null;
            if (_triggerAbstractMechanism != null)
            {
                trigger = _triggerAbstractMechanism.GetType();
            }

            if (!string.IsNullOrEmpty(Settings.TriggerClassName))
            {
                var test = Type.GetType(Settings.TriggerClassName);

                if (test != null && test == trigger)
                {
                    if (!_triggerAbstractMechanism.IsRunning())
                    {
                        try
                        {
                            _triggerAbstractMechanism.Start();
                            _refUi.SetStatus(_refUi.TrigStatusCtrl, READY, "Trigger ready", true);
                            _refUi.SetStatus(_refUi.TriggerTestBtnCtrl, ORIGINAL, null, true);
                        }
                        catch (Exception)
                        {
                            _refUi.LogEvent("Could not start trigger instance.");
                            _refUi.SetStatus(_refUi.TrigStatusCtrl, NOT_READY, "Could not start.", true);
                            _refUi.SetStatus(_refUi.TriggerTestBtnCtrl, ORIGINAL, null, false);
                        }
                    }
                    else
                    {
                        var initSuccess = _triggerAbstractMechanism.Init(Settings.TriggerParameters);
                        _refUi.LogEvent(initSuccess
                            ? "Trigger mechanism is already started. Reinitialised."
                            : "Trigger mechanism is already started. Reinitialization failed..");
                    }
                }
                else
                {
                    if (test != null)
                    {
                        if (!VerifyTriggerClass(Settings.TriggerClassName))
                        {
                            if (trigger != null)
                            {
                                _refUi.LogEvent("Unknown new trigger class. Will keep last trigger mechanism.");
                            }
                            else
                            {
                                _refUi.SetStatus(_refUi.TrigStatusCtrl, NOT_READY, "Could not find trigger class.", true);
                                _refUi.SetStatus(_refUi.SettingsTrigClassCtrl, NOT_READY, null, true);
                            }
                        }
                        else
                        {
                            TriggerAbstract tmp;

                            try
                            {
                                tmp = (TriggerAbstract)Activator.CreateInstance(test);
                            }
                            catch (Exception)
                            {
                                _refUi.LogEvent("Could not instanciate the class " + Settings.TriggerClassName);
                                tmp = null;
                            }

                            if (tmp != null)
                            {
                                var success = false;

                                try
                                {
                                    success = tmp.Init(Settings.TriggerParameters);
                                }
                                catch (Exception)
                                {
                                    _refUi.LogEvent("Error while initializing instance with the following parameters: " + StringUtil.MergeString(Settings.TriggerParameters, ","));
                                    tmp.Dispose();
                                    tmp = null;
                                }

                                if (!success)
                                {
                                    _refUi.LogEvent("Instance failed to initialize with following parameters: " + StringUtil.MergeString(Settings.TriggerParameters, ","));
                                    _refUi.LogEvent("Is the trigger mechanism connected?");
                                    if (tmp != null)
                                    {
                                        tmp.Dispose();
                                        tmp = null;
                                    }
                                }
                            }

                            if (tmp != null && _triggerAbstractMechanism != null)
                            {
                                try
                                {
                                    _triggerAbstractMechanism.Triggered -= TriggerAbstractActivated;
                                    _triggerAbstractMechanism.Stop();
                                    _triggerAbstractMechanism.Dispose();
                                    _refUi.LogEvent("Former Trigger class unloaded: " + trigger);
                                }
                                catch (Exception e)
                                {
                                    _refUi.LogEvent("Error while trying to stop former trigger mechanism (" + trigger + "): "
                                        + e.Message + "\r\n" + e.StackTrace);
                                }
                                finally
                                {
                                    _triggerAbstractMechanism = null;
                                }
                            }

                            if (tmp != null)
                            {

                                _triggerAbstractMechanism = tmp;

                                try
                                {
                                    _triggerAbstractMechanism.Triggered += TriggerAbstractActivated;
                                    _triggerAbstractMechanism.Start();
                                    _refUi.SetStatus(_refUi.TrigStatusCtrl, READY, "Waiting for trigger.", true);
                                    _refUi.SetStatus(_refUi.SettingsTrigClassCtrl, ORIGINAL, null, true);
                                }
                                catch (Exception e)
                                {
                                    _refUi.LogEvent("Could not start the new trigger mechanism (" + test + "): "
                                        + e.Message + "\r\n" + e.StackTrace);
                                    _triggerAbstractMechanism = null;
                                }
                            }

                            // Si après tout ce bazar on a un triggerMechanism nul, alors on affiche une erreur pour le status.
                            if (_triggerAbstractMechanism == null)
                            {
                                _refUi.SetStatus(_refUi.TrigStatusCtrl, NOT_READY, "Problem encountered.", true);
                                _refUi.SetStatus(_refUi.TriggerTestBtnCtrl, ORIGINAL, null, false);
                            }
                            else
                            {
                                _refUi.SetStatus(_refUi.TriggerTestBtnCtrl, ORIGINAL, null, true);
                            }
                        }
                    }
                    else
                    {
                        _refUi.LogEvent("Could not find the trigger class. Please change the trigger class or remove it.");
                        _refUi.SetStatus(_refUi.TrigStatusCtrl, NOT_READY, "Trigger class not found.", true);
                        _refUi.SetStatus(_refUi.SettingsTrigClassCtrl, NOT_READY, null, true);
                        _refUi.SetStatus(_refUi.TriggerTestBtnCtrl, ORIGINAL, null, false);
                    }
                }
            }
            else
            {
                _refUi.SetStatus(_refUi.TrigStatusCtrl, NOT_READY, "No trigger configured.", true);
                _refUi.SetStatus(_refUi.TriggerTestBtnCtrl, ORIGINAL, null, false);
            }
        }

        /// <summary>
        /// Envoi manuellement une événement disant que le trigger s'est déclenché, sur un fil séparé.
        /// </summary>
        public void SendTriggerEvent()
        {
            var t = new Task(() => TriggerAbstractActivated(null, null));
            t.Start();
        }

        /// <summary>
        /// On passe par cette méthode pour indiquer que le déclencheur s'est activé.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TriggerAbstractActivated(object sender, EventArgs e)
        {
            if (TriggerTestMode)
            {
                _refUi.LogEvent("(Trigger test) Trigger fired!");
                return;
            }

            lock (_locker)
            {
                if (_state != SnapShotState.Blocked)
                {
                    if (InActivationFrame())
                    {
                        if (_eosCamera != null && _state == SnapShotState.Ready)
                        {
                            if (Settings.IsCameraPicture)
                            {
                                if (Settings.PoolVerboseLog) _refUi.LogEvent("Sent TAKE PICTURE command to camera.");
                                try
                                {
                                    _eosCamera.TakePicture();
                                    SetSnapShotState(SnapShotState.Stopped);
                                }
                                catch (Exception err)
                                {
                                    LogEvent("Could not send command to camera.", err);
                                }
                                
                            }
                            else
                            {
                                if (Settings.VideoDuration > 0)
                                {
                                    var tmp = new Task(StartVideo);
                                    tmp.Start();
                                }
                                else
                                {
                                    _refUi.LogEvent("Cannot start video record. Duration is below 1 second.");
                                }
                            }

                        }
                        else
                        {
                            if (Settings.PoolVerboseLog) _refUi.LogEvent("Camera is not ready. Current state is " + _state);
                        }
                    }
                }
                else
                {
                    if (Settings.PoolVerboseLog) _refUi.LogEvent("Cannot send new tasks for camera while processing leftover media files.");
                }
            }
        }

        /// <summary>
        /// Vérifie que le moment présent est à l'intérieur du temps d'activation valide.
        /// </summary>
        /// <returns>Vrai si l'on n'est pas à l'extérieur du temps permis.</returns>
        private bool InActivationFrame()
        {
            if (_activationWindow == null)
            {
                return true;
            }

            var hour = DateTime.Now.Hour;
            var minute = DateTime.Now.Minute;

            // Si plus petit que temps d'activation minimal.
            if (hour < _activationWindow[0].Hours || (hour == _activationWindow[0].Hours && minute < _activationWindow[0].Minutes))
            {
                _refUi.LogEvent("Rejected trigger event because of time window: " + Settings.TimeTriggerWindow + " now: " + hour + ":" + minute);
                return false;
            }
            if (hour > _activationWindow[1].Hours || (hour == _activationWindow[1].Hours && minute > _activationWindow[1].Minutes))
            {
                _refUi.LogEvent("Rejected trigger event because of time window: " + Settings.TimeTriggerWindow + " now: " + hour + ":" + minute);
                return false;
            }

            return true;
        }

        private bool VerifyTriggerClass(string className)
        {
            return VerifyClass(className,
                "Nomadlogic.iBrand.SnapShot.Trigger.Impl",
                "Invalid Trigger class name. Must use one of the following: ",
                typeof(TriggerAbstract));
        }

        public bool ValidateWorkerClassName(string tmpWorkerClass)
        {
            var errorOccuredWorker = false;
            if (string.IsNullOrEmpty(tmpWorkerClass))
            {
                Settings.CustomWorker = null;
            }
            else
            {
                if (!VerifyWorkerClass(tmpWorkerClass))
                {
                    errorOccuredWorker = true;
                    _refUi.SetStatus(_refUi.SettingsWorkerClassCtrl, NOT_READY, null, true);
                }
                else
                {
                    _refUi.SetStatus(_refUi.SettingsWorkerClassCtrl, ORIGINAL, null, true);
                    Settings.CustomWorker = tmpWorkerClass;
                }
            }

            return errorOccuredWorker;
        }

        public bool ValidateTriggerClassName(string tmpTrigClass)
        {
            var errorOccured = false;

            try
            {
                if (string.IsNullOrEmpty(tmpTrigClass))
                {
                    InitTriggerClass();
                    Settings.TriggerClassName = "";
                }
                else
                {
                    var inTheList = VerifyTriggerClass(tmpTrigClass);
                    if (inTheList)
                    {
                        // On tente de charger la classe immédiatement.
                        var oldClassName = Settings.TriggerClassName;
                        Settings.TriggerClassName = tmpTrigClass;

                        try
                        {
                            InitTriggerClass();
                        }
                        catch (Exception)
                        {
                            // Rien
                        }

                        if (_triggerAbstractMechanism == null)
                        {
                            // Il y a eu une erreur.
                            Settings.TriggerClassName = oldClassName;
                            errorOccured = true;
                        }
                    }
                    else
                    {
                        errorOccured = true;
                    }
                }
            }
            catch (Exception)
            {
                errorOccured = true;
            }

            _refUi.SetStatus(_refUi.SettingsTrigClassCtrl, errorOccured ? NOT_READY : ORIGINAL, null, true);

            return errorOccured;
        }



        private bool VerifyClass(string className, string nameSpace, string message, Type baseAssign)
        {
            var retour = false;
            // Vérification que la classe de trigger est dans la liste valide.
            var theList = Assembly.GetExecutingAssembly().GetTypes().ToList().Where(t => t.Namespace == nameSpace).ToList();

            foreach (var type in theList)
            {
                if (type.FullName.Equals(className) && baseAssign.IsAssignableFrom(type)) // Type valide et faisant partie du groupe.
                {
                    retour = true;
                    break;
                }
            }

            if (!retour)
            {
                _refUi.LogEvent(message);
                _refUi.LogEvent("- (None)");
                foreach (var type in theList)
                {
                    _refUi.LogEvent("- " + type.FullName);
                }
            }

            return retour;
        }

        private bool VerifyWorkerClass(string className)
        {
            return VerifyClass(className,
                "Nomadlogic.iBrand.SnapShot.CustomWorker.Impl",
                "Invalid Custom Worker class name. Must use one of the following: ",
                typeof(CustomWorker<SnapShotCustomWorkerParameter, FileInfo>));
        }

        /// <summary>
        /// Initialise le pool FTP au démarrage.
        /// </summary>
        private void InitFtpPool()
        {
            if (Settings.UseFtp)
            {
                _refUi.SetStatus(_refUi.FtpTestBtnCtrl, ORIGINAL, "Initializing", false);
                _refUi.SetStatus(_refUi.ProcessFilesBtnCtrl, ORIGINAL, "Waiting for Ftp pool", false);

                var tmp = new Task(() =>
                {
                    TestPoolFtp();
                    _poolFtp.ActiveLog = Settings.PoolVerboseLog;
                    _poolWorker.ActiveLog = Settings.PoolVerboseLog;
                    _refUi.SetStatus(_refUi.FtpTestBtnCtrl, ORIGINAL, "Test Ftp", true);
                    _refUi.SetStatus(_refUi.ProcessFilesBtnCtrl, ORIGINAL, "Upload left over files", true);
                });
                tmp.Start();
            }
            else
            {
                _refUi.SetStatus(_refUi.FtpTestBtnCtrl, ORIGINAL, "Not used", false);
                _refUi.SetStatus(_refUi.ProcessFilesBtnCtrl, ORIGINAL, null, true);
            }
        }

        public void RequestTestPoolFtp()
        {
            if (Settings.UseFtp)
            {
                if (_poolFtp != null && _poolFtp.IsRunning())
                {
                    _refUi.LogEvent("Ftp pool test cannot be run on running pool instance. It is already working.");
                }
                else
                {
                    var tmp = new Task(TestPoolFtp);
                    tmp.Start();
                }
            }
        }

        private void TestPoolFtp()
        {
            if (_poolFtp == null)
            {
                _poolFtp = new FtpPool(Settings.FtpServer, Settings.FtpUser, Settings.FtpPassword);
            }
            var res = _poolFtp.TestConnection();

            // Test du répertoire d'archive
            var testDir = Settings.FtpArchive;
            if (!string.IsNullOrEmpty(testDir))
            {
                if (!Directory.Exists(testDir))
                {
                    try
                    {
                        var info = Directory.CreateDirectory(testDir);
                        _refUi.LogEvent("Created archive directory: " + info.FullName);
                    }
                    catch (Exception e)
                    {
                        _refUi.LogEvent("Error while creating archive direcory: " + e.Message + "\r\n" + e.StackTrace);
                    }
                }
            }

            if (res == null)
            {
                _poolFtp.WorkerFinished += TranfertFinished;
                FtpPool.StartPoolInstance(_poolFtp, "Ftp Pool");
                _refUi.SetStatus(_refUi.ProcessFilesBtnCtrl, ORIGINAL, null, true);
            }
            else
            {
                if (!string.IsNullOrEmpty(Settings.BackupFtpServer))
                {
                    var oldRes = res;
                    // On essaie une deuxième connexion avec un secour de secour.
                    _poolFtp = new FtpPool(Settings.BackupFtpServer, Settings.BackupFtpUser, Settings.BackupFtpPassword);
                    res = _poolFtp.TestConnection();
                    if (res == null)
                    {
                        _poolFtp.WorkerFinished += TranfertFinished;
                        FtpPool.StartPoolInstance(_poolFtp, "Ftp Pool");
                        _refUi.SetStatus(_refUi.ProcessFilesBtnCtrl, ORIGINAL, null, true);
                        _refUi.LogEvent("Backup Ftp pool was started instead of principal Ftp. Reason for failure: " + oldRes);
                    }
                    else
                    {
                        _refUi.LogEvent("Could not start both principal and backup Ftp pool.");
                        _refUi.LogEvent("Primary Ftp error: " + oldRes);
                        _refUi.LogEvent("Secondary Ftp error: " + res);
                        _refUi.SetStatus(_refUi.FtpStatusCtrl, NOT_READY, "Could not start Ftp pool.", true);
                    }
                }
                else
                {
                    _refUi.LogEvent("Could not start Ftp pool. " + res);
                    _refUi.LogEvent("No backup Ftp pool set up.");
                    _refUi.SetStatus(_refUi.FtpStatusCtrl, NOT_READY, "Could not start Ftp pool.", true);
                }
            }
        }

        public void TranfertFinished(bool success, string[] fileName)
        {
            if (!success)
            {
                _refUi.LogEvent("Error during file transfert: " + fileName);
            }
        }

        private void SendToFtp(string[] files, string[] subDir)
        {
            var prefix = "";
            if (!string.IsNullOrEmpty(Settings.FtpDirectory))
            {
                prefix = Settings.FtpDirectory + "/";
            }
            for (var i = 0; i < subDir.Length; i++)
            {
                subDir[i] = prefix + subDir[i];
            }

            _poolFtp.AddFtpTransfer(subDir, files, _workerNbr++, Settings.FtpArchive);
        }

        private void CustomWorkerFinished(bool success, FileInfo result)
        {
            // Une fois que l'on a terminé le traitement d'un fichier, on l'envoi sur le FTP, au besoin.
            if (Settings.UseFtp && _poolFtp != null && result != null)
            {
                string[] files = { result.FullName };
                var subDirs = new string[1];

                if (result.FullName.EndsWith(pictureFileType))
                {
                    subDirs[0] = ftpPicturesSubdir;
                }
                else
                {
                    subDirs[0] = ftpVideosSubdir;
                }

                _poolFtp.AddFtpTransfer(subDirs, files, _workerNbr++, Settings.FtpArchive);
            }
        }

        /// <summary>
        /// Lit et initialise la classe qui va gérer la connexion et les événements liés au caméras.
        /// </summary>
        public void InitCameraFrameWork()
        {
            if (_eosManager == null)
            {
                // todo Ajouter éventuellement la possibilité de gérer des composantes Nikon.
                SafeCall(() =>
                {
                    _eosManager = new EosFrameworkManager();
                    _eosManager.CameraAdded += OnCameraAdded;
                    _eosManager.LoadFramework();
                }, ex =>
                {
                    RemoveCameraFramework();
                    _refUi.LogEvent("Could not load EOS Framework. Camera usage impossible.");
                });
            }

            var t = new Task(LoadCamera);
            t.Start();

            // Test des répertoires destinations des fichiers de la caméra.
            var dir = Environment.CurrentDirectory;
            if (!Directory.Exists(dir + PICTURE_DIR))
            {
                var di = Directory.CreateDirectory(dir + PICTURE_DIR);

                if (di.Exists)
                {
                    _refUi.LogEvent("Created pictures directory.");
                }
                else
                {
                    _refUi.LogEvent("Could not create pictures directory.");
                    _refUi.SetStatus(_refUi.CamTestBtnCtrl, ORIGINAL, null, false);
                    _refUi.SetStatus(_refUi.CamActivateBtnCtrl, ORIGINAL, null, false);
                }
            }
            if (!Directory.Exists(dir + VIDEO_DIR))
            {
                var di = Directory.CreateDirectory(dir + VIDEO_DIR);

                if (di.Exists)
                {
                    _refUi.LogEvent("Created videos directory.");
                }
                else
                {
                    _refUi.LogEvent("Could not create videos directory.");
                    _refUi.SetStatus(_refUi.CamTestBtnCtrl, ORIGINAL, null, false);
                    _refUi.SetStatus(_refUi.CamActivateBtnCtrl, ORIGINAL, null, false);
                }
            }
        }

        /// <summary>
        /// Indique si l'initialisation d'une caméra est faite et qu'une caméra est effectivement prête.
        /// </summary>
        public bool IsCameraInitialized => _eosCamera != null;

        /// <summary>
        /// Indique si la classe de Trigger est instanciée et prête à l'utilisation.
        /// </summary>
        public bool IsTriggerInitialized => _triggerAbstractMechanism != null;

        private void SafeCall(Action action, Action<Exception> exceptionHandler)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
               log.Error("Could not complete call: " + ex.StackTrace);
                exceptionHandler(ex);
            }
        }

        /// <summary>
        /// Enlève la caméra et le framework caméra.
        /// </summary>
        public void RemoveCameraFramework()
        {
            if (_eosManager != null)
            {
                SetSnapShotState(SnapShotState.Absent);

                if (_eosCamera != null)
                {
                    _eosCamera.FileCreatedOnCamera -= OnFileCreatedOnCamera;
                    _eosCamera.FileSavedOnHost -= OnFileSavedOnHost;
                    _eosCamera.Dispose();
                    _refUi.SetStatus(_refUi.CamActivateBtnCtrl, ORIGINAL, null, false);
                    _eosCamera = null;
                }

                _eosManager.ReleaseFramework();
                _eosManager = null;
                _refUi.SetStatus(_refUi.CamStatusCtrl, NOT_READY, null, true);
            }
        }

        /// <summary>
        /// Méthode déléguée appellée quand la caméra est ajoutée au système.
        /// </summary>
        /// <param name="sender">L'origine de l'événement</param>
        /// <param name="e">Les paramètres de l'événement.</param>
        private void OnCameraAdded(object sender, EventArgs e)
        {
            if (_eosCamera != null) return;
            var k = new Task(LoadCamera);
            k.Start();
        }

        /// <summary>
        /// Charge la première caméra Canon détectée pour être liée à cette instance.
        /// </summary>
        private void LoadCamera()
        {
            lock (_locker)
            {
                if (_eosCamera == null)
                {
                    IEnumerable<EosCamera> cameras = _eosManager.GetCameras();

                    if (cameras.Any())
                    {
                        foreach (var camera in cameras)
                        {
                            camera.FileCreatedOnCamera += OnFileCreatedOnCamera;
                            camera.FileSavedOnHost += OnFileSavedOnHost;
                            camera.Shutdown += OnCameraShutDown;
                            camera.Error += OnErrorOnCamera;
                            _eosCamera = camera;
                            _refUi.SetStatus(_refUi.CamActivateBtnCtrl, ORIGINAL, null, true);

                            if (_state != SnapShotState.ErrorOccured)
                            {
                                SetSnapShotState(SnapShotState.Ready);
                                _refUi.LogEvent("Camera connected. SN = " + camera.SerialNumber);
                            }

                            break;//On connecte seulememnt la premiere camera
                        }
                    }
                    else
                    {
                        _refUi.SetStatus(_refUi.CamStatusCtrl, NOT_READY, "No camera connected", true);
                    }
                }
            }
        }

        /// <summary>
        /// Quand la caméra n'est plus accessible du système, on ferme les instances associées.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="args">Les paramètres de l'événement.</param>
        public void OnCameraShutDown(object sender, EventArgs args)
        {
            if (_eosCamera != null)
            {
                if (_state != SnapShotState.ErrorOccured)
                {
                    SetSnapShotState(SnapShotState.Absent);
                    _refUi.LogEvent("Camera disconnected");
                }

                _eosCamera.FileCreatedOnCamera -= OnFileCreatedOnCamera;
                _eosCamera.FileSavedOnHost -= OnFileSavedOnHost;
                _eosCamera.Error -= OnErrorOnCamera;
                _eosCamera.Dispose();
                _eosCamera = null;
                _refUi.SetStatus(_refUi.CamActivateBtnCtrl, ORIGINAL, null, false);
            }
        }

        /// <summary>
        /// Quand on détecte un fichier créé sur la caméra, on tente de le télécharger dans les répertoires de travail.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="dirItem">L'identifiant du fichier sur la caméra.</param>
        /// <param name="camera">L'instance de la caméra.</param>
        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (Settings.PoolVerboseLog) _refUi.LogEvent("Sent SAVE FILE command to camera.");
            SetSnapShotState(SnapShotState.Transfering);

            //                Task t = new Task(new Action(() =>
            //                {
            var dir = Environment.CurrentDirectory;
            string extension;

            if (Settings.IsCameraPicture)
            {
                dir += PICTURE_DIR;
                extension = pictureFileType;
            }
            else
            {
                dir += VIDEO_DIR;
                extension = cameraVideoFileType;
            }

            var fileName = dir + GenerateUniqueId.GetId() + extension;

            if (File.Exists(fileName))
                File.Delete(fileName);

            if (camera != null)
            {
                try
                {
                    camera.SavePicture(dirItem, fileName);
                }
                catch (Exception e)
                {
                    _refUi.LogEvent("Error occured while saving picture to hard disk.");
                    log.Error(e);
                    SetSnapShotState(_eosCamera != null ? SnapShotState.Ready : SnapShotState.Absent);
                }
            }
            //                }));
            //                t.Start();
        }

        /// <summary>
        /// Dans le cas d'une erreur détectée sur la caméra, on journalise et on tente de traiter au mieux.
        /// </summary>
        /// <param name="sender">L'envoyeur.</param>
        /// <param name="e">L'argument d'événement.</param>
        private void OnErrorOnCamera(object sender, EosExceptionEventArgs e)
        {
            _refUi.LogEvent("Error message sent from camera: " + e.Exception.Message);
            log.Error(e);
        }

        /// <summary>
        /// Quand le fichier multimédia est sauvegardé sur la machine locale.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="e">Les paramètres du fichier.</param>
        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            var path = e.FilePath;
            _refUi.LogEvent("New file saved from camera: " + StringUtil.GetFileName(path));
            SetSnapShotState(SnapShotState.Ready);

            // On met dans un Task pour garder le UI rapide.
            var tmp = new Task(() =>
            {
                // Si on a une action custom, on exécute celle-ci et rien d'autre.
                if (!string.IsNullOrEmpty(Settings.CustomWorker))
                {
                    var p = new SnapShotCustomWorkerParameter
                    {
                        Settings = Settings,
                        WorkFile = new FileInfo(path)
                    };
                    _poolWorker.AddCustomWorker(_workerNbr++, Type.GetType(Settings.CustomWorker), p, this);
                }
                else
                {
                    if (path.EndsWith(cameraVideoFileType))
                    {
                        // Si c'est un vidéo, on le transforme en AVI et on créé un thumbnail.

                        var videoFile = path.Substring(0, path.Length - cameraVideoFileType.Length) + videoFileType;
                        FfmpegUtil.MovToAvi(path, videoFile, Settings.LocationFfmpeg, 1280, 720, Settings.VideoFileSize);
                        if (File.Exists(videoFile))
                        {
                            File.Delete(path);
                        }
                        var thumbFile = videoFile + pictureFileType;
                        CreateThumbnail(videoFile);

                        // Si on utilise le FTP, on l'envoi vers le FTP.
                        if (Settings.UseFtp)
                        {
                            if (File.Exists(videoFile) && File.Exists(thumbFile))
                            {
                                string[] files = { videoFile, thumbFile };
                                string[] subDirs = { ftpVideosSubdir, ftpThumbnailsSubdir };
                                SendToFtp(files, subDirs);
                            }
                            else
                            {
                                _refUi.LogEvent("Could not sent video file because thumbnail was not created: " + videoFile);
                            }
                        }
                    }
                    else
                    {
                        // Si on utilise le FTP, on l'envoi vers le FTP.
                        if (Settings.UseFtp)
                        {
                            // On envoi via le FTP
                            string[] files = { path };
                            string[] subDirs = { ftpPicturesSubdir };
                            SendToFtp(files, subDirs);
                        }
                    }
                }
            });
            tmp.Start();
        }

        /// <summary>
        /// Interprète et assigne la valeur des fenêtres d'activation via le paramètre.
        /// <br/>Si le paramètre est null ou vide, alors cela indique aucune fenêtre d'activation. Sinon, on tente de le lire
        /// et d'interpréter ces valeurs. Si on en est incapable, on retourne faux (erreur) et on laisse la dernière
        /// configuration en place. Si on peut interpréter les valeurs, alors on retourne vrai et on modifie les
        /// fenêtres de temps actives.
        /// </summary>
        /// <param name="timeRestriction">Une chaîne avec comme format "HH:mm-HH:mm" pour début-fin.</param>
        /// <returns></returns>
        public bool ValidateTriggerTime(string timeRestriction)
        {

            if (string.IsNullOrEmpty(timeRestriction))
            {
                _activationWindow = null;
                return true;
            }
            try
            {
                var res = timeRestriction.Split('-');
                var nbr = new int[2];

                if (res.Length != 2)
                {
                    _refUi.SetStatus(_refUi.SettingsTriggerTimeCtrl, NOT_READY, null, true);
                    _refUi.LogEvent("Could not parse activation window parameters: " + timeRestriction);
                    return false;
                }

                var parsed = new TimeSpan[2];

                for (var i = 0; i < 2; i++)
                {
                    var sp = res[i].Split(':');
                    nbr[0] = int.Parse(sp[0]);
                    nbr[1] = int.Parse(sp[1]);

                    if (nbr[0] < 0 || nbr[0] > 24)
                    {
                        _refUi.SetStatus(_refUi.SettingsTriggerTimeCtrl, NOT_READY, null, true);
                        _refUi.LogEvent("Invalid time. Hours between 0 and 24 (both inclusive): " + timeRestriction);
                        return false;
                    }
                    if (nbr[1] < 0 || nbr[1] > 59)
                    {
                        _refUi.SetStatus(_refUi.SettingsTriggerTimeCtrl, NOT_READY, null, true);
                        _refUi.LogEvent("Invalid time.Minutes between 0 and 59 (both inclusive): " + timeRestriction);
                        return false;
                    }

                    parsed[i] = new TimeSpan(nbr[0], nbr[1], 0);
                }

                // On vérifie que le début est plus petit que la fin.
                if (parsed[0].CompareTo(parsed[1]) >= 0)
                {
                    _refUi.SetStatus(_refUi.SettingsTriggerTimeCtrl, NOT_READY, null, true);
                    _refUi.LogEvent("Lower bound is not lower than upper bound for time activation: " + timeRestriction);
                    return false;
                }

                _activationWindow = parsed;
                Settings.TimeTriggerWindow = timeRestriction;
                _refUi.SetStatus(_refUi.SettingsTriggerTimeCtrl, ORIGINAL, null, true);
                return true;
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
                _refUi.SetStatus(_refUi.SettingsTriggerTimeCtrl, NOT_READY, null, true);
                _refUi.LogEvent("Could not parse activation window parameters: " + timeRestriction);
            }

            return false;
        }

        /// <summary>
        /// À appeller si on modifie la classe de configuration.
        /// </summary>
        public void UpdatePoolConfigs()
        {
            if (_poolFtp != null)
            {
                _poolFtp.ActiveLog = Settings.PoolVerboseLog;
                _poolFtp.MaxWorkers = Settings.ConcurrentWorkersFtp;
            }
            if (_poolWorker != null)
            {
                _poolWorker.ActiveLog = Settings.PoolVerboseLog;
                _poolWorker.MaxWorkers = Settings.ConcurrentWorkersCustom;
            }
        }

        /// <summary>
        /// Sauvegarde le fichier sur disque.
        /// </summary>
        public void SaveSettingsToFile()
        {
            using (var file = File.CreateText(settingsFile))
            {
                _serializer.Serialize(file, Settings);
                _refUi.LogEvent("Settings saved to file.");
            }
        }

        public void ProcessLeftoverFiles()
        {
            // Uniquement permis si la caméra ne fait rien en ce moment (ou si elle est absente).
            if (_state == SnapShotState.Absent || _state == SnapShotState.Ready)
            {

                var tmp = new Task(() =>
                {
                    var prefix = "";
                    if (!string.IsNullOrEmpty(Settings.FtpDirectory))
                    {
                        prefix = Settings.FtpDirectory + "/";
                    }
                    string[] pictureDir = { prefix + ftpPicturesSubdir };
                    string[] videoDir = { prefix + ftpVideosSubdir, prefix + ftpThumbnailsSubdir };

                    _refUi.SetStatus(_refUi.ProcessFilesBtnCtrl, ORIGINAL, null, false);
                    SetSnapShotState(SnapShotState.Blocked);

                    var picInfo = new DirectoryInfo(Environment.CurrentDirectory + PICTURE_DIR);
                    var vidInfo = new DirectoryInfo(Environment.CurrentDirectory + VIDEO_DIR);

                    IOrderedEnumerable<FileInfo> picListe = null;
                    IOrderedEnumerable<FileInfo> vidListe = null;

                    if (picInfo.Exists)
                    {
                        var picTab = picInfo.GetFiles("*" + pictureFileType);
                        picListe = picTab.OrderByDescending(f => f.CreationTime);
                    }
                    if (vidInfo.Exists)
                    {
                        var vidTab = vidInfo.GetFiles("*" + cameraVideoFileType);
                        vidListe = vidTab.OrderByDescending(f => f.CreationTime);
                    }

                    var nbrFileSent = 0;

                    if (picListe != null)
                    {
                        foreach (var fi in picListe)
                        {
                            if (fi.Exists)
                            {
                                if (!string.IsNullOrEmpty(Settings.CustomWorker))
                                {
                                    var sscwp = new SnapShotCustomWorkerParameter
                                    {
                                        Settings = Settings,
                                        WorkFile = fi
                                    };
                                    var clazz = Type.GetType(Settings.CustomWorker);
                                    _poolWorker.AddCustomWorker(_workerNbr++, clazz, sscwp, this);
                                    nbrFileSent++;
                                }
                                else if (Settings.UseFtp && _poolFtp != null && _poolFtp.IsRunning())
                                {
                                    string[] files = { fi.FullName };
                                    _poolFtp.AddFtpTransfer(pictureDir, files, _workerNbr++, Settings.FtpArchive);
                                    nbrFileSent++;
                                }
                            }
                        }
                    }

                    if (vidListe != null)
                    {
                        foreach (var fi in vidListe)
                        {
                            if (fi.Exists)
                            {
                                if (!string.IsNullOrEmpty(Settings.CustomWorker))
                                {
                                    var sscwp = new SnapShotCustomWorkerParameter
                                    {
                                        Settings = Settings,
                                        WorkFile = fi
                                    };
                                    var clazz = Type.GetType(Settings.CustomWorker);
                                    _poolWorker.AddCustomWorker(_workerNbr++, clazz, sscwp, this);
                                    nbrFileSent++;
                                }
                                else if (Settings.UseFtp && _poolFtp != null && _poolFtp.IsRunning())
                                {
                                    // On vérifie si un thumbnail a été créé. Sinon, on tente de le créer.
                                    if (!File.Exists(fi.FullName + pictureFileType))
                                    {
                                        CreateThumbnail(fi.FullName);
                                    }

                                    if (File.Exists(fi.FullName + pictureFileType))
                                    {
                                        string[] files = { fi.FullName, fi.FullName + pictureFileType };
                                        _poolFtp.AddFtpTransfer(videoDir, files, _workerNbr++, Settings.FtpArchive);
                                        nbrFileSent += 2;
                                    }
                                    else
                                    {
                                        _refUi.LogEvent("Could not create thumbnail for file: " + fi.FullName);
                                    }
                                }
                            }
                        }
                    }

                    _refUi.LogEvent("Sent " + nbrFileSent + " files to be treated. Will wait until both pools are empty.");

                    /* On attend maintenant que le bassin d'envoi se vide. */
                    while ((_poolFtp != null && _poolFtp.GetQueueSize() > 0) || _poolWorker.GetQueueSize() > 0)
                    {
                        Thread.Sleep(1000);
                        Thread.Yield();
                    }

                    SetSnapShotState(_eosCamera != null ? SnapShotState.Ready : SnapShotState.Absent);
                    _refUi.SetStatus(_refUi.ProcessFilesBtnCtrl, ORIGINAL, null, true);
                    _refUi.LogEvent("Finished uploading leftover media files.");
                });

                tmp.Start();
            }
        }

        public void LogEvent(string message)
        {
            _refUi.LogEvent(message);
        }

        public void LogEvent(string message, Exception e)
        {
            _refUi.LogEvent(message, e);
        }
    }
}
