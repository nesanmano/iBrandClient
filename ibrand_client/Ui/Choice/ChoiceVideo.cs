using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceVideo : Choice
    {
        private readonly VideoCaptureCanonTextBox2 _videoField;

        public ChoiceVideo()
        {
            _videoField = new VideoCaptureCanonTextBox2();
        }

        public override bool Enabled
        {
            set => _videoField.Enabled = value;
            internal get => _videoField.Enabled;
        }

        public override void SetFocusable(bool focusable)
        {
            _videoField.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _videoField.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _videoField.Name = "C_" + No;

            var videoFieldWidth = availableWidth;

            if (videoFieldWidth > Constants.MAX_WIDTH)
                videoFieldWidth = Constants.MAX_WIDTH;

            _videoField.SetSize(new Size(videoFieldWidth - (2 * Constants.CHOICE_MARGIN), _videoField.Size.Height));

            questionPanel.Controls.Add(_videoField);

            return _videoField.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _videoField.Size.Height + Constants.CHOICE_PAD;
        }

        public override bool IsAnswered()
        {
            return _videoField.Text.Length > 0;
        }

        public override string GetAnswerText()
        {
            return _videoField.Text;
        }

        public override void ResetAnswer()
        {
            _videoField.Reset();
            CheckAndUpdateDefaultAnswer();
        }

        public override void SetFocus()
        {
            _videoField.Focus();
        }

        public override bool HasFocus()
        {
            return _videoField.Focused;
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
