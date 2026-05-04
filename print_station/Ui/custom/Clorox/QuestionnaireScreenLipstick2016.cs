using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Clorox
{
    public class QuestionnaireScreenLipstick2016 : AbstractPrintStationScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;

        private readonly Bitmap _checkmark;
        private TextBox _emailTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;

        private bool _optInChecked1;

        public QuestionnaireScreenLipstick2016(string backgroundImgEnSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInArea1 = new Rectangle(new Point(48, 236), new Size(1133, 65));

            _nextArea = new Rectangle(new Point(549, 344), new Size(180, 60));
            _closeArea = new Rectangle(new Point(1185, 36), new Size(52, 52));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _emailTextBox.Text = "";
            _lastNameTextBox.Text = "";


            _optInChecked1 = false;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(60, 250));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1200) / 2, 420, 1200, 333);
        }

        private bool ValidateEmailAddress()
        {
            if (ValidateEmailAddress(4, _emailTextBox.Text)) return true;

            var mBox = new MessageDialog();
            mBox.ShowMessage(_activeLg.Equals("en") ? "Email address not valid." : "Le courriel n'est pas valide.");

            return false;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_firstNameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

//                SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
//                SaveAnswerToQuestion(2, 1, _firstNameTextBox.Text);
//                SaveAnswerToQuestion(3, 1, _lastNameTextBox.Text);
//                SaveAnswerToQuestion(4, 1, _emailTextBox.Text);
//                SaveAnswerToQuestionCheck(5, 1, _optInChecked1);

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
                Location = new Point(187, 48),
                Size = new Size(800, 35),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _lastNameTextBox = new TextBox
            {
                Location = new Point(187, 108),
                Size = new Size(800, 35),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(187, 164),
                Size = new Size(800, 35),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 16)
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
        //    else if (InArea(e.X, e.Y, _optInArea1))
        //    {
        //        _optInChecked1 = !_optInChecked1;

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