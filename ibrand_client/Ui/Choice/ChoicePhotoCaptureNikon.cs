using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrand.Common;

namespace NomadLogic.iBrand.Ui.Choice
{
    
    //Version pour camera Nikon. FSF
    public class ChoicePhotoCaptureNikon : Choice
    {
        protected CaptureTextBoxNikon _photo_field;

        public ChoicePhotoCaptureNikon()
        {
            _photo_field = new CaptureTextBoxNikon(false);
        }

        public override bool Enabled
        {
            set
            {
                _photo_field.Enabled = value;
            }
        }

        public override void SetFocusable(bool focusable)
        {
            _photo_field.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _photo_field.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _photo_field.Name = "C_" + No;

            int photoFieldWidth = availableWidth;

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
            if (_photo_field.Text == null)
                return false;

            return _photo_field.Text.Length > 0 && _photo_field.PictureTaken;
        }

        public override String GetAnswerText()
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
            _photo_field.Dispose();
        }

        public void StartLookForPictureThread()
        {
            _photo_field.StartThread();
        }

        public override void SetAnswer(Answer answer)
        {
            throw new NotImplementedException();
        }
    }
}
