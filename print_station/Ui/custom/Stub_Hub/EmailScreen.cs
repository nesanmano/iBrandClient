using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.PrintStation.Ui.custom.Stub_Hub
{
    public class EmailScreen : AbstractScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private TextBox _emailTextBox;

        public EmailScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            AddTextBoxs();
            AddKeyboard();

            PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(new Point(807, 483), new Size(263, 118));
            _closeArea = new Rectangle(new Point(1717, 43), new Size(119, 122));
        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextBox.Text = "";
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(160, 574, 1600, 452);
        }

        private bool ValidateEmailAddress()
        {
            /*var questionnaire = MainForm.GetInstance().Questionnaire;

            var question = questionnaire.GetQuestion(1);
            var choiceText = (ChoiceText) question.GetChoice(1);
            var answer = new Answer(-1, question.No, choiceText.No);
            answer.Response = _emailTextBox.Text;
            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern() || _emailTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR)
                    ? "L'adresse courriel est non-valide."
                    : "Email address not valid.");

                answer.Response = "";

                choiceText.SetAnswer(answer);

                return false;
            }*/

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            return ValidateEmailAddress();
        }

        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _emailTextBox = new TextBox
            {
                Location = new Point(624, 359),
                Size = new Size(650, 76),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 30)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _closeArea))
            {
                Close();
            }
            else if (InArea(e.X, e.Y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                    Next();
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextBox.Focus();
        }
    }
}