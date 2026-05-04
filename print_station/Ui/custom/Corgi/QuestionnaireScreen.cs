using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Corgi
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _catArea;
        private readonly Rectangle _closeArea;
        private readonly Rectangle _dogArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea;
        private readonly Rectangle _waiverArea;
        private readonly Bitmap _checkmark;
        private int _catChecked;
        private int _dogChecked;
        private TextBox _emailTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private TextBox _numberOfCatTextBox;
        private TextBox _numberOfDogTextBox;
        private int _optInChecked;

        public QuestionnaireScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _dogArea = new Rectangle(new Point(411, 171), new Size(53, 53));
            _catArea = new Rectangle(new Point(411, 227), new Size(53, 53));
            _optInArea = new Rectangle(new Point(123, 288), new Size(792, 48));
            _waiverArea = new Rectangle(new Point(943, 288), new Size(251, 48));

            _nextArea = new Rectangle(new Point(574, 343), new Size(190, 78));
            _closeArea = new Rectangle(new Point(1180, 10), new Size(60, 60));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _lastNameTextBox.Text = "";
            _emailTextBox.Text = "";

            _dogChecked = 0;
            _catChecked = 0;
            _optInChecked = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_dogChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(425, 181));
        //    }

        //    if (_catChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(425, 234));
        //    }

        //    if (_optInChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(134, 303));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1239)/2, 400, 1239, 400);
        }

        private bool ValidateEmailAddress()
        {
            var questionnaire = GetCurrentQuestionnaire();

            var question = questionnaire.GetQuestion(3);
            var choiceText = (ChoiceText) question.GetChoice(1);
            /*var answer = new Answer(-1, question.No, choiceText.No) {Response = _emailTextBox.Text};
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

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_firstNameTextBox.Text.Length > 0 && _lastNameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

                if (_optInChecked == 0)
                {
                    mBox.ShowMessage(_activeLg.Equals("en")
                        ? "You must agree to the photo release and waiver."
                        : "Vous devez accepter les conditions.");
                    return false;
                }

                //SaveAnswerToQuestion(1, 1, _firstNameTextBox.Text);

                //SaveAnswerToQuestion(2, 1, _lastNameTextBox.Text);

                if (_optInChecked == 1)
                {
                    //SaveAnswerToQuestion(8, 1, new Answer(-1, 8, 1));
                }

                if (_dogChecked == 1)
                {
                    //SaveAnswerToQuestion(4, 1, new Answer(-1, 4, 1));

                    if (_numberOfDogTextBox.Text.Length > 0)
                    {
                        //SaveAnswerToQuestion(5, 1, _numberOfDogTextBox.Text);
                    }
                    else
                    {
                        mBox.ShowMessage("You must enter how many dog you have.");
                        return false;
                    }
                }

                if (_catChecked == 1)
                {
                    //SaveAnswerToQuestion(6, 1, new Answer(-1, 6, 1));

                    if (_numberOfCatTextBox.Text.Length > 0)
                    {
                        //SaveAnswerToQuestion(7, 1, _numberOfCatTextBox.Text);
                    }
                    else
                    {
                        mBox.ShowMessage("You must enter how many cat you have.");

                        return false;
                    }
                }

                return true;
            }

            if (_activeLg.Equals("en"))
                mBox.ShowMessage("Please answer all questions.");
            else
                mBox.ShowMessage("Vous devez répondre à toutes les questions.");

            return false;
        }

        private void AddTextBoxs()
        {
            _firstNameTextBox = new TextBox();
            _firstNameTextBox.Location = new Point(297, 65);
            _firstNameTextBox.Size = new Size(370, 50);
            _firstNameTextBox.TabIndex = 0;
            _firstNameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _lastNameTextBox = new TextBox();
            _lastNameTextBox.Location = new Point(855, 65);
            _lastNameTextBox.Size = new Size(305, 50);
            _lastNameTextBox.TabIndex = 1;
            _lastNameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();

            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point(240, 119);
            _emailTextBox.Size = new Size(425, 50);
            _emailTextBox.TabIndex = 3;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _numberOfDogTextBox = new TextBox();
            _numberOfDogTextBox.Location = new Point(869, 171);
            _numberOfDogTextBox.Size = new Size(83, 50);
            _numberOfDogTextBox.TabIndex = 4;
            _numberOfDogTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _numberOfDogTextBox.TextChanged += OnTextChanged;
            _numberOfDogTextBox.KeyPress += NumericKeyPress;

            Controls.Add(_numberOfDogTextBox);
            _numberOfDogTextBox.BringToFront();

            _numberOfCatTextBox = new TextBox();
            _numberOfCatTextBox.Location = new Point(869, 229);
            _numberOfCatTextBox.Size = new Size(83, 50);
            _numberOfCatTextBox.TabIndex = 5;
            _numberOfCatTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _numberOfCatTextBox.TextChanged += OnTextChanged;
            _numberOfCatTextBox.KeyPress += NumericKeyPress;

            Controls.Add(_numberOfCatTextBox);
            _numberOfCatTextBox.BringToFront();
        }

        private void NumericKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
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
        //    else if (InArea(e.X, e.Y, _optInArea))
        //    {
        //        _optInChecked = _optInChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _dogArea))
        //    {
        //        _dogChecked = _dogChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _catArea))
        //    {
        //        _catChecked = _catChecked == 0 ? 1 : 0;

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