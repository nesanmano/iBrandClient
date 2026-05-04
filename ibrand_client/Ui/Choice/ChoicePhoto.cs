using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoicePhoto : Choice
    {
        private readonly PhotoTextBox _photoField;

        public ChoicePhoto()
        {
            _photoField = new PhotoTextBox();
        }

        public override bool Enabled
        {
            set
            {
                _photoField.Enabled = value;
            }

            internal get => _photoField.Enabled;
        }

        public override void SetFocusable(bool focusable)
        {
            _photoField.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _photoField.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _photoField.Name = "C_" + No;

            _photoField.SetSize(new Size(availableWidth - (2 * Constants.CHOICE_MARGIN), _photoField.Size.Height));

            questionPanel.Controls.Add(_photoField);

            return _photoField.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _photoField.Size.Height + Constants.CHOICE_PAD;
        }

        /*public void UIStyle(UIStyle ui_style)
        {
            _email_field.setBorderColor(ui_style.getBorderColor());
        }*/

        public override bool IsAnswered()
        {
            return _photoField.Text.Length > 0;
        }

        public override string GetAnswerText()
        {
            return _photoField.Text;
        }


        public override void ResetAnswer()
        {
            _photoField.Reset();
            CheckAndUpdateDefaultAnswer();
        }

        public override void SetFocus()
        {
            _photoField.Focus();
        }

        public override bool HasFocus()
        {
            return _photoField.Focused;
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
