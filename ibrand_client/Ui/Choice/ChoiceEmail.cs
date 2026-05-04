using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceEmail : Choice
    {
        private readonly EmailTextBox _emailField;

        public ChoiceEmail()
        {
            _emailField = new EmailTextBox();
        }

        public override bool Enabled
        {
            set
            {
                _emailField.Enabled = value;
            }

            internal get => _emailField.Enabled;
        }

        public override void SetFocusable(bool focusable)
        {
            _emailField.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _emailField.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _emailField.Name = "C_" + No;

            var emailFieldWidth = availableWidth;

            if (emailFieldWidth > Constants.MAX_WIDTH)
                emailFieldWidth = Constants.MAX_WIDTH;

            _emailField.SetSize(new Size(emailFieldWidth - (2 * Constants.CHOICE_MARGIN), _emailField.Size.Height));

            questionPanel.Controls.Add(_emailField);

            return _emailField.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _emailField.Size.Height + Constants.CHOICE_PAD;
        }

        public override bool IsAnswered()
        {
            return _emailField.Text.Length > 0;
        }

        public override string GetAnswerText()
        {
            return _emailField.Text;
        }

        public override void SetAnswer(Answer answer)
        {
            if (_emailField.InvokeRequired)
            {
                _emailField.Invoke(new SetAnswerDelegate(SetAnswer), answer);
                return;
            }

            _emailField.Text = answer.Response;
        }

        public override void ResetAnswer()
        {
            _emailField.Reset();
            CheckAndUpdateDefaultAnswer();
        }

        public override void SetFocus()
        {
            _emailField.Focus();
        }

        public override bool HasFocus()
        {
            return _emailField.Focused;
        }

        public override void SetText()
        {
            //do nothing
        }
    }
}
