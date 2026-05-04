using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.PrintStation.Ui.custom.Harry_Potter
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private TextBox _emailTextBox;
        private TextBox _nameTextBox;

        public QuestionnaireScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(new Point(549, 283), new Size(147, 57));
            _closeArea = new Rectangle(new Point(1190, 17), new Size(70, 71));
        }

        public override void Recycle()
        {
            base.Recycle();

            _nameTextBox.Text = "";
            _emailTextBox.Text = "";
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(0, 342, 1239, 405);
        }

        private bool ValidateEmailAddress()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var question = questionnaire.GetQuestion(2);
            var choiceText = (ChoiceEmail) question.GetChoice(1);
            var answer = new Answer{QuestionNo = question.No, ChoiceNo = choiceText.No};
            answer.Response = _emailTextBox.Text;
            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern() || _emailTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Email address not valid.");

                answer.Response = "";

                choiceText.SetAnswer(answer);

                return false;
            }

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var mBox = new MessageDialog();

            if (_nameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

                var questionName = questionnaire.GetQuestion(1);
                var choiceName = (ChoiceText) questionName.GetChoice(1);

                var answerName = new Answer {QuestionNo = questionName.No, ChoiceNo = choiceName.No, Response = _nameTextBox.Text};
                choiceName.SetAnswer(answerName);

                return true;
            }

            mBox.ShowMessage("Please answer all questions.");
            return false;
        }

        public void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _nameTextBox = new TextBox();
            _nameTextBox.Location = new Point(202, 206);
            _nameTextBox.Size = new Size(395, 50);
            _nameTextBox.TabIndex = 0;
            _nameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _nameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_nameTextBox);
            _nameTextBox.BringToFront();

            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point(705, 206);
            _emailTextBox.Size = new Size(530, 50);
            _emailTextBox.TabIndex = 2;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (InArea(e.X, e.Y, _closeArea))
        //    {
        //        Close();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextBox.Focus();
        }
    }
}