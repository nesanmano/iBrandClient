using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework.Internal.SDK;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util.Camera.Canon.CameraProperty;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Common.Ui.CameraRing
{
    public delegate void CopyCameraSettings(List<AbstractCameraProperty> properties, string cameraNo);
    
    [PublicAPI]
    public partial class CameraSettingsDialog : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly EosCamera _camera;

        private readonly Iso _iso;
        private readonly AeMode _aeMode;
        private readonly Av _av;
        private readonly ExposureComp _exposureComp;
        private readonly ImageQuality _imageQuality;
        private readonly MeteringMode _meteringMode;
        private readonly Tv _tv;
        private readonly WhiteBalance _whiteBalance;

        private readonly string _cameraNo;

        private volatile bool _livePreviewStopped = true;

        public CopyCameraSettings OnCopyCameraSettings;

        public CameraSettingsDialog(EosCamera camera)
        {
            _camera = camera;

            if (_camera.IsInLiveViewMode)
                _camera.StopLiveView();

            _iso = new Iso(_camera);
            _aeMode = new AeMode(_camera);
            _av = new Av(_camera);
            _exposureComp = new ExposureComp(_camera);
            _imageQuality = new ImageQuality(_camera);
            _meteringMode = new MeteringMode(_camera);
            _tv = new Tv(_camera);
            _whiteBalance = new WhiteBalance(_camera);

            _cameraNo = _camera.Author;

            InitializeComponent();
            InitProperties();
            AddListeners();
        }

        private void AddListeners()
        {
            isoComboBox.SelectedIndexChanged += OnIsoSelectionChanged;
            aeModeComboBox.SelectedIndexChanged += OnAeModeSelectionChanged;
            tvComboBox.SelectedIndexChanged += OnTvSelectionChanged;
            avComboBox.SelectedIndexChanged += OnAvSelectionChanged;
            meteringComboBox.SelectedIndexChanged += OnMeteringModeSelectionChanged;
            exposureCompComboBox.SelectedIndexChanged += OnExposureCompSelectionChanged;
            imageQualityComboBox.SelectedIndexChanged += OnImageQualitySelectionChanged;
            whiteBalanceComboBox.SelectedIndexChanged += OnWhiteBalanceSelectionChanged;

            startEvfButton.Click += OnStartEvfClicked;
            stopEvfButton.Click += OnStopEvfClicked;
            takePictureButton.Click += OnTakePictureClicked;
            copyPropertiesButton.Click += OnCopyCameraSettingsClicked;

            focus2LeftButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Near3); };
            focusLeftButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Near2); };
            focus2RightButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Far3); };
            focusRightButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Far2); };

            _camera.LiveViewUpdate += HandlePictureUpdate;
            _camera.FileSavedOnHost += HandlePictureUpdate;
            _camera.LiveViewStopped += OnLiveViewStopped;

        }

        private void OnLiveViewStopped(object sender, EventArgs e)
        {
            _livePreviewStopped = true;
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (IsDisposed) return;

            var img = e.GetImage();
            UpdatePicture(img);
        }

        private void UpdatePicture(Image image)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePicture(image)));
                return;
            }

            if (image == null)
            {
                log.Error("Image reference was null. Skipping.");
                //this.SafeCall(() => MsgBox.Show(this, "Image received from camera was null. Please check settings.", "Problem with camera.", MessageBoxButtons.OK, MessageBoxIcon.Hand));
                return;
            }

            var finalBitmap = ResizeImage(image, pictureBox.Width, pictureBox.Height);
            pictureBox.Image = finalBitmap;
        }

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            var result = new Bitmap(width, height);
            // Set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                //Set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
                graphics.Dispose();
            }

            return result;
        }

        private void OnFocusButtonClicked(uint parameter)
        {
            var resp = _camera.SendCommand(Edsdk.CameraCommand_DriveLensEvf,
                                 (int)parameter);
            if (Edsdk.EDS_ERR_OK != resp)
            {
                log.Error("Send Command Drive Lens error");
            }
        }

        private void OnStartEvfClicked(object sender, EventArgs e)
        {
            meteringComboBox.Enabled = false;
            stopEvfButton.Enabled = true;
            takePictureButton.Enabled = false;
            startEvfButton.Enabled = false;

            _livePreviewStopped = false;

            try
            {
                _camera.StartLiveView();
            }
            catch (Exception ex)
            {
                meteringComboBox.Enabled = true;
                stopEvfButton.Enabled = false;
                takePictureButton.Enabled = true;
                startEvfButton.Enabled = true;
                
                _livePreviewStopped = true;

                MsgBox.Show(this, ex.Message, Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnTakePictureClicked(object sender, EventArgs e)
        {
            try
            {
                _camera.TakePicture();
            }
            catch (Exception ex)
            {
                MsgBox.Show(this, ex.Message, Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCopyCameraSettingsClicked(object sender, EventArgs e)
        {
            if (OnCopyCameraSettings == null) return;
            var properties = new List<AbstractCameraProperty>
            {
                _iso,
                _aeMode,
                _av,
                _exposureComp,
                _imageQuality,
                _meteringMode,
                _tv,
                _whiteBalance
            };


            OnCopyCameraSettings(properties, _cameraNo);
        }

        private void OnStopEvfClicked(object sender, EventArgs e)
        {
            meteringComboBox.Enabled = true;
            stopEvfButton.Enabled = false;
            takePictureButton.Enabled = true;
            startEvfButton.Enabled = true;

            try
            {
                _camera.StopLiveView();
            }
            catch (Exception ex)
            {
                MsgBox.Show(this, ex.Message, Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnIsoSelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)isoComboBox.SelectedItem;
                _iso.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnAeModeSelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)aeModeComboBox.SelectedItem;
                _aeMode.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnAvSelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)avComboBox.SelectedItem;
                _av.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnExposureCompSelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)exposureCompComboBox.SelectedItem;
                _exposureComp.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnImageQualitySelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)imageQualityComboBox.SelectedItem;
                _imageQuality.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnMeteringModeSelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)meteringComboBox.SelectedItem;
                _meteringMode.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnTvSelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)tvComboBox.SelectedItem;
                _tv.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnWhiteBalanceSelectionChanged(object sender, EventArgs e)
        {
            try {
                var value = (string)whiteBalanceComboBox.SelectedItem;
                _whiteBalance.UpdateSelectedValue(_camera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void InitProperties()
        {
            cameraNoTextBox.Text = _cameraNo;
            
            PopulateCombo(isoComboBox, _iso.SupportedDisplayValues, _iso.SelectedValue);
            PopulateCombo(aeModeComboBox, _aeMode.SupportedDisplayValues, _aeMode.SelectedValue);
            PopulateCombo(tvComboBox, _tv.SupportedDisplayValues, _tv.SelectedValue);
            PopulateCombo(avComboBox, _av.SupportedDisplayValues, _av.SelectedValue);
            PopulateCombo(meteringComboBox, _meteringMode.SupportedDisplayValues, _meteringMode.SelectedValue);
            PopulateCombo(exposureCompComboBox, _exposureComp.SupportedDisplayValues, _exposureComp.SelectedValue);
            PopulateCombo(imageQualityComboBox, _imageQuality.SupportedDisplayValues, _imageQuality.SelectedValue);
            PopulateCombo(whiteBalanceComboBox, _whiteBalance.SupportedDisplayValues, _whiteBalance.SelectedValue);
        }

        private static void PopulateCombo(ComboBox comboBox, IReadOnlyCollection<string> values, string selectedValue)
        {
            comboBox.Items.Clear();

            if (values.Count == 0)
            {
                comboBox.Enabled = false;
            }
            else
            {
                var index = -1;
                var nbr = -1;
                foreach (var value in values)
                {
                    nbr++;
                    comboBox.Items.Add(value);
                    if (selectedValue.Equals(value))
                    {
                        index = nbr;
                    }
                }

                comboBox.SelectedIndex = index;
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_livePreviewStopped)
            {
                MsgBox.Show(this, "You must stop live view before exit.", Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }
            
            if (!_cameraNo.Equals(cameraNoTextBox.Text))
            {
                _camera.Artist = cameraNoTextBox.Text;
            }

            _camera.LiveViewUpdate -= HandlePictureUpdate;
            _camera.FileSavedOnHost -= HandlePictureUpdate;
            
            base.OnFormClosing(e);
        }

    }
}
