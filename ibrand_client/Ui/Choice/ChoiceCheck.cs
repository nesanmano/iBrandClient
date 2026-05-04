using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceCheck : Choice
    {
        private CheckTextBox _checkbox;

        private readonly int _originalHeight;

        public ChoiceCheck()
        {
            Init();

            _originalHeight = _checkbox.Height;

        }

        private void Init()
        {
            _checkbox = new CheckTextBox();
            _checkbox.CheckBox.CheckStateChanged += OnResponseChanged;
        }

        public override void SetFocusable(bool focusable)
        {
            _checkbox.SetFocusable(focusable);
        }

        public override bool Enabled
        {
            set => _checkbox.Enabled = value;
            internal get => _checkbox.Enabled;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            SetText();

            _checkbox.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _checkbox.Name = "C_" + No;

            var checkboxWidth = availableWidth - _checkbox.GetCheckSize().Width - (2 * Constants.CHOICE_MARGIN);

            _checkbox.SetSize(new Size(
                checkboxWidth,
                GetTextHeight(g, _checkbox.Font, checkboxWidth, GetText())));

            questionPanel.Controls.Add(_checkbox);

            return _checkbox.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            var checkboxWidth = availableWidth - _checkbox.GetCheckSize().Width  - (2 * Constants.CHOICE_MARGIN);

            var nbrOfLines = StringUtil.GetNumberOfLines(g, _checkbox.Font, checkboxWidth, GetText());

            //return Math.Max((_checkbox.Font.Height * nbrOfLines), this._checkbox.Height) + Constants.CHOICE_PAD;
            return (_originalHeight * nbrOfLines) + Constants.CHOICE_PAD;
        }

        public override void SetText()
        {
            var text = GetText() ?? "";

            _checkbox.Text = text;
            //_checkbox.Font = new Font("Arial", 13F, FontStyle.Regular);
        }

        public override void SetAnswer(Answer answer)
        {
            if (_checkbox.InvokeRequired)
            {
                _checkbox.Invoke(new SetAnswerDelegate(SetAnswer), answer);
                return;
            }
            
            if (answer == null)
            {
                _checkbox.Checked = false;
            } 
            else 
            {
                if (_question.No == answer.QuestionNo)
                {
                    _checkbox.Checked = _no == answer.ChoiceNo;
                }
                else
                {
                    _checkbox.Checked = false;
                }
            }
        }

        public bool IsChecked()
        {
            return _checkbox.Checked;
        }

        public override void ResetAnswer()
        {
            _checkbox.CheckBox.CheckStateChanged -= OnResponseChanged;
            _checkbox.Checked = false;
            _checkbox.CheckBox.CheckStateChanged += OnResponseChanged;
        }

        public override void SetFocus()
        {
            _checkbox.Focus();
        }

        public override bool HasFocus()
        {
            return _checkbox.Focused;
        }

        /*private float ConvertUnits(Graphics gr, float value, GraphicsUnit from_unit, GraphicsUnit to_unit)
        {
            if (from_unit == to_unit) return value;

            // Convert to pixels. 
            switch (from_unit)
            {
                case GraphicsUnit.Document:
                    value *= gr.DpiX / 300;
                    break;
                case GraphicsUnit.Inch:
                    value *= gr.DpiX;
                    break;
                case GraphicsUnit.Millimeter:
                    value *= gr.DpiX / 25.4F;
                    break;
                case GraphicsUnit.Pixel:
                    // Do nothing.
                    break;
                case GraphicsUnit.Point:
                    value *= gr.DpiX / 72;
                    break;
                default:
                    throw new System.Exception("Unknown input unit " + from_unit.ToString() + " in FontInfo.ConvertUnits");
            }

            // Convert from pixels to the new units. 
            switch (to_unit)
            {
                case GraphicsUnit.Document:
                    value /= gr.DpiX / 300;
                    break;
                case GraphicsUnit.Inch:
                    value /= gr.DpiX;
                    break;
                case GraphicsUnit.Millimeter:
                    value /= gr.DpiX / 25.4F;
                    break;
                case GraphicsUnit.Pixel:
                    // Do nothing.
                    break;
                case GraphicsUnit.Point:
                    value /= gr.DpiX / 72;
                    break;
                default:
                    throw new System.Exception("Unknown output unit " + to_unit.ToString() + " in FontInfo.ConvertUnits");
            }

            return value;
        }*/
    }
}
