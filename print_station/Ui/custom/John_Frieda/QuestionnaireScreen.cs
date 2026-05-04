using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.John_Frieda
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _waiverArea;
        private readonly Bitmap _checkmark;
        private TextBox _emailTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private int _optInChecked1;
        private TextBox _postalCodeTextBox;

        public QuestionnaireScreen(string backgroundImgEnSrc, string backgroundImageSrc,
            PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInArea1 = new Rectangle(new Point(108, 402), new Size(180, 60));

            _waiverArea = new Rectangle(new Point(790, 475), new Size(243, 40));

            _nextArea = new Rectangle(new Point(568, 253), new Size(200, 70));
            _closeArea = new Rectangle(new Point(1180, 5), new Size(113, 94));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _lastNameTextBox.Text = "";
            _emailTextBox.Text = "";
            _postalCodeTextBox.Text = "";

            _optInChecked1 = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked1 == 1)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(277, 205));
        //        }
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1239)/2, 372, 1239, 405);
        }

        private bool ValidateEmailAddress()
        {
            /*var questionnaire = MainForm.GetInstance().Questionnaire;

            var question = questionnaire.GetQuestion(3);
            var choiceText = (ChoiceText) question.GetChoice(1);
            var answer = new Answer(-1, question.No, choiceText.No);
            answer.Response = _emailTextBox.Text;
            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern() || _emailTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                if (_activeLg.Equals("en"))
                    mBox.ShowMessage("Email address not valid.");
                else
                    mBox.ShowMessage("Le courriel n'est pas valide.");
                answer.Response = "";

                choiceText.SetAnswer(answer);

                return false;
            }*/

            return true;
        }

        private bool ValidatePostalCode()
        {
            /*var questionnaire = MainForm.GetInstance().Questionnaire;

            var questionZip = questionnaire.GetQuestion(4);
            var choiceZip = (ChoiceText) questionZip.GetChoice(1);

            var answerZip = new Answer(-1, questionZip.No, choiceZip.No);
            answerZip.Response = _postalCodeTextBox.Text;
            choiceZip.SetAnswer(answerZip);

            if (!questionZip.ValidateAnswersPattern() || _postalCodeTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                if (_activeLg.Equals("en"))
                    mBox.ShowMessage("Postal code is not valid.");
                else
                    mBox.ShowMessage("Le code postal n'est pas valide.");

                answerZip.Response = "";

                choiceZip.SetAnswer(answerZip);

                return false;
            }*/

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var mBox = new MessageDialog();

            if (_firstNameTextBox.Text.Length > 0 && _lastNameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0 &&
                _postalCodeTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

                if (!ValidatePostalCode())
                {
                    return false;
                }

                if (_optInChecked1 == 0)
                {
                    if (_activeLg.Equals("en"))
                        mBox.ShowMessage("You must agree to the photo release and waiver.");
                    else
                        mBox.ShowMessage("Vous devez accepter les conditions.");
                    return false;
                }

                var questionName = questionnaire.GetQuestion(1);
                var choiceName = (ChoiceText) questionName.GetChoice(1);

                /*var answerName = new Answer(-1, questionName.No, choiceName.No);
                answerName.Response = _firstNameTextBox.Text;
                choiceName.SetAnswer(answerName);

                var questionLastName = questionnaire.GetQuestion(2);
                var choiceLastName = (ChoiceText) questionLastName.GetChoice(1);

                var answerLastName = new Answer(-1, questionLastName.No, choiceLastName.No);
                answerLastName.Response = _lastNameTextBox.Text;
                choiceLastName.SetAnswer(answerLastName);

                if (_optInChecked1 == 1)
                {
                    var question = questionnaire.GetQuestion(5);
                    var choiceCheck = (ChoiceCheck) question.GetChoice(1);
                    var answer = new Answer(-1, question.No, 1);
                    choiceCheck.SetAnswer(answer);
                }

                var questionLanguage = questionnaire.GetQuestion(8);

                var choice = (ChoiceCombo) questionLanguage.GetChoice(1);
                var subChoiceNo = 1;
                if (_activeLg.Equals("en"))
                    subChoiceNo = 2;
                var answerLanguage = new Answer(QuestionTypeEnum.Combo, 8, choice.No, subChoiceNo);
                choice.SetAnswer(answerLanguage);*/


                return true;
            }
            if (_activeLg.Equals("en"))
                mBox.ShowMessage("Please answer all questions.");
            else
                mBox.ShowMessage("Vous devez répondre à toutes les questions.");
            return false;
        }

        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _firstNameTextBox = new TextBox();
            _firstNameTextBox.Location = new Point(250, 71);
            _firstNameTextBox.Size = new Size(420, 50);
            _firstNameTextBox.TabIndex = 0;
            _firstNameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _lastNameTextBox = new TextBox();
            _lastNameTextBox.Location = new Point(819, 71);
            _lastNameTextBox.Size = new Size(360, 50);
            _lastNameTextBox.TabIndex = 1;
            _lastNameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();

            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point(212, 133);
            _emailTextBox.Size = new Size(460, 50);
            _emailTextBox.TabIndex = 3;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _postalCodeTextBox = new TextBox();
            _postalCodeTextBox.Location = new Point(819, 133);
            _postalCodeTextBox.Size = new Size(155, 50);
            _postalCodeTextBox.TabIndex = 4;
            _postalCodeTextBox.MaxLength = 7;
            _postalCodeTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _postalCodeTextBox.TextChanged += OnTextChanged;

            Controls.Add(_postalCodeTextBox);
            _postalCodeTextBox.BringToFront();
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

        //        Bitmap release = null;
        //        if (_activeLg.Equals("en"))
        //            release = GetNoneLanguageBitmap("agreement_en.png");
        //        else
        //            release = GetNoneLanguageBitmap("agreement_fr.png");
        //        if (release != null)
        //        {
        //            new WaiverDialog(release).ShowDialog();
        //            release.Dispose();
        //        }

        //        ResumeTimer();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea1))
        //    {
        //        if (_optInChecked1 == 0)
        //        {
        //            _optInChecked1 = 1;
        //        }
        //        else
        //        {
        //            _optInChecked1 = 0;
        //        }

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