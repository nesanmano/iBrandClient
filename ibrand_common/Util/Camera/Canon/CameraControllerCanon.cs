using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util.Camera.Canon
{
    /// <summary>
    /// Implémentation d'un contrôleur pour la gestion des caméras du manufacturier Canon.
    /// </summary>
    [PublicAPI]
    public class CameraControllerCanon : ICameraController<CameraConfigCanon>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static CameraControllerCanon _instance;
        private EosFrameworkManager _manager;
        private readonly List<EosCamera> _cameras = new List<EosCamera>();
        private CameraControllerStateEnum _currentState = CameraControllerStateEnum.Undefined;
        private readonly ISet<ILiveViewListener> _liveViewListeners = new HashSet<ILiveViewListener>();
        private readonly ISet<IPictureListener> _picturesListeners = new HashSet<IPictureListener>();
        private readonly ISet<IVideoListener> _videosListeners = new HashSet<IVideoListener>();
        private RequestExecutorPicturesCanon _currentPictureRequest;
        private RequestExecutorVideoCanon _currentVideoRequest;
        private DirectoryInfo _pictureSaveDirectory = new DirectoryInfo(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures));

        public delegate void NewCameraDetected(EosCamera camera);
        public event NewCameraDetected NewCameraEvent;

        private CameraControllerCanon()
        {
            // Singleton
        }

        /// <summary>
        /// Retourne la référence vers l'unique instance de cette classe.
        /// Est automatiquement initialisé avec comme répertoire de travail le "local media directory" de l'application en cours.
        /// On peut changer ce répertoire n'importe quand.
        /// </summary>
        /// <returns>L'unique instance de ce gestionnaire.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static CameraControllerCanon GetInstance()
        {
            return _instance ?? (_instance = new CameraControllerCanon
            {
                PictureSaveDirectory = new DirectoryInfo(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures))
            });
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (_manager == null) return;
            log.Trace("Disposing of Canon controller.");

            _currentPictureRequest?.CancelRequest();

            foreach (var listener in _liveViewListeners)
            {
                listener.OnForcedUnregister();
            }
            _liveViewListeners.Clear();

            foreach (var listener in _picturesListeners)
            {
                listener.OnForcedUnregister();
            }
            _picturesListeners.Clear();

            foreach (var listener in _videosListeners)
            {
                listener.OnForcedUnregister();
            }
            _videosListeners.Clear();

            foreach (var camera in _cameras)
            {
                if (camera.IsInLiveViewMode) camera.StopLiveView();
                camera.LiveViewUpdate -= HandlePictureUpdate;
                camera.Shutdown -= OnCameraShutDown;
                camera.FileCreatedOnCamera -= OnFileCreatedOnCamera;
                camera.FileSavedOnHost -= OnFileSavedOnHost;
                log.Trace("Camera disposed: author=" + camera.Author);
                camera.Dispose();
            }

            _manager.CameraAdded -= HandleCameraAdded;
            _manager.ReleaseFramework();
            _manager = null;
            log.Trace("Controller destroyed.");
            _currentState = CameraControllerStateEnum.Disposed;
            GC.Collect();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Init()
        {
            if (_manager != null) return true;

            try
            {
                _manager = new EosFrameworkManager();
                _manager.CameraAdded += HandleCameraAdded;
                _manager.LoadFramework();
                log.Trace("Canon Framework loaded. Initialisation will continue.");

                var cameras = _manager.GetCameras();
                
                foreach (var camera in cameras)
                {
                    camera.LiveViewUpdate += HandlePictureUpdate;
                    camera.FileCreatedOnCamera += OnFileCreatedOnCamera;
                    camera.FileSavedOnHost += OnFileSavedOnHost;
                    camera.Shutdown += OnCameraShutDown;
                    camera.IsErrorTolerantMode = true;
                    _cameras.Add(camera);
                    log.Trace("Camera has been added: Author={0}", camera.Author);
                }             

            }
            catch (Exception e)
            {
                log.Error(e, "Could not initialize Camera Controller for Canon Cameras.");
                Dispose();
                _currentState = CameraControllerStateEnum.ErrorInitialization;
                return false;
            }

            _currentState = CameraControllerStateEnum.Initialized;
            log.Trace("Canon Framework Initialisation sequence finished.");
            return true;
        }

        /// <summary>
        /// Retourne un dictionnaire des caméras, indexées par numéros de série.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Dictionary<string, EosCamera> GetCurrentEosCameras()
        {
            return _cameras.ToDictionary(eosCamera => eosCamera.SerialNumber);
        }

        public IEnumerator<EosCamera> EosCameras => _cameras.GetEnumerator();

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void HandlePictureUpdate(object sender, EosLiveImageEventArgs e)
        {
            if (_manager == null) return;
            log.Trace("Live View frame received. Broadcasting to {0} listeners.", _liveViewListeners.Count);

            foreach (var listener in _liveViewListeners)
            {
                listener.OnLiveViewUpdate(e.GetImage());
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (_manager == null) return;
            if (_currentPictureRequest != null) return; // On ne traite pas si on est en train de faire un traitement d'images.
            log.Trace("New picture detected in camera: Author={0}", camera.Author);

            camera.SavePicture(dirItem,
                PictureSaveDirectory.FullName + camera.Author + "_" + camera.SerialNumber
                + '_' + GenerateUniqueId.GetId() + ".jpg");
        }

        /// <summary>
        /// Sauvegarde sur disque une image qui se trouve sur la caméra. Prend uniquement comme source les images qui se trouvent sur la
        ///  carte SD de la caméra et qui ne sont pas gérés par une demande de prise d'image.
        /// Sauvegarde les images dans le répertoire configuré par le gestionnaire, à moins que l'on fournisse une instance de fichier info.
        /// </summary>
        /// <param name="dirItem">L'identifiant de l'image.</param>
        /// <param name="camera">La référence de la caméra.</param>
        /// <param name="forcedFilePath">Si désiré, utilise un chemin de fichier différent que celui par défaut.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveFileOnCamera(IntPtr dirItem, EosCamera camera, FileInfo forcedFilePath = null)
        {
            if (_manager == null) return;
            if (_currentPictureRequest != null) return; // On ne traite pas si on est en train de faire un traitement d'images.
            
            var fileSave = forcedFilePath?.FullName ?? 
                           PictureSaveDirectory.FullName + camera.Author + "_" + camera.SerialNumber + '_' + GenerateUniqueId.GetId() + ".jpg";
            
            log.Trace("Preparing to save file in camera: Author={0}, Path='{1}'", camera.Author, fileSave);
            
            camera.SavePicture(dirItem, fileSave);
        }

        /// <summary>
        /// Répertoire où l'on veut stocker les images prises automatiquement par les caméras. Fonctionne indépendemment des demandes
        /// de prises photo via méthode.
        /// </summary>
        public DirectoryInfo PictureSaveDirectory
        {
            get => _pictureSaveDirectory;

            set { if (value.Exists) _pictureSaveDirectory = value; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (_manager == null) return;
            if (_currentPictureRequest != null) return;

            if (!File.Exists(e.FilePath))
            {
                log.Error("A file referenced by a camera does not exists. Discarding event: " + e.FilePath);
                return;
            }

            var ids = StringUtil.ExtractUniqueIdsFromFileName(StringUtil.GetFileName(e.FilePath));

            foreach (var listener in _picturesListeners)
            {
                listener.PictureTaken(e.FilePath, ids);
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void HandleCameraAdded(object sender, EventArgs e)
        {
            if (_manager == null) return;
            log.Trace("CanonCameraManager - HandleCameraAdded");

            try
            {
                var camera = _manager.GetLastConnectedCamera();

                if (camera == null) return;

                //_cameras.Exists()
                var existingCam = _cameras.FirstOrDefault(c => c.SerialNumber.Equals(camera.SerialNumber, StringComparison.InvariantCultureIgnoreCase));
                if (existingCam != null)
                {
                    RemoveEventHandlersFromCam(existingCam);
                    _cameras.Remove(existingCam);
                }
                
                camera.LiveViewUpdate += HandlePictureUpdate;
                camera.FileCreatedOnCamera += OnFileCreatedOnCamera;
                camera.FileSavedOnHost += OnFileSavedOnHost;
                camera.Shutdown += OnCameraShutDown;
                if (camera.IsInLiveViewMode) camera.StopLiveView();

                _cameras.Add(camera);

                // Si c'est la première caméra et que l'on a déjà des écoutant de live view, on part ce dernier.
                if (_cameras.Count == 1 && _liveViewListeners.Any())
                {
                    _cameras[0].StartLiveView();
                }

                NewCameraEvent?.Invoke(camera);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error during camera added event.");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnCameraShutDown(object sender, EventArgs args)
        {
            if (_manager == null) return;
            log.Trace("CanonCameraManager - OnCameraShutDown");

            var camera = (EosCamera)sender;

            var index = _cameras.IndexOf(camera);
            if (index == -1) return;

            var cam = _cameras[index];

            try
            {
                if (cam.IsInLiveViewMode)
                {
                    cam.StopLiveView();
                }
            }
            catch (Exception e)
            {
                log.Info("Could not detect if camera was in LiveView Mode. Probably because handle was destroyed prematurely: {0}", e.Message);
            }
            finally
            {
                RemoveEventHandlersFromCam(cam);
                cam.Dispose();
            }

            _cameras.Remove(camera);

            if (index == 0 && _liveViewListeners.Any() && _cameras.Any())
            {
                // On change la caméra qui fait le live view pour la nouvelle tête de liste.
                _cameras[0].StartLiveView();
            }
        }

        private void RemoveEventHandlersFromCam(EosCamera cam)
        {
            cam.LiveViewUpdate -= HandlePictureUpdate;
            cam.Shutdown -= OnCameraShutDown;
            cam.FileCreatedOnCamera -= OnFileCreatedOnCamera;
            cam.FileSavedOnHost -= OnFileSavedOnHost;
        }

        /// <summary>
        /// Donne une forme pour permettre l'affichage de message d'erreurs directement à l'écran à l'intention de l'usager.
        /// Ne fera rien si nul.
        /// </summary>
        public Form MainFormWindow { get; set; }

        /// <summary>
        /// Tente d'envoyer un message d'erreur sur la forme donnée en paramètre à l'instance du contrôleur, pour que l'usager la voit.
        /// Cet usager peut alors déterminer si on recommence ou si on annule la requête.
        /// Si on n'a pas fourni de forme via "MainFormWindow", ne fera rien, et retourne false.
        /// </summary>
        /// <param name="message">Le message à afficher à l'usager.</param>
        /// <param name="callback">La méthode à appeller avec le résultat du dialogue. Optionnel.</param>
        /// <returns>true si un dialogue a été envoyé à l'usager. false si aucun dialogue n'a pu être envoyé.</returns>
        internal bool ShowErrorMessageBox(string message, Action<DialogResult> callback)
        {
            if (MainFormWindow == null) return false;

            if (!MainFormWindow.InvokeRequired)
            {
                InvokeErrorMessageBox(message, callback);
            }
            else
            {
                MainFormWindow.Invoke(new Action(() =>
                {
                    InvokeErrorMessageBox(message, callback);
                }));
            }

            return true;
        }

        // Ne pas appeller cette méthode avant d'avoir vérifier sa sécurité d'invocation.
        private static void InvokeErrorMessageBox(string message, Action<DialogResult> callback)
        {
            var result = MessageBox.Show(GetInstance().MainFormWindow, message, Resources.common.app_title, MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            callback?.Invoke(result);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int NbrCameraReady()
        {
            if (_manager == null) throw new InvalidOperationException();
            return _cameras.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public CameraControllerStateEnum CurrentState()
        {
            return _manager == null ? CameraControllerStateEnum.NotInitialized : _currentState;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool RegisterForLiveView(ILiveViewListener listener)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (listener == null) return false;

            if (!_liveViewListeners.Contains(listener))
            {
                _liveViewListeners.Add(listener);
            }

            if (_liveViewListeners.Any() && _cameras.Any())
            {
                // On change la caméra qui fait le live view pour la nouvelle tête de liste.
                var cam = _cameras[0];
                if (!cam.IsInLiveViewMode)
                {
                    cam.StartLiveView();
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool UnRegisterForLiveView(ILiveViewListener listener)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (listener == null) return false;

            if (_liveViewListeners.Contains(listener))
            {
                _liveViewListeners.Remove(listener);
            }

            if (!_liveViewListeners.Any() && _cameras.Any())
            {
                // On change la caméra qui fait le live view pour la nouvelle tête de liste.
                var cam = _cameras[0];
                if (cam.IsInLiveViewMode)
                {
                    cam.StopLiveView();
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsLiveViewEnabled()
        {
            if (_manager == null) throw new InvalidOperationException();
            return _liveViewListeners.Any();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public CameraConfigCanon GetCameraConfiguration(int cameraIndex)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (_cameras.Count >= cameraIndex) return null;

            var cam = _cameras[cameraIndex];
            var config = new CameraConfigCanon
            {
                Author = cam.Author,
                Artist = cam.Artist,
                BatteryLevel = cam.BatteryLevel,
                BatteryQuality = cam.BatteryQuality,
                Copyright = cam.Copyright,
                DeviceDescription = cam.DeviceDescription,
                ImageQuality = cam.ImageQuality,
                LiveViewAutoFocus = cam.LiveViewAutoFocus,
                LiveViewColorTemperature = cam.LiveViewColorTemperature,
                LiveViewWhiteBalance = cam.LiveViewWhiteBalance,
                FirmwareVersion = cam.FirmwareVersion,
                Focus = cam.Focus,
                ProductName = cam.ProductName,
                SerialNumber = cam.SerialNumber,
                WhiteBalance = cam.WhiteBalance,
                OwnerName = cam.OwnerName
            };
            return config;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool? SetCameraConfiguration(int cameraIndex, CameraConfigCanon config)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (_cameras.Count >= cameraIndex || config == null) return null;

            var cam = _cameras[cameraIndex];
            var mod = false;

            if (cam.Artist != config.Artist && config.Artist != null) { cam.Artist = config.Artist; mod = true; }
            if (cam.BatteryLevel != config.BatteryLevel && config.BatteryLevel != null) { cam.BatteryLevel = config.BatteryLevel.Value; mod = true; }
            if (cam.BatteryQuality != config.BatteryQuality && config.BatteryQuality != null) { cam.BatteryQuality = config.BatteryQuality.Value; mod = true; }
            if (cam.Copyright != config.Copyright && config.Copyright != null) { cam.Copyright = config.Copyright; mod = true; }
            if (!cam.ImageQuality.Equals(config.ImageQuality) && config.ImageQuality != null) { cam.ImageQuality = config.ImageQuality.Value; mod = true; }
            if (cam.LiveViewAutoFocus != config.LiveViewAutoFocus && config.LiveViewAutoFocus != null) { cam.LiveViewAutoFocus = config.LiveViewAutoFocus.Value; mod = true; }
            if (cam.LiveViewColorTemperature != config.LiveViewColorTemperature && config.LiveViewColorTemperature != null) { cam.LiveViewColorTemperature = config.LiveViewColorTemperature.Value; mod = true; }
            if (cam.LiveViewWhiteBalance != config.LiveViewWhiteBalance && config.LiveViewWhiteBalance != null) { cam.LiveViewWhiteBalance = config.LiveViewWhiteBalance.Value; mod = true; }
            if (cam.WhiteBalance != config.WhiteBalance && config.WhiteBalance != null) { cam.WhiteBalance = config.WhiteBalance.Value; mod = true; }
            if (cam.OwnerName != config.OwnerName && config.OwnerName != null) { cam.OwnerName = config.OwnerName; mod = true; }

            return mod;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TakePictures(IPictureRequest request)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (request == null) return false;
            if (_currentPictureRequest != null) return false;

            // On teste la validité de la requête d'images
            if (_cameras.Count < request.MinCameraNbr)
            {
                log.Warn("Rejected picture request because minimal number of cameras was not met.");
                return false;
            }
            if (request.PictureListener == null)
            {
                log.Warn("Rejected picture request because listner was null.");
                return false;
            }

            if (request.SaveDirectory == null)
            {
                request.SaveDirectory = PictureSaveDirectory;
            }

            _currentPictureRequest = new RequestExecutorPicturesCanon(this, request, new CancellationTokenSource());
            _currentPictureRequest.StartRequest();
            _currentState = CameraControllerStateEnum.TakingPictures;

            return true;
        }

        /// <summary>
        /// Callback de l'exécuteur de prises photo une fois qu'il a terminé.
        /// </summary>
        /// <param name="exec">La référence de l'exécuteur.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void TakePicturesFinished(RequestExecutorPicturesCanon exec)
        {
            // Ne fait rien si cette méthode n'est pas appellée avec la référence de l'exécutant actuel.
            if (exec == null || _currentPictureRequest == null || _currentPictureRequest != exec) return;

            _currentPictureRequest = null;
            _currentState = CameraControllerStateEnum.Initialized;

            if (exec.Request.Cancelled)
                exec.Request.PictureListener.PictureRequestCancelled(exec.Request);
            else
                exec.Request.PictureListener.PictureRequestCompleted(exec.Request);

            exec.Dispose();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool StartVideo(IVideoRequest request)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (request == null) return false;
            if (_currentVideoRequest != null) return false;

            if (request.VideoListener == null)
            {
                log.Warn("Rejected video request because listener was null.");
                return false;
            }

            _currentVideoRequest = new RequestExecutorVideoCanon(this, request, new CancellationTokenSource());
            _currentVideoRequest.StartRequest();
            _currentState = CameraControllerStateEnum.TakingVideo;

            return true;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool StopVideo(IVideoRequest request)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (_currentVideoRequest == null || _currentVideoRequest.Request != request) return false;
            
            _currentVideoRequest.CancelRequest();
            return true;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool RegisterForNewPictureEvent(IPictureListener listener)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (listener == null) return false;

            if (!_picturesListeners.Contains(listener))
            {
                _picturesListeners.Add(listener);
            }

            return true;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool UnRegisterForNewPictureEvent(IPictureListener listener)
        {
            if (_manager == null) throw new InvalidOperationException();
            if (listener == null) return false;

            if (_picturesListeners.Contains(listener))
            {
                _picturesListeners.Remove(listener);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal EosCamera[] GetCameras(int minCameraNbr)
        {
            if (_cameras.Count < minCameraNbr) throw new ArgumentException($"Not enough cameras to return: asked={minCameraNbr}, needed={_cameras.Count}");
            return _cameras.GetRange(0, minCameraNbr).ToArray();
        }

        /// <summary>
        /// Méthode de callback
        /// </summary>
        /// <param name="exec"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void CancelCurrentPictureRequest(RequestExecutorPicturesCanon exec)
        {
            // Ne fait rien si cette méthode n'est pas appellée avec la référence de l'exécutant actuel.
            if (exec == null || _currentPictureRequest == null || _currentPictureRequest != exec) return;
            exec.CancelRequest();
        }

        /// Callback quand il faut annuler la l'exécuteur de requête.
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void CancelCurrentVideoRequest(RequestExecutorVideoCanon exec)
        {
            // Ne fait rien si cette méthode n'est pas appellée avec la référence de l'exécutant actuel.
            if (exec == null || _currentVideoRequest == null || _currentVideoRequest != exec) return;
            exec.CancelRequest();
        }

        /// Callback quand le vidéo se termine normalement.
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void VideoCaptureFinished(RequestExecutorVideoCanon exec)
        {
            // Ne fait rien si cette méthode n'est pas appellée avec la référence de l'exécutant actuel.
            if (exec == null || _currentVideoRequest == null || _currentVideoRequest != exec) return;
    
            _currentVideoRequest = null;
            _currentState = CameraControllerStateEnum.Initialized;
    
            if (exec.Request.Cancelled)
                exec.Request.VideoListener.VideoRequestCancelled(exec.Request);
            else
                exec.Request.VideoListener.VideoRequestCompleted(exec.Request);
    
            exec.Dispose();
        }
    }
}
