using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Appelle un dialogue où l'on peut entrer les coordonnées de normalisation pour un groupement d'images.
    /// </summary>
    public partial class FileBundleCalibrationDialog : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static bool _dirty;
        private readonly string _baseDir;
        private readonly FileInfo _transformFile;
        private FileBundleNormalizationTransforms _transforms;
        private CancellationTokenSource _cts;
        private Task _task;
        private bool _blockExit;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="baseDir">Le répertoire qui contient les fichiers à analyser.</param>
        public FileBundleCalibrationDialog(string baseDir)
        {
            InitializeComponent();

            _baseDir = baseDir;

            boxC1X.KeyPress += OnCharInput;
            boxC1Y.KeyPress += OnCharInput;
            boxC1W.KeyPress += OnCharInput;
            boxC1H.KeyPress += OnCharInput;
            boxC1R.KeyPress += OnCharInput;

            boxC2X.KeyPress += OnCharInput;
            boxC2Y.KeyPress += OnCharInput;
            boxC2W.KeyPress += OnCharInput;
            boxC2H.KeyPress += OnCharInput;
            boxC2R.KeyPress += OnCharInput;

            _transformFile = new FileInfo(FileBundleNormalizationTransforms.DefaultTransformsJsonFileLocation);
            if (!_transformFile.Exists) return;

            _transforms = JsonUtil.ReadJsonFile<FileBundleNormalizationTransforms>(_transformFile);
            if (_transforms.GlobalParameters.Input == null) return;

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

            chkOnlyTranslation.Checked = _transforms.GlobalParameters.OnlyTranslation;

            chkOnlyTranslation.CheckedChanged += OnCheckChange;
        }

        private static void OnCheckChange(object sender, EventArgs e)
        {
            _dirty = true;
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.SafeCall(CheckButtonEnableStatus);
        }

        private void CheckButtonEnableStatus()
        {
            btnClose.Enabled = true;

            if (_task != null)
            {
                btnPreview.Enabled = false;
                btnClear.Enabled = false;
                btnGenerate.Enabled = false;
                Invalidate();
                return;
            }

            var validPictureDir = Directory.Exists(_baseDir) && Directory.GetFiles(_baseDir).Any();

            if (validPictureDir && _transforms != null)
            {
                btnPreview.Enabled = true;
            }
            else
            {
                btnPreview.Enabled = false;
            }

            btnGenerate.Enabled = ValidateTextBoxCoordinate() && validPictureDir;

            btnClear.Enabled = _transforms != null;
            Invalidate();
        }

        private bool ExtractCoordinates(out int[][] values)
        {
            try
            {
                values = new[]
                {
                    new[]
                    {
                        int.Parse(boxC1X.Text), int.Parse(boxC1Y.Text), int.Parse(boxC1W.Text), int.Parse(boxC1H.Text), int.Parse(boxC1R.Text)
                    },
                    new[]
                    {
                        int.Parse(boxC2X.Text), int.Parse(boxC2Y.Text), int.Parse(boxC2W.Text), int.Parse(boxC2H.Text), int.Parse(boxC2R.Text)
                    }
                };
            }
            catch (Exception)
            {
                values = new int[0][];
                return false;
            }
            return true;
        }

        private bool ValidateTextBoxCoordinate()
        {
            if (!ExtractCoordinates(out int[][] coorValues)) return false;

            var fine = true;

            fine &= ChangeControlForeGroundColor(boxC1X, coorValues[0][0] < 0);
            fine &= ChangeControlForeGroundColor(boxC1Y, coorValues[0][1] < 0);
            fine &= ChangeControlForeGroundColor(boxC1W, coorValues[0][2] <= 0);
            fine &= ChangeControlForeGroundColor(boxC1H, coorValues[0][3] <= 0);
            fine &= ChangeControlForeGroundColor(boxC1R, coorValues[0][4] < Math.Max(coorValues[0][2], coorValues[0][3]));

            fine &= ChangeControlForeGroundColor(boxC2X, coorValues[1][0] < 0);
            fine &= ChangeControlForeGroundColor(boxC2Y, coorValues[1][1] < 0);
            fine &= ChangeControlForeGroundColor(boxC2W, coorValues[1][2] <= 0);
            fine &= ChangeControlForeGroundColor(boxC2H, coorValues[1][3] <= 0);
            fine &= ChangeControlForeGroundColor(boxC2R, coorValues[1][4] < Math.Max(coorValues[1][2], coorValues[1][3]));
            
            return fine;
        }

        private bool ChangeControlForeGroundColor(Control c, bool check)
        {
            if (check)
            {
                this.SafeCall(() => { c.ForeColor = Color.Red; c.Invalidate(); });
                return false;
            }
            this.SafeCall(() => { c.ForeColor = Color.Black; c.Invalidate(); });
            return true;
        }

        private void OnCharInput(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true; // Cancelle l'entrée de caractère.
            }
            else
            {
                _dirty = true;
                this.SafeCall(CheckButtonEnableStatus);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (File.Exists(_transformFile.FullName))
                File.Delete(_transformFile.FullName);
            _transforms = null;
            _dirty = false;
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

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!ExtractCoordinates(out int[][] coorValues))
            {
                // Coordonnées ne peuvent être transformées en entiers valides. Avertir l'usager et mettre dans le log.
                var strErreur1 = $"Box1 : [{boxC1X.Text},{boxC1Y.Text},{boxC1W.Text},{boxC1H.Text}], R={boxC1R.Text}";
                var strErreur2 = $"Box2 : [{boxC2X.Text},{boxC2Y.Text},{boxC2W.Text},{boxC2H.Text}], R={boxC2R.Text}";
                log.Warn($"Could not extract values from text boxes => {strErreur1} ; {strErreur2}");

                MsgBox.Show(this, $"Could not read coordinates from dialog:\n\n{strErreur1}\n{strErreur2}");

                ChangeControlForeGroundColor(btnGenerate, true);
                return;
            }
            ChangeControlForeGroundColor(btnGenerate, false);

            var files = Directory.GetFiles(_baseDir, "*.jpg").ToList();
            FileUtil.SortFileCollection(files);

            List<CoordinatesInput> inputs = null;
            try
            {
                progBar.Value = 0;
                progBar.Minimum = 0;
                progBar.Maximum = files.Count;

                inputs = new List<CoordinatesInput>
                {
                    new CoordinatesInput
                    {
                        Zone = new Rectangle(coorValues[0][0],coorValues[0][1],coorValues[0][2],coorValues[0][3]),
                        Radius = coorValues[0][4]
                    },
                    new CoordinatesInput
                    {
                        Zone = new Rectangle(coorValues[1][0],coorValues[1][1],coorValues[1][2],coorValues[1][3]),
                        Radius = coorValues[1][4]
                    }
                };

                _transforms = OpenCvUtil.CalculateNormalizationTransforms(inputs, files, chkOnlyTranslation.Checked, null,
                    nbr =>
                    {
                        this.SafeCall(() => { progBar.Value = nbr; });
                    });
            }
            catch (Exception err)
            {
                btnGenerate.ForeColor = Color.Red;
                btnGenerate.Invalidate();
                log.Error(err, "Could not generate transformation files in directory with following inputs: \n\tOnly translation= {2}\n\tinputs=\t{0}\tdirectory= {1}\n",
                    StringUtil.FormatEnumerable(inputs, true, 3), _baseDir, chkOnlyTranslation.Checked);
                _transforms = null;
            }

            if (_transforms != null)
            {
                JsonUtil.WriteJsonFile(_transformFile, _transforms);
            }
            _dirty = false;
            CheckButtonEnableStatus();
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
                else
                {
                    return;
                }
            }

            if (_dirty)
            {
                var result = MessageBox.Show(this, @"Changes are detected. Are you sure you want to discard them?", @"Changes detected",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    Close();
                }
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
