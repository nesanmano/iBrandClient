using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.PeneraBread 
{
    class QuestionnaireAll2018 : AbstractScreen
    {
            // ReSharper disable once UnusedMember.Local
            private static readonly Logger log = LogManager.GetCurrentClassLogger();

            private readonly TextBox _nameTextbox = new TextBox();
            private readonly TextBox _emailTextbox = new TextBox();
            private readonly TextBox _smsNumberTextBox = new TextBox();

        private bool _checkState1;
            private bool _checkState2;
            private readonly Rectangle _checkArea1 = new Rectangle(88, 266, 35, 35);
            private readonly Rectangle _checkArea2 = new Rectangle(88, 324, 35, 35);
        private readonly Point _checkLocation1 = new Point(77, 255);
            private readonly Point _checkLocation2 = new Point(77, 311);
            private readonly Bitmap _checkBitmap;

            private readonly Rectangle _nextArea;
            private readonly Rectangle _cancelArea;

            public QuestionnaireAll2018(string backgroundImageSrc, ScreenConfig sc)
                : base(backgroundImageSrc, backgroundImageSrc, sc)
            {
                AddTextBoxes();
                AddKeyboard();

                //PictureBox.MouseClick += MouseDownHandler;

                //_cancelArea = new Rectangle(1220, 15, 61, 51);
                _nextArea = new Rectangle(560, 378, 159, 46);

                _checkBitmap = GetNoneLanguageBitmap("checkmark.png");
            }

            private void AddKeyboard()
            {
                const int width = 1278;
                const int height = 300;
                const int x = 1;
                const int y = 480;

                AddKeyboardToControls(x, y, width, height);
            }

            private void AddTextBoxes()
            {
                var font = new Font(FontFamily.GenericSansSerif, 15);
                this.InitTextBox(_nameTextbox, new Rectangle(344, 68, 589, 10), font);
                _nameTextbox.TabIndex = 1;
                this.InitTextBox(_emailTextbox, new Rectangle(344, 130, 589, 20), font);
                _emailTextbox.TabIndex = 2;
                this.InitTextBox(_smsNumberTextBox, new Rectangle(435, 198, 498, 20), font);
                _emailTextbox.TabIndex = 3;
        }

            public override void Recycle()
            {
                base.Recycle();

                _checkState1 = false;
                _checkState2 = false;
                _emailTextbox.Text = "";
                _nameTextbox.Text = "";
                _smsNumberTextBox.Text = "";
            }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_checkState1) DrawImage(g, _checkBitmap, _checkLocation1);

            if (_checkState2) DrawImage(g, _checkBitmap, _checkLocation2);
        }

        private bool SetAnswersToQuestionnaire()
        {
            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your name!");
                return false;
            }


            // Validation courriel
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(2, 1, _emailTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Invalid email");
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(_smsNumberTextBox.Text))
            {


                if (StringUtil.IsValidPhoneNumber(_smsNumberTextBox.Text))
                {
                    //SaveAnswerToQuestion(3, 1, _smsNumberTextBox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Invalid phone number.");
                    return false;
                }
            }


            // Validation check
            SaveAnswerToQuestionCheck(4, 1, _checkState1);

            if (_checkState2 == true)
            {
                SaveAnswerToQuestionCheck(5, 1, _checkState2);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must consent to allow us to use your photos for promotional activities.");
                return false;
            }

            return true;
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _checkArea1))
            {
                _checkState1 = !_checkState1;
                Refresh();
                return false;
            }

            if (InArea(x, y, _checkArea2))
            {
                _checkState2 = !_checkState2;
                Refresh();
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                }

                return false;
            }

            if (InArea(x, y, _cancelArea))
            {
                FirstScreen();
                return false;
            }

            return true;
        }

        public override void SetActivated()
            {
                base.SetActivated();
                _nameTextbox.Focus();
            }
    }
}



