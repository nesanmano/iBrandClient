using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using NomadLogic.Multimedia.Gpu.OpenCV;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.Multimedia.Gpu.Ui.ChromaKeyDialogs
{
    internal class ChromaKeyCieLuvDataControl : IDisposable
    {
        private readonly Panel _pnColor;
        private readonly Button _btnRemove;
        private readonly Label _lblColor;
        private ChromaKeyInfoOpenCvCieLuvData _ckData;
        private readonly TextBox _txtboxTolerance;
        private readonly Button _btnToleranceUp;
        private readonly Button _btnToleranceDown;
        private readonly TextBox _txtboxSmoothing;
        private readonly Button _btnSmoothingUp;
        private readonly Button _btnSmoothingDown;

        public event EventHandler ChromaKeyChange;
        public event EventHandler CallToRemove;

        internal ChromaKeyCieLuvDataControl(ChromaKeyInfoOpenCvCieLuvData ckData)
        {
            _ckData = ckData;

            _pnColor = new Panel();
            _lblColor = new Label();
            _btnRemove = new Button();

            _txtboxTolerance = new TextBox();
            _btnToleranceUp = new Button();
            _btnToleranceDown = new Button();

            _txtboxSmoothing = new TextBox();
            _btnSmoothingUp = new Button();
            _btnSmoothingDown = new Button();

            _pnColor.SuspendLayout();

            _pnColor.Controls.Add(_btnRemove);
            _pnColor.Controls.Add(_lblColor);
            _pnColor.Controls.Add(_txtboxTolerance);
            _pnColor.Controls.Add(_txtboxSmoothing);
            _pnColor.Controls.Add(_btnSmoothingDown);
            _pnColor.Controls.Add(_btnSmoothingUp);
            _pnColor.Controls.Add(_btnToleranceDown);
            _pnColor.Controls.Add(_btnToleranceUp);

            // Met les contrôles dans un tableau, dans l'ordre q'ils sont supposés apparaître.
            var ctrlArray = new Control[] { _lblColor, _txtboxTolerance, _btnToleranceUp, _btnToleranceDown, _txtboxSmoothing, _btnSmoothingUp, _btnSmoothingDown, _btnRemove };

            _pnColor.Location = new Point(0, 0);
            _pnColor.Disposed += OnDispose;

            var pos = 0;
            // 
            // lblColor
            // 
            _lblColor.BackColor = Color.FromArgb(ckData.R, ckData.G, ckData.B);
            _lblColor.BorderStyle = BorderStyle.FixedSingle;
            _lblColor.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _lblColor.Size = new Size(30, 30);
            
            // Tolerance
            _txtboxTolerance.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _txtboxTolerance.Size = new Size(60, 30);
            _txtboxTolerance.Text = ckData.Tolerance.ToString(CultureInfo.InvariantCulture);
            _txtboxTolerance.BorderStyle = BorderStyle.FixedSingle;
            _txtboxTolerance.TextChanged += OnChromaKeyChange;

            _btnToleranceUp.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _btnToleranceUp.Size = new Size(30, 30);
            _btnToleranceUp.Text = @"+";
            _btnToleranceUp.UseVisualStyleBackColor = true;
            _btnToleranceUp.Click += OnValueChangeUp;

            _btnToleranceDown.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _btnToleranceDown.Size = new Size(30, 30);
            _btnToleranceDown.Text = @"-";
            _btnToleranceDown.UseVisualStyleBackColor = true;
            _btnToleranceDown.Click += OnValueChangeDown;

            // Smoothing
            _txtboxSmoothing.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _txtboxSmoothing.Size = new Size(60, 30);
            _txtboxSmoothing.Text = ckData.Smoothing.ToString(CultureInfo.InvariantCulture);
            _txtboxSmoothing.BorderStyle = BorderStyle.FixedSingle;
            _txtboxSmoothing.TextChanged += OnChromaKeyChange;

            _btnSmoothingUp.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _btnSmoothingUp.Size = new Size(30, 30);
            _btnSmoothingUp.Text = @"+";
            _btnSmoothingUp.UseVisualStyleBackColor = true;
            _btnSmoothingUp.Click += OnValueChangeUp;

            _btnSmoothingDown.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _btnSmoothingDown.Size = new Size(30, 30);
            _btnSmoothingDown.Text = @"-";
            _btnSmoothingDown.UseVisualStyleBackColor = true;
            _btnSmoothingDown.Click += OnValueChangeDown;

            // btnRemove
            // 
            _btnRemove.Location = new Point(CalculatePosition(pos++, ctrlArray), 2);
            _btnRemove.Size = new Size(60, 30);
            _btnRemove.Text = @"Remove";
            _btnRemove.UseVisualStyleBackColor = true;
            _btnRemove.Click += OnClick;

            // Taille finale du panel.
            _pnColor.Size = new Size(CalculatePosition(pos, ctrlArray), 34);

            _pnColor.ResumeLayout(false);
        }

        private void OnValueChangeDown(object sender, EventArgs e) {
            if (sender == _btnToleranceDown) {
                if (!double.TryParse(_txtboxTolerance.Text, out var val)) return;
                val--;
                _txtboxTolerance.Text = val.ToString(CultureInfo.InvariantCulture);
            } else if (sender == _btnSmoothingDown) {
                if (!double.TryParse(_txtboxSmoothing.Text, out var val)) return;
                val--;
                _txtboxSmoothing.Text = val.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void OnValueChangeUp(object sender, EventArgs e) {
            if (sender == _btnToleranceUp) {
                if (!double.TryParse(_txtboxTolerance.Text, out var val)) return;
                val++;
                _txtboxTolerance.Text = val.ToString(CultureInfo.InvariantCulture);
            } else if (sender == _btnSmoothingUp) {
                if (!double.TryParse(_txtboxSmoothing.Text, out var val)) return;
                val++;
                _txtboxSmoothing.Text = val.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static int CalculatePosition(int index, Control[] ctrlArray) {
            var val = 0;
            val += 5 * (index + 1);
            for (var i = 0; i < index; i++) { val += ctrlArray[i].Width; }
            return val;
        }

        private void OnDispose(object sender, EventArgs e)
        {
            Dispose();
        }

        private void OnChromaKeyChange(object sender, EventArgs e)
        {
            double val;
            if (sender == _txtboxSmoothing)
            {
                if (double.TryParse(_txtboxSmoothing.Text, out val)) {
                    _txtboxSmoothing.ForeColor = Color.Black;
                    
                    if (val < 0) {
                        val = 0; 
                        _txtboxSmoothing.Click -= OnChromaKeyChange;
                        _txtboxSmoothing.Text = val.ToString(CultureInfo.InvariantCulture);
                        _txtboxSmoothing.Click += OnChromaKeyChange;
                    }

                    if (_ckData.Smoothing == val) return;

                    if (val > _ckData.Tolerance) {
                        _txtboxTolerance.Click -= OnChromaKeyChange;
                        _ckData.Tolerance = val;
                        _txtboxTolerance.Text = _ckData.Tolerance.ToString(CultureInfo.InvariantCulture);
                        _txtboxTolerance.Click += OnChromaKeyChange;
                    }
                    _ckData.Smoothing = val;

                    ChromaKeyChange?.Invoke(this, EventArgs.Empty);
                }
                else {
                    _txtboxSmoothing.ForeColor = Color.Brown;
                }
            }
            else if (sender == _txtboxTolerance)
            {
                if (double.TryParse(_txtboxTolerance.Text, out val)) {
                    _txtboxTolerance.ForeColor = Color.Black;
                    
                    if (val < 0) {
                        val = 0; 
                        _txtboxTolerance.Click -= OnChromaKeyChange;
                        _txtboxTolerance.Text = val.ToString(CultureInfo.InvariantCulture);
                        _txtboxTolerance.Click += OnChromaKeyChange;
                    }

                    if (_ckData.Tolerance == val) return;

                    _ckData.Tolerance = val;

                    if (_ckData.Tolerance < _ckData.Smoothing) {
                        _txtboxSmoothing.Click -= OnChromaKeyChange;
                        _ckData.Smoothing = _ckData.Tolerance;
                        _txtboxSmoothing.Text = _ckData.Smoothing.ToString(CultureInfo.InvariantCulture);
                        _txtboxSmoothing.Click += OnChromaKeyChange;
                    }

                    ChromaKeyChange?.Invoke(this, EventArgs.Empty);
                }
                else {
                    _txtboxTolerance.ForeColor = Color.Brown;
                }
            }
        }

        public Panel GetPanel()
        {
            return _pnColor;
        }

        private void OnClick(object sender, EventArgs e)
        {
            CallToRemove?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _ckData = null;
            //_pnColor.Dispose();
            _btnRemove.Click -= OnClick;
            _btnRemove.Dispose();
            _txtboxTolerance.TextChanged -= OnChromaKeyChange;
            _txtboxTolerance.Dispose();
            _txtboxSmoothing.TextChanged -= OnChromaKeyChange;
            _txtboxSmoothing.Dispose();
            _lblColor.Dispose();
        }

        ~ChromaKeyCieLuvDataControl()
        {
            Dispose();
        }
    }
}

