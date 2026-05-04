using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceTimer : Choice
    {
        public ChoiceTimerTextBox.TimeChangedDelegate _on_time_change;
        
        private readonly ChoiceTimerTextBox _textBox;


        public ChoiceTimer()
        {
            _textBox = new ChoiceTimerTextBox();
            _textBox.OnTimerChange += OnTimerChange;
        }

        private void OnTimerChange(string time, bool reset)
        {
            if (_on_time_change != null)
                _on_time_change(time, reset);
        }

        public override bool Enabled
        {
            set => _textBox.Enabled = value;
            internal get => _textBox.Enabled;
        }

        public void SetMaxTime(string maxTime)
        {
            _textBox.SetMaxTime(int.Parse(maxTime));
        }

        public void SetCountDown(string countDown)
        {
            _textBox.SetCountDown(int.Parse(countDown));
        }

        public override void SetFocusable(bool focusable)
        {
            _textBox.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _textBox.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _textBox.Name = "C_" + No;

            if (availableWidth > Constants.MAX_WIDTH)
                availableWidth = Constants.MAX_WIDTH;

            var videoFieldWidth = availableWidth;

            _textBox.SetSize(new Size(videoFieldWidth - (2 * Constants.CHOICE_MARGIN), _textBox.Size.Height));

            questionPanel.Controls.Add(_textBox);

            return _textBox.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _textBox.Size.Height + Constants.CHOICE_PAD;
        }

        /// <summary>
        /// Indique si une réponse est contenue dans le choix.
        /// </summary>
        /// <returns>TRUE si du texte, quel qu'il soit, est présent dans la réponse.</returns>
        public override bool IsAnswered()
        {
            if (_textBox.Running)
                return false;

            return _textBox.Text.Length > 0;
        }

        public override string GetAnswerText()
        {
            return _textBox.Text;
        }

        /// <summary>
        /// Réinitialise le choix au complet.
        /// </summary>
        public override void ResetAnswer()
        {
            _textBox.Reset();
            CheckAndUpdateDefaultAnswer();
        }

        public override void SetFocus()
        {
            _textBox.Focus();
        }

        public override bool HasFocus()
        {
            return _textBox.Focused;
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
