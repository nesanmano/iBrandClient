using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Flook
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _optInArea2;
        private readonly Rectangle _waiverArea;
        private readonly Bitmap _checkmark;
        private TextBox _emailTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private int _optInChecked1;
        private int _optInChecked2;

        public QuestionnaireScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInArea1 = new Rectangle(new Point(272, 195), new Size(627, 40));
            _optInArea2 = new Rectangle(new Point(272, 235), new Size(390, 58));

            _waiverArea = new Rectangle(new Point(901, 203), new Size(174, 32));

            _nextArea = new Rectangle(new Point(574, 343), new Size(180, 58));
            _closeArea = new Rectangle(new Point(1180, 10), new Size(55, 55));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _lastNameTextBox.Text = "";
            _emailTextBox.Text = "";

            _optInChecked1 = 0;
            _optInChecked2 = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked1 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(278, 205));
        //    }

        //    if (_optInChecked2 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(278, 245));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1239)/2, 400, 1239, 400);
        }

        private bool ValidateEmailAddress()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var question = questionnaire.GetQuestion(3);
            var choiceText = (ChoiceText) question.GetChoice(1);
//            var answer = new Answer(-1, question.No, choiceText.No) {Response = _emailTextBox.Text};
//            choiceText.SetAnswer(answer);
//
//            if (question.ValidateAnswersPattern() && _emailTextBox.Text.Length != 0) return true;var mBox = new MessageDialog();
//            mBox.ShowMessage(_activeLg.Equals("en") ? "Email address not valid." : "Le courriel n'est pas valide.");
//            answer.Response = "";
//
//            choiceText.SetAnswer(answer);

            return false;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var mBox = new MessageDialog();

            if (_firstNameTextBox.Text.Length > 0 && _lastNameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

                if (_optInChecked1 == 0)
                {
                    mBox.ShowMessage(_activeLg.Equals("en")
                        ? "You must agree to the photo release and waiver."
                        : "Vous devez accepter les conditions.");
                    return false;
                }

                var questionName = questionnaire.GetQuestion(1);
                var choiceName = (ChoiceText) questionName.GetChoice(1);

//                var answerName = new Answer(-1, questionName.No, choiceName.No);
//                answerName.Response = _firstNameTextBox.Text;
//                choiceName.SetAnswer(answerName);
//
//                var questionLastName = questionnaire.GetQuestion(2);
//                var choiceLastName = (ChoiceText) questionLastName.GetChoice(1);
//
//                var answerLastName = new Answer(-1, questionLastName.No, choiceLastName.No);
//                answerLastName.Response = _lastNameTextBox.Text;
//                choiceLastName.SetAnswer(answerLastName);

                /*Question question;
                Answer answer;
                ChoiceCheck choiceCheck;
                if (_optInChecked1 == 1)
                {
                    question = questionnaire.GetQuestion(4);
                    choiceCheck = (ChoiceCheck) question.GetChoice(1);
                    answer = new Answer(-1, question.No, 1);
                    choiceCheck.SetAnswer(answer);
                }

                if (_optInChecked2 != 1) return true;question = questionnaire.GetQuestion(5);
                choiceCheck = (ChoiceCheck) question.GetChoice(1);
                answer = new Answer(-1, question.No, 1);
                choiceCheck.SetAnswer(answer);*/

                return true;
            }

            mBox.ShowMessage(_activeLg.Equals("en")
                ? "Please answer all questions."
                : "Vous devez répondre à toutes les questions.");
            return false;
        }

        private void AddTextBoxs()
        {
            _firstNameTextBox = new TextBox
            {
                Location = new Point(250, 71),
                Size = new Size(420, 50),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _lastNameTextBox = new TextBox
            {
                Location = new Point(819, 71),
                Size = new Size(360, 50),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(212, 133),
                Size = new Size(460, 50),
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
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
        //    else if (InArea(e.X, e.Y, _waiverArea))
        //    {
        //        SuspendTimer();

        //        var release = GetNoneLanguageBitmap("agreement.jpg");

        //        if (release != null)
        //        {
        //            new WaiverImageDialog(release).ShowDialog();
        //            release.Dispose();
        //        }

        //        ResumeTimer();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea1))
        //    {
        //        _optInChecked1 = _optInChecked1 == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea2))
        //    {
        //        _optInChecked2 = _optInChecked2 == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextBox.Focus();
        }
    }
}