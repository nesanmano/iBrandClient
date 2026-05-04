using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Camera;
using NomadLogic.Common.Util.Camera.Canon;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Multimedia.Gpu.Ui.BackgroundPictureDialog
{
    public partial class BackgroundPictureManagerDialog : Form, IPictureListener
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string NEW_BASE_PICTURE_REQUEST = "newBase";
        private const string NEW_OVERLAY_PICTURE_REQUEST = "newOverlay";

        /// <summary>
        /// Délégué pour activer/désactiver les boutons.
        /// </summary>
        /// <param name="buttons">L'ensemble des boutons à activer/déactiver.</param>
        /// <param name="state">L'état d'activation voulu.</param>
        /// <param name="postStateAction">Action à exécuter une fois l'état modifié.</param>
        protected delegate void ChangeButtonState(Button[] buttons, bool state, Action postStateAction);

        private CameraControllerCanon _cameraController;
        private string _originalBkg;
        private string _currentBkg;
        private string _currentOverlay;
        private Bitmap _currentCompositeBmp;
        private bool _blockChanges;
        private string _keyedFile;

        public BackgroundPictureManagerDialog()
        {
            InitializeComponent();

            picCurrentBackground.SizeMode = PictureBoxSizeMode.Zoom;
            picOverlay.SizeMode = PictureBoxSizeMode.Zoom;
            picComposite.SizeMode = PictureBoxSizeMode.Zoom;

            FormClosing += CheckForSave;

            UpdateButtonUi(true);
        }

        private void CheckForSave(object sender, FormClosingEventArgs e)
        {
            if (_currentBkg == null || Equals(_originalBkg, _currentBkg)) return;

            _blockChanges = true;
            var result = MessageBox.Show("A new background image was taken. Use it as new global background image?", "Background image changed", 
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (result == DialogResult.Yes)
            {
                try
                {
                    var basePicFilePath = GetBaseTransformationFilePath();
                    File.WriteAllText(basePicFilePath, _currentBkg);
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                    MessageBox.Show("Could not save new base picture. Error message:\n" + exception.Message, "Error.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                FormClosing -= CheckForSave;
            }

            _blockChanges = false;
        }

        private static string GetBaseTransformationFilePath()
        {
            var tmp = DirectoryUtil.GetLocalNomadLogicDir() + "BasePictureFileInfo.txt";
            return tmp;
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _cameraController = CameraControllerCanon.GetInstance();
            _cameraController.Init();

            var tmp = GetBaseTransformationFilePath();
            if (!File.Exists(tmp)) return;
            _originalBkg = File.ReadAllText(tmp);
            if (!File.Exists(_originalBkg))
            {
                File.Delete(tmp);
            }
            else
            {
                SetNewBasePicture(_originalBkg);
            }
        }

        private void ChangePictureMethod(PictureBox pBox, Bitmap newPicture)
        {
            try
            {
                var newSize = MathUtil.GetNewSizeToFitInsideContainer(pBox.Width, pBox.Height, newPicture.Width, newPicture.Height);

                var copy = newPicture.Resize((int)newSize.Width, (int)newSize.Height);

                pBox.Image?.Dispose();
                pBox.Image = copy;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateButtonUi(bool activate)
        {
            _blockChanges = !activate;
            btnLoadFromDisk.Enabled = activate;
            btnTakeBkg.Enabled = activate;
            btnTakeOverlay.Enabled = activate;
            btnSaveComposite.Enabled = activate && _currentCompositeBmp != null;
            btnViewFullScreen.Enabled = activate && _currentCompositeBmp != null;
        }

        private void OnBtnClick(object sender, EventArgs e)
        {
            if (_blockChanges) return;

            if (sender == btnLoadFromDisk)
            {
                UpdateButtonUi(false);
                var newThread = new Thread(() =>
                {
                    var diag = new OpenFileDialog();
                    if (diag.ShowDialog() == DialogResult.OK && File.Exists(diag.FileName))
                    {
                        FormUtil.SafeCall(this, () =>
                        {
                            SetNewBasePicture(diag.FileName);
                        });
                    }
                    FormUtil.SafeCall(this, () =>
                    {
                        UpdateButtonUi(true);
                    });
                });
                newThread.SetApartmentState(ApartmentState.STA);
                newThread.Start();
            }
            else if (sender == btnTakeBkg)
            {
                TakePicture(NEW_BASE_PICTURE_REQUEST);
            }
            else if (sender == btnSaveComposite)
            {
                if (_currentCompositeBmp == null) return;
                var saveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + GenerateUniqueId.GetId() + ".png";
                _currentCompositeBmp.Save(ImageFormat.Png, 9, saveFilePath);
                if (File.Exists(saveFilePath))
                {
                    MessageBox.Show(this,
                        "Composite preview saved on Desktop.",
                        "iBrand", MessageBoxButtons.OK);
                }
            }
            else if (sender == btnTakeOverlay)
            {
                TakePicture(NEW_OVERLAY_PICTURE_REQUEST);
            }
            else if (sender == btnViewFullScreen)
            {
                UpdateButtonUi(false);
                if (_currentCompositeBmp == null) return;
                var diag = new PreviewDialog(_currentCompositeBmp);
                diag.ShowDialog();
                UpdateButtonUi(true);
            }
        }

        private void TakePicture(string requestType)
        {
            UpdateButtonUi(false);
            if (_cameraController.CurrentState() == CameraControllerStateEnum.Initialized)
            {
                var request = new PictureRequestSingle
                {
                    PictureListener = this,
                    PictureToTakeNbr = 1,
                    RequestTag = requestType
                };
                _cameraController.TakePictures(request);
            }
            else
            {
                MessageBox.Show(this,
                    "Canon camera manager is not initialized. Cannot take picture.",
                    "iBrand", MessageBoxButtons.OK);
                UpdateButtonUi(true);
            }
        }

        public void OnForcedUnregister()
        {
            MessageBox.Show(this,
                "Canon camera manager went offline.",
                "iBrand", MessageBoxButtons.OK);
        }

        public void PictureRequestCompleted(IPictureRequest request)
        {
            SetNewPicture(request);
        }

        public void PictureRequestCancelled(IPictureRequest request)
        {
            
            SetNewPicture(request);
        }

        public void PictureTaken(string filePath, string[] cameraId)
        {
            SetNewOverlayPicture(filePath);
            FormUtil.SafeCall(this, () => { UpdateButtonUi(true); });
        }

        private void SetNewPicture(IPictureRequest request)
        {
            try
            {
                if (request.Cancelled)
                {
                    log.Warn("Picture request was cancelled.");
                    return;
                }

                if (request.ErrorDetected)
                {
                    FormUtil.SafeCall(this, () => {
                        MessageBox.Show(this,
                        "Error during picture capture. Please try again.",
                        "iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                    log.Warn("Error during picture request.");
                    return;
                }

                var picture = request.PicturesTaken[0][0];

                if (request.RequestTag.Equals(NEW_BASE_PICTURE_REQUEST))
                {
                    SetNewBasePicture(picture.FullName);
                }
                else
                {
                    SetNewOverlayPicture(picture.FullName);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                FormUtil.SafeCall(this, () => {
                    MessageBox.Show(this,
                    "Error during picture event. Message:\n" + e.Message,
                    "iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            finally
            {
                FormUtil.SafeCall(this, () => { UpdateButtonUi(true); });
            }
        }

        private void SetNewBasePicture(string pictureFullName)
        {
            BitmapExtentions.MatchExifData(pictureFullName);
            _currentBkg = pictureFullName;
            FormUtil.SafeCall(this, () => { ChangePictureMethod(picCurrentBackground, FileUtil.LoadBitmap(_currentBkg)); });

            if (_currentOverlay == null) return;

            UpdateCompositeImage();
        }

        private void UpdateCompositeImage()
        {
            if (_currentBkg == null && _keyedFile == null) return;
            
            try
            {
                // Si on a accès aux deux Bitmap, on peut faire la composition.
                _currentCompositeBmp?.Dispose();
                // Quand on aura un masque de composition.
                //_currentCompositeBmp = ImageUtil.CreateBaseCompositeImage(_currentBkg, _currentOverlay);

                // Temporaire, just un overlay
                using (var overlay = FileUtil.LoadBitmap(_keyedFile))
                {
                    _currentCompositeBmp = FileUtil.LoadBitmap(_currentBkg);
                    _currentCompositeBmp.AddOverlay(overlay);
                }

                FormUtil.SafeCall(this, () => { ChangePictureMethod(picComposite, _currentCompositeBmp); });
            }
            catch (Exception e)
            {
                log.Error(e);
                FormUtil.SafeCall(this, () => {
                    MessageBox.Show("Error during composite image creation.", "iBrand",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private void SetNewOverlayPicture(string filePath)
        {
            BitmapExtentions.MatchExifData(filePath);

            var keyedFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
            
            ChromaKeyManager.ChromaKey(filePath, keyedFile);

            if (_keyedFile != null)
            {
                File.Delete(_keyedFile);
            }
            
            if (!File.Exists(keyedFile))
            {
                _keyedFile = null;
                FormUtil.SafeCall(this, () => {
                    MessageBox.Show("Could not create keyed overlay image.", "iBrand",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
                return;
            }
            
            _keyedFile = keyedFile;
            
            _currentOverlay = filePath;
            FormUtil.SafeCall(this, () => { ChangePictureMethod(picOverlay, FileUtil.LoadBitmap(_keyedFile)); });

            if (_currentBkg == null) return;

            UpdateCompositeImage();
        }
    }
}
