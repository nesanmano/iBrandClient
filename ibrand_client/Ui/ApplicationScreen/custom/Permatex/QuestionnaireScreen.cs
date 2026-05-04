using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Permatex
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;

        private int _optIn1Checked;

        private readonly Rectangle _optInArea1;

        private TextBox _emailTextBox;
        
        private readonly Bitmap _checkmark;

        public QuestionnaireScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            //PictureBox.MouseClick += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _optInArea1 = new Rectangle(new Point(180, 198), new Size(935, 61));

            _nextArea = new Rectangle(new Point(518, 278), new Size(241, 96));
        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextBox.Text = "";
            _optIn1Checked = 0;
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optIn1Checked == 1)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(188, 207));
        //        }
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls(5, 380, 1270, 400);
        }

        private bool ValidateEmailAddress()
        {
            /*var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_email_no);
            var choiceText = (ChoiceText)question.GetChoice(1);
            var answer = new Answer(-1, question.No, choiceText.No);
            answer.Response = _emailTextBox.Text;
            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern() || _emailTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                if (_activeLg.Equals(Constants.FR))
                    mBox.ShowMessage("L'adresse courriel est non-valide.");
                else
                    mBox.ShowMessage("Email address not valid.");

                answer.Response = "";

                choiceText.SetAnswer(answer);

                return false;
            }*/

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            /*var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var questionEmail = questionnaire.GetQuestion(1);
            var choiceEmail = (ChoiceText)questionEmail.GetChoice(1);

            var mBox = new MessageDialog();

            if (_emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

                if (_optIn1Checked == 1)
                {
                    var question = questionnaire.GetQuestion(2);
                    var choiceCheck = (ChoiceCheck)question.GetChoice(1);
                    var answer = new Answer(-1, question.No, 1);
                    choiceCheck.SetAnswer(answer);
                }

                return true;
            }

            if (_activeLg.Equals(Constants.FR))
                mBox.ShowMessage("Vous devez répondre à toutes les questions.");
            else
                mBox.ShowMessage("Please answer all questions.");*/

            return false;
        }

        public void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point(431, 114);
            _emailTextBox.Size = new Size(537, 50);
            _emailTextBox.TabIndex = 0;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 24);
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea1))
        //    {
        //        if (_optIn1Checked == 0)
        //        {
        //            _optIn1Checked = 1;
        //        }
        //        else
        //        {
        //            _optIn1Checked = 0;
        //        }

        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextBox.Focus();
        }

    }
}
