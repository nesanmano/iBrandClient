using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.sofina
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;
        
        private TextBox _emailTextbox;
        private TextBox _phoneTextbox;

        public QuestionnaireScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(525, 265, 225, 50);
        }

        public override void Recycle()
        {
            base.Recycle();
            
            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";
        }
        
        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1200) / 2, 330, 1200, 365);
        }

        private void AddTextBoxs()
        {
            var boxSize = new Size(674, 35);
            const int X_POS1 = 365;
            const int Y_ROW1 = 123;
            const int Y_ROW2 = 188;

            _emailTextbox = new TextBox
            {
                Location = new Point(X_POS1, Y_ROW2),
                Size = boxSize,
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _emailTextbox.TextChanged += OnTextChanged;
            Controls.Add(_emailTextbox);
            _emailTextbox.BringToFront();

            _phoneTextbox = new TextBox
            {
                Location = new Point(X_POS1, Y_ROW1),
                Size = boxSize,
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 14)
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
        //        {
        //            Next();
        //        }
        //    }
        //}
        
        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && !StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                    ? "Invalid email."
                    : "Courriel Invalide");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && !StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
            {
                mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                    ? "Invalid phone number"
                    : "Téléphone invalide");
                return false;
            }

//            /* Question facultatives */
//            SaveAnswerToQuestion(6, 1, _emailTextbox.Text);
//            SaveAnswerToQuestion(5, 1, _phoneTextbox.Text);
            
            return true;
        }
    }
}
