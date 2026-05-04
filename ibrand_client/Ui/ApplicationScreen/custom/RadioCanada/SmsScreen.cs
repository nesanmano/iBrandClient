using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class SmsScreen : AbstractScreen
    {
        readonly Rectangle _previousArea;

        private TextBox _smsTextBox;
        
        public SmsScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _previousArea = new Rectangle(new Point(590, 278), new Size(106, 97));
        }

        public override void Recycle()
        {
            base.Recycle();

            _smsTextBox.Text = "";
        }


        private void AddKeyboard()
        {

            AddKeyboardToControls(415,390,470, 390);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_smsTextBox.Text.Length <= 0) return true;

            if (StringUtil.IsValidPhoneNumber(_smsTextBox.Text))
            {
//                SaveAnswerToQuestion(6, 1, _smsTextBox.Text);
//                if (_smsTextBox.Text.Equals("5141234567")){
//                    SaveAnswerToQuestionCheck(10, 1, true);
//                }
//            }
//            else
//            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Numéro de téléphone non valide.");
                return false;
            }

            return true;
        }

        private void AddTextBoxs()
        {
            _smsTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(269, 197),
                Size = new Size(734, 50),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 26)
            };
            _smsTextBox.TextChanged += OnTextChanged;

            Controls.Add(_smsTextBox);
            _smsTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (!InArea(e.X, e.Y, _previousArea)) return;

        //    if (SetAnswersToQuestionnaire())
        //    {
        //        Back(2);
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _smsTextBox.Focus();
        }
    }
}
