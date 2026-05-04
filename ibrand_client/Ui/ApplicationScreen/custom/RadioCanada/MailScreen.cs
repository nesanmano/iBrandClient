using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class MailScreen : AbstractScreen
    {
        readonly Rectangle _previousArea;

        private TextBox _mailTextBox;
        
        public MailScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBox();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _previousArea = new Rectangle(new Point(590, 278), new Size(106, 97));
        }

        public override void Recycle()
        {
            base.Recycle();

            _mailTextBox.Text = "";
        }


        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1239) / 2, 420, 1239, 365);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_mailTextBox.Text.Length <= 0) return true;

            if (StringUtil.IsValidEmail(_mailTextBox.Text))
            {
                //SaveAnswerToQuestion(5, 1, _mailTextBox.Text);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Adresse courriel invalide.");
                return false;
            }

            return true;
        }

        private void AddTextBox()
        {
            _mailTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(271, 194),
                Size = new Size(821, 50),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 26)
            };
            _mailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_mailTextBox);
            _mailTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (!InArea(e.X, e.Y, _previousArea)) return;

        //    if (SetAnswersToQuestionnaire())
        //    {
        //        Back(1);
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _mailTextBox.Focus();
        }
    }
}
