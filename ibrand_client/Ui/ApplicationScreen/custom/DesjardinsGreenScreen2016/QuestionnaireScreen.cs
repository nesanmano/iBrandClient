using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.DesjardinsGreenScreen2016
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _checkTipSheetRectangle;
        private readonly Rectangle _checkContestRectangle;

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _emailTextbox;
        private TextBox _phoneTextbox;

        private readonly Bitmap _checkmark;

        private bool _checkTipSheet;
        private bool _checkContest;

        public QuestionnaireScreen(string backgroundImgEnSrc, string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _checkTipSheetRectangle = new Rectangle(new Point(120, 167), new Size(1001, 67));
            _checkContestRectangle = new Rectangle(new Point(122, 236), new Size(1004, 67));
            _nextArea = new Rectangle(new Point(585, 338), new Size(115, 53));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextbox.Text = "";
            _lastNameTextbox.Text = "";
            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";

            _checkTipSheet = false;
            _checkContest = false;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkTipSheet)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(132, 161));
        //    }

        //    if (_checkContest)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(132, 229));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1205) / 2, 395, 1205, 375);
        }

        private void AddTextBoxs()
        {
            _firstNameTextbox = new TextBox
            {
                Location = new Point(278, 47),
                Size = new Size(367, 35),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _firstNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_firstNameTextbox);
            _firstNameTextbox.BringToFront();

            _lastNameTextbox = new TextBox
            {
                Location = new Point(812, 47),
                Size = new Size(380, 35),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _lastNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_lastNameTextbox);
            _lastNameTextbox.BringToFront();

            _emailTextbox = new TextBox
            {
                Location = new Point(312, 104),
                Size = new Size(333, 35),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _emailTextbox.TextChanged += OnTextChanged;
            Controls.Add(_emailTextbox);
            _emailTextbox.BringToFront();

            _phoneTextbox = new TextBox
            {
                Location = new Point(849, 104),
                Size = new Size(343, 35),
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _phoneTextbox.TextChanged += OnTextChanged;
            Controls.Add(_phoneTextbox);
            _phoneTextbox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _checkTipSheetRectangle))
        //    {
        //        _checkTipSheet = !_checkTipSheet;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _checkContestRectangle))
        //    {
        //        _checkContest = !_checkContest;

        //        Refresh();
        //    }
           
        //}

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_emailTextbox.Text.Length > 0 && _firstNameTextbox.Text.Length > 0 && _lastNameTextbox.Text.Length > 0 && _phoneTextbox.Text.Length > 0)
            {
               
                if (!StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    mBox.ShowMessage("Invalid phone number.");
                    return false;
                }

                if (!StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    mBox.ShowMessage("Invalid email.");
                    return false;
                }
                
                /*
                SaveAnswerToQuestion(1, 1, _firstNameTextbox.Text);
                SaveAnswerToQuestion(2, 1, _lastNameTextbox.Text);
                SaveAnswerToQuestion(3, 1, _emailTextbox.Text);
                SaveAnswerToQuestion(4, 1, _phoneTextbox.Text);
                SaveAnswerToQuestionCheck(5, 1, _checkTipSheet);
                SaveAnswerToQuestionCheck(6, 1, _checkContest);
                */

                return true;
            }

            mBox.ShowMessage("You must answer all questions.");

            return false;
        }
    }
}
