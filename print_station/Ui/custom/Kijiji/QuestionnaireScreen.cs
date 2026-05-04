using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Kijiji
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;

        private readonly Rectangle _frenchArea;
        private readonly Rectangle _englishArea;
        private readonly Bitmap _checkmark;
        //private TextBox _emailTextBox;
        private TextBox _smsTextBox;

        private int _frenchChecked;
        private int _englishChecked;

        public QuestionnaireScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _frenchArea = new Rectangle(new Point(208, 174), new Size(150, 40));
            _englishArea = new Rectangle(new Point(414, 174), new Size(140, 40));

            _nextArea = new Rectangle(new Point(591, 314), new Size(95, 95));
            _closeArea = new Rectangle(new Point(1180, 10), new Size(55, 55));
        }

        public override void Recycle()
        {
            base.Recycle();

            _smsTextBox.Text = "";
            //_emailTextBox.Text = "";

            _frenchChecked = 0;
            _englishChecked = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_frenchChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(215, 171));
        //    }

        //    if (_englishChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(421, 171));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControlsNumeric((Size.Width - 415) / 2, 436, 415,333);
        }

        /*private bool ValidateEmailAddress()
        {
            if (!string.IsNullOrEmpty(_emailTextBox.Text) && StringUtil.IsValidEmail(_emailTextBox.Text)) return true;

            var mBox = new MessageDialog();
            mBox.UpdateStatusLabel("Le courriel n'est pas valide. Email address not valid.");

            return false;
        }*/

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = GetCurrentQuestionnaire();

            var mBox = new MessageDialog();

            if (_smsTextBox.Text.Length > 0 /*|| _emailTextBox.Text.Length > 0*/)
            {
                if (!StringUtil.IsValidPhoneNumber(_smsTextBox.Text))
                {
                    mBox.ShowMessage("Numéro de téléphone non valide. Invalid phone number.");
                    return false;
                }

                    if (_englishChecked == 0 && _frenchChecked == 0)
                {
                    mBox.ShowMessage("Vous devez sélectionner la langue. You must select language.");
                    return false;
                }

            }

            /*if (_emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false; // Question 3
                }

                SaveAnswerToQuestion(3, 1, _emailTextBox.Text);
            }*/

//            SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
//            SaveAnswerToQuestion(2, 1, _smsTextBox.Text);
//
//            if (_frenchChecked == 1)
//                SaveAnswerToQuestion(4, 1, 2);
//            else if (_englishChecked == 1)
//                SaveAnswerToQuestion(4, 1, 1);

            return true;
        }

        private void AddTextBoxs()
        {
            _smsTextBox = new TextBox
            {
                Location = new Point(358, 87),
                Size = new Size(680, 45),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            _smsTextBox.TextChanged += OnTextChanged;

            Controls.Add(_smsTextBox);
            _smsTextBox.BringToFront();

            /*_emailTextBox = new TextBox
            {
                Location = new Point(190, 35),
                Size = new Size(820, 35),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();*/

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
        //    else if (InArea(e.X, e.Y, _frenchArea))
        //    {
        //        _frenchChecked = 1;
        //        _englishChecked = 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _englishArea))
        //    {
        //        _frenchChecked = 0;
        //        _englishChecked = 1;

        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _smsTextBox.Focus();
        }
    }
}