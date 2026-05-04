using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

#pragma warning disable 618

namespace NomadLogic.PrintStation.Ui.custom.Ste
{
    // ReSharper disable once InconsistentNaming
    class QuestionnaireScreen_STE201901_info : AbstractPrintStationScreen
    {
        private TextBox _nameTextbox;
        private TextBox _mailTextbox;
        private TextBox _smsTextbox;

        private readonly Rectangle _nextArea = new Rectangle(583, 892, 515, 90);
        private readonly Rectangle _cancelArea = new Rectangle(1325, 33, 336, 112);

        public QuestionnaireScreen_STE201901_info(string backgroundImageEnSrc, string backgroundImageFrSrc, PrintStationScreenConfig sc)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();
        }


        private void AddKeyboard()
        {
            AddKeyboardToControls<KeyboardControl>(-1, 472, 1460, 410);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            
            this.InitTextBox(out _nameTextbox, new Rectangle(415, 231, 980, 50), font);
            this.InitTextBox(out _mailTextbox, new Rectangle(415, 321, 980, 50), font);
            this.InitTextBox(out _smsTextbox, new Rectangle(415, 406, 980, 50), font);
        }


        public override void Recycle()
        {
            base.Recycle();

            _nameTextbox.Text = "";
            _mailTextbox.Text = "";
            _smsTextbox.Text = "";
        }

        private bool SetAnswersToQuestionnaire()
        {
            // Validation Nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(3, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Vous devez indiquez votre nom." : "You must enter your name.");
                return false;
            }

            // Validation Courriel
            if (!string.IsNullOrWhiteSpace(_mailTextbox.Text) && StringUtil.IsValidEmail(_mailTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _mailTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Vous devez indiquez votre courriel." : "You must enter your email.");
                return false;
            }

            // Validation Mobile
            if (!string.IsNullOrWhiteSpace(_smsTextbox.Text) && StringUtil.IsValidPhoneNumber(_smsTextbox.Text))
            {
                //SaveAnswerToQuestion(5, 1, _smsTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Vous devez indiquez votre numéro de mobile." : "You must enter your mobile number.");
                return false;
            }

            return true;
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next(1);
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
