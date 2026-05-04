using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{

    public class ChoicePhotoCaptureCanon : Choice
    {
        protected CaptureTextBoxCanon _photo_field;
        
        public ChoicePhotoCaptureCanon()
        {
            SetCaptureTextBox();
        }

        protected virtual void SetCaptureTextBox()
        {
            _photo_field = new CaptureTextBoxCanon();
            _photo_field.Initialize();
        }

        public override bool Enabled
        {
            set
            {
                _photo_field.Enabled = value;
            }
            internal get => _photo_field.Enabled;
        }

        public override void SetFocusable(bool focusable)
        {
            _photo_field.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _photo_field.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _photo_field.Name = "C_" + No;

            var photoFieldWidth = availableWidth;

            if (photoFieldWidth > Constants.MAX_WIDTH)
                photoFieldWidth = Constants.MAX_WIDTH;

            _photo_field.SetSize(new Size(photoFieldWidth - (2 * Constants.CHOICE_MARGIN), _photo_field.Size.Height));

            questionPanel.Controls.Add(_photo_field);

            return _photo_field.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _photo_field.Size.Height + Constants.CHOICE_PAD;
        }

        public override bool IsAnswered()
        {
            return _photo_field.Text?.Length > 0;
        }

        public override string GetAnswerText()
        {
            return _photo_field.Text;
        }

        public override void ResetAnswer()
        {
            _photo_field.Reset();
        }

        public override void SetFocus()
        {
            _photo_field.Focus();
        }

        public override bool HasFocus()
        {
            return _photo_field.Focused;
        }

        public override void SetText()
        {
            //do nothing
        }

        public override void Dispose()
        {
            _photo_field.DisposeCamrera();
        }

        public override void SetAnswer(Answer answer)
        {
            if (answer != null) _photo_field.Text = answer.Response;
        }

        public bool Print
        {
            set 
            { 
                _photo_field.CanPrint = value; 
            }
        }

        public bool LivePreview
        {
            set
            {
                _photo_field.LivePreview = value;
            }
        }
    }
}
