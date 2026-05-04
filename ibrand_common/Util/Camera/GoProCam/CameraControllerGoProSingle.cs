using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using GoPro.Hero.Hero3;
using NLog;
using NomadLogic.Common.Util.Networking;

namespace NomadLogic.Common.Util.Camera.GoProCam
{
    /// <summary>
    /// Contrôleur pour la caméra GoPro.
    /// Il n'est possible que d'accéder à une seule caméra GoPro à la fois avec ce contrôleur.
    /// Voir ici pour le code source: https://github.com/r1pper/GoPro.Hero/tree/master/src/GoPro.Hero
    /// Il n'y a malheureusement pas de documentation publique sur les commandes GoPro: elles sont toutes le résultat d'efforts par des
    /// individus par rétro-ingénierie.
    /// </summary>
    public class CameraControllerGoProSingle : ICameraController<CameraConfigGoPro>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static CameraControllerGoProSingle _instance;
        private Hero3Camera _mainCamera;
        private CameraControllerStateEnum _currentState = CameraControllerStateEnum.Undefined;
        private readonly ISet<ILiveViewListener> _liveViewListeners = new HashSet<ILiveViewListener>();
        private readonly ISet<IPictureListener> _picturesListeners = new HashSet<IPictureListener>();
        private readonly ISet<IPictureListener> _videoListeners = new HashSet<IPictureListener>();
        private DirectoryInfo _pictureSaveDirectory = new DirectoryInfo(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures));
        private RequestExecutorGoPro _currentPictureRequest;
        
        private CameraControllerGoProSingle()
        {
            // Singleton
        }

        /// <summary>
        /// Retourne la référence vers l'unique instance de cette classe.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static CameraControllerGoProSingle GetInstance()
        {
            return _instance ?? (_instance = new CameraControllerGoProSingle());
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            _mainCamera = null;
            log.Trace("Controller destroyed.");
            _currentState = CameraControllerStateEnum.Disposed;
            GC.Collect();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Init()
        {
            if (_mainCamera != null) return true;

            _mainCamera = GetCamera();
            if (_mainCamera == null)
            {
                log.Error("Could not initialize Camera Controller for GoPro Camera.");
                Dispose();
                _currentState = CameraControllerStateEnum.ErrorInitialization;
                return false;
            }

            _currentState = CameraControllerStateEnum.Initialized;
            log.Trace("GoPro Camera Initialisation sequence finished.");
            return true;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public int NbrCameraReady()
        {
            return _mainCamera == null ? 0 : 1;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public CameraControllerStateEnum CurrentState()
        {
            return _mainCamera == null ? CameraControllerStateEnum.NotInitialized : _currentState;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool RegisterForLiveView(ILiveViewListener listener)
        {
            if (_mainCamera == null) throw new InvalidOperationException();
            if (listener == null) return false;
            
            if (!_liveViewListeners.Contains(listener))
            {
                _liveViewListeners.Add(listener);
            }

            if (_liveViewListeners.Any())
            {
                // On change la caméra qui fait le live view pour la nouvelle tête de liste.

                _mainCamera.EnableLivePreview();
                return _mainCamera.ExtendedSettings().LiveFeed;
            }

            return false;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool UnRegisterForLiveView(ILiveViewListener listener)
        {
            if (_mainCamera == null) throw new InvalidOperationException();
            if (listener == null) return false;

            if (_liveViewListeners.Contains(listener))
            {
                _liveViewListeners.Remove(listener);
            }

            if (!_liveViewListeners.Any())
            {
                // On change la caméra qui fait le live view pour la nouvelle tête de liste.

                _mainCamera.DisableLivePreview();
                return !_mainCamera.ExtendedSettings().LiveFeed;
            }

            return false;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsLiveViewEnabled()
        {
            if (_mainCamera == null) throw new InvalidOperationException();
            return _mainCamera.ExtendedSettings().LiveFeed;
        }

        /// <summary>
        /// Il n'y a qu'une seule caméra pour ce contrôleur. Si l'index est autre que zéro, on retourne null.
        /// </summary>
        /// <param name="cameraIndex">Index de la caméra. Ne peut être que zéro pour retourner de l'information.</param>
        /// <returns>Une instance de configuration de la caméra actuelle.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public CameraConfigGoPro GetCameraConfiguration(int cameraIndex)
        {
            if (_mainCamera == null) throw new InvalidOperationException();
            if (cameraIndex != 0) return null;

            var extSettings = _mainCamera.ExtendedSettings();
            var bacpackStatus = _mainCamera.BacpacStatus();
            var bacpacInformation = _mainCamera.BacpacInformation();
            
            var config = new CameraConfigGoPro
            {
                Author = bacpacInformation.Id,
                BatteryLevel = bacpackStatus.BacpacBattery,
                Copyright = "N/A",
                DeviceDescription = "N/A",
                FirmwareVersion = $"{bacpacInformation.FirmwareVersion.Major}.{bacpacInformation.FirmwareVersion.Minor}.{bacpacInformation.FirmwareVersion.Build}.{bacpacInformation.FirmwareVersion.Revision}",
                ProductName = bacpacInformation.Model.ToString(),
                SerialNumber = bacpacInformation.MacAddress,
                ExtendedSettings = extSettings,
                BacpackStatus = bacpackStatus,
                BacpackInformation = bacpacInformation
            };
            return config;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool? SetCameraConfiguration(int cameraIndex, CameraConfigGoPro config)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TakePictures(IPictureRequest request)
        {
            if (_mainCamera == null) throw new InvalidOperationException();
            if (request == null) return false;
            if (_currentPictureRequest != null) return false;

            // On teste la validité de la requête d'images
            if (request.MinCameraNbr != 1)
            {
                log.Warn("Rejected picture request because this camera controller only supports one camera.");
                return false;
            }
            if (request.PictureListener == null)
            {
                log.Warn("Rejected picture request because listner was null.");
                return false;
            }

            _currentPictureRequest = new RequestExecutorGoPro(this, request, new CancellationTokenSource());
            _currentPictureRequest.StartRequest();
            _currentState = CameraControllerStateEnum.TakingPictures;

            return true;
        }

        /// <summary>
        /// Callback de l'exécuteur de prises photo une fois qu'il a terminé.
        /// </summary>
        /// <param name="exec">La référence de l'exécuteur.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void TakePicturesFinished(RequestExecutorGoPro exec)
        {
            // Ne fait rien si cette méthode n'est pas appellée avec la référence de l'exécutant actuel.
            if (exec == null || _currentPictureRequest == null || _currentPictureRequest != exec) return;

            _currentPictureRequest = null;
            _currentState = CameraControllerStateEnum.Initialized;

            if (exec.Request.Cancelled)
                exec.Request.PictureListener?.PictureRequestCancelled(exec.Request);
            else
                exec.Request.PictureListener?.PictureRequestCompleted(exec.Request);

            exec.Dispose();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool StartVideo(IVideoRequest listener)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool StopVideo(IVideoRequest listener)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool RegisterForNewPictureEvent(IPictureListener listener)
        {
            if (_mainCamera == null) throw new InvalidOperationException();
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
            if (_mainCamera == null) throw new InvalidOperationException();
            if (listener == null) return false;

            if (_picturesListeners.Contains(listener))
            {
                _picturesListeners.Remove(listener);
            }

            return true;
        }

        #region Méthode propres à ce contrôleur.

        /// <summary>
        /// Méthode de callback
        /// </summary>
        /// <param name="exec"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void CancelCurrentPictureRequest(RequestExecutorGoPro exec)
        {
            // Ne fait rien si cette méthode n'est pas appellée avec la référence de l'exécutant actuel.
            if (exec == null || _currentPictureRequest == null || _currentPictureRequest != exec) return;
            exec.CancelRequest();
        }

        public Hero3Camera GetCamera()
        {
            if (_mainCamera != null) return _mainCamera;

            Hero3Camera camera = null;

            try
            {
                if (NetUtil.Ping("10.5.5.9", 2, 1000))
                {
                    camera = GoPro.Hero.Camera.Create<Hero3Camera>("10.5.5.9");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Problem getting camera.");
            }

            if (camera == null)
            {
                log.Warn("Could not ping camera.");
                return null;
            }

            /*
            try
            {
                camera.Mode(out Mode currentMode);
                if (currentMode != Mode.Photo) camera.Mode(Mode.Photo);
            }
            catch (Exception e)
            {
                log.Warn(e, "Could not change to photo mode.");
            }
            */

            return camera;
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

        /// <summary>
        /// Répertoire où l'on veut stocker les images prises automatiquement par les caméras. Fonctionne indépendemment des demandes
        /// de prises photo via méthode.
        /// </summary>
        public DirectoryInfo PictureSaveDirectory
        {
            get => _pictureSaveDirectory;

            set { if (value.Exists) _pictureSaveDirectory = value; }
        }

        public Hero3Camera[] GetCameras(int requestMinCameraNbr)
        {
            return new [] { _mainCamera };
        }

        #endregion


    }
}
