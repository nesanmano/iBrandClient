using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Common.Util
{
    [Obsolete("On utilise désormais la classe 'CameraControllerCanon' pour la gestion des caméras Canon.", false)]
    public class CanonCameraManager
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public event EventHandler<EosLiveImageEventArgs> LiveViewUpdate;
        public event DirItemEventHandler FileCreatedOnCamera;
        public event EventHandler<EosFileEventArgs> FileSavedOnHost;
        public event EventHandler CameraShutdown;
        public event EventHandler CameraAdded;

        private EosFrameworkManager _manager;

        private readonly List<EosCamera> _cameras = new List<EosCamera>();

        private static CanonCameraManager _canonManager;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static CanonCameraManager GetInstance()
        {
            return _canonManager ?? (_canonManager = new CanonCameraManager());
        }

        public static bool IsInitialized()
        {
            return _canonManager != null;
        }

        private CanonCameraManager()
        {
            StartUp();
        }

        public void StopLiveView()
        {
            try
            {
                if (_cameras.Count <= 0) return;

                if (_cameras[0].IsInLiveViewMode)
                    _cameras[0].StopLiveView();
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
            }
        }

        public void OnCameraShutDown(object sender, EventArgs args)
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - OnCameraShutDown"); }

            var camera = (EosCamera)sender;

            var index = _cameras.IndexOf(camera);
            if (index == -1) return;

            var cam = _cameras[index];

            cam.LiveViewUpdate -= HandlePictureUpdate;
            cam.Shutdown -= OnCameraShutDown;
            cam.FileCreatedOnCamera -= OnFileCreatedOnCamera;
            cam.FileSavedOnHost -= OnFileSavedOnHost;
            cam.Dispose();

            _cameras.Remove(camera);

            CameraShutdown?.Invoke(this, EventArgs.Empty);
        }

        public void ShutDown()
        {
            if (_manager == null) return;

            StopLiveView();

            foreach (var camera in _cameras)
            {
                camera.LiveViewUpdate -= HandlePictureUpdate;
                camera.Shutdown -= OnCameraShutDown;
                camera.FileCreatedOnCamera -= OnFileCreatedOnCamera;
                camera.FileSavedOnHost -= OnFileSavedOnHost;
                camera.Dispose();
            }

            _manager.CameraAdded -= HandleCameraAdded;
            _manager.ReleaseFramework();
            _manager = null;
            _canonManager = null;
            GC.Collect();
        }

        public bool IsCameraConnected()
        {
            return _cameras.Count > 0;
        }

        public int CameraCount()
        {
            return _cameras.Count;
        }

        public bool IsLiveView()
        {
            try
            {
                if (_cameras.Count > 0)
                {
                    return _cameras[0].IsInLiveViewMode;
                }

            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
            }

            return false;
        }

        public bool TakePicture()
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - TakePicture"); }

            try
            {
                if (_cameras.Count <= 0) return true;

                StopLiveView();
                _cameras[0].TakePicture();

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
                return false;
            }
        }


        public bool PressShutterButton()
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - PressShutterButton"); }

            try
            {
                if (_cameras.Count <= 0) return true;

                StopLiveView();
                _cameras[0].PressShutterButton();

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
                return false;
            }
        }

        public bool ReleaseShutterButton()
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - PressShutterButton"); }

            try
            {
                if (_cameras.Count <= 0) return true;

                StopLiveView();
                _cameras[0].ReleaseShutterButton();

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
                return false;
            }
        }

        public bool StartRecord()
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - StartRecord"); }

            try
            {
                if (_cameras.Count > 0)
                {
                    _cameras[0].StartRecord();
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
                return false;
            }
        }

        public bool StopRecord()
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - StopRecord"); }

            try
            {
                if (_cameras.Count > 0)
                {
                    _cameras[0].StopRecord();
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
                return false;
            }
        }

        private void StartUp()
        {
            SafeCall(() =>
            {
                _manager = new EosFrameworkManager();
                _manager.LoadFramework();
                LoadCamera();
                
                _manager.CameraAdded += HandleCameraAdded;
            }, ex =>
            {
                ShowErrorMessageBox(ex.Message);
                ShutDown();
            });
        }

        public EosCamera GetCamera()
        {
            return _cameras.Count > 0 ? _cameras[0] : null;
        }

        private void HandlePictureUpdate(object sender, EosLiveImageEventArgs e)
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - HandlePictureUpdate"); }

            LiveViewUpdate?.Invoke(sender, e);
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - OnFileCreatedOnCamera"); }

            FileCreatedOnCamera?.Invoke(sender, dirItem, camera);
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - OnFileSavedOnHost"); }

            FileSavedOnHost?.Invoke(sender, e);
        }

        private void OnCameraAdded(object sender, EventArgs e)
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - OnCameraAdded"); }

            CameraAdded?.Invoke(sender, e);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void SafeCall(Action action, Action<Exception> exceptionHandler)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error during camera event handling.");
            }
        }

        private void HandleCameraAdded(object sender, EventArgs e)
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - HandleCameraAdded"); }

            try
            {
                var camera = _manager.GetLastConnectedCamera();

                if (camera == null) return;

                camera.LiveViewUpdate += HandlePictureUpdate;
                camera.FileCreatedOnCamera += OnFileCreatedOnCamera;
                camera.FileSavedOnHost += OnFileSavedOnHost;
                camera.Shutdown += OnCameraShutDown;
                camera.StopLiveView();
                _cameras.Add(camera);
                OnCameraAdded(sender, e);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error during camera added event.");
            }
        }

        public void StartLiveView()
        {
            try
            {
                if (_cameras.Count > 0)
                {
                    _cameras[0].StartLiveView();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// La forme principale d'affichage courante. Utilisée uniquement pour l'affichage de messages d'erreurs destinés à l'usager via
        /// des MessageBox. Si null, toutes les fonctions de ces messages d'erreurs ne seront pas utilisés.
        /// </summary>
        public static Form MainFormWindow { get; set; }

        /// <summary>
        /// Indique si l'on veut afficher les messages d'erreur des caméra à l'écran, quand on a une forme d'associée.
        /// Cette propriété est voulue à fin de déboguage principalement.
        /// </summary>
        public bool ShowCameraErrorMessagesInDialog { get; set; }

        /// <summary>
        /// Détermine s'il faut invoquer de façon "thread-safe" un appel d'affichage de dialogue d'erreur.
        /// On fait, dans tous les cas, on log de niveau "Trace" de l'erreur.
        /// </summary>
        /// <param name="message">Le message à afficher.</param>
        private void ShowErrorMessageBox(string message)
        {
            log.Trace("Recovarable error from camera: " + message);

            if (MainFormWindow == null || !ShowCameraErrorMessagesInDialog) return;

            FormUtil.SafeCall(MainFormWindow, () =>
            {
                MessageBox.Show(MainFormWindow, message, Resources.common.app_title, MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            });
        }

        private void LoadCamera()
        {
            if (DebugEvents) { log.Trace("CanonCameraManager - LoadCamera"); }

            var cameras = _manager.GetCameras();

            foreach (var camera in cameras)
            {
                camera.LiveViewUpdate += HandlePictureUpdate;
                camera.FileCreatedOnCamera += OnFileCreatedOnCamera;
                camera.FileSavedOnHost += OnFileSavedOnHost;
                camera.Shutdown += OnCameraShutDown;
                camera.IsErrorTolerantMode = true;
                _cameras.Add(camera);
                OnCameraAdded(null, null);
            }
        }

        public bool DebugEvents { get; set; }
    }
}
