using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework.Internal.SDK;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util.Camera.Canon.CameraProperty;

namespace NomadLogic.Common.Ui.CameraRing
{
    public partial class MultipleCameraSettingsDialog : Form
    {

        private volatile bool _closing;

        private readonly object _lock = new object();

        public delegate void RemoveCameraDelegate(EosCamera camera);

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        EosCamera _selectedCamera;

        Iso _iso;
        private AeMode _aeMode;
        private Av _av;
        private ExposureComp _exposureComp;
        private ImageQuality _imageQuality;
        private MeteringMode _meteringMode;
        private Tv _tv;
        private WhiteBalance _whiteBalance;

        private readonly Dictionary<string, EosCamera> _cameras;

        public CopyCameraSettings OnCopyCameraSettings;

        public MultipleCameraSettingsDialog(Dictionary<string, EosCamera> cameras)
        {
            _cameras = cameras;

            InitializeComponent();

            PopulateCameraComboBox();

            AddListeners();
        }

        private void PopulateCameraComboBox()
        {
            var cameraName = new List<string>(_cameras.Keys);

            cameraName.Sort(delegate (string x, string y)
            {
                int numX;
                var parsed = int.TryParse(x, out numX);

                if (!parsed) return 0;

                int numY;
                parsed = int.TryParse(y, out numY);

                if (!parsed) return 1;

                if (numX == numY) return 0;

                if (numX < numY) return -1;

                return 1;

            });

            PopulateCombo(camerasComboBox, cameraName, null);

            camerasComboBox.SelectedIndexChanged += OnCameraSelectionChanged;
        }

        private void OnCameraSelectionChanged(object sender, EventArgs e)
        {
            try
            {

                if (_selectedCamera != null)
                {
                    _selectedCamera.LiveViewUpdate -= HandlePictureUpdate;
                    _selectedCamera.FileSavedOnHost -= HandlePictureUpdate;

                    if (_selectedCamera.IsInLiveViewMode)
                        _selectedCamera.StopLiveView();
                }

                if (camerasComboBox.SelectedItem != null)
                {

                    _selectedCamera = _cameras[(string)camerasComboBox.SelectedItem];
                    _selectedCamera.LiveViewUpdate += HandlePictureUpdate;
                    _selectedCamera.FileSavedOnHost += HandlePictureUpdate;

                    InitProperties(_selectedCamera);

                    OnStartEvfClicked(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }

            pictureBox.Image = null;
            pictureBox.Invalidate();
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
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed)
                UpdatePicture(e.GetImage());
        }

        public void OnCameraShutdown(object sender, EventArgs args)
        {
            if (sender is EosCamera camera)
            {
                RemoveCamera(camera);
            }
        }

        private void RemoveCamera(EosCamera camera)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new RemoveCameraDelegate(RemoveCamera), camera);
                return;
            }

            if (_selectedCamera != null && _selectedCamera.Author.Equals(camera.Author))
            {
                _selectedCamera.LiveViewUpdate -= HandlePictureUpdate;
                _selectedCamera.FileSavedOnHost -= HandlePictureUpdate;

                pictureBox.Image = null;
                pictureBox.Invalidate();

                ClearCombos();

                _selectedCamera = null;

                camerasComboBox.Text = null;
            }

            _cameras.Remove(camera.Author);
            camerasComboBox.Items.RemoveAt(camerasComboBox.Items.IndexOf(camera.Author));
        }

        private void UpdatePicture(Image image)
        {
            lock (_lock)
            {
                if (_closing)
                    return;
            }

            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdatePicture(image)));
                }
                else
                {
                    var finalBitmap = image.Resize(pictureBox.Width, pictureBox.Height, null, g =>
                    {
                        var pen = new Pen(Color.Red) { Width = 1 };
                        g.DrawLine(pen, new Point(pictureBox.Width / 2, 0), new Point(pictureBox.Width / 2, pictureBox.Height));
                        g.DrawLine(pen, new Point(0, pictureBox.Height / 2), new Point(pictureBox.Width, pictureBox.Height / 2));
                    });

                    pictureBox.Image = finalBitmap;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
        
        private void OnFocusButtonClicked(uint parameter)
        {
            if (_selectedCamera == null) return;
            var resp = _selectedCamera.SendCommand(Edsdk.CameraCommand_DriveLensEvf,
                (int)parameter);
            if (Edsdk.EDS_ERR_OK != resp)
            {
                log.Error("Send Command Drive Lens error");
            }
        }
        
        private void OnStartEvfClicked(object sender, EventArgs e)
        {
            if (_selectedCamera == null) return;
            meteringComboBox.Enabled = false;
            stopEvfButton.Enabled = true;
            takePictureButton.Enabled = false;
            startEvfButton.Enabled = false;

            try
            {

                _selectedCamera.StartLiveView();
            }
            catch (Exception ex)
            {
                meteringComboBox.Enabled = true;
                stopEvfButton.Enabled = false;
                takePictureButton.Enabled = true;
                startEvfButton.Enabled = true;
                try
                {
                    _selectedCamera.StopLiveView();
                }
                catch (Exception)
                {
                    // ignored
                }
                MsgBox.Show(this, ex.Message, Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnTakePictureClicked(object sender, EventArgs e)
        {
            try
            {
                _selectedCamera?.TakePicture();
            }
            catch (Exception ex)
            {
                MsgBox.Show(this, ex.Message, Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCopyCameraSettingsClicked(object sender, EventArgs e)
        {
            if (OnCopyCameraSettings == null || _selectedCamera == null) return;
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
            
            OnCopyCameraSettings(properties, _selectedCamera.Author);
        }

        private void OnStopEvfClicked(object sender, EventArgs e)
        {
            if (_selectedCamera == null) return;
            meteringComboBox.Enabled = true;
            stopEvfButton.Enabled = false;
            takePictureButton.Enabled = true;
            startEvfButton.Enabled = true;

            try
            {

                _selectedCamera.StopLiveView();
            }
            catch (Exception ex)
            {
                MsgBox.Show(this, ex.Message, Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void OnIsoSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)isoComboBox.SelectedItem;
                _iso.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnAeModeSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)aeModeComboBox.SelectedItem;
                _aeMode.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnAvSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)avComboBox.SelectedItem;
                _av.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnExposureCompSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)exposureCompComboBox.SelectedItem;
                _exposureComp.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnImageQualitySelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)imageQualityComboBox.SelectedItem;
                _imageQuality.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnMeteringModeSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)meteringComboBox.SelectedItem;
                _meteringMode.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnTvSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)tvComboBox.SelectedItem;
                _tv.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnWhiteBalanceSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)whiteBalanceComboBox.SelectedItem;
                _whiteBalance.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void InitProperties(EosCamera camera)
        {
            _iso = new Iso(camera);
            _aeMode = new AeMode(camera);
            _av = new Av(camera);
            _exposureComp = new ExposureComp(camera);
            _imageQuality = new ImageQuality(camera);
            _meteringMode = new MeteringMode(camera);
            _tv = new Tv(camera);
            _whiteBalance = new WhiteBalance(camera);


            PopulateCombo(isoComboBox, _iso.SupportedDisplayValues, _iso.SelectedValue);
            PopulateCombo(aeModeComboBox, _aeMode.SupportedDisplayValues, _aeMode.SelectedValue);
            PopulateCombo(tvComboBox, _tv.SupportedDisplayValues, _tv.SelectedValue);
            PopulateCombo(avComboBox, _av.SupportedDisplayValues, _av.SelectedValue);
            PopulateCombo(meteringComboBox, _meteringMode.SupportedDisplayValues, _meteringMode.SelectedValue);
            PopulateCombo(exposureCompComboBox, _exposureComp.SupportedDisplayValues, _exposureComp.SelectedValue);
            PopulateCombo(imageQualityComboBox, _imageQuality.SupportedDisplayValues, _imageQuality.SelectedValue);
            PopulateCombo(whiteBalanceComboBox, _whiteBalance.SupportedDisplayValues, _whiteBalance.SelectedValue);
        }

        private void ClearCombos()
        {
            isoComboBox.Items.Clear();
            isoComboBox.Text = null;

            aeModeComboBox.Items.Clear();
            aeModeComboBox.Text = null;

            tvComboBox.Items.Clear();
            tvComboBox.Text = null;

            avComboBox.Items.Clear();
            avComboBox.Text = null;

            meteringComboBox.Items.Clear();
            meteringComboBox.Text = null;

            exposureCompComboBox.Items.Clear();
            exposureCompComboBox.Text = null;

            imageQualityComboBox.Items.Clear();
            imageQualityComboBox.Text = null;

            whiteBalanceComboBox.Items.Clear();
            whiteBalanceComboBox.Text = null;
        }

        private static void PopulateCombo(ComboBox comboBox, IReadOnlyCollection<string> values, string selectedValue)
        {
            comboBox.Items.Clear();
            comboBox.Text = null;

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
                    if (value.Equals(selectedValue))
                    {
                        index = nbr;
                    }
                }

                comboBox.Enabled = true;
                comboBox.SelectedIndex = index;
            }

        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            lock (_lock)
            {
                _closing = true;
            }

            if (_selectedCamera == null) return;
            try
            {
                _selectedCamera.LiveViewUpdate -= HandlePictureUpdate;
                _selectedCamera.FileSavedOnHost -= HandlePictureUpdate;

                if (_selectedCamera.IsInLiveViewMode)
                    _selectedCamera.StopLiveView();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                _selectedCamera = null;
            }
        }

    }
}
