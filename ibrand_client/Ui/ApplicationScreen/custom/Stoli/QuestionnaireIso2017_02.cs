using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Stoli
{
    public class QuestionnaireIso2017_02 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();

        private bool _checkState1;
        private bool _checkState2;
        private readonly Rectangle _checkArea1 = new Rectangle(81, 224, 740, 63);
        private readonly Rectangle _checkArea2 = new Rectangle(81, 310, 740, 54);
        private readonly Point _checkLocation1 = new Point(85, 240);
        private readonly Point _checkLocation2 = new Point(85, 320);
        private readonly Bitmap _checkBitmap;

        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        public QuestionnaireIso2017_02(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _cancelArea = new Rectangle(1220, 15, 61, 51);
            _nextArea = new Rectangle(555, 380, 176, 55);

            _checkBitmap = GetNoneLanguageBitmap("check.png");
        }

        private void AddKeyboard()
        {
            const int width = 1228;
            const int height = 341;
            const int x = 30;
            const int y = 444;

            AddKeyboardToControls(x, y, width, height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 22);
            this.InitTextBox(_nameTextbox, new Rectangle(292, 100, 502, 45), font);
            _nameTextbox.TabIndex = 1;
            this.InitTextBox(_emailTextbox, new Rectangle(618, 167, 502, 45), font);
            _emailTextbox.TabIndex = 2;
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState1 = false;
            _checkState2 = false;
            _emailTextbox.Text = "";
            _nameTextbox.Text = "";
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState1) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation1);

        //    if (_checkState2) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation2);
        //}

        private bool SetAnswersToQuestionnaire()
        {
            

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _nameTextbox.Text.Trim());
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
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Adresse courriel non valide." : "Invalid email");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your email.");
                return false;
            }
           
            // Validation check
            SaveAnswerToQuestionCheck(3, 1, _checkState1);

            SaveAnswerToQuestionCheck(4, 1, _checkState2);


            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _checkArea1))
        //    {
        //        _checkState1 = !_checkState1;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _checkArea2))
        //    {
        //        _checkState2 = !_checkState2;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        FirstScreen();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextbox.Focus();
        }
    }
}
