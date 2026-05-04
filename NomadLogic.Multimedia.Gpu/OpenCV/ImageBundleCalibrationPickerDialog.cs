using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Win32;
using NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson;

// ReSharper disable LocalizableElement

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Forme pour facilement sélectionner les zones de vérification d'une calibration de groupe d'image d'une CameraRing.
    /// </summary>
    public partial class ImageBundleCalibrationPickerDialog : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static bool _dirty;
        private string _baseDir;
        private FileInfo _transformFile;
        private FileBundleNormalizationTransforms _transforms;
        private CancellationTokenSource _cts;
        private Task _task;
        private bool _blockExit;
        private readonly Matrix _pictureMatrix = new Matrix();
        private static readonly Matrix IDENTITY_MATRIX = new Matrix();
        private Bitmap _firstPictureBitmap;
        private float _zoomFactor = 1f;
        private float _translationX;
        private float _translationY;
        private readonly System.Timers.Timer _timer;
        private readonly List<CoordinatesInput> _inputs = new List<CoordinatesInput>();
        private readonly Rectangle _viewRectangle;
        private bool _mouseDrag;
        private readonly float[] _mousePointOne = new float[2];
        private readonly float[] _mousePointTwo = new float[2];
        private PointF? _offsetPoint = new PointF();
        private readonly Pen _penDrag = new Pen(Color.Blue, 2);
        private readonly Pen _penCoord = new Pen(Color.Red, 2);
        private readonly float[,] _rectanglePoints = new float[4, 2];
        private readonly float[] _offsetValues = new float[4];
        private readonly List<float[,]> _inputsDraw = new List<float[,]>();
        private readonly bool _showSaveLastBaseDir;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="workDir">Le répertoire de travail de base d'où chercher la première image.</param>
        /// <param name="overrideLastDirectory">Indique qu'il faut affichier le dernier répertoire recherché, et mettre ce dernier à jour. Vrai par défaut,
        /// si faux, n'affiche aucun répertoire par défaut et ne modifie pas le dernier répertoire enregistré en mémoire.</param>
        public ImageBundleCalibrationPickerDialog(string workDir, bool overrideLastDirectory = true)
        {
            InitializeComponent();

            _showSaveLastBaseDir = overrideLastDirectory;
            _baseDir = workDir;
            mainPictureBox.Paint += MainPictureBoxOnPaint;
            mainPictureBox.MouseDown += OnMainPictureMouseDown;
            mainPictureBox.MouseUp += OnMainPictureMouseUp;
            mainPictureBox.MouseMove += OnMainPictureMouseMove;
            mainPictureBox.MouseLeave += OnMainPictureMouseLeave;
            _viewRectangle = new Rectangle(0, 0, mainPictureBox.Width, mainPictureBox.Height);

            tbZoom.ValueChanged += TrackBarValueChanged;
            _timer = new System.Timers.Timer { Interval = 500, AutoReset = false };
            _timer.Elapsed += Timer_Tick;

        }

        private void OnMainPictureMouseLeave(object sender, EventArgs e)
        {
            if (!_mouseDrag) return;
            _mouseDrag = false;
            mainPictureBox.Invalidate();
        }

        private void OnMainPictureMouseMove(object sender, MouseEventArgs e)
        {
            if (_task != null) return;
            if (!_mouseDrag || _mousePointTwo == null) return;
            _mousePointTwo[0] = e.X;
            _mousePointTwo[1] = e.Y;

            // ReSharper disable once PossibleInvalidOperationException
            // Calcul des points de dessein
            var rect = MathUtil.MakeRectangle(_mousePointOne[0], _mousePointOne[1], _mousePointTwo[0], _mousePointTwo[1]);
            UpdateRectanglePoint(rect, _rectanglePoints);

            mainPictureBox.Invalidate();
        }

        private static void UpdateRectanglePoint(Rectangle rect, float[,] pointsArray)
        {
            // Point 0 (haut-gauche)
            pointsArray[0, 0] = rect.X;
            pointsArray[0, 1] = rect.Y;
            // Point 1 (haut-droit)
            pointsArray[1, 0] = rect.X + rect.Width;
            pointsArray[1, 1] = rect.Y;
            // Point 2 (bas-gauche)
            pointsArray[2, 0] = rect.X;
            pointsArray[2, 1] = rect.Y + rect.Height;
            // Point 3 (bas-droit)
            pointsArray[3, 0] = rect.X + rect.Width;
            pointsArray[3, 1] = rect.Y + rect.Height;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void OnMainPictureMouseUp(object sender, MouseEventArgs e)
        {
            if (_task != null) return;
            if (!_mouseDrag || (_mousePointOne[0] == _mousePointTwo[0] && _mousePointOne[1] == _mousePointTwo[1]))
            {
                return;
            }

            _mouseDrag = false;

            var pX1 = (_mousePointOne[0] - _offsetValues[0]) / _zoomFactor;
            var pY1 = (_mousePointOne[1] - _offsetValues[1]) / _zoomFactor;
            var pX2 = (_mousePointTwo[0] - _offsetValues[0]) / _zoomFactor;
            var pY2 = (_mousePointTwo[1] - _offsetValues[1]) / _zoomFactor;

//            var func = new Func<object, string>(obj => ((float) obj).ToString("0.00", CultureInfo.InvariantCulture));
//            var sb = new StringBuilder();
//            sb.AppendLine("Mouse up event triggered.");
//            sb.AppendLine($"\tZoom factor = {_zoomFactor.ToString("0.00", CultureInfo.InvariantCulture)}");
//            sb.AppendLine($"\tMouse points : One={StringUtil.FormatEnumerable(_mousePointOne, false, 0, ",", true, func)}, Two={StringUtil.FormatEnumerable(_mousePointOne, false, 0, ",", true, func)}");
//            sb.AppendLine($"\tOffset value = {StringUtil.FormatEnumerable(_offsetValues, false, 0, ",", true, func)}");
//            sb.AppendLine($"\tMFinal coordinate: pX1={pX1.ToString("0.00", CultureInfo.InvariantCulture)}, pY1={pY1.ToString("0.00", CultureInfo.InvariantCulture)}, pX2={pX2.ToString("0.00", CultureInfo.InvariantCulture)}, pY2={pY2.ToString("0.00", CultureInfo.InvariantCulture)}");
//            System.Diagnostics.Debug.Write(sb.ToString());
            
            var rect = MathUtil.MakeRectangle(pX1, pY1, pX2, pY2);
            if (_inputs.Count == 2)
            {
                _inputs.Clear();
                _inputsDraw.Clear();
            }

            var coord = new CoordinatesInput
            {
                Zone = rect,
                Radius = Math.Max(rect.Width, rect.Height) * 3
            };
            _inputs.Add(coord);
            
            //System.Diagnostics.Debug.WriteLine($"\tNew Coordinate = {coord}");
            
            UpdateCoordsLabel();
            CheckButtonEnableStatus();

            var rectDraw = MathUtil.MakeRectangle(_mousePointOne[0], _mousePointOne[1], _mousePointTwo[0], _mousePointTwo[1]);
            var points = new float[4, 2];
            UpdateRectanglePoint(rectDraw, points);
            _inputsDraw.Add(points); // Il faudra mettre à jour ces valeurs à chaque recalcul de zoom/translation

            //System.Diagnostics.Debug.WriteLine($"\tNew DrawPoints = {_inputsDraw}");
            
            mainPictureBox.Invalidate();
        }

        private void UpdateCoordsLabel()
        {
            if (_inputs.Count < 1)
            {
                boxC1X.Text = "-";
                boxC1Y.Text = "-";
                boxC1W.Text = "-";
                boxC1H.Text = "-";
                boxC1R.Text = "-";
            }
            if (_inputs.Count < 2)
            {
                boxC2X.Text = "-";
                boxC2Y.Text = "-";
                boxC2W.Text = "-";
                boxC2H.Text = "-";
                boxC2R.Text = "-";
            }
            if (_inputs.Count >= 1)
            {
                var input = _inputs[0];
                boxC1X.Text = input.Zone.X.ToString();
                boxC1Y.Text = input.Zone.Y.ToString();
                boxC1W.Text = input.Zone.Width.ToString();
                boxC1H.Text = input.Zone.Height.ToString();
                boxC1R.Text = input.Radius.ToString(CultureInfo.InvariantCulture);
            }
            if (_inputs.Count >= 2)
            {
                var input = _inputs[1];
                boxC2X.Text = input.Zone.X.ToString();
                boxC2Y.Text = input.Zone.Y.ToString();
                boxC2W.Text = input.Zone.Width.ToString();
                boxC2H.Text = input.Zone.Height.ToString();
                boxC2R.Text = input.Radius.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void OnMainPictureMouseDown(object sender, MouseEventArgs e)
        {
            if (_task != null) return;
            _mouseDrag = true;
            _mousePointOne[0] = e.X;
            _mousePointOne[1] = e.Y;
            _mousePointTwo[0] = e.X;
            _mousePointTwo[1] = e.Y;
        }

        private void TrackBarValueChanged(object sender, EventArgs e)
        {
            _zoomFactor = tbZoom.Value / 100f;
            //System.Diagnostics.Debug.WriteLine("Zoom : " + _zoomFactor);
            _timer.Stop();
            _timer.Start();
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            mainPictureBox.Invalidate();
        }

        private void MainPictureBoxOnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            if (_firstPictureBitmap == null || _firstPictureBitmap.Width == 0 || _firstPictureBitmap.Height == 0) return;

            if (!_mouseDrag)
            {
                CalculateOffset();

                _pictureMatrix.Reset();
                _pictureMatrix.Scale(_zoomFactor, _zoomFactor);
                _pictureMatrix.Translate(_offsetValues[0], _offsetValues[1], MatrixOrder.Append);

                /*Console.WriteLine("Draw Matrix: zoom=[{0}], offset=[{1}, {2}], draw offset=[{3}, {4}]",
                                  _zoomFactor.ToString(CultureInfo.InvariantCulture),
                                  _offsetValues[0].ToString(CultureInfo.InvariantCulture),
                                  _offsetValues[1].ToString(CultureInfo.InvariantCulture),
                                  _offsetValues[2].ToString(CultureInfo.InvariantCulture),
                                  _offsetValues[3].ToString(CultureInfo.InvariantCulture));
                */
            }

            var g = paintEventArgs.Graphics;
            g.Transform = _pictureMatrix;
            // ReSharper disable once PossibleInvalidOperationException
            g.DrawImage(_firstPictureBitmap, _offsetPoint.Value);

            // Si on est en train de dessiner un point
            if (_mouseDrag && _mousePointOne != null && _mousePointTwo != null)
            {
                DrawRectangleZone(g, _penDrag, _rectanglePoints);
            }

            // On affiche maintenant les coordonnées inscrites.
            foreach (var coorArray in _inputsDraw)
            {
                DrawRectangleZone(g, _penCoord, coorArray);
            }
        }

        private static void DrawRectangleZone(Graphics g, Pen pen, float[,] pointArray)
        {
            g.Transform = IDENTITY_MATRIX;
            g.DrawLine(pen, pointArray[0, 0], pointArray[0, 1], pointArray[1, 0], pointArray[1, 1]);
            g.DrawLine(pen, pointArray[1, 0], pointArray[1, 1], pointArray[3, 0], pointArray[3, 1]);
            g.DrawLine(pen, pointArray[0, 0], pointArray[0, 1], pointArray[2, 0], pointArray[2, 1]);
            g.DrawLine(pen, pointArray[2, 0], pointArray[2, 1], pointArray[3, 0], pointArray[3, 1]);
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void CalculateOffset()
        {

            var newSizeX = _firstPictureBitmap.Width * _zoomFactor;
            var newSizeY = _firstPictureBitmap.Height * _zoomFactor;

            if (newSizeX < _viewRectangle.Width)
            {
                _offsetValues[2] = (_viewRectangle.Width - newSizeX) / 2;
                _offsetValues[0] = 0;
            }
            else
            {
                _offsetValues[0] = -1 * (newSizeX - _viewRectangle.Width) * (_translationX / 100);
                _offsetValues[2] = 0;
            }

            if (newSizeY < _viewRectangle.Height)
            {
                _offsetValues[3] = (_viewRectangle.Height - newSizeY) / 2;
                _offsetValues[1] = 0;
            }
            else
            {
                _offsetValues[1] = -1 * (newSizeY - _viewRectangle.Height) * (_translationY / 100);
                _offsetValues[3] = 0;
            }

            // On recalcule les points de coordonnées déjà entrés.
            for (var i = 0; i < _inputs.Count && i < _inputsDraw.Count; i++)
            {
                var originalRect = _inputs[i];

                var x = (int)(originalRect.Zone.X * _zoomFactor + (_offsetValues[2] == 0
                                   ? _offsetValues[0]
                                   : _offsetValues[2]));
                var y = (int)(originalRect.Zone.Y * _zoomFactor + (_offsetValues[3] == 0
                                   ? _offsetValues[1]
                                   : _offsetValues[3]));
                var w = (int)(originalRect.Zone.Width * _zoomFactor);
                var h = (int)(originalRect.Zone.Height * _zoomFactor);

                var modRect = new Rectangle(x, y, w, h);
                UpdateRectanglePoint(modRect, _inputsDraw[i]);
            }
        }

        #region Overrides of Form

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            _transformFile = new FileInfo(FileBundleNormalizationTransforms.DefaultTransformsJsonFileLocation);
            _transforms = null;
            if (File.Exists(_transformFile.FullName))
            {
                _transforms = JsonUtil.ReadJsonFile<FileBundleNormalizationTransforms>(_transformFile);
                if (_transforms.GlobalParameters.Input == null || _transforms.GlobalParameters.Input.Count != 2) { _transforms = null; }
                else
                {
                    boxC1X.Text = _transforms.GlobalParameters.Input[0].Zone.X.ToString();
                    boxC1Y.Text = _transforms.GlobalParameters.Input[0].Zone.Y.ToString();
                    boxC1W.Text = _transforms.GlobalParameters.Input[0].Zone.Width.ToString();
                    boxC1H.Text = _transforms.GlobalParameters.Input[0].Zone.Height.ToString();
                    boxC1R.Text = _transforms.GlobalParameters.Input[0].Radius.ToString(CultureInfo.InvariantCulture);

                    boxC2X.Text = _transforms.GlobalParameters.Input[1].Zone.X.ToString();
                    boxC2Y.Text = _transforms.GlobalParameters.Input[1].Zone.Y.ToString();
                    boxC2W.Text = _transforms.GlobalParameters.Input[1].Zone.Width.ToString();
                    boxC2H.Text = _transforms.GlobalParameters.Input[1].Zone.Height.ToString();
                    boxC2R.Text = _transforms.GlobalParameters.Input[1].Radius.ToString(CultureInfo.InvariantCulture);

                    _inputs.Clear();
                    _inputs.AddRange(_transforms.GlobalParameters.Input);
                    _inputsDraw.Add(new float[4,2]);
                    _inputsDraw.Add(new float[4,2]);
                }
            }

            if (_baseDir == null && _showSaveLastBaseDir)
            {
                // Tente de charge le dernier répertoire en mémoire.
                var lastRepFile = GetLastOpenedDirectoryFile();
                if (File.Exists(lastRepFile))
                {
                    var dir = new DirectoryInfo(File.ReadAllText(lastRepFile));
                    if (dir.Exists)
                    {
                        _baseDir = dir.FullName;
                    }
                }
            }
            
            SetBaseDirectory();
            CheckButtonEnableStatus();
        }

        #endregion

        private string GetLastOpenedDirectoryFile()
        {
            return DirectoryUtil.GetLocalNomadLogicDir() + "lastOpenedNormalizationDirectory.txt";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (File.Exists(_transformFile.FullName))
                File.Delete(_transformFile.FullName);
            _transforms = null;
            _dirty = false;
            CheckButtonEnableStatus();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (_inputs == null || _inputs.Count != 2 || _task != null) return;

            var files = Directory.GetFiles(_baseDir, "*.jpg").ToList();
            if (!files.Any()) return;
            FileUtil.SortFileCollection(files);

            _cts = new CancellationTokenSource();
            _task = Task.Run(() =>
            {
                try
                {
                    _transforms = null;
                    _dirty = true;

                    this.SafeCall(() =>
                    {
                        progBar.Value = 0;
                        progBar.Minimum = 0;
                        progBar.Maximum = files.Count;
                        btnGenerate.ForeColor = Color.Black;
                        btnGenerate.Invalidate();
                    });

                    _transforms = OpenCvUtil.CalculateNormalizationTransforms(_inputs, files, false, null,
                                                                              nbr => { this.SafeCall(() => { progBar.Value = nbr; }); });
                }
                catch (Exception err)
                {
                    var formatedInputs = StringUtil.FormatEnumerable(_inputs, true, 2);
                    var errormessage = $"\nCould not generate transformation files in directory with following inputs:\n\tinputs:\n\t\t{formatedInputs}\n\tdirectory:\n\t\t{_baseDir}\n";
                    log.Error(err, errormessage);
                    _transforms = null;

                    this.SafeCall(() =>
                    {
                        btnGenerate.ForeColor = Color.Red;
                        btnGenerate.Invalidate();
                        MsgBox.Show(this, $"{errormessage}\nError message:\n{err.Message}");
                    });
                }
                finally
                {
                    if (_transforms != null)
                    {
                        JsonUtil.WriteJsonFile(_transformFile, _transforms);
                        _dirty = false;
                    }

                    _cts = null;
                    _task = null;

                    CheckButtonEnableStatus();
                }
            }, _cts.Token);

            CheckButtonEnableStatus();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (_task != null) return;

            var originalFiles = Directory.GetFiles(_baseDir).ToList();
            FileUtil.SortFileCollection(originalFiles);
            progBar.Value = 0;
            progBar.Minimum = 0;
            progBar.Maximum = originalFiles.Count;
            btnClear.Enabled = false;
            btnPreview.Enabled = false;
            btnGenerate.Enabled = false;
            btnChangeDir.Enabled = false;

            _cts = new CancellationTokenSource();
            _task = Task.Run(() =>
            {
                try
                {
                    DirectoryInfo tmpDir = null;
                    // Déplace les fichiers dans un répertoire temporaire
                    if (!_cts.Token.IsCancellationRequested)
                    {
                        tmpDir =
                            Directory.CreateDirectory(DirectoryUtil.GetLocalApplicationWorkDir() +
                                                      GenerateUniqueId.GetId());
                    }

                    // Transforme les fichiers.
                    if (!_cts.Token.IsCancellationRequested && tmpDir != null)
                    {
                        OpenCvUtil.NormalizeFileBundleWithTransforms(originalFiles, _transforms, tmpDir, null,
                                                                     _cts.Token, current =>
                                                                     {
                                                                         this.SafeCall(() => { progBar.Value = current; });
                                                                     });
                    }

                    if (!_cts.Token.IsCancellationRequested && tmpDir != null)
                    {
                        // Affiche le Dialogue
                        _blockExit = true;
                        new MultiplePicturesPreviewDlg(tmpDir.FullName).ShowDialog();
                    }

                    // Quand on revient, on détruit les fichiers.
                    if (tmpDir != null && Directory.Exists(tmpDir.FullName))
                        FileUtil.DeleteFolderContent(tmpDir.FullName);
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
                finally
                {
                    _task = null;
                    _cts = null;
                    _blockExit = false;
                    this.SafeCall(CheckButtonEnableStatus);
                }
            }, _cts.Token);
        }

        private void CheckButtonEnableStatus()
        {
            this.SafeCall(() =>
            {
                btnClose.Enabled = true;

                if (_task != null)
                {
                    btnPreview.Enabled = false;
                    btnClear.Enabled = false;
                    btnGenerate.Enabled = false;
                    btnChangeDir.Enabled = false;
                    Invalidate();
                    return;
                }

                btnPreview.Enabled = _transforms != null && _baseDir != null;

                btnGenerate.Enabled = ValidateCoordinate() && _baseDir != null;

                btnClear.Enabled = _transforms != null;

                btnChangeDir.Enabled = true;

                Invalidate();
            });
        }

        private bool ValidateCoordinate()
        {
            if (_inputs.Count < 2) return false;
            if (_inputs[0].Zone.IntersectsWith(_inputs[1].Zone)) return false;
            return true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_blockExit) return;

            if (_task != null)
            {
                var result = MessageBox.Show(this, @"Normalization in preparation. Cancel?", @"Normalization running",
                                             MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    _cts.Cancel();

                    Task.WaitAll(_task);

                    Close();
                }
                else { return; }
            }

            if (_dirty)
            {
                var result = MessageBox.Show(this, @"Changes are detected. Are you sure you want to discard them?", @"Changes detected",
                                             MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes) { Close(); }
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            SetViewValuesManually(100, 50, 50);
        }

        private void SetViewValuesManually(int zoom, int horiz, int vert)
        {
            tbZoom.ValueChanged -= TrackBarValueChanged;
            vScroll.Scroll -= ScrollEvent;
            hScroll.Scroll -= ScrollEvent;

            tbZoom.Value = zoom;
            vScroll.Value = horiz;
            hScroll.Value = vert;

            tbZoom.ValueChanged += TrackBarValueChanged;
            vScroll.Scroll += ScrollEvent;
            hScroll.Scroll += ScrollEvent;

            _zoomFactor = 1.0f;
            _translationX = 50;
            _translationY = 50;

            if (_firstPictureBitmap != null) mainPictureBox.Invalidate();
        }

        private void ScrollEvent(object sender, ScrollEventArgs e)
        {
            if (_firstPictureBitmap == null) return;
            if (e.Type != ScrollEventType.EndScroll) return;
            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll) { _translationX = e.NewValue; }
            else { _translationY = e.NewValue; }
            mainPictureBox.Invalidate();
        }

        private void btnChangeDir_Click(object sender, EventArgs e)
        {
            var newThread = new Thread(() =>
                                       {
                                           var diag = new FolderBrowserDialog();
                                           if (diag.ShowDialog() != DialogResult.OK) return;
                                           _baseDir = diag.SelectedPath;
                                           SetBaseDirectory();
                                       });
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }

        private void SetBaseDirectory()
        {
            var noMessage = _baseDir == null;
            var check = Directory.Exists(_baseDir);
            if (check)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var files = Directory.GetFiles(_baseDir, "*.jpg", SearchOption.TopDirectoryOnly);
                if (!files.Any()) { check = false; }
                else
                {
                    FileUtil.SortFileCollection(files);
                    try
                    {
                        BitmapExtentions.MatchExifData(files[0]);
                        _firstPictureBitmap = FileUtil.LoadBitmap(files[0], 96); // Prend des images de 96 DPI pour les calculs d'Images. Je ne sais pas pourquoi 96...
                        if (_firstPictureBitmap.Height == 0 || _firstPictureBitmap.Width == 0) { check = false; }
                        this.SafeCall(() => lblBaseDir.Text = _baseDir);
                        CheckButtonEnableStatus();

                        /* On calcule les valeurs de zoom maximales. */
                        var maxZoomedValues = MathUtil.GetNewSizeToFitInsideContainer(
                            _viewRectangle.Width, _viewRectangle.Height, _firstPictureBitmap.Width, _firstPictureBitmap.Height);
                        var maxZoomedRatio = _firstPictureBitmap.Width / (double)maxZoomedValues.Width;
                        var minZoomedRatio = _viewRectangle.Width / (double)_firstPictureBitmap.Width;

                        this.SafeCall(() =>
                        {
                            tbZoom.Minimum = (int)(minZoomedRatio * 100);
                            tbZoom.Maximum = (int)(maxZoomedRatio * 100);

                            //System.Diagnostics.Debug.WriteLine($"Zoom min/max set to :{tbZoom.Minimum}/{tbZoom.Maximum}");
                            
                            SetViewValuesManually((tbZoom.Maximum - tbZoom.Minimum) / 2, 50, 50);
                        });

                        if (_showSaveLastBaseDir)
                        {
                            // Sauvegarde ce répertoire comme le dernier répertoire en mémoire.
                            var lastRepFile = GetLastOpenedDirectoryFile();
                            File.WriteAllText(lastRepFile, _baseDir);
                        }

                    }
                    catch (Exception e)
                    {
                        log.Warn(e, "Invalid first image in directory: {0}", _baseDir);
                        check = false;
                    }
                }
            }

            // En cas d'erreur on met des valeurs nulles et on avertit l'usager.
            if (check) return;
            this.SafeCall(() =>
            {
                if (!noMessage) MessageBox.Show(this, "Invalid category for bundle calibration.", "Ibrand", MessageBoxButtons.OK);
                lblBaseDir.Text = "";
                _baseDir = null;
                _firstPictureBitmap?.Dispose();
                CheckButtonEnableStatus();
                SetViewValuesManually(100, 50, 50);
            });
        }
    }
}
