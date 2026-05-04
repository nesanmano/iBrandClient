using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceDate : Choice
    {
        private readonly DateComboBoxes _dateCombos;

        public ChoiceDate()
        {
            _dateCombos = new DateComboBoxes();
        }

        public override bool Enabled
        {
            set
            {
                _dateCombos.Enabled = value;
            }

            internal get => _dateCombos.Enabled;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _dateCombos.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _dateCombos.Name = "C_" + No;

            _dateCombos.SetSize(new Size(availableWidth - (2 * Constants.CHOICE_MARGIN), _dateCombos.Size.Height));

            questionPanel.Controls.Add(_dateCombos);

            return _dateCombos.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _dateCombos.Size.Height + Constants.CHOICE_PAD;
        }

        /*public void UIStyle(UIStyle ui_style)
        {
            _email_field.setBorderColor(ui_style.getBorderColor());
        }*/

        public override bool IsAnswered()
        {
            return _dateCombos.Text.Length > 0;
        }

        public override string GetAnswerText()
        {
            return _dateCombos.Text;
        }


        public override void ResetAnswer()
        {
            _dateCombos.Reset();
            CheckAndUpdateDefaultAnswer();
        }

        public override void SetFocus()
        {
            _dateCombos.Focus();
        }

        public override bool HasFocus()
        {
            return _dateCombos.Focused;
        }

        public override void SetText()
        {
            //do nothing
        }

        public override void SetAnswer(Answer answer)
        {
            throw new NotImplementedException();
        }
    }
}
