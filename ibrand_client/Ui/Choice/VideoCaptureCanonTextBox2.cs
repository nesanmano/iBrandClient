using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Trigger;
using NomadLogic.Common.Util.Trigger.LaserBeam;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class VideoCaptureCanonTextBox2 : UserControl
    {
        protected delegate void SetTextDelegate(string text);
        protected delegate void ShowFileDelegate(string text);
        protected delegate void EnableButtonDelegate(bool enable);
        public event Action VideoFileReady;

        private TriggerManagerBase _lbm;
        private TriggerManagerBase.TriggerEventHandler _lbmHandler;

        private EosFrameworkManager _manager;

        private EosCamera _camera;

        private string _fileName;

        private volatile bool _captureOngoing;

        private int _countDown;

        private int _countDownValue;

        private readonly Timer _timer;

        private bool _showStopStartButton = true;

        ~VideoCaptureCanonTextBox2()
        {
            SetLaserBeamManager(null);
        }

        public VideoCaptureCanonTextBox2()
        {
            InitializeComponent();

            textBox.Enabled = false;

            //this.previewButton.Enabled = false;
            stopButton.Enabled = false;

            previewButton.Click += OnPreviewButtonClick;
            startButton.Click += OnStartButtonClick;
            stopButton.Click += OnStopButtonClick;

            StartUp();

            _timer = new Timer {Interval = 1000};
            //1 sec
            _timer.Tick += OnTimerEventt;
        }

        public bool ShowStartStopButtons
        {
            set
            {
                _showStopStartButton = value;
                if (!value)
                {
                    startButton.Click -= OnStartButtonClick;
                    stopButton.Click -= OnStopButtonClick;

                    Controls.Remove(startButton);
                    Controls.Remove(stopButton);
                }
            }
        }

        private void StartUp()
        {
            SafeCall(() =>
            {
                _manager = new EosFrameworkManager();
                _manager.CameraAdded += HandleCameraAdded;
                _manager.LoadFramework();
                LoadCamera();
            }, ex =>
            {
                MessageBox.Show(ex.ToString(), @"FrameworkLoadError", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectCamera();
            });
        }

        public void OnTimerEventt(object sender, EventArgs e)
        {
            if (_countDown > 0)
            {
                _countDown--;
                SetTextBoxText(_countDown.ToString());
                if (_countDown == 0)
                {
                    OnStopButtonClick(this, EventArgs.Empty);
                }
            }          
        }

        /// <summary>
        /// Associe un gestionnaire de laser pour gérer le démarrage/arrêt de la capture vidéo.
        /// <br/>Quand on passe une nouvelle instance de gestionnaire, l'ancien gestionnaire est désassocié, mais pas détruit, puis le nouveau
        /// gestionnaire de laser est associé à l'instance, et démarré. S'il ne peut être correctement démarré, on lancera un message d'erreur.
        /// Quand on passe une référence nulle, on désassocie le gestionnaire laser courant, si existant.
        /// </summary>
        /// <param name="lbm">Le gestionnaire de laser.</param>
        public void SetLaserBeamManager(LaserBeamManager lbm)
        {
            // Si la référence est différente, on désassocie l'ancien gestionnaire (si existant)
            if (lbm != _lbm) {
                if (_lbm != null) 
                {
                    _lbm.Triggered -= _lbmHandler;
                    _lbmHandler = null;
                    _lbm.Dispose();
                    _lbm = null;
                }
            }

            _lbm = lbm;

            // Si la nouvelle instance n'est pas nulle, on associe.
            if (_lbm != null)
            {
                if (_lbm.IsRunning)
                {
                    _lbm.Stop();
                }

                _lbmHandler = LaserOccluded;
                _lbm.Triggered += _lbmHandler;
                if (!_lbm.Init())
                {
                    // En cas d'erreur, on avertit et nullifie le gestionnaire.
                    MsgBox.Show(WorkingArea.Container, "Cannot find laser USB component, or correctly associate it.",
                                "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _lbm.Triggered -= _lbmHandler;
                    _lbmHandler = null;
                    _lbm.Dispose();
                    _lbm = null;
                }
                else
                {
                    _lbm.Start();
                }
            }

            // Activation ou déactivation des boutons.
            if (_lbm == null)
            {
                startButton.Enabled = true;
                stopButton.Enabled = true;
            }
            else
            {
                startButton.Enabled = false;
                stopButton.Enabled = false;
            }
        }

        /// <summary>
        /// Temps de capture vidéo à utiliser avec le gestionnaire de laser, si présent.
        /// <br/>L'unité de cette valeur est en secondes.
        /// </summary>
        public float VideoCaptureTime
        {
            get;
            set;
        }

        /// <summary>
        /// Méthode appellée quand le gestionnaire laser détecte une coupure du faisceau lumineux.
        /// </summary>
        private void LaserOccluded()
        {
            if (_camera != null && _lbm != null && VideoCaptureTime > 0.0f && !_captureOngoing)
            {
                Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            _captureOngoing = true;
                            _camera.StartRecord();
                            Thread.Sleep((int)(VideoCaptureTime * 1000));
                            _camera.StopRecord();
                            _captureOngoing = false;
                        }
                        catch (System.Exception ex)
                        {
                            MsgBox.Show(WorkingArea.Container, "Cannot complete laser occlusion video capture because of following exception:\n" + ex.Message,
                                "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _camera.StopRecord();
                        }
                    });
            }
        }

        /// <summary>
        /// Méthode déléguée appellée quand la caméra est ajoutée au système.
        /// </summary>
        /// <param name="sender">L'origine de l'événement</param>
        /// <param name="e">Les paramètres de l'événement.</param>
        private void HandleCameraAdded(object sender, EventArgs e)
        {
            if (_camera == null)
                LoadCamera();
        }

        private void SafeCall(Action action, Action<System.Exception> exceptionHandler)
        {
            try
            {
                action();
            }
            catch (System.Exception ex)
            {
                if (InvokeRequired) Invoke(exceptionHandler, ex);
                else exceptionHandler(ex);
            }
        }

        /// <summary>
        /// Démarre la capture vidéo via un clic. Si l'instance possède un gestionnaire de laser associé, cette méthode ne fait rien.
        /// </summary>
        /// <param name="sender">L'origine du clic.</param>
        /// <param name="args">Les paramètres de l'événement.</param>
        public void OnStartButtonClick(object sender, EventArgs args)
        {
            try
            {
                if (_camera != null && _lbm == null)
                {
                    SetTextBoxText("");
                    EnableStartButton(false);
                    EnablePreviewButton(false);
                    EnableStopButton(true);
                    _camera.StartRecord();

                    if (_countDownValue > 0)
                    {
                        _countDown = _countDownValue;
                        _timer.Start();
                    }
                }
                
            }
            catch (System.Exception ex)
            {
                MsgBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Stoppe la capture vidéo via un clic.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="args">Les paramètres de l'événement.</param>
        public void OnStopButtonClick(object sender, EventArgs args)
        {
            try
            {
                if (_camera != null && _lbm == null)
                {
                    EnableStopButton(false);

                    SetTextBoxText("Waiting for video to transfer...");

                    _camera.StopRecord();

                    if (_countDownValue > 0)
                    {
                        _timer.Stop();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MsgBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Quand la caméra n'est plus accessible du système, on ferme les instances associées.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="args">Les paramètres de l'événement.</param>
        public void OnCameraShutDown(object sender, EventArgs args)
        {
            _camera.FileCreatedOnCamera -= OnFileCreatedOnCamera;
            _camera.FileSavedOnHost -= OnFileSavedOnHost;
            _camera.Dispose();
            _camera = null;
            SetCameraConnected(false);
        }

        /// <summary>
        /// Quand on détecte un fichier créé sur la caméra, on tente de le télécharger dans les répertoires de travail de iBrand.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="dirItem">L'identifiant du fichier sur la caméra.</param>
        /// <param name="camera">L'instance de la caméra.</param>
        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {

            var fileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + ".MOV";

            if (File.Exists(fileName))
                File.Delete(fileName);

            camera.SavePicture(dirItem, fileName);
        }

        public void OnPreviewButtonClick(object sender, EventArgs args)
        {
            /*String file_name = this.textBox.Text;
            if (file_name != null && file_name.Length > 0)
            {
                AbstractConfiguration abstractConfiguration = ServicesContainer.GetService<NomadConfiguration>();
                String file = abstractConfiguration.VideosCanonDir + file_name;
                if (File.Exists(file))
                    ShowFile(file);
            }*/

            var vsd = new VideoSelectionDialog(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos)) {Owner = WorkingArea.Container};
            var result = vsd.ShowDialog(WorkingArea.Container);

            if (result == DialogResult.OK)
            {
                _fileName = vsd.SelectedFile;
                SetTextBoxText(StringUtil.GetFileName(_fileName));
            }

        }

        /// <summary>
        /// Génère et affiche un thumbnail d'un fichier vidéo.
        /// </summary>
        /// <param name="fileName">Le nom du fichier d'où extraire le thumbnail.</param>
        // ReSharper disable once UnusedMember.Local
        private void ShowFile(string fileName)
        {
            if (WorkingArea.Container.InvokeRequired)
            {
                WorkingArea.Container.Invoke(new ShowFileDelegate(ShowFile), fileName);
                return;
            }
            
            var thumbnailFileName = fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal)) + ".jpg";

            thumbnailFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos_thumbnail) + StringUtil.GetFileName(thumbnailFileName);

            if (File.Exists(thumbnailFileName))
                File.Delete(thumbnailFileName);


            //TDB trouver le nombre de secondes ou generer le thumbnail automatiquement FSF
            new FfmpegThumbnailsGenerator().GenerateThumbnail(fileName, thumbnailFileName, 1);

            if (File.Exists(thumbnailFileName))
            {
                var stream = new FileStream(thumbnailFileName, FileMode.Open);
                var picture = new Bitmap(stream);
                stream.Close();
                var dlg = new PreviewDialog(picture) {Owner = WorkingArea.Container};
                dlg.ShowDialog(WorkingArea.Container);
            }
        }

        /// <summary>
        /// Quand le fichier vidéo est sauvegardé sur la machine locale.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="e">Les paramètres du fichier vidéo.</param>
        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            _fileName = e.FilePath;
            SetTextBoxText(StringUtil.GetFileName(e.FilePath));
            EnableStartButton(true);
            EnablePreviewButton(true);

            // Appelle les délégués qui veulent écouter quand un fichier vidéo est prêt.
            var handler = VideoFileReady;
            handler?.Invoke();
        }

        public void EnablePreviewButton(bool enable)
        {
            if (previewButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new EnableButtonDelegate(EnablePreviewButton), enable);
                return;
            }

            previewButton.Enabled = enable;

        }

        public void EnableStartButton(bool enable)
        {
            if (startButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new EnableButtonDelegate(EnableStartButton), enable);
                return;
            }

            startButton.Enabled = enable;

        }

        public void EnableStopButton(bool enable)
        {
            if (stopButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new EnableButtonDelegate(EnableStopButton), enable);
                return;
            }

            stopButton.Enabled = enable;

        }

        public void SetTextBoxText(string text)
        {
            if (textBox.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new SetTextDelegate(SetTextBoxText), text);
                return;
            }

            textBox.Text = text;
            textBox.Invalidate();

        }

        /// <summary>
        /// Charge la première caméra Canon détectée pour être liée à cette instance.
        /// </summary>
        private void LoadCamera()
        {
            IEnumerable<EosCamera> cameras = _manager.GetCameras();

            if (cameras.Any())
            {
                foreach (var camera in cameras)
                {
                    camera.FileCreatedOnCamera += OnFileCreatedOnCamera;
                    camera.FileSavedOnHost += OnFileSavedOnHost;
                    camera.Shutdown += OnCameraShutDown;
                    _camera = camera;
                    SetCameraConnected(true);
                    break;//On connect seulement la premiere camera
                }
            }
            else
            {
                SetCameraConnected(false);
            }
        }

       
        public override string Text => StringUtil.GetFileName(_fileName);

        public void SetCountDown(int countDown)
        {
            _countDownValue = countDown;
            _countDown = _countDownValue;
        }

        public void SetSize(Size size)
        {
            Size = size;

            textBox.Width = size.Width - startButton.Width - stopButton.Width - previewButton.Width - 15;

            if (_showStopStartButton)
            {
                var initialLocation = startButton.Location;

                startButton.Location = new Point(textBox.Width + 5, initialLocation.Y);

                stopButton.Location = new Point(textBox.Width + startButton.Width + 10, initialLocation.Y);

                previewButton.Location = new Point(textBox.Width + startButton.Width + stopButton.Width + 15, initialLocation.Y);
            }
            else
            {
                previewButton.Location = new Point(textBox.Width + 5, previewButton.Location.Y);
            }
        }

        public void Reset()
        {
            _fileName = "";
            if (textBox.Text.IndexOf("Camera", StringComparison.Ordinal) == -1)
                textBox.Text = "";
        }

        public new bool Enabled
        {
            get => base.Enabled;

            set
            {
                textBox.Enabled = false;
                startButton.Enabled = value;
                base.Enabled = value;

                textBox.BackColor = Color.LightGray;
                Reset();
            }
        }

        public void SetCameraConnected(bool isConnected)
        {
            var result = isConnected ? "Camera connected." : "No camera connected.";

            SetTextBoxText(result);
        }

        public void DisconnectCamera()
        {
            if (_manager != null)
            {
                if( _camera != null )
                {
                    _camera.FileCreatedOnCamera -= OnFileCreatedOnCamera;
                    _camera.FileSavedOnHost -= OnFileSavedOnHost;
                    _camera.Dispose();
                }

                _manager.ReleaseFramework();
                _manager = null;
                GC.Collect();
            }
        }
    }
}
