using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.XmlTransformation.image;
using NomadLogic.Multimedia.XmlTransformation.Layer;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class PreviewDialogTransformation : Form, IDisposable
    {
        private readonly float _previewFactor = 0.25f;

        private delegate void ChangePicture(Bitmap newImage);

        private delegate void ChangeButtonState(Button b, bool state);

        private readonly ChangePicture _changePictureDelegate;
        private readonly PhotoLayer _mainLayer;
        private Transformation _mainTransformation;
        private readonly IRespondent _resp;
        private Bitmap _currentImage;
        private bool _blockChanges;
        private readonly int _defX;
        private readonly int _defY;
        private readonly int _defRot;
        private readonly int _defSca;
        private readonly IPreviewDialogTransformationReceiver _receiver;
        private bool _finalTransformationDone;
        private readonly ChangeButtonState _buttonDeleg;

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public PreviewDialogTransformation(IPreviewDialogTransformationReceiver receiver, IRespondent resp, TransformationsController tc)
        {
            InitializeComponent();

            _changePictureDelegate = ChangePictureMethod;

            // Force la création des "handles" de la forme.
            {
                // ReSharper disable once NotAccessedVariable
                var tmp = ButtonKeepIt.Handle;
                tmp = ButtonApply.Handle;
            }

            _buttonDeleg = ChangeButtonStateMethod;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            TransformedPictureBox.Image = null;
            TransformedPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

            _receiver = receiver;
            _currentImage = null;
            _resp = resp;
            var tmpTrans = tc.GetTransformations(resp, true);
            if (!tmpTrans?.Any() ?? true) throw new System.Exception();

            _mainTransformation = tmpTrans[0];

            foreach (var al in _mainTransformation.Layers)
            {
                if (!(al is PhotoLayer layer)) continue;
                _mainLayer = layer;
                break;
            }

            var varX = (int)(_mainTransformation.FinalWidth * 0.125);
            var varY = (int)(_mainTransformation.FinalHeight * 0.125);

            _blockChanges = true;

            SliderRotation.Minimum = -180;
            SliderRotation.Maximum = 180;
            NumRotation.Minimum = -180;
            NumRotation.Maximum = 180;
            SliderRotation.Value = _mainLayer.Rotation;
            NumRotation.Value = _mainLayer.Rotation;
            _defRot = _mainLayer.Rotation;

            var scale = _mainLayer.Scale;
            SliderScale.Minimum = 1;
            SliderScale.Maximum = scale[1] * 2;
            NumScale.Minimum = 1;
            NumScale.Maximum = scale[1] * 4;
            SliderScale.Value = scale[1];
            NumScale.Value = scale[1];
            _defSca = scale[1];

            SliderX.Minimum = _mainLayer.X - varX;
            SliderX.Maximum = _mainLayer.X + varX;
            SliderX.Value = _mainLayer.X;
            NumX.Minimum = _mainLayer.X - (varX * 2);
            NumX.Maximum = _mainLayer.X + (varX * 2);
            NumX.Value = _mainLayer.X;
            _defX = _mainLayer.X;

            SliderY.Minimum = _mainLayer.Y - varY;
            SliderY.Maximum = _mainLayer.Y + varY;
            SliderY.Value = _mainLayer.Y;
            NumY.Minimum = _mainLayer.Y - (varY * 2);
            NumY.Maximum = _mainLayer.Y + (varY * 2);
            NumY.Value = _mainLayer.Y;
            _defY = _mainLayer.Y;

            _blockChanges = false;

            // On rappetisse les layers désormais
            _mainTransformation.ScaleTransformation(_previewFactor);

            new Task(LaunchTransform).Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartPosition = FormStartPosition.Manual;
            PointToScreen(WorkingArea.Container.Location);
            var x = WorkingArea.Container.Location.X + WorkingArea.Container.Width / 2 - Size.Width / 2;
            var y = WorkingArea.Container.Location.Y + (WorkingArea.Container.Height - Height) / 2;
            Location = new Point(x, y);
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            if (_currentImage != null)
            {
                TransformedPictureBox.Image = null;
                TransformedPictureBox.Invalidate();
                _currentImage.Dispose();
                _currentImage = null;

                _mainLayer.X = (int)((float)NumX.Value * _previewFactor);
                _mainLayer.Y = (int)((float)NumY.Value * _previewFactor);
                _mainLayer.Rotation = SliderRotation.Value;
                var newScale = new int[2];
                newScale[1] = (int)((float)NumScale.Value * _previewFactor);
                _mainLayer.Scale = newScale;

                new Task(LaunchTransform).Start();
            }
        }

        private void NumRotation_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(NumRotation.Value, SliderRotation);
            }
        }

        private void NumScale_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(NumScale.Value, SliderScale);
            }
        }

        private void NumX_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(NumX.Value, SliderX);
            }
        }

        private void NumY_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(NumY.Value, SliderY);
            }
        }

        private void SliderRotation_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(SliderRotation.Value, NumRotation);
            }
        }

        private void SliderScale_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(SliderScale.Value, NumScale);
            }
        }

        private void SliderX_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(SliderX.Value, NumX);
            }
        }

        private void SliderY_change(object sender, EventArgs e)
        {
            if (!_blockChanges)
            {
                ValueRange(SliderY.Value, NumY);
            }
        }

        private void ValueRange(int value, NumericUpDown control)
        {
            _blockChanges = true;
            if (value < control.Minimum)
            {
                control.Value = control.Minimum;
            }
            else if (value > control.Maximum)
            {
                control.Value = control.Maximum;
            }
            else
            {
                control.Value = value;
            }
            _blockChanges = false;
        }

        private void ValueRange(decimal value, TrackBar control)
        {
            _blockChanges = true;
            if (value < control.Minimum)
            {
                control.Value = control.Minimum;
            }
            else if (value > control.Maximum)
            {
                control.Value = control.Maximum;
            }
            else
            {
                control.Value = (int)value;
            }
            _blockChanges = false;
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            _blockChanges = true;
            NumX.Value = _defX;
            SliderX.Value = _defX;
            NumY.Value = _defY;
            SliderY.Value = _defY;
            SliderScale.Value = _defSca;
            NumScale.Value = _defSca;
            SliderRotation.Value = _defRot;
            NumRotation.Value = _defRot;
            _blockChanges = false;
        }

        private void PreviewDialogTransformation_FormClosing(object sender, FormClosingEventArgs e)
        {
            TransformedPictureBox.Image = null;
            TransformedPictureBox.Invalidate();
            
            Dispose();
        }

        private void SaveCustomTransformation()
        {
            if (!_finalTransformationDone)
            {
                _mainTransformation.ResetValues();

                _mainLayer.X = (int)NumX.Value;
                _mainLayer.Y = (int)NumY.Value;
                _mainLayer.Rotation = (int)NumRotation.Value;
                var newScale = new int[2];
                newScale[1] = (int)NumScale.Value;
                _mainLayer.Scale = newScale;

                var result = _mainTransformation.TransformImage();
                
                var toSave = DirectoryUtil.GetLocalIbrandTerminalDataDir(_resp.ProjectId, _resp.QuestionnaireId, _resp.TerminalId,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + 
                    GenerateUniqueId.GetId() + "_t_" + "." + _mainTransformation.OutputFormat;

                if (_mainTransformation.OutputFormat.ToLower().Equals("png"))
                {
                    result.Save(ImageFormat.Png, 9, toSave);
                }
                else
                {
                    result.Save(ImageFormat.Jpeg, 90, toSave);
                }

                result.Dispose();

                _receiver.SetTransformedPicture(toSave);
                _finalTransformationDone = true;

                Close();
            }
        }

        public new void Dispose()
        {
            // Ça dispose aussi le layer de photo.
            if (_mainTransformation != null)
            {
                _mainTransformation.ResetValues();
                _mainTransformation.Dispose();
                _mainTransformation = null;
            }
            if (_currentImage != null)
            {
                _currentImage.Dispose();
                _currentImage = null;
            }
        }

        /// <summary>
        /// Lance la transformation du preview et met l'image dans la boîte d'image.
        /// <br/>Idéalement, à appeller d'un thread séparé.
        /// </summary>
        private void LaunchTransform()
        {
            ButtonKeepIt.Invoke(_buttonDeleg, ButtonKeepIt, false);
            ButtonCancel.Invoke(_buttonDeleg, ButtonCancel, false);
            var result = _mainTransformation.TransformImage();

            TransformedPictureBox.Invoke(_changePictureDelegate, result);
            ButtonKeepIt.Invoke(_buttonDeleg, ButtonKeepIt, true);
            ButtonCancel.Invoke(_buttonDeleg, ButtonCancel, true);
            if (_currentImage != null)
            {
                _currentImage.Dispose();
                _currentImage = null;
            }
            _currentImage = result;
        }

        private void ChangePictureMethod(Bitmap newPicture)
        {
            TransformedPictureBox.Image = newPicture.Scale(0, TransformedPictureBox.Size.Height);
        }

        private void ChangeButtonStateMethod(Button b, bool state)
        {
            b.Enabled = state;
        }

        ~PreviewDialogTransformation()
        {
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonKeepIt_Click(object sender, EventArgs e)
        {
            SaveCustomTransformation();
        }
    }
}
