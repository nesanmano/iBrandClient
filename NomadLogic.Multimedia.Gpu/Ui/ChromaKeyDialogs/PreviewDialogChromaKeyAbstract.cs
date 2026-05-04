using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using sony_camera_api;
using sony_camera_api.Api;

// ReSharper disable StaticMemberInGenericType

namespace NomadLogic.Multimedia.Gpu.Ui.ChromaKeyDialogs
{
    /// <summary>
    /// Classe de base pour le dialogue de configuration de la clé chromatique.
    /// </summary>
    /// <typeparam name="TChromaKeyType">Le type de clé chromatique utilisé par la classe.</typeparam>
    public abstract partial class PreviewDialogChromaKeyAbstract<TChromaKeyType> where TChromaKeyType : ChromaKeyInfoAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Délégué pour activer/désactiver les boutons.
        /// </summary>
        /// <param name="buttons">L'ensemble des boutons à activer/déactiver.</param>
        /// <param name="state">L'état d'activation voulu.</param>
        /// <param name="postStateAction">Action à exécuter une fois l'état modifié.</param>
        protected delegate void ChangeButtonState(Button[] buttons, bool state, Action postStateAction);

        /// <summary>
        /// Font pour du texte plus petit pour certains éléments.
        /// </summary>
        protected static readonly Font SMALLER_FONT = new Font("Times", 7);

        /// <summary>
        /// Sémaphore pour indiquer qu'il ne faut pas exécuter d'événements sur le UI.
        /// </summary>
        protected bool _blockChanges;
        protected readonly ChangeButtonState _buttonDeleg;
        private CanonCameraManager _cameraManager;
        private CameraApiClient _sonyCamera;
        private SsdpDiscovery _discovery;
        
        protected readonly Button _btnFullSizePreview;
        protected readonly Button _btnSavePreview;
        protected TChromaKeyType _filter;

        protected string _sourceBitmapFile;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly string DEFAULT_IMAGE = DirectoryUtil.GetLocalNomadLogicAssetsDir() + @"not_keyed.jpg";

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        protected PreviewDialogChromaKeyAbstract()
        {
            InitializeComponent();

            pbOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pbTransformed.SizeMode = PictureBoxSizeMode.Zoom;
            
            // Force la création des "handles" de la forme.
            {
                // ReSharper disable once JoinDeclarationAndInitializer
                // ReSharper disable once NotAccessedVariable
                IntPtr tmp;
                tmp = ButtonSaveToDisk.Handle;
                tmp = ButtonApply.Handle;
                tmp = ButtonDefaultGreenScreen.Handle;
                tmp = ButtonDefaultBlueScreen.Handle;
                tmp = btnTakePicture.Handle;
                tmp = ButtonClose.Handle;
            }

            _buttonDeleg = ChangeButtonStateMethod;

            _btnFullSizePreview = new Button
            {
                Font = SMALLER_FONT,
                Location = new Point(80 + 6, 308),
                Size = new Size(80, 23),
                UseVisualStyleBackColor = true
            };
            _btnFullSizePreview.Click += OnFullPreviewClick;

            _btnSavePreview = new Button
            {
                Font = SMALLER_FONT,
                Location = new Point(160 + 9, 308),
                Size = new Size(80, 23),
                UseVisualStyleBackColor = true
            };
            _btnSavePreview.Click += OnSavePreviewClick;

            panelKeyedPicture.Controls.Add(_btnFullSizePreview);
            panelKeyedPicture.Controls.Add(_btnSavePreview);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            cameraVendorComboBox.Items.AddRange(new object[]
            {
                "Canon"
                ,"Sony"
            });
        }

        /// <summary>
        /// Initialise le filtre de clé chromatique à partir du disque.
        /// </summary>
        protected abstract void InitializeFilterFromDisk();

        /// <summary>
        /// Initialize les éléments
        /// </summary>
        protected abstract void InitCustomElements();

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitCustomElements();

            InitializeFilterFromDisk();
            
            StartPosition = FormStartPosition.Manual;

            int locX;
            int locY;
            int pW;
            int pH;
            if (Owner == null)
            {
                locX = 0;
                locY = 0;
                pW = Screen.PrimaryScreen.WorkingArea.Width;
                pH = Screen.PrimaryScreen.WorkingArea.Height;
            }
            else
            {
                locX = Owner.Location.X;
                locY = Owner.Location.Y;
                pW = Owner.Width;
                pH = Owner.Height;
            }

            var x = locX + pW / 2 - Size.Width / 2;
            var y = locY + (pH - Size.Height) / 2;
            Location = new Point(x, y);
            
            SetLanguageTextsForLabels();

            if (_filter.CameraVendor == ChromaKeyInfoCameraVendorEnum.Canon)
            {
                InitCanonCamera();
                cameraVendorComboBox.SelectedIndex = 0;
            }
            else
            {
                InitSonyCamera();
                cameraVendorComboBox.SelectedIndex = 1;
            }

            cameraVendorComboBox.SelectedIndexChanged += OnCameraVendorSelectionChanged;
        }

        private void InitCanonCamera()
        {
            if (_cameraManager != null) return;
            _cameraManager = CanonCameraManager.GetInstance();

            _cameraManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            _cameraManager.FileSavedOnHost += OnFileSavedOnHost;
        }

        private void InitSonyCamera()
        {
            _discovery = new SsdpDiscovery();
            _discovery.SonyCameraDeviceDiscovered += OnSonyCameraDiscovered;
            _discovery.Timeout += OnTimeout;
            Task.Run(() => _discovery.SearchSonyCameraDevices());
        }

        private void OnCameraVendorSelectionChanged(object sender, EventArgs e)
        {
            if (cameraVendorComboBox.SelectedIndex == 0)//Canon
            {
                _filter.CameraVendor = ChromaKeyInfoCameraVendorEnum.Canon;
                InitCanonCamera();
            }
            else//Sony
            {
                _filter.CameraVendor = ChromaKeyInfoCameraVendorEnum.Sony;
                InitSonyCamera();
            }
        }

        private void OnTimeout(object sender, EventArgs e)
        {
            if (_sonyCamera != null)
                return;

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(this, @"Camera is not connected.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }));
                return;
            }

            MessageBox.Show(this, @"Camera is not connected.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private async void OnSonyCameraDiscovered(object sender, SonyCameraDeviceEventArgs e)
        {
            var endpoints = e.SonyCameraDevice.Endpoints; // Dictionary of each service name and endpoint.
            _sonyCamera = new CameraApiClient(new Uri(endpoints["camera"]));

            await _sonyCamera.StartRecModeAsync();
        }

        /// <summary>
        /// Assigne les textes des contrôles de la forme selon la langue sélectionnée.
        /// </summary>
        protected virtual void SetLanguageTextsForLabels()
        {
            ButtonApply.Text = "Apply";
            ButtonClose.Text = "Close";
            ButtonDefaultGreenScreen.Text = "Default Green";
            ButtonDefaultBlueScreen.Text = "Default Blue";
            ButtonSaveToDisk.Text = "Save";
            btnTakePicture.Text = "Take picture";

            _btnFullSizePreview.Text = "Full preview";
            _btnSavePreview.Text = "Save preview";
            
            lblOriginalPicture.Text = "Original";
            lblKeyedPicture.Text = "Keyed";
        }

        private static void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            camera.SavePicture(dirItem, DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + GenerateUniqueId.GetId() + ".jpg");
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { ChangePictureMethod(e.FilePath); }));
                return;
            }

            ChangePictureMethod(e.FilePath);
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            LaunchTransform();
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            _blockChanges = true;
            ResetToDefault(sender);

            _blockChanges = false;
        }

        /// <summary>
        /// Réinitialise la configuration de keying.
        /// </summary>
        /// <param name="sender">Le contrôle qui a provoqué cet événement.</param>
        protected abstract void ResetToDefault(object sender);

        private void PreviewDialogTransformation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cameraManager == null) return;
            _cameraManager.FileCreatedOnCamera -= OnFileCreatedOnCamera;
            _cameraManager.FileSavedOnHost -= OnFileSavedOnHost;
        }

        /// <summary>
        /// Lance la transformation du preview et met l'image dans la boîte d'image.
        /// </summary>
        protected void LaunchTransform()
        {
            new Task(() =>
            {
                if (_blockChanges) return;

                if (_sourceBitmapFile == null) return;

                Button[] buttonArr =
                {
                    ButtonSaveToDisk, ButtonClose, ButtonApply, ButtonDefaultGreenScreen,
                    ButtonDefaultBlueScreen
                };

                Invoke(_buttonDeleg, buttonArr, false, new Action(() =>
                {
                    Task.Run(() =>
                    {
                        LaunchTransfromDo();
                        Invoke(_buttonDeleg, buttonArr, true, null);
                    });
                }));
            }).Start();
        }

        /// <summary>
        /// Exécute l'action de transformation de clé chromatique.
        /// </summary>
        protected abstract void LaunchTransfromDo();

        /// <summary>
        /// Va chercher l'image sur disque et la redimensionne pour l'afficher dans le contrôle.
        /// </summary>
        /// <param name="pict"></param>
        /// <returns></returns>
        protected Bitmap GetImageForControl(string pict)
        {
            var bmp = FileUtil.LoadBitmap(pict);

            var ratio = (float)pbOriginal.Size.Width / bmp.Width;

            var resized = bmp.Resize((int)(bmp.Width * ratio), (int)(bmp.Height * ratio));

            bmp.Dispose();

            return resized;
        }

        private void ChangePictureMethod(string newPicture)
        {
            try
            {
                BitmapExtentions.MatchExifData(newPicture);

                var fromFile = FileUtil.LoadBitmap(newPicture);
                var copy = fromFile.Clone(new Rectangle(0, 0, fromFile.Width, fromFile.Height), fromFile.PixelFormat);
                fromFile.Dispose();

                pbOriginal.Image?.Dispose();
                pbOriginal.Image = copy;
                _sourceBitmapFile = newPicture;
                SourceImageChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Méthode invoquée quand l'image source à été modifiée.
        /// </summary>
        protected abstract void SourceImageChanged();

        private static void ChangeButtonStateMethod(Button[] buttons, bool state, Action postStateAction)
        {
            foreach (var b in buttons)
            {
                b.Enabled = state;
            }

            postStateAction?.Invoke();
        }

        ~PreviewDialogChromaKeyAbstract()
        {
            Dispose();
        }

        private async void btnTakePicture_Click(object sender, EventArgs e)
        {
            if (_blockChanges)
            {
                return;
            }

            var pictureTaken = false;

            if (cameraVendorComboBox.SelectedIndex == 0)
            {
                if (_cameraManager != null)
                {
                    if (_cameraManager.IsCameraConnected())
                    {
                        pictureTaken = _cameraManager.TakePicture();
                    }
                }
            }
            else
            {
                if (_sonyCamera != null)
                {
                    try
                    {
                        var func = await _sonyCamera.GetCameraFunctionAsync();

                        if (!func.Equals("Remote Shooting"))
                            await _sonyCamera.SetCameraFunctionAsync("Remote Shooting");

                        var pictureUrl = await _sonyCamera.ActTakePictureAsync();
                        //
                        if (pictureUrl.Count > 0)
                        {
                            var fileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + GenerateUniqueId.GetId() + ".jpg";
                            DownloadFile(pictureUrl[0], fileName);
                            ChangePictureMethod(fileName);
                            pictureTaken = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }

            if (pictureTaken) return;

            MessageBox.Show("No camera detected. Using stock picture.", "iBrand", MessageBoxButtons.OK);

            ChangePictureMethod(DEFAULT_IMAGE);
        }

        private static void DownloadFile(string urlAddress, string location)
        {

            var webClient = new WebClient();
            //AsyncCompletedEventHandler completedEvent = new AsyncCompletedEventHandler(Completed);
            try
            {

                //webClient.DownloadFileCompleted += completedEvent;

                // The variable that will be holding the url address (making sure it starts with http://)
                var url = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);


                // Start downloading the file
                webClient.DownloadFile(url, location);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //webClient.DownloadFileCompleted -= completedEvent;
                webClient.Dispose();
            }

        }

        private void ButtonSaveToDisk_Click(object sender, EventArgs e)
        {
            _blockChanges = true;
            SaveChromaKeyConfigToDisk();

            _blockChanges = false;
        }

        /// <summary>
        /// Sauvegarde la configuration de clé chromatique sur disque.
        /// </summary>
        protected abstract void SaveChromaKeyConfigToDisk();

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                Close();
            }
        }

        /// <summary>
        /// Retourne l'instance du PictureBox de transformation.
        /// Si on utilise une sous-classe ou si on attache des événements à l'instance, on peut surcharger cette méthode.
        /// </summary>
        /// <returns></returns>
        protected virtual PictureBox GetTransformedPictureBox()
        {
            return new PictureBox();
        }

        private void OnFullPreviewClick(object sender, EventArgs e) {
            var bmp = GetFullPreviewKeyedImage();
            if (bmp == null) return;
            var diag = new PreviewDialog(bmp);
            diag.ShowDialog();
        }

        /// <summary>
        /// Obtient l'image originale avec les transformations de clé chromatique.
        /// </summary>
        /// <returns>Un Btmap à afficher. Si on ne peut pas faire le keying, on retourne null.</returns>
        protected abstract Bitmap GetFullPreviewKeyedImage();

        private void OnSavePreviewClick(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(_sourceBitmapFile)) return;
            var saveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + GenerateUniqueId.GetId() + ".png";
            SavePreview(saveFilePath);
            if (File.Exists(saveFilePath)) { MessageBox.Show(this, 
                "Image preview size saved on Desktop.", 
                "iBrand", MessageBoxButtons.OK); }
        }

        /// <summary>
        /// Sauvegarde le fichier original avec les paramètres de clé actuel sur disque.
        /// </summary>
        /// <param name="path">Le chemin de fichier à utiliser.</param>
        protected abstract void SavePreview(string path);

        private void btnLoadPicture_Click(object sender, EventArgs e)
        {
            ShowDirectoryPicker();
        }
        
        private void ShowDirectoryPicker()
        {
            var newThread = new Thread(() =>
            {
                var diag = new OpenFileDialog();
                if (diag.ShowDialog() != DialogResult.OK) return;
                FormUtil.SafeCall(this, () =>
                {
                    ChangePictureMethod(diag.FileName);
                });
            });
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }
    }
}
