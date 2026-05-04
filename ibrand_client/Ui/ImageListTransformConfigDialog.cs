using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms.Extensions;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;

namespace NomadLogic.iBrandDesktop.Ui
{
    [Obsolete]
    public partial class ImageListTransformConfigDialog : Form
    {
        private enum DummyImageType
        {
            CheckeredColor,
            SolidColor,
            NumberedImages
        }

        private const string FILE_FILTER_IMAGES = @"All graphic files |*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|JPEG files|*.jpg;*.jpeg|PNG files|*.png";
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IList<QuestionnaireInfo> _questionnaires;
        private readonly bool _autoLoad;
        private ImageListTransformOptions _config;
        private FileInfo _configFile;
        private volatile bool _initializing;
        private bool _cameraLandscape;
        private readonly List<string> _dummyFiles = new List<string>();

        private static readonly Color[] ColorArray =
        {
            Color.Aquamarine,
            Color.Aqua,
            Color.CadetBlue,
            Color.CornflowerBlue,
            Color.DarkBlue,
            Color.Chartreuse,
            Color.DarkGreen,
            Color.DarkCyan,
            Color.Teal,
            Color.Coral,
            Color.Chocolate,
            Color.Brown,
            Color.DarkMagenta,
            Color.BlueViolet,
            Color.DeepPink,
            Color.Red,
            Color.Crimson,
            Color.DarkOrange,
            Color.Yellow,
            Color.Tomato,
            Color.DarkSeaGreen,
            Color.CadetBlue
        };

        private int[] _lastCustomColors;
        private readonly ScreenConfig _screenConfig;
        private PreviewOverlayInfo _configOverlay;
        private FileInfo _configOverlayFile;

        /// <summary>
        /// Charge un unique questionnaire, le charge automatiquement. On peut aussi fournir une instance de configuration d'écran pour plus d'information.
        /// </summary>
        /// <param name="questionnaire">Le questionnaire présentement chargé.</param>
        /// <param name="sc">Optionel. Les configurations.</param>
        public ImageListTransformConfigDialog(QuestionnaireInfo questionnaire, ScreenConfig sc = null)
        {
            InitializeComponent();

            _questionnaires = new List<QuestionnaireInfo> { questionnaire };
            _autoLoad = true;
            _screenConfig = sc;
        }

        /// <summary>
        /// Charge le dialogue avec une liste de questionnaires possibles. Aucun n'est automatiquement chargé, et on ne peut pas donner de configuration
        /// d'écran.
        /// </summary>
        /// <param name="questionnairesList">La liste de questionnaires possibles.</param>
        public ImageListTransformConfigDialog(IList<QuestionnaireInfo> questionnairesList)
        {
            InitializeComponent();

            _questionnaires = questionnairesList;
            _autoLoad = false;
            _screenConfig = null;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_config != null && _configFile != null)
            {
                _configFile.Refresh();
                JsonUtil.WriteJsonFile(_configFile, _config);
            }

            if (_configOverlay != null && _configOverlayFile != null)
            {
                _configOverlayFile.Refresh();
                var list = new List<PreviewOverlayInfo> { _configOverlay };
                JsonUtil.WriteJsonFile(_configOverlayFile, list);
            }

            base.OnClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            #region Questionnaire and camera file settings

            // Initialize le combo des questionnaires disponibles.
            foreach (var quest in _questionnaires)
            {
                cmbQuestionnaire.Items.Add(quest);
            }

            if (!_autoLoad)
            {
                cmbQuestionnaire.SelectedIndex = -1;
            }
            else if (cmbQuestionnaire.Items.Count > 0)
            {
                cmbQuestionnaire.SelectedIndex = 0;
            }
            cmbQuestionnaire.SelectionChangeCommitted += CmbQuestionnaireOnSelectionChangeCommitted;

            radioOrientationLandscape.Checked = true;
            _cameraLandscape = true;
            cmbCameraResolution.SelectedIndex = 0;

            radioOrientationLandscape.CheckedChanged += RadioOrientationOnCheckedChanged;
            radioOrientationPortrait.CheckedChanged += RadioOrientationOnCheckedChanged;
            
            cmbCameraResolution.SelectionChangeCommitted += (sender, args) => ClearDummies();
            cmbDummyType.SelectionChangeCommitted += (sender, args) => ClearDummies();

            if (_screenConfig?.PotentialNumberOfPicturesTaken != null)
            {
                nudPotentialNbrOfPictures.Value = (decimal)_screenConfig.PotentialNumberOfPicturesTaken;
                nudPotentialNbrOfPictures.Enabled = false; // On bloque le changement de valeur.
            }
            else
            {
                nudPotentialNbrOfPictures.ValueChanged += (sender, args) => ClearDummies();
            }

            if (_screenConfig?.LivePreviewSize != null)
            {
                cmbPreviewSize.Items.Clear();
                cmbPreviewSize.Items.Add($"{_screenConfig.LivePreviewSize.Value.Width},{_screenConfig.LivePreviewSize.Value.Height} ({(float)_screenConfig.LivePreviewSize.Value.Width / _screenConfig.LivePreviewSize.Value.Height})");
                cmbPreviewSize.SelectedIndex = 0;
                cmbPreviewSize.Enabled = false;
            }
            else
            {
                cmbPreviewSize.SelectedIndex = 0;
                cmbPreviewSize.SelectionChangeCommitted += (sender, args) => ClearDummies();
            }

            #endregion

            // RotateFlip Combos
            foreach (var rf in Enum.GetValues(typeof(RotateFlipType)))
            {
                cmbPrintRotateFlip.Items.Add(rf);
            }

            foreach (var rf in Enum.GetValues(typeof(DummyImageType)))
            {
                cmbDummyType.Items.Add(rf);
            }

            cmbDummyType.SelectedIndex = 0;

            #region Print Section

            nudPrintOffsetPerc.ValueChanged += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.OffsetPerc = (float)nudPrintOffsetPerc.Value / 100f);
            cmbPrintRotateFlip.SelectionChangeCommitted += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.RotateFlip = cmbPrintRotateFlip.SelectedItem as RotateFlipType?);
            nudPrintOffsetX.ValueChanged += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.OffsetExtraSide = (int)nudPrintOffsetX.Value);
            nudPrintOffsetY.ValueChanged += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.OffsetExtraTop = (int)nudPrintOffsetY.Value);
            chkPrintFrameOver.CheckedChanged += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.FrameOver = chkPrintFrameOver.Checked);
            chkPrintUseOnlyFirstPicture.CheckedChanged += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.UseOnlyFirstPicture = chkPrintUseOnlyFirstPicture.Checked);
            chkPrintResizeToFrame.CheckedChanged += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.UseFullFramePicture = chkPrintResizeToFrame.Checked);
            chkPrintbkgColorEnable.CheckedChanged += (sender, args) => UpdateConfig(ChkPrintbkgColorEnableOnCheckedChanged);
            btnPrintColorPicker.Click += (sender, args) => UpdateConfig(BtnPrintPickColorClick);
            nudPrintJpegSaveQuality.ValueChanged += (sender, args) => UpdateConfig(() =>
                _config.PrintOptions.JpegSaveQuality = (int)nudPrintJpegSaveQuality.Value);
            btnPrintFileSelector.Click += (sender, args) => UpdateConfig(BtnPrintPickFile);
            btnPrintReset.Click += (sender, args) => UpdateConfig(BtnPrintResetClick);
            btnPrintViewFullPicture.Click += (sender, args) => ShowFullPicture(pboxPrintExample);
            btnPrintExampleGenerate.Click += (sender, args) => GeneratePrintPicture();

            #endregion

            #region Overlay Info

            btnOverlayFileSelection.Click += (sender, args) => UpdateOverlayConfig(BtnOverlayPickFile);
            btnOverlayReset.Click += (sender, args) => UpdateOverlayConfig(BtnOverlayResetClick);
            btnOverlayViewFull.Click += (sender, args) => ShowFullPicture(pboxOverlayExample);
            btnOverlayGenerate.Click += (sender, args) => GenerateOverlayPicture();
            
            var nudOverlayPointCtrls = new []
            {
                nudOverlayPointX, nudOverlayPointY
            };
            var nudOverlayRcetangleCtrls = new[]
            {
                nudOverlayRectangleX, nudOverlayRectangleY, nudOverlayRectangleW, nudOverlayRectangleH
            };

            var test = StringUtil.FormatEnumerable(GetType().GetMethods().Select(p => p.ToString()),true, 2);
            log.Error(test);

            // Assignation originale
            AssignToValueChangedOverlayNud(true, nudOverlayPointCtrls);
            AssignToValueChangedOverlayNud(true, nudOverlayRcetangleCtrls);

            chkOverlayPointUse.CheckedChanged += (sender, args) => UpdateOverlayConfig(() =>
            {
                AssignToValueChangedOverlayNud(false, nudOverlayPointCtrls);

                if (chkOverlayPointUse.Checked)
                {
                    DisableEnableAll(true, panelOverlayPoint);
                }
                else
                {
                    DisableEnableAll(false, panelOverlayPoint);
                    _configOverlay.Position = new Point(0,0);
                }

                AssignToValueChangedOverlayNud(true, nudOverlayRcetangleCtrls);
            });

            chkOverlayRectangleUse.CheckedChanged += (sender, args) => UpdateOverlayConfig(() =>
            {
                AssignToValueChangedOverlayNud(false, nudOverlayRcetangleCtrls);

                if (chkOverlayRectangleUse.Checked)
                {
                    DisableEnableAll(true, panelOverlayRectangle);
                    _configOverlay.OriginalConstraint = new Rectangle
                    {
                        X = (int)nudOverlayPointX.Value,
                        Y = (int)nudOverlayRectangleY.Value,
                        Width = (int)nudOverlayRectangleW.Value,
                        Height = (int)nudOverlayRectangleH.Value
                    };
                }
                else
                {
                    DisableEnableAll(false, panelOverlayRectangle);
                    _configOverlay.OriginalConstraint = null;
                }
                
                AssignToValueChangedOverlayNud(true, nudOverlayRcetangleCtrls);
            });

            #endregion

            /* End */
            DisableEnableAll(false, tabControlMain);
            if (_autoLoad) CmbQuestionnaireOnSelectionChangeCommitted(null, null);
        }

        #region Overlay Preview

        private void AssignToValueChangedOverlayNud(bool add, IEnumerable<NumericUpDown> ctrls)
        {
            foreach (var c in ctrls)
            {
                if (add)
                {
                    c.ValueChanged += NudOverlayValueChanged;
                }
                else
                {
                    c.ValueChanged -= NudOverlayValueChanged;
                }
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void NudOverlayValueChanged(object o, EventArgs eventArgs)
        {
            UpdateOverlayConfig(() =>
            {
                if (o == nudOverlayPointX || o == nudOverlayPointY || o == chkOverlayPointUse)
                {
                    _configOverlay.Position = new Point((int)nudOverlayPointX.Value, (int)nudOverlayPointY.Value);
                }
                else if (o == nudOverlayRectangleX || o == nudOverlayRectangleY || o == nudOverlayRectangleW || o == nudOverlayRectangleH || o == chkOverlayRectangleUse)
                {
                    _configOverlay.OriginalConstraint = new Rectangle
                    {
                        X = (int)nudOverlayRectangleX.Value,
                        Y = (int)nudOverlayRectangleY.Value,
                        Width = (int)nudOverlayRectangleW.Value,
                        Height = (int)nudOverlayRectangleH.Value
                    };
                }
            });
        }

        private void BtnOverlayResetClick()
        {
            var result = MessageBox.Show(this, @"Reset preview overlay settings?", @"iBrand", MessageBoxButtons.OKCancel);
            if (result != DialogResult.OK) return;
            _configOverlay = new PreviewOverlayInfo();
            InitializeOverlaySection();
        }

        private void InitializeOverlaySection()
        {
            if (_configOverlay == null) return;

            var previousInitValue = _initializing;
            _initializing = true;

            if (_configOverlay == null)
            {
                DisableEnableAll(false, tabPreviewOverlay);
                btnOverlayReset.Enabled = true;
            }
            else
            {
                DisableEnableAll(true, tabPreviewOverlay);
                
                var co = _configOverlay;

                if (_configOverlay.Position.X != 0 || _configOverlay.Position.Y != 0)
                {
                    nudOverlayPointX.Value = _configOverlay.Position.X;
                    nudOverlayPointY.Value = _configOverlay.Position.Y;

                    DisableEnableAll(true, panelOverlayPoint);
                }
                else
                {
                    DisableEnableAll(false, panelOverlayPoint);
                }

                if (_configOverlay.OriginalConstraint != null)
                {
                    var rect = _configOverlay.OriginalConstraint.Value;
                    nudOverlayRectangleX.Value = rect.X;
                    nudOverlayRectangleY.Value = rect.Y;
                    nudOverlayRectangleW.Value = rect.Width;
                    nudOverlayRectangleH.Value = rect.Height;

                    DisableEnableAll(true, panelOverlayRectangle);
                }
                else
                {
                    var pSizeStr = cmbPreviewSize.SelectedItem as string;
                    if (pSizeStr != null)
                    {
                        var pSize = StringUtil.SizeFromStringDefinition(pSizeStr.Split(' ')[0]);

                        nudOverlayRectangleX.Value = 0;
                        nudOverlayRectangleY.Value = 0;
                        nudOverlayRectangleW.Value = pSize.Width;
                        nudOverlayRectangleH.Value = pSize.Height;
                    }
                    DisableEnableAll(false, panelOverlayRectangle);
                }
                
                if (!co.Position.Equals(new Point(0, 0)))
                {
                    chkOverlayPointUse.Checked = true;
                }
                else
                {
                    chkOverlayPointUse.Checked = false;
                }
                chkOverlayRectangleUse.Checked = co.OriginalConstraint != null;

                lblOverlayFileName.Text = co.FileName != null ? StringUtil.GetFileName(co.FileName) : "";

                Refresh();
            }

            _initializing = previousInitValue;
        }

        private void BtnOverlayPickFile()
        {
            var newThread = new Thread(() =>
            {
                var currentFile = _configOverlay.FileName;

                var diag = new OpenFileDialog
                {
                    FileName = currentFile,
                    Filter = FILE_FILTER_IMAGES,
                    FilterIndex = 1,
                    CheckFileExists = true,
                    Multiselect = false
                };

                var result = diag.ShowDialog();
                if (result != DialogResult.OK) return;
                if (!File.Exists(diag.FileName)) return;

                FormUtil.SafeCall(this, () =>
                {
                    lblOverlayFileName.Text = StringUtil.GetFileName(diag.FileName);
                    _configOverlay.FileName = diag.FileName;
                });
            });
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }

        private void GenerateOverlayPicture()
        {
            var pSizeStr = cmbPreviewSize.SelectedItem as string;
            if (pSizeStr == null || _configOverlay == null) return;

            if (!_dummyFiles.Any())
            {
                GenerateDummyImages();
                if (!_dummyFiles.Any()) return;
            }
            
            var createdImage = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".jpg");
            var pSize = StringUtil.SizeFromStringDefinition(pSizeStr.Split(' ')[0]);
            using (var overlayImage = FileUtil.LoadBitmap(_configOverlay.FileName))
            {
                using (var baseImage = FileUtil.LoadBitmap(_dummyFiles[0]))
                {
                    using (var result = baseImage.ResizeForView(pSize.Width, pSize.Height, overlayImage, _configOverlay.Position, _configOverlay.OriginalConstraint))
                    {
                        result.Save(80, createdImage);
                    }
                }
            }
            if (File.Exists(createdImage))
            {
                pboxOverlayExample.ImageLocation = createdImage;
            }
        }

        #endregion

        #region Print

        private void GeneratePrintPicture()
        {
            var qInfo = cmbQuestionnaire.SelectedItem as QuestionnaireInfo;
            if (qInfo == null || _config?.PrintOptions == null) return;

            if (!_dummyFiles.Any())
            {
                GenerateDummyImages();
                if (!_dummyFiles.Any()) return;
            }

            var mockScreen = new MockScreen();
            var mockScreenConfig = new ScreenConfig(qInfo.ProjectId, qInfo.Id, 0);

            var createdImage = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".jpg");
            ImageListTransform.CreatePictureToPrint(mockScreen, _dummyFiles, mockScreenConfig, _config.PrintOptions, createdImage);

            if (File.Exists(createdImage))
            {
                pboxPrintExample.ImageLocation = createdImage;
            }
        }
        
        private void BtnPrintResetClick()
        {
            var result = MessageBox.Show(this, @"Reset Print image settings?", @"iBrand", MessageBoxButtons.OKCancel);
            if (result != DialogResult.OK) return;
            _config.PrintOptions = new ImageListTransformOptionsPrint();
            InitializePrintSection();
        }

        private void BtnPrintPickFile()
        {
            if (_config.PrintOptions.PrintFrameSelector != null && !(_config.PrintOptions.PrintFrameSelector is ImageSelectorSingle))
            {
                // On désactive le choix.
                lblPrintFrameFilePath.Text = @"Cannot use file selector. Other type of selection has precedence";
            }
            else
            {
                var newThread = new Thread(() =>
                {
                    var selector = _config.PrintOptions.PrintFrameSelector as ImageSelectorSingle;
                    var currentFile = selector?.ImageFile;

                    var diag = new OpenFileDialog
                    {
                        FileName = currentFile,
                        Filter = FILE_FILTER_IMAGES,
                        FilterIndex = 1,
                        CheckFileExists = true,
                        Multiselect = false
                    };

                    var result = diag.ShowDialog();
                    if (result != DialogResult.OK) return;
                    if (!File.Exists(diag.FileName)) return;

                    FormUtil.SafeCall(this, () =>
                    {
                        lblPrintFrameFilePath.Text = StringUtil.GetFileName(diag.FileName);
                        _config.PrintOptions.PrintFrameSelector = new ImageSelectorSingle(diag.FileName);
                    });
                });
                newThread.SetApartmentState(ApartmentState.STA);
                newThread.Start();
            }
        }

        private void BtnPrintPickColorClick()
        {
            var diag = new ColorDialog
            {
                AllowFullOpen = true,
                SolidColorOnly = false,
                ShowHelp = false,
                Color = _config.PrintOptions.BackgroundColor ?? Color.White,
                FullOpen = true,
                AnyColor = true,
                CustomColors = _lastCustomColors
            };
            var result = diag.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                _lastCustomColors = diag.CustomColors;
                _config.PrintOptions.BackgroundColor = diag.Color;
                chkPrintbkgColorEnable.Checked = true; // provoque l'événement.
            }
        }

        private void ChkPrintbkgColorEnableOnCheckedChanged()
        {
            var state = chkPrintbkgColorEnable.Checked;
            btnPrintColorPicker.Enabled = state;
            if (state)
            {
                lblPrintBackgroundColorExample.Visible = true;
                if (!_config.PrintOptions.BackgroundColor.HasValue)
                {
                    _config.PrintOptions.BackgroundColor = Color.White;
                }
                lblPrintBackgroundColorExample.BackColor = _config.PrintOptions.BackgroundColor.Value;
            }
            else
            {
                lblPrintBackgroundColorExample.Visible = false;
            }
        }
        
        private void InitializePrintSection()
        {
            if (_config == null) return;

            var previousInitValue = _initializing;
            _initializing = true;

            if (_config.PrintOptions == null)
            {
                DisableEnableAll(false, tabPagePrint);
                btnPrintReset.Enabled = true;
            }
            else
            {
                var po = _config.PrintOptions;
                if (po.RotateFlip.HasValue)
                {
                    cmbPrintRotateFlip.SelectedItem = po.RotateFlip.Value;
                }
                else
                {
                    cmbPrintRotateFlip.SelectedIndex = -1;
                }

                if (po.OffsetPerc.HasValue)
                {
                    nudPrintOffsetPerc.Value = (int)(po.OffsetPerc.Value * 100);
                }
                else
                {
                    nudPrintOffsetPerc.Value = 5;
                }

                nudPrintOffsetX.Value = po.OffsetExtraSide;
                nudPrintOffsetY.Value = po.OffsetExtraTop;

                chkPrintFrameOver.Checked = po.FrameOver;
                chkPrintbkgColorEnable.Checked = po.BackgroundColor.HasValue;
                if (po.BackgroundColor.HasValue)
                {
                    lblPrintBackgroundColorExample.Visible = true;
                    lblPrintBackgroundColorExample.BackColor = po.BackgroundColor.Value;
                }
                else
                {
                    lblPrintBackgroundColorExample.Visible = false;
                }

                chkPrintResizeToFrame.Checked = po.UseFullFramePicture;
                chkPrintUseOnlyFirstPicture.Checked = po.UseOnlyFirstPicture;

                nudPrintJpegSaveQuality.Value = po.JpegSaveQuality;

                var singleSelector = po.PrintFrameSelector as ImageSelectorSingle;
                lblPrintFrameFilePath.Text = singleSelector != null ? StringUtil.GetFileName(singleSelector.ImageFile) : "";

                DisableEnableAll(true, tabPagePrint);
                Refresh();
            }

            _initializing = previousInitValue;
        }

        #endregion

        #region section de méthodes pour toutes les sections

        /// <summary>
        /// Vérifie si on peut mettre à jour la configuration et si oui, exécute l'action donné en paramètre.
        /// </summary>
        /// <param name="exec"></param>
        private void UpdateConfig(Action exec)
        {
            if (_config != null && !_initializing)
            {
                exec?.Invoke();
            }
        }

        private void UpdateOverlayConfig(Action exec)
        {
            if (_configOverlay != null && !_initializing)
            {
                exec?.Invoke();
            }
        }

        private void DisableEnableAll(bool state, Control source)
        {
            if (InvokeRequired)
            {
                FormUtil.SafeCall(this, () => DisableEnableAll(state, source));
                return;
            }

            foreach (var ctrl in source.GetAll(typeof(ComboBox), typeof(TextBox), typeof(CheckBox), typeof(NumericUpDown), typeof(Button)))
            {
                ctrl.Enabled = state;
            }
        }

        private void CmbQuestionnaireOnSelectionChangeCommitted(object sender, EventArgs eventArgs)
        {
            var qInfo = cmbQuestionnaire.SelectedItem as QuestionnaireInfo;
            if (qInfo == null) return;
            
            InitializeFromFile(qInfo);
        }
        
        private void GenerateDummyImages()
        {
            flowImages.Controls.Clear();

            foreach (var imageFile in _dummyFiles)
            {
                try
                {
                    File.Delete(imageFile);
                }
                catch (System.Exception)
                {
                    // die nice
                }
            }

            _dummyFiles.Clear();

            var maxFile = nudPotentialNbrOfPictures.Value;
            var dummyType = (DummyImageType)cmbDummyType.SelectedItem;
            var imageSize = StringUtil.SizeFromStringDefinition(cmbCameraResolution.SelectedItem.ToString().Split(' ')[0]);
            var thmbList = new List<Bitmap>();

            var cIndex = 0;
            var w = _cameraLandscape ? imageSize.Width : imageSize.Height;
            var h = _cameraLandscape ? imageSize.Height : imageSize.Width;

            Font font = null;
            Rectangle[] rectCoords = null;

            for (var i = 0; i < maxFile; i++)
            {
                var fileName = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".jpg");

                using (var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        switch (dummyType)
                        {
                            case DummyImageType.CheckeredColor:

                                if (rectCoords == null)
                                {
                                    if (_cameraLandscape)
                                    {
                                        var yMid = h / 2;
                                        var xSplitWidth = w / 3;
                                        rectCoords = new[]
                                        {
                                            new Rectangle(0,0, xSplitWidth, yMid),
                                            new Rectangle(xSplitWidth, yMid, xSplitWidth, yMid),
                                            new Rectangle(xSplitWidth * 2, 0, xSplitWidth, yMid)
                                        };
                                    }
                                    else
                                    {
                                        var ySplit = h / 3;
                                        var xMid = w / 2;
                                        rectCoords = new[]
                                        {
                                            new Rectangle(0,0, xMid, ySplit),
                                            new Rectangle(xMid, ySplit, xMid, ySplit),
                                            new Rectangle(0, ySplit * 2, xMid, ySplit)
                                        };
                                    }
                                }

                                // Fond
                                g.FillRectangle(Brushes.White, 0, 0, w, h);

                                // rectangle 1
                                foreach (var rec in rectCoords)
                                {
                                    g.FillRectangle(new SolidBrush(ColorArray[cIndex]), rec);
                                    cIndex++;
                                    if (cIndex >= ColorArray.Length) cIndex = 0;
                                }
                                break;

                            case DummyImageType.NumberedImages:
                                if (font == null)
                                {
                                    font = new Font(FontFamily.GenericSerif, 0.6f * h, FontStyle.Bold, GraphicsUnit.Pixel);
                                }

                                g.FillRectangle(new SolidBrush(ColorArray[cIndex]), 0, 0, w, h);
                                var text = (i + 1).ToString();
                                var size = g.MeasureString(text, font);
                                g.DrawString(text, font, Brushes.Black, (w - size.Width) / 2.0f, (h - size.Height) / 2.0f);

                                cIndex++; if (cIndex >= ColorArray.Length) cIndex = 0;
                                break;

                            case DummyImageType.SolidColor:
                                g.FillRectangle(new SolidBrush(ColorArray[cIndex]), 0, 0, w, h);

                                cIndex++; if (cIndex >= ColorArray.Length) cIndex = 0;
                                break;
                        }
                    }

                    bmp.Save(80, fileName);

                    // Création du thumbnail.
                    var thmb = bmp.Resize(80, (int)(80f / w * h));
                    thmbList.Add(thmb);
                }

                _dummyFiles.Add(fileName);
            }

            foreach (var thmb in thmbList)
            {
                var pic = new PictureBox
                {
                    AutoSize = true,
                    Image = thmb
                };

                // On créé un version mini de chaque fichier.
                flowImages.Controls.Add(pic);
            }
            flowImages.Refresh();
        }
        
        private void InitializeFromFile(QuestionnaireInfo qInfo)
        {
            if (_initializing) return;

            if (qInfo == null) return;
            _initializing = true;

            _config = new ImageListTransformOptions();
            _configFile = new FileInfo(DirectoryUtil.GetLocalIbrandQuestionnaireDir(qInfo.ProjectId, qInfo.Id) + _config.DefaultConfigFileName);
            _configFile.Refresh();

            if (!_configFile.Exists)
            {
                JsonUtil.WriteJsonFile(_configFile, _config);
            }
            else
            {
                JsonUtil.UpdateFromJson(_configFile, _config);
            }

            _configOverlay = new PreviewOverlayInfo();
            _configOverlayFile = new FileInfo(DirectoryUtil.GetLocalIbrandQuestionnaireDir(qInfo.ProjectId, qInfo.Id) + _configOverlay.DefaultConfigFileName);
            _configOverlayFile.Refresh();

            if (!_configOverlayFile.Exists)
            {
                var list = new List<PreviewOverlayInfo> { _configOverlay };
                JsonUtil.WriteJsonFile(_configOverlayFile, list);
            }
            else
            {
                var list = new List<PreviewOverlayInfo>();
                JsonUtil.UpdateFromJson(_configOverlayFile, list);
                _configOverlay = list.Any() ? list.First() : new PreviewOverlayInfo();
            }

            InitializePrintSection();
            InitializeOverlaySection();

            /* TODO: Autres actions, à venir */

            _initializing = false;
        }
        
        private void ClearDummies()
        {
            _dummyFiles.Clear();
            flowImages.Controls.Clear();
            Refresh();
        }
        
        private void RadioOrientationOnCheckedChanged(object o, EventArgs eventArgs)
        {
            var btn = o as RadioButton;
            if (btn == null) return;

            var newVal = btn == radioOrientationLandscape && btn.Checked;
            if (_cameraLandscape == newVal) return;
            _cameraLandscape = newVal;
            ClearDummies();
        }

        private static void ShowFullPicture(PictureBox pictureBox)
        {
            var fileShown = pictureBox?.ImageLocation;
            if (fileShown == null || !File.Exists(fileShown)) return;
            var img = Image.FromFile(fileShown);
            new PreviewDialog(img).Show();
        }

        #endregion
    }
}
