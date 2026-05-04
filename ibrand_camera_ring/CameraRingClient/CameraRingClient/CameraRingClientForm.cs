using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework.Internal.SDK;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Camera;
using NomadLogic.Common.Util.Camera.Canon;
using NomadLogic.Common.Util.Camera.Canon.CameraProperty;

namespace NomadLogic.iBrand.CameraRingClient
{
    /// <summary>
    /// Forme principale pour l'exécution du client du Camera Ring
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public partial class CameraRingClientForm : Form, IPictureListener
    {
        internal class CameraComboItem : IComparable<CameraComboItem>
        {
            internal CameraComboItem(EosCamera cam)
            {
                Value = cam;
            }

            internal EosCamera Value { get; }
            public bool IsDuplicate { get; set; }

            public override bool Equals(object other)
            {
                return other is CameraComboItem arg && Value.SerialNumber.Equals(arg.Value.SerialNumber);
            }

            public override int GetHashCode()
            {
                return Value.SerialNumber.GetHashCode();
            }

            public int CompareTo(CameraComboItem other)
            {
                var x = Value.Author;
                var y = other.Value.Author;

                var parsed = int.TryParse(x, out var numX);

                if (!parsed) return 0;

                parsed = int.TryParse(y, out var numY);

                if (!parsed) return 1;

                if (numX == numY) return 0;

                if (numX < numY) return -1;

                return 1;
            }

            public override string ToString()
            {
                return Value.Author;
            }
        }

        private volatile bool _closing;

        private readonly object _lock = new object();

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private EosCamera _selectedCamera;

        private readonly CameraSettingsManagerCanon _cameraSettingsManager = new CameraSettingsManagerCanon();

        private readonly Dictionary<string, EosCamera> _cameras;
        private readonly CameraControllerCanon _manager;
        private CameraRingFileChecker _cameraFileCheck;
        private CameraRingNetworkManager _networkManager;
        private volatile bool _duplicatesDetected;

        private readonly List<string> _lastMessages = new List<string>();
        private readonly CameraRingConfiguration _config;
        private int _nbrFileChecked;
        private CameraPropertiesGroup _currentPropertiesGroup;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        public CameraRingClientForm()
        {
            if (File.Exists(CameraRingConfiguration.DefaultFileLocation))
            {
                _config = FileUtil.ReadJsonFile<CameraRingConfiguration>(new FileInfo(CameraRingConfiguration.DefaultFileLocation));
            }
            else
            {
                _config = new CameraRingConfiguration();
                FileUtil.WriteJsonFile(new FileInfo(CameraRingConfiguration.DefaultFileLocation), _config);
            }

            _manager = CameraControllerCanon.GetInstance();
            if (!_manager.Init()) throw new Exception("Could not initialize Camera Controller for Canon cameras.");

            _cameras = _manager.GetCurrentEosCameras();
            log.Trace("Initial cameras collected.");
            _manager.NewCameraEvent += OnNewCameraEvent;
            _manager.RegisterForNewPictureEvent(this);
            _manager.PictureSaveDirectory = new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir());
            CleanWorkDir();

            foreach (var eosCamera in _cameras)
            {
                eosCamera.Value.Shutdown += OnCameraShutdown;
                _cameraSettingsManager.UpdateSettingsForCamera(eosCamera.Value);
            }

            InitializeComponent();

            PreviousCameraButton.Enabled = false;
            NextCameraButton.Enabled = false;

            PopulateCameraComboBox(false);
            camerasComboBox.SelectionChangeCommitted += OnCameraSelectionChanged;

            /* Voir ixi pour plus d'information sur le Draw d'un combobox
             * http://stackoverflow.com/questions/4667532/colour-individual-items-in-a-winforms-combobox
             */
            camerasComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            camerasComboBox.DrawItem += OnCameraComboBoxDraw;

            if (camerasComboBox.Items.Count > 0)
            {
                camerasComboBox.SelectedIndex = 0;
                OnCameraSelectionChanged(null, null);
            }

            InitPropertyComboBox(isoComboBox, OnIsoSelectionChanged);
            InitPropertyComboBox(aeModeComboBox, OnAeModeSelectionChanged);
            InitPropertyComboBox(tvComboBox, OnTvSelectionChanged);
            InitPropertyComboBox(avComboBox, OnAvSelectionChanged);
            InitPropertyComboBox(meteringComboBox, OnMeteringModeSelectionChanged);
            InitPropertyComboBox(exposureCompComboBox, OnExposureCompSelectionChanged);
            InitPropertyComboBox(imageQualityComboBox, OnImageQualitySelectionChanged);
            InitPropertyComboBox(whiteBalanceComboBox, OnWhiteBalanceSelectionChanged);

            startEvfButton.Click += OnStartEvfClicked;
            stopEvfButton.Click += OnStopEvfClicked;
            copyPropertiesButton.Click += OnCopyCameraSettingsClicked;
            toolStripStatusLabelStatusNetwork.Click += OnSetLocalAddress;
            toolStripStatusLabelLastTimeStamp.Click += OnNetworkStatusLabelClick;

            focus2LeftButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Near3); };
            focusLeftButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Near2); };
            focus2RightButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Far3); };
            focusRightButton.Click += delegate { OnFocusButtonClicked(Edsdk.EvfDriveLens_Far2); };
        }

        private void InitPropertyComboBox(ComboBox propertyComboBox, EventHandler selectionChangedAction)
        {
            propertyComboBox.SelectionChangeCommitted += selectionChangedAction;
            propertyComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            propertyComboBox.DrawItem += OnPropertyComboBoxDraw;
        }

        private void CleanWorkDir()
        {
            var minTime = DateTime.Now.AddMinutes(-5);
            foreach (var file in FileUtil.GetFiles(_manager.PictureSaveDirectory.FullName, "*.*"))
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < minTime)
                {
                    fileInfo.Delete();
                }
            }
        }

        private static void OnCameraComboBoxDraw(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            var comboObject = ((ComboBox)sender).Items[e.Index];
            var text = comboObject.ToString();

            var brush = Brushes.Black;
            if (comboObject is CameraComboItem camItem && camItem.IsDuplicate)
            {
                brush = Brushes.DarkRed;
            }

            e.Graphics.DrawString(text, ((Control)sender).Font, brush, e.Bounds.X, e.Bounds.Y);
        }

        private void OnPropertyComboBoxDraw(object sender, DrawItemEventArgs e)
        {
            var comboObject = ((ComboBox)sender).Items[e.Index];

            AbstractCameraProperty propToCheck = null;
            if (sender == aeModeComboBox)
            {
                propToCheck = _currentPropertiesGroup.AeMode;
            } else if (sender == isoComboBox)
            {
                propToCheck = _currentPropertiesGroup.Iso;
            }
            else if (sender == tvComboBox)
            {
                propToCheck = _currentPropertiesGroup.Tv;
            }
            else if (sender == avComboBox)
            {
                propToCheck = _currentPropertiesGroup.Av;
            }
            else if (sender == meteringComboBox)
            {
                propToCheck = _currentPropertiesGroup.MeteringMode;
            }
            else if (sender == exposureCompComboBox)
            {
                propToCheck = _currentPropertiesGroup.ExposureComp;
            }
            else if (sender == imageQualityComboBox)
            {
                propToCheck = _currentPropertiesGroup.ImageQuality;
            }
            else if (sender == whiteBalanceComboBox)
            {
                propToCheck = _currentPropertiesGroup.WhiteBalance;
            }

            var text = comboObject.ToString();

            e.DrawBackground();
            if (propToCheck != null && !_cameraSettingsManager.IsValueSharedByAllActiveGroups(propToCheck.PropertyId, text))
            {
                var rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.FillRectangle(new SolidBrush(Color.DarkSalmon), rectangle);
            }
            
            e.Graphics.DrawString(text, ((Control)sender).Font, Brushes.Black, e.Bounds.X, e.Bounds.Y);
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ChangeStatusScriptMessage(Resources.Text.CameraRingClientForm_No_Timestamp, false);

            // Initialise et lance le manager réseau. A besoin d'un handle, donc doit être invoué dans cette méthode.
            _networkManager = new CameraRingNetworkManager
            {
                UpdateStatus = (status, text) =>
                {
                    Invoke(new Action(() =>
                    {
                        Bitmap bmp;
                        switch (status)
                        {
                            case CameraRingNetworkManager.NetworkStatusEnum.Connected:
                                bmp = Properties.Resources.green_signal;
                                break;
                            case CameraRingNetworkManager.NetworkStatusEnum.Error:
                                bmp = Properties.Resources.red_signal;
                                break;
                            default:
                                bmp = Properties.Resources.blue_signal;
                                break;
                        }
                        toolStripStatusLabelStatusNetwork.Image = bmp;
                        toolStripStatusLabelStatusNetwork.Text = text;
                    }));
                }
            };
            _networkManager.Start();
            log.Trace("Network manager started.");

            CheckAndWarnForDuplicateAuthor();
        }

        private void CheckAndWarnForDuplicateAuthor()
        {
            var duplicates = new List<string>();

            CameraComboItem previous = null;
            foreach (var item in camerasComboBox.Items)
            {
                if (!(item is CameraComboItem camItem)) continue;

                if (previous != null && previous.Value.Author.Equals(camItem.Value.Author))
                {
                    camItem.IsDuplicate = true;
                    duplicates.Add(camItem.Value.Author);
                }

                previous = camItem;
            }

            var sb = new StringBuilder();
            _duplicatesDetected = duplicates.Any();

            if (_duplicatesDetected)
            {
                sb.Append(Resources.Text.CustomServerConfigurationDialog_Label_DuplicateDetected);
                const string delimitator = ", ";
                foreach (var duplicate in duplicates)
                {
                    sb.Append(duplicate);
                    sb.Append(delimitator);
                }
                sb.Remove(sb.Length - delimitator.Length, delimitator.Length);
            }

            Invoke(new Action(() =>
            {
                camerasComboBox.Invalidate();
                toolStripStatusDuplicate.Text = sb.ToString(); // Texte vide si aucun problème.
            }));
        }

        private void OnNewCameraEvent(EosCamera camera)
        {
            if (camera == null) return;
            lock (_lock)
            {
                if (_cameras.ContainsKey(camera.SerialNumber))
                {
                    // Cas où une caméra déconnecte et se reconnecte trop rapidement.
                    _cameras.Remove(camera.SerialNumber);
                    _cameraSettingsManager.UpdateSettingsForCamera(camera, false);
                }
                _cameras.Add(camera.SerialNumber, camera);
                _cameraSettingsManager.UpdateSettingsForCamera(camera);

                Invoke(new Action(() =>
                {
                    PopulateCameraComboBox(true);
                    CheckAndWarnForDuplicateAuthor();
                    ChangeStatusScriptMessage($"Camera Added: {camera.Author}");

                    if (camerasComboBox.Items.Count != 1)
                        return; // On ne change la sélection que si la première caméra viens de se connecter.
                    camerasComboBox.SelectedIndex = 0;
                    OnCameraSelectionChanged(null, null);
                }));
            }
        }

        private void PopulateCameraComboBox(bool repopulate)
        {
            var cameraItemList = _cameras.Select(camera => new CameraComboItem(camera.Value)).ToList();

            cameraItemList.Sort();

            if (log.IsTraceEnabled)
            {
                if (cameraItemList.Any())
                {
                    var sb = new StringBuilder();
                    sb.Append("Sorted value for comboBox:");
                    foreach (var cam in cameraItemList)
                    {
                        sb.AppendLine();
                        sb.Append("\t-> ");
                        sb.Append(cam);
                    }
                    log.Trace(sb.ToString);
                }
                else
                {
                    log.Trace("No camera to populate combo box.");
                }
            }

            PopulateCombo(camerasComboBox, cameraItemList, repopulate ? camerasComboBox.SelectedItem as CameraComboItem : null);

            log.Trace("Camera Combo box populated.");
        }

        private void OnCameraSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var wasInLiveView = false;
                lock (_lock)
                {
                    if (_selectedCamera != null)
                    {
                        _selectedCamera.LiveViewUpdate -= HandlePictureUpdate;

                        if (_selectedCamera.IsInLiveViewMode)
                        {
                            _selectedCamera.StopLiveView();
                            wasInLiveView = true;
                        }
                    }

                    if (camerasComboBox.SelectedItem != null)
                    {

                        var selectedCam = camerasComboBox.SelectedItem as CameraComboItem;

                        if (selectedCam?.Value.SerialNumber == null || !_cameras.ContainsKey(selectedCam.Value.SerialNumber))
                        {
                            log.Warn("Selected value was not a valid camera dictionary value: {0}",
                                camerasComboBox.SelectedItem);
                            return;
                        }

                        _selectedCamera = _cameras[selectedCam.Value.SerialNumber];

                        log.Trace("Camera selected a={0}, index={1}, sn={2}", _selectedCamera.Author,
                            camerasComboBox.SelectedIndex, _selectedCamera.SerialNumber);
                        _selectedCamera.LiveViewUpdate += HandlePictureUpdate;
                        InitCurrentlySelectedCameraProperties(_selectedCamera);

                        if (wasInLiveView)
                        {
                            _selectedCamera.StartLiveView();
                        }
                    }
                }

                startEvfButton.Enabled = !wasInLiveView;
                stopEvfButton.Enabled = wasInLiveView;

                PreviousCameraButton.Enabled = camerasComboBox.SelectedIndex >= 1;
                NextCameraButton.Enabled = camerasComboBox.SelectedIndex < camerasComboBox.Items.Count - 1;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }

            pictureBox.Image = null;
            pictureBox.Invalidate();
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed)
                UpdatePicture(e.GetImage());
        }

        private void OnCameraShutdown(object sender, EventArgs args)
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
                Invoke(new Action(() => RemoveCamera(camera)));
                return;
            }

            lock (_lock)
            {
                try
                {
                    if (camera == null)
                    {
                        CheckCameraList();
                        RemoveCamOnError();
                        return;
                    }

                    var serialId = camera.SerialNumber;

                    if (serialId == null)
                    {
                        log.Warn("Invalid serial number on camera: serial={0}, author={1}", camera.SerialNumber,
                            camera.Author);
                        CheckCameraList();
                        RemoveCamOnError();
                        return;
                    }

                    log.Trace("Removing camera. Author={0}, serial={1}", camera.Author, serialId);
                    if (_selectedCamera != null && _selectedCamera.SerialNumber.Equals(serialId))
                    {
                        _selectedCamera.LiveViewUpdate -= HandlePictureUpdate;

                        pictureBox.Image = null;
                        pictureBox.Invalidate();

                        ClearCombos();

                        _selectedCamera = null;

                        camerasComboBox.Text = null;
                    }

                    _cameras.Remove(serialId);
                    _cameraSettingsManager.UpdateSettingsForCamera(camera, false);
                    var index = camerasComboBox.Items.IndexOf(camera);
                    camerasComboBox.Items.RemoveAt(index);

                    ChangeStatusScriptMessage($"Camera disconnected: {camera.Author}");

                    if (camerasComboBox.Text != null) return;
                    if (index > 0 && index <= camerasComboBox.Items.Count)
                    {
                        camerasComboBox.SelectedIndex = index - 1;
                    }
                    else
                    {
                        camerasComboBox.SelectedIndex = -1;
                    }
                    OnCameraSelectionChanged(null, null);
                }
                catch (Exception e)
                {
                    log.Error(e, "Error when trying to remove camera.");
                    RemoveCamOnError();
                }
            }
        }

        /// <summary>
        /// Vérifie l'intégrité de la liste de caméra en détectant lesquelles des caméras ne sont plus disponibles.
        /// </summary>
        private void CheckCameraList()
        {
            var camToDelete = new HashSet<string>();
            var camToDeleteAuthor = new HashSet<string>();
            foreach (var cams in _cameras)
            {
                try
                {
                    if (cams.Value.Author == null)
                    {
                        camToDelete.Add(cams.Key);
                        camToDeleteAuthor.Add("DC-" + cams.Key);
                        continue;
                    }

                    if (cams.Value.IsSessionOpen) continue;

                    camToDelete.Add(cams.Key);
                    camToDeleteAuthor.Add(cams.Value.Author);
                }
                catch (Exception)
                {
                    camToDelete.Add(cams.Key);
                    camToDeleteAuthor.Add(cams.Value.Author);
                }
            }

            if (!camToDelete.Any()) return;

            foreach (var cam in camToDelete)
            {
                _cameras.Remove(cam);
                _cameraSettingsManager.UpdateActiveSettingForCamera(cam, false);
            }

            var sb = new StringBuilder();

            sb.Append("Following cameras were dropped: (");
            foreach (var auth in camToDeleteAuthor)
            {
                sb.Append(auth);
                sb.Append(',');
            }
            sb.Remove(sb.Length, 1);
            sb.Append(')');

            Invoke(new Action(() =>
            {
                PopulateCameraComboBox(true);
                CheckAndWarnForDuplicateAuthor();
                ChangeStatusScriptMessage(sb.ToString());
                camerasComboBox.SelectedIndex = 0;
                OnCameraSelectionChanged(null, null);
            }));
        }

        private void RemoveCamOnError()
        {
            try
            {
                if (_selectedCamera != null)
                {
                    _selectedCamera.LiveViewUpdate -= HandlePictureUpdate;
                    _selectedCamera = null;
                }
                camerasComboBox.SelectedIndex = -1;
                ClearCombos();
                OnCameraSelectionChanged(null, null);
            }
            catch (Exception)
            {
                // Die nice.
            }
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

        #region Event for config
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
            startEvfButton.Enabled = false;

            try
            {

                _selectedCamera.StartLiveView();
            }
            catch (Exception ex)
            {
                meteringComboBox.Enabled = true;
                stopEvfButton.Enabled = false;
                startEvfButton.Enabled = true;
                try
                {
                    _selectedCamera.StopLiveView();
                }
                catch (Exception)
                {
                    // ignored
                }
                MsgBox.Show(this, ex.Message, Common.Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCopyCameraSettingsClicked(object sender, EventArgs e)
        {
            lock (_lock)
            {
                // Si la caméra à une propriété non-patageable de sélectionnée, on resfuse la propagation et on affiche un message en ce sens à l'usager.
                if (!_cameraSettingsManager.PropagateGroupSelectionToAllActiveGroups(_selectedCamera.SerialNumber, _cameras))
                {
                    MessageBox.Show(this, 
                        Resources.Text.CameraRingClientForm_Cannot_Propagate_Camera_Settings, 
                        Resources.Text.CameraRingClientForm_Cannot_Execute_Action, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void OnStopEvfClicked(object sender, EventArgs e)
        {
            if (_selectedCamera == null) return;
            meteringComboBox.Enabled = true;
            stopEvfButton.Enabled = false;
            startEvfButton.Enabled = true;

            try
            {
                _selectedCamera.StopLiveView();
            }
            catch (Exception ex)
            {
                MsgBox.Show(this, ex.Message, Common.Resources.common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnIsoSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                var value = (string)isoComboBox.SelectedItem;
                _currentPropertiesGroup.Iso.UpdateSelectedValue(_selectedCamera, value);
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
                _currentPropertiesGroup.AeMode.UpdateSelectedValue(_selectedCamera, value);
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
                _currentPropertiesGroup.Av.UpdateSelectedValue(_selectedCamera, value);
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
                _currentPropertiesGroup.ExposureComp.UpdateSelectedValue(_selectedCamera, value);
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
                _currentPropertiesGroup.ImageQuality.UpdateSelectedValue(_selectedCamera, value);
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
                _currentPropertiesGroup.MeteringMode.UpdateSelectedValue(_selectedCamera, value);
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
                _currentPropertiesGroup.Tv.UpdateSelectedValue(_selectedCamera, value);
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
                _currentPropertiesGroup.WhiteBalance.UpdateSelectedValue(_selectedCamera, value);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        #endregion

        private void InitCurrentlySelectedCameraProperties(EosObject camera)
        {
            _currentPropertiesGroup = _cameraSettingsManager.GetPropertiesGroup(camera.SerialNumber);

            PopulateCombo(isoComboBox, _currentPropertiesGroup.Iso.SupportedDisplayValues, _currentPropertiesGroup.Iso.SelectedValue);
            PopulateCombo(aeModeComboBox, _currentPropertiesGroup.AeMode.SupportedDisplayValues, _currentPropertiesGroup.AeMode.SelectedValue);
            PopulateCombo(tvComboBox, _currentPropertiesGroup.Tv.SupportedDisplayValues, _currentPropertiesGroup.Tv.SelectedValue);
            PopulateCombo(avComboBox, _currentPropertiesGroup.Av.SupportedDisplayValues, _currentPropertiesGroup.Av.SelectedValue);
            PopulateCombo(meteringComboBox, _currentPropertiesGroup.MeteringMode.SupportedDisplayValues, _currentPropertiesGroup.MeteringMode.SelectedValue);
            PopulateCombo(exposureCompComboBox, _currentPropertiesGroup.ExposureComp.SupportedDisplayValues, _currentPropertiesGroup.ExposureComp.SelectedValue);
            PopulateCombo(imageQualityComboBox, _currentPropertiesGroup.ImageQuality.SupportedDisplayValues, _currentPropertiesGroup.ImageQuality.SelectedValue);
            PopulateCombo(whiteBalanceComboBox, _currentPropertiesGroup.WhiteBalance.SupportedDisplayValues, _currentPropertiesGroup.WhiteBalance.SelectedValue);
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

        private static void PopulateCombo<T>(ComboBox comboBox, IReadOnlyCollection<T> values, T selectedValue)
        {
            comboBox.Items.Clear();
            comboBox.Text = null;

            if (values.Count == 0)
            {
                comboBox.Enabled = false;
            }
            else
            {
                foreach (var value in values)
                {
                    comboBox.Items.Add(value);
                }

                comboBox.Enabled = true;

                if (selectedValue == null) return;

                var index = -1;
                var found = false;
                using (var ite = values.GetEnumerator())
                {
                    while (ite.MoveNext())
                    {
                        index++;
                        var val = ite.Current;
                        if (!val?.Equals(selectedValue) ?? false) continue;
                        found = true;
                        break;
                    }
                }
                comboBox.SelectedIndex = found ? index : -1;
            }
        }

        /// <inheritdoc/>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            log.Trace("Closing sequence started.");
            lock (_lock)
            {
                _closing = true;

                _manager.UnRegisterForNewPictureEvent(this);

                if (_selectedCamera == null) return;
                try
                {
                    _selectedCamera.StopLiveView();
                    _selectedCamera.LiveViewUpdate -= HandlePictureUpdate;
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

            _cameraSettingsManager.Dispose();
            _manager.Dispose();
            _networkManager.Stop();
            log.Trace("Closing sequence finished.");
        }

        private void PreviousCameraButton_Click(object sender, EventArgs e)
        {
            var newIndex = camerasComboBox.SelectedIndex - 1;

            if (newIndex < 0)
            {
                newIndex = 0;
            }

            camerasComboBox.SelectedIndex = newIndex;
            OnCameraSelectionChanged(null, null);
        }

        private void NextCameraButton_Click(object sender, EventArgs e)
        {
            var newIndex = camerasComboBox.SelectedIndex + 1;

            if (camerasComboBox.Items.Count <= newIndex)
            {
                newIndex = camerasComboBox.Items.Count - 1;
            }

            camerasComboBox.SelectedIndex = newIndex;
            OnCameraSelectionChanged(null, null);
        }

        /// <inheritdoc/>
        public void OnForcedUnregister()
        {
            lock (_lock)
            {
                if (_closing) return;
            }
            log.Warn("Form was forced to de-register from camera manager. Closing.");
            Close(); // Force la fermeture de la forme.
        }

        /// <inheritdoc/>
        public void PictureRequestCompleted(IPictureRequest request)
        {
            // Non-utilisé.
        }

        /// <inheritdoc/>
        public void PictureRequestCancelled(IPictureRequest request)
        {
            // Non-utilisé.
        }

        /// <inheritdoc/>
        public void PictureTaken(string filePath, string[] ids)
        {
            // Réception d'une image via un déclencheur ou une prise manuelle.
            lock (_lock)
            {
                if (_duplicatesDetected)
                {
                    ChangeStatusScriptMessage(Resources.Text.CustomServerConfigurationDialog_Label_CancelDueToDuplicates +
                        DateTime.Now.ToShortTimeString());
                    return;
                }

                if (_cameraFileCheck == null)
                {
                    _nbrFileChecked++;
                    _cameraFileCheck = new CameraRingFileChecker(_config.MissingCameraToleranceNumber, _cameras.Values.Select(cam => cam.SerialNumber),
                        new TimeSpan(0, 0, 0, _config.CheckFileTimerSeconds),
                        (zipFileName, iscancelled, undoneSerials) =>
                        {
                            _cameraFileCheck = null;
                            if (File.Exists(zipFileName) && !iscancelled)
                            {
                                if (!_networkManager.IsServerDetected)
                                {
                                    ChangeStatusScriptMessage(Resources.Text.CameraRingClientForm_PictureTaken_All_Files_No_Server +
                                            DateTime.Now.ToShortTimeString());
                                    return;
                                }

                                ChangeStatusScriptMessage(Resources.Text.CameraRingClientForm_PictureTaken_BeginSend, false);
                                _networkManager.SendFileToServer(zipFileName, () => // Success callback
                                {
                                    Invoke(new Action(() =>
                                    {
                                        var sb = new StringBuilder();
                                        sb.Append(Resources.Text.CameraRingClientForm_PictureTaken_Data_sent_to_server);
                                        sb.Append(DateTime.Now.ToShortTimeString());
                                        if (undoneSerials.Any())
                                        {
                                            sb.Append(" (Ignored: ");
                                            foreach (var serial in undoneSerials)
                                            {
                                                sb.Append(_cameras.ContainsKey(serial) ? _cameras[serial].Author : "D/C");
                                                sb.Append(',');
                                            }
                                            sb.Remove(sb.Length - 1, 1);
                                            sb.Append(")");
                                        }

                                        ChangeStatusScriptMessage(sb.ToString());
                                    }));
                                    CleanWorkDir();
                                }, err => // Error callback
                                {
                                    log.Error(err);
                                    Invoke(new Action(() =>
                                    {
                                        ChangeStatusScriptMessage(Resources.Text.CameraRingClientForm_PictureTaken_Send_attempt_failed +
                                            DateTime.Now.ToShortTimeString());
                                    }));
                                    CleanWorkDir();
                                }, kbDone => // Progress callback
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (toolStripProgressBarSendToServer.ProgressBar != null)
                                            toolStripProgressBarSendToServer.ProgressBar.Value = kbDone;
                                    }));
                                }, (min, max) => // Init callback
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (toolStripProgressBarSendToServer.ProgressBar == null) return;
                                        toolStripProgressBarSendToServer.ProgressBar.Minimum = min;
                                        toolStripProgressBarSendToServer.ProgressBar.Maximum = max;
                                    }));
                                });
                            }
                            else
                            {
                                Invoke(new Action(() =>
                                {
                                    var sb = new StringBuilder();
                                    foreach (var serial in undoneSerials)
                                    {
                                        sb.Append(_cameras.ContainsKey(serial) ? _cameras[serial].Author : "D/C");
                                        sb.Append(',');
                                    }
                                    sb.Remove(sb.Length - 1, 1);

                                    var finalText = string.Format(
                                            Resources.Text.CameraRingClientForm_PictureTaken_Missing_Camera_Files,
                                            undoneSerials.Count, _cameras.Count, DateTime.Now.ToShortTimeString(),
                                            sb, _config.MissingCameraToleranceNumber);

                                    ChangeStatusScriptMessage(finalText);
                                }));
                            }
                        });
                    var tokenSource = new CancellationTokenSource();
                    var task = new Task(() => _cameraFileCheck.CheckForNewImageFiles(tokenSource.Token));
                    task.Start();

                    Invoke(new Action(() => ChangeStatusScriptMessage("New file check started: " + _nbrFileChecked, false)));
                }
            }

            _cameraFileCheck?.NewFileDetected(filePath, ids[1]);
        }

        private void ChangeStatusScriptMessage(string message, bool recordMessage = true)
        {
            toolStripStatusLabelLastTimeStamp.Text = message;
            if (recordMessage)
            {
                if (_lastMessages.Count == 10) _lastMessages.RemoveAt(0);
            }
            _lastMessages.Add(message);
        }

        private void OnNetworkStatusLabelClick(object sender, EventArgs eventArgs)
        {
            var sb = new StringBuilder();
            sb.Append("Server info: ");
            sb.Append(_networkManager.IsServerDetected ? "Connected" : "Unconnected");
            sb.Append("\nAddress:");
            sb.Append(_networkManager.ServerAddress);
            sb.Append(':');
            sb.Append(_networkManager.ServerPort);
            sb.Append(_networkManager.GetCustomAddress != null ? "\nUsing custom address." : "\nUsing SSDP detection.");
            if (_lastMessages.Any())
            {
                sb.AppendLine(" ");
                sb.AppendLine("Last messages:");
                foreach (var message in _lastMessages)
                {
                    sb.AppendLine(message);
                }
            }

            MessageBox.Show(this, sb.ToString(), @"Server status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSetLocalAddress(object sender, EventArgs e)
        {
            var diag = new CustomServerConfigurationDialog(_networkManager);
            diag.ShowDialog(this);
        }

        private void ConfigurationButton_Click(object sender, EventArgs e)
        {
            var diag = new RingConfigurationDialog(_networkManager, _config);
            diag.ShowDialog(this);
        }
    }
}
