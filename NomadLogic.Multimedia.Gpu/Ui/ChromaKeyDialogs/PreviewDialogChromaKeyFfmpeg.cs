using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.Multimedia.Gpu.Ui.ChromaKeyDialogs
{
    /// <summary>
    /// Implémentation pour le dialogue de visualisation de clé chromatique avec FFMPEG.
    /// </summary>
    [SuppressMessage("ReSharper", "LocalizableElement")]
    public class PreviewDialogChromaKeyFfmpeg : PreviewDialogChromaKeyAbstract<FfmpegChromaKeyInfo>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Label _lblRed;
        private TrackBar _redMin;
        private TrackBar _blendMin;
        private TrackBar _similarityMin;
        private TrackBar _blueMin;
        private TrackBar _greenMin;
        private Label _lblBlend;
        private Label _lblSimilarity;
        private Label _lblBlue;
        private Label _lblGreen;
        private TextBox _textBoxBlend;
        private TextBox _textBoxSimilarity;
        private TextBox _textBoxBlue;
        private TextBox _textBoxGreen;
        private TextBox _textBoxRed;
        private Panel _panelValues;

        /// <inheritdoc />
        protected override void ResetToDefault(object sender)
        {
            _filter = sender == ButtonDefaultGreenScreen ? FfmpegChromaKeyInfo.NewInstanceChromaKeyGreen() : FfmpegChromaKeyInfo.NewInstanceChromaKeyBlue();

            _redMin.Value = _filter.Red;
            _greenMin.Value = _filter.Green;
            _blueMin.Value = _filter.Blue;
            _similarityMin.Value = (int)(_filter.Similarity * 100);
            _blendMin.Value = (int)(_filter.Blend * 100);
        }

        /// <inheritdoc />
        protected override void InitializeFilterFromDisk()
        {
            _filter = ChromaKeyManager.GetDefaultChromaConfigFfmpeg();

            // Initialisation des barres
            InitTrackBarRgb(_redMin, _filter.Red, _textBoxRed);
            InitTrackBarRgb(_greenMin, _filter.Green, _textBoxGreen);
            InitTrackBarRgb(_blueMin, _filter.Blue, _textBoxBlue);
            InitTrackBarFloat(_similarityMin, (int)(_filter.Similarity * 100), _textBoxSimilarity);
            InitTrackBarFloat(_blendMin, (int)(_filter.Blend * 100), _textBoxBlend);
        }

        /// <inheritdoc />
        protected override void InitCustomElements()
        {

            _lblRed = new Label();
            _redMin = new TrackBar();
            _panelValues = new Panel();
            _blendMin = new TrackBar();
            _similarityMin = new TrackBar();
            _blueMin = new TrackBar();
            _greenMin = new TrackBar();
            _lblBlend = new Label();
            _lblSimilarity = new Label();
            _lblBlue = new Label();
            _lblGreen = new Label();
            _textBoxRed = new TextBox();
            _textBoxGreen = new TextBox();
            _textBoxBlue = new TextBox();
            _textBoxSimilarity = new TextBox();
            _textBoxBlend = new TextBox();
            ((System.ComponentModel.ISupportInitialize)(_redMin)).BeginInit();
            _panelValues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(_blendMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_similarityMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_blueMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_greenMin)).BeginInit();

            // 
            // lblRed
            // 
            _lblRed.Location = new Point(3, 4);
            _lblRed.Name = "lblRed";
            _lblRed.Size = new Size(34, 40);
            _lblRed.TabIndex = 17;
            _lblRed.Text = "Red";
            _lblRed.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _redMin
            // 
            _redMin.LargeChange = 10;
            _redMin.Location = new Point(131, 8);
            _redMin.Maximum = 255;
            _redMin.Name = "_redMin";
            _redMin.Size = new Size(334, 45);
            _redMin.SmallChange = 2;
            _redMin.TabIndex = 28;
            // 
            // PanelValues
            // 
            _panelValues.Controls.Add(_textBoxBlend);
            _panelValues.Controls.Add(_textBoxSimilarity);
            _panelValues.Controls.Add(_textBoxBlue);
            _panelValues.Controls.Add(_textBoxGreen);
            _panelValues.Controls.Add(_textBoxRed);
            _panelValues.Controls.Add(_blendMin);
            _panelValues.Controls.Add(_similarityMin);
            _panelValues.Controls.Add(_blueMin);
            _panelValues.Controls.Add(_greenMin);
            _panelValues.Controls.Add(_redMin);
            _panelValues.Controls.Add(_lblBlend);
            _panelValues.Controls.Add(_lblSimilarity);
            _panelValues.Controls.Add(_lblBlue);
            _panelValues.Controls.Add(_lblGreen);
            _panelValues.Controls.Add(_lblRed);
            _panelValues.Location = new Point(550, 12);
            _panelValues.Name = "PanelValues";
            _panelValues.Size = new Size(471, 321);
            _panelValues.TabIndex = 1;
            // 
            // _blendMin
            // 
            _blendMin.LargeChange = 30;
            _blendMin.Location = new Point(131, 239);
            _blendMin.Maximum = 100;
            _blendMin.Name = "_blendMin";
            _blendMin.Size = new Size(334, 45);
            _blendMin.TabIndex = 36;
            // 
            // _similarityMin
            // 
            _similarityMin.LargeChange = 30;
            _similarityMin.Location = new Point(131, 179);
            _similarityMin.Maximum = 100;
            _similarityMin.Name = "_similarityMin";
            _similarityMin.Size = new Size(334, 45);
            _similarityMin.TabIndex = 34;
            // 
            // _blueMin
            // 
            _blueMin.LargeChange = 30;
            _blueMin.Location = new Point(131, 119);
            _blueMin.Maximum = 255;
            _blueMin.Name = "_blueMin";
            _blueMin.Size = new Size(334, 45);
            _blueMin.SmallChange = 15;
            _blueMin.TabIndex = 32;
            // 
            // _greenMin
            // 
            _greenMin.LargeChange = 10;
            _greenMin.Location = new Point(131, 59);
            _greenMin.Maximum = 255;
            _greenMin.Name = "_greenMin";
            _greenMin.Size = new Size(334, 45);
            _greenMin.SmallChange = 2;
            _greenMin.TabIndex = 30;
            // 
            // lblBlend
            // 
            _lblBlend.Location = new Point(3, 234);
            _lblBlend.Name = "lblBlend";
            _lblBlend.Size = new Size(34, 50);
            _lblBlend.TabIndex = 26;
            _lblBlend.Text = "Blend";
            _lblBlend.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSimilarity
            // 
            _lblSimilarity.Location = new Point(3, 169);
            _lblSimilarity.Name = "lblSimilarity";
            _lblSimilarity.Size = new Size(52, 50);
            _lblSimilarity.TabIndex = 25;
            _lblSimilarity.Text = "Similarity";
            _lblSimilarity.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBlue
            // 
            _lblBlue.Location = new Point(3, 119);
            _lblBlue.Name = "lblBlue";
            _lblBlue.Size = new Size(34, 50);
            _lblBlue.TabIndex = 24;
            _lblBlue.Text = "Blue";
            _lblBlue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGreen
            // 
            _lblGreen.Location = new Point(3, 59);
            _lblGreen.Name = "lblGreen";
            _lblGreen.Size = new Size(42, 50);
            _lblGreen.TabIndex = 23;
            _lblGreen.Text = "Green";
            _lblGreen.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBoxRed
            // 
            _textBoxRed.Location = new Point(54, 15);
            _textBoxRed.Name = "textBoxRed";
            _textBoxRed.Size = new Size(71, 20);
            _textBoxRed.TabIndex = 37;
            // 
            // textBoxGreen
            // 
            _textBoxGreen.Location = new Point(54, 75);
            _textBoxGreen.Name = "textBoxGreen";
            _textBoxGreen.Size = new Size(71, 20);
            _textBoxGreen.TabIndex = 38;
            // 
            // textBoxBlue
            // 
            _textBoxBlue.Location = new Point(54, 135);
            _textBoxBlue.Name = "textBoxBlue";
            _textBoxBlue.Size = new Size(71, 20);
            _textBoxBlue.TabIndex = 39;
            // 
            // textBoxSimilarity
            // 
            _textBoxSimilarity.Location = new Point(54, 185);
            _textBoxSimilarity.Name = "textBoxSimilarity";
            _textBoxSimilarity.Size = new Size(71, 20);
            _textBoxSimilarity.TabIndex = 40;
            // 
            // textBoxBlend
            // 
            _textBoxBlend.Location = new Point(54, 250);
            _textBoxBlend.Name = "textBoxBlend";
            _textBoxBlend.Size = new Size(71, 20);
            _textBoxBlend.TabIndex = 41;

            //Evenements des Sliders
            _redMin.ValueChanged += ValueChanged;
            _greenMin.ValueChanged += ValueChanged;
            _blueMin.ValueChanged += ValueChanged;
            _similarityMin.ValueChanged += ValueChanged;
            _blendMin.ValueChanged += ValueChanged;

            _textBoxRed.KeyPress += TextBoxKeyPressDecimal;
            _textBoxRed.TextChanged += TextChanged;

            _textBoxGreen.KeyPress += TextBoxKeyPressDecimal;
            _textBoxGreen.TextChanged += TextChanged;

            _textBoxBlue.KeyPress += TextBoxKeyPressDecimal;
            _textBoxBlue.TextChanged += TextChanged;

            _textBoxSimilarity.KeyPress += TextBoxKeyPressDecimal;
            _textBoxSimilarity.TextChanged += TextChanged;

            _textBoxBlend.KeyPress += TextBoxKeyPressDecimal;
            _textBoxBlend.TextChanged += TextChanged;

            Controls.Add(_panelValues);
            ((System.ComponentModel.ISupportInitialize)(_blendMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_similarityMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_blueMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_greenMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_redMin)).EndInit();
            _panelValues.ResumeLayout(false);
            _panelValues.PerformLayout();
        }

        private static void InitTrackBarRgb(TrackBar bar, int value, Control texBox)
        {
            bar.LargeChange = 3;
            bar.Minimum = 0;
            bar.Maximum = 255;
            bar.SmallChange = 2;
            bar.TickFrequency = 20;
            bar.Value = value;
            texBox.Text = value.ToString();
        }

        private static void InitTrackBarFloat(TrackBar bar, int value, Control textBox)
        {
            bar.LargeChange = 1;
            bar.Minimum = 0;
            bar.Maximum = 20;
            bar.SmallChange = 1;
            bar.TickFrequency = 1;
            bar.Value = value;
            textBox.Text = value.ToString();
        }

        /// <inheritdoc />
        protected override void SetLanguageTextsForLabels()
        {
            base.SetLanguageTextsForLabels();

            _lblRed.Text = "Red";
            _lblGreen.Text = "Green";
            _lblBlue.Text = "Blue";
            _lblSimilarity.Text = "Similarity";
            _lblBlend.Text = "Blend";
        }

        /// <inheritdoc />
        protected override void SaveChromaKeyConfigToDisk()
        {
            if (!JsonUtil.WriteJsonFile(new FileInfo(FfmpegChromaKeyInfo.DefaultConfigFileLocation), _filter))
            {
                MessageBox.Show(this, "Could not write filter configuration to disk.", "Ibrand",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <inheritdoc />
        protected override Bitmap GetFullPreviewKeyedImage()
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>();
            var tmpOut = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
            FfmpegUtil.ChromakeyImage(conf.FfmpegLocation, _sourceBitmapFile, tmpOut, _filter);

            Bitmap copy = null;
            if (File.Exists(tmpOut))
            {
                var fromFile = FileUtil.LoadBitmap(tmpOut);
                copy = fromFile.Clone(new Rectangle(0, 0, fromFile.Width, fromFile.Height), fromFile.PixelFormat);
                fromFile.Dispose();
                File.Delete(tmpOut);
            }
            else
            {
                log.Error("Temporary filtered file is inexistent: {0}", tmpOut);
            }

            return copy;
        }

        /// <inheritdoc />
        protected override void SavePreview(string path)
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>();
            FfmpegUtil.ChromakeyImage(conf.FfmpegLocation, _sourceBitmapFile, path, _filter);
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (_blockChanges)
            {
                return;
            }

            if (sender == _redMin)
            {
                _filter.Red = _redMin.Value;
                _textBoxRed.Text = _redMin.Value.ToString();
            }
            else if (sender == _greenMin)
            {
                _filter.Green = _greenMin.Value;
                _textBoxGreen.Text = _greenMin.Value.ToString();
            }
            else if (sender == _blueMin)
            {
                _filter.Blue = _blueMin.Value;
                _textBoxBlue.Text = _blueMin.Value.ToString();
            }
            else if (sender == _similarityMin)
            {
                _filter.Similarity = (float)_similarityMin.Value / 100;
                _textBoxSimilarity.Text = _similarityMin.Value.ToString();
            }
            else if (sender == _blendMin)
            {
                _filter.Blend = (float)_blendMin.Value / 100;
                _textBoxBlend.Text = _blendMin.Value.ToString();
            }
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private new void TextChanged(object sender, EventArgs e)
        {
            var box = sender as TextBox;
            if (!(box?.Text.Length > 0)) return;
            var val = int.Parse(box.Text);

            if (val >= 255 || val <= 0) return;
            if (sender == _textBoxRed)
            {
                _filter.Red = val;
                _redMin.Value = val;
            }
            else if (sender == _textBoxGreen)
            {
                _filter.Green = val;
                _greenMin.Value = val;
            }
            else if (sender == _textBoxBlue)
            {
                _filter.Blue = val;
                _blueMin.Value = val;
            }
            else if (sender == _textBoxBlend)
            {
                if (val >= 100 || val <= 0) return;
                _filter.Blend = (float)val / 100;
                _blendMin.Value = val;
            }
            else if (sender == _textBoxSimilarity)
            {
                if (val >= 100 || val <= 0) return;
                _filter.Similarity = (float)val / 100;
                _similarityMin.Value = val;
            }
        }

        private void TextBoxKeyPressDecimal(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            if (!(sender is TextBox box)) return;
            if (!char.IsDigit(e.KeyChar)) return;
            var val = box.Text + e.KeyChar;
            var valInt = int.Parse(val);

            if (sender == _textBoxGreen || sender == _textBoxBlue || sender == _textBoxRed)
            {
                if (valInt > 255)
                {
                    e.Handled = true;
                }
            }
            else if (sender == _textBoxBlend || sender == _textBoxSimilarity)
            {
                if (valInt > 100)
                {
                    e.Handled = true;
                }
            }
        }

        /// <inheritdoc />
        protected override void LaunchTransfromDo()
        {
            var bmp = GetImageForControl(_sourceBitmapFile);
            var conf = ServicesContainer.GetService<NomadConfiguration>();

            var tmpIn = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".jpg");
            var tmpOut = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
            bmp.Save(90, tmpIn);

            FfmpegUtil.ChromakeyImage(conf.FfmpegLocation, tmpIn, tmpOut, _filter);
            ImageUtil.DebugDisplayTransparencyOnImage(tmpOut);

            File.Delete(tmpIn);

            Invoke(new Action(() =>
            {
                if (File.Exists(tmpOut))
                {
                    var fromFile = FileUtil.LoadBitmap(tmpOut);
                    var copy = fromFile.Clone(new Rectangle(0, 0, fromFile.Width, fromFile.Height), fromFile.PixelFormat);
                    fromFile.Dispose();
                    File.Delete(tmpOut);

                    pbTransformed.Image?.Dispose();
                    pbTransformed.Image = copy;

                }
                else
                {
                    log.Error("Temporary filtered file is inexistent: {0}", tmpOut);
                }
            }));
        }

        /// <inheritdoc />
        protected override void SourceImageChanged()
        {
            // rien ici.
        }
    }
}