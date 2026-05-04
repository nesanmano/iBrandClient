using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia.Gpu.OpenCV;

namespace NomadLogic.Multimedia.Gpu.Ui.ChromaKeyDialogs
{
    /// <summary>
    /// Forme pour visualiser et configurer la configuration de ChromaKey en utilisant OpenCV.
    /// </summary>
    public class PreviewDialogChromaKeyOpenCv : PreviewDialogChromaKeyAbstract<ChromaKeyInfoOpenCvCieLuv>
    {
        private ImageBox _imageBox;
        private FlowLayoutPanel _flowPanel;
        private ChannelSplitImageLuv _baseImageData;
        private Image<Bgra, byte> _transformedImageData;
        private Rectangle _clicRectangle;
        private Button _btnResetImageZoom;

        private Point _rectStartPoint;
        private Rectangle _rect;
        private Rectangle _realImageRect;
        private readonly Brush _selectionBrush = new SolidBrush(Color.FromArgb(128, 64, 64, 64));
        private bool _scale;

        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        private static void ConvertCoordinates(ImageBox pic, out int x0, out int y0, int x, int y) {

            x0 = 0;
            y0 = 0;
            if (pic.Image == null) { return; }

            var picHgt = pic.ClientSize.Height;
            var picWid = pic.ClientSize.Width;
            var imgHgt = pic.Image.Size.Height;
            var imgWid = pic.Image.Size.Width;

            x0 = x;
            y0 = y;
            switch (pic.SizeMode)
            {
                case PictureBoxSizeMode.AutoSize:
                case PictureBoxSizeMode.Normal:
                    // These are okay. Leave them alone.
                    break;
                case PictureBoxSizeMode.CenterImage:
                    x0 = x - (picWid - imgWid) / 2;
                    y0 = y - (picHgt - imgHgt) / 2;
                    break;
                case PictureBoxSizeMode.StretchImage:
                    x0 = (int)(imgWid * x / (float)picWid);
                    y0 = (int)(imgHgt * y / (float)picHgt);
                    break;
                case PictureBoxSizeMode.Zoom:
                    var picAspect = picWid / (float)picHgt;
                    var imgAspect = imgWid / (float)imgWid;
                    if (picAspect > imgAspect)
                    {
                        y0 = (int)(imgHgt * y / (float)picHgt);
                        float scaledWidth = imgWid * picHgt / imgHgt;
                        var dx = (picWid - scaledWidth) / 2;
                        x0 = (int)((x - dx) * imgHgt / picHgt);
                    }
                    else
                    {
                        x0 = (int)(imgWid * x / (float)picWid);
                        float scaledHeight = imgHgt * picWid / imgWid;
                        var dy = (picHgt - scaledHeight) / 2;
                        y0 = (int)((y - dy) * imgWid / picWid);
                    }
                    break;
            }
        } 
        
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            _rectStartPoint = e.Location;
            Invalidate();
        }
        
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            ConvertCoordinates(_imageBox, out var x0, out var y0, e.X, e.Y);

            //Coordinates at input picture box
            if (e.Button != MouseButtons.Left)
                return;
            var tempEndPoint = e.Location;
            _rect.Location = new Point(
                Math.Min(_rectStartPoint.X, tempEndPoint.X),
                Math.Min(_rectStartPoint.Y, tempEndPoint.Y));
            _rect.Size = new Size(
                Math.Abs(_rectStartPoint.X - tempEndPoint.X),
                Math.Abs(_rectStartPoint.Y - tempEndPoint.Y));
            
            //Coordinates at real image - Create ROI
            ConvertCoordinates(_imageBox, out x0, out y0, 
            _rectStartPoint.X, _rectStartPoint.Y);
            ConvertCoordinates(_imageBox, out var x1, out var y1, tempEndPoint.X, tempEndPoint.Y);
            _realImageRect.Location = new Point(
                Math.Min(x0, x1),
                Math.Min(y0, y1));
            _realImageRect.Size = new Size(
                Math.Abs(x0 - x1),
                Math.Abs(y0 - y1));
            
            ((PictureBox)sender).Invalidate();
        }
        
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_imageBox.Image == null) return;
            if (_rect.Width <= 0 || _rect.Height <= 0) return;
            e.Graphics.SetClip(_rect, System.Drawing.Drawing2D.CombineMode.Exclude);
            e.Graphics.FillRectangle(_selectionBrush, new Rectangle(0, 0, ((PictureBox)sender).Width, ((PictureBox)sender).Height));
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e) {
            if (_transformedImageData == null) return;

            if (_scale) {
                _scale = false;
                _transformedImageData.ROI = new Rectangle(0,0,_baseImageData.BaseMat.Width, _baseImageData.BaseMat.Height);
            } else if (_realImageRect.Width > 0 && _realImageRect.Height > 0) {
                _transformedImageData.ROI = _realImageRect;
                _scale = true;
            } else {
                _transformedImageData.ROI = new Rectangle(0,0,_baseImageData.BaseMat.Width, _baseImageData.BaseMat.Height);
            }

            _imageBox.Image = _transformedImageData;
            _realImageRect = Rectangle.Empty;
            _rect = new Rectangle();
        }
        
        /// <inheritdoc />
        protected override PictureBox GetTransformedPictureBox()
        {
            _imageBox = new ImageBox
            {
                FunctionalMode = ImageBox.FunctionalModeOption.Minimum,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            _imageBox.Paint += pictureBox_Paint;
            _imageBox.MouseUp += pictureBox_MouseUp;
            _imageBox.MouseDown += pictureBox_MouseDown;
            _imageBox.MouseMove += pictureBox_MouseMove;
            return _imageBox;
        }

        /// <inheritdoc />
        protected override void InitializeFilterFromDisk() {
            _filter = ChromaKeyManager.GetDefaultChromaConfigOpenCvLuv();
            InitFlowPanelValues();
        }

        private void InitFlowPanelValues()
        {
            var priorBlock = _blockChanges;
            _blockChanges = true;
            var ite = _flowPanel.Controls.GetEnumerator();
            while (ite != null && ite.MoveNext())
            {
                var cont = ite.Current as Control;
                cont?.Dispose();
            }
            _flowPanel.Controls.Clear();

            foreach (var keyData in _filter.KeyingData)
            {
                AddChromaKeyData(keyData, false);
            }
            _blockChanges = priorBlock;
        }

        private void AddChromaKeyData(ChromaKeyInfoOpenCvCieLuvData keyData, bool addToKeyingData = true)
        {
            var priorBlock = _blockChanges;
            _blockChanges = true;
            if (addToKeyingData) _filter.KeyingData.Add(keyData);

            var colorPanel = new ChromaKeyCieLuvDataControl(keyData);
            colorPanel.CallToRemove += (sender, args) =>
            {
                colorPanel.Dispose();
                _filter.KeyingData.Remove(keyData);
                LaunchTransform();
            };
            colorPanel.ChromaKeyChange += (sender, args) =>
            {
                LaunchTransform();
            };
            _flowPanel.Controls.Add(colorPanel.GetPanel());
            _blockChanges = priorBlock;
        }

        /// <inheritdoc />
        protected override void InitCustomElements()
        {
            pbOriginal.MouseClick += OnOriginalPictureClicked;
            ButtonApply.Visible = false;

            _btnResetImageZoom = new Button
            {
                Font = SMALLER_FONT,
                Location = new Point(3, 308),
                Size = new Size(80, 23),
                Text = @"Reset keying",
                UseVisualStyleBackColor = true
            };
            _btnResetImageZoom.Click += OnResetClick;
            
            panelKeyedPicture.Controls.Add(_btnResetImageZoom);
            panelKeyedPicture.Controls.Add(_btnFullSizePreview);
            panelKeyedPicture.Controls.Add(_btnSavePreview);
            panelKeyedPicture.Invalidate();

            _flowPanel = new FlowLayoutPanel
            {
                Location = new Point(550, 12),
                Name = "PanelValues",
                Size = new Size(471, 321),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = true,
                Parent = this,
                Visible = true,
                Enabled = true,
                BorderStyle = BorderStyle.Fixed3D
            };

            Controls.Add(_flowPanel);
            _flowPanel.PerformLayout();
        }
        
        /// <inheritdoc />
        protected override Bitmap GetFullPreviewKeyedImage()
        {
            if (_filter.KeyingData.Count <= 0) return null;
            return OpenCvUtil.ChromaKeyOpenCv(_sourceBitmapFile, _filter);
        }

        /// <inheritdoc />
        protected override void SavePreview(string path) {
            if (_filter.KeyingData.Count <= 0) return;
            OpenCvUtil.ChromaKeyOpenCv(_sourceBitmapFile, path, _filter);
        }

        private void OnResetClick(object sender, EventArgs e) {
            if (_transformedImageData == null) return;
            _transformedImageData.ROI = new Rectangle(0,0,_baseImageData.BaseMat.Width, _baseImageData.BaseMat.Height);
            _imageBox.Image = _transformedImageData;
            Invalidate();
            _scale = false;
        }

        private void OnOriginalPictureClicked(object sender, MouseEventArgs e)
        {
            if (_baseImageData == null) return;

            if (e.X < _clicRectangle.X || e.X > _clicRectangle.X + _clicRectangle.Width) return;
            if (e.Y < _clicRectangle.Y || e.Y > _clicRectangle.Y + _clicRectangle.Height) return;

            var bgr = _baseImageData.BaseImageBgr.GetData(e.Y - _clicRectangle.Y, e.X - _clicRectangle.X);

            var tmp = new ChromaKeyInfoOpenCvCieLuvData
            {
                Tolerance = 15,
                Smoothing = 5,
                R = bgr[2],
                G = bgr[1],
                B = bgr[0]
            };
            AddChromaKeyData(tmp);
            LaunchTransform();
        }

        /// <inheritdoc />
        protected override void ResetToDefault(object sender)
        {
            _filter = sender == ButtonDefaultGreenScreen ? ChromaKeyInfoOpenCvCieLuv.NewInstanceChromaKeyGreen() : ChromaKeyInfoOpenCvCieLuv.NewInstanceChromaKeyBlue();
            InitFlowPanelValues();

            Task.Run(() =>
            {
                Task.Delay(100);
                LaunchTransform();
            });
        }

        /// <inheritdoc />
        protected override void LaunchTransfromDo()
        {
            if (_baseImageData == null || _transformedImageData == null) return;

            OpenCvUtil.ChromaKeyOpenCv(_baseImageData, _filter, false, null, _transformedImageData.Data);
            _imageBox.Invalidate();
        }

        /// <inheritdoc />
        protected override void SaveChromaKeyConfigToDisk()
        {
            if (!JsonUtil.WriteJsonFile(new System.IO.FileInfo(ChromaKeyInfoOpenCvCieLuv.DefaultConfigFileLocation), _filter))
            {
                MessageBox.Show(this, @"Could not write filter to file.", @"iBrand",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <inheritdoc />
        protected override void SourceImageChanged()
        {
            PrepareOpenCvImages();
        }

        private void PrepareOpenCvImages()
        {
            _baseImageData?.Dispose();
            _transformedImageData?.Dispose();
            _transformedImageData = null;
            _imageBox.Image = null;
            _imageBox.DisplayedImage = null;
            _baseImageData = null;
            
            _baseImageData = OpenCvUtil.SplitImage<ChannelSplitImageLuv>(_sourceBitmapFile, pbOriginal.Size);
            _clicRectangle = new Rectangle(
                (pbOriginal.Size.Width - _baseImageData.BaseMat.Width) / 2, 
                (pbOriginal.Size.Height - _baseImageData.BaseMat.Height) / 2, 
                _baseImageData.BaseMat.Width, 
                _baseImageData.BaseMat.Height);

            _transformedImageData = _baseImageData.GetReferencedImageDataFromSplit();
            _imageBox.Image = _transformedImageData;

            if (_filter != null)
            {
                LaunchTransform();
            }
        }
    }
}