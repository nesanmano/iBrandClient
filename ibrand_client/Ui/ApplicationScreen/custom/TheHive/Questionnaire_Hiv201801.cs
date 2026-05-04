using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.TheHive
{
    // ReSharper disable once InconsistentNaming
    public class Questionnaire_Hiv201801 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _smsTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();

        private readonly Rectangle _nextArea = new Rectangle(290, 476, 220, 46);

        public Questionnaire_Hiv201801(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls<KeyboardControl>(-1, 666, 740, 274);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);
            this.InitTextBox(_smsTextbox, new Rectangle(262, 341, 444, 30), font, 1);
            this.InitTextBox(_emailTextbox, new Rectangle(262, 383, 444, 30), font, 1);
        }

        public override void Recycle()
        {
            base.Recycle();

            _smsTextbox.Text = "";
            _emailTextbox.Text = "";
        }
        
        private bool SetAnswersToQuestionnaire()
        {
            // Validation prénom
            if (!string.IsNullOrWhiteSpace(_smsTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_smsTextbox.Text))
                {
                    //SaveAnswerToQuestion(6, 1, _smsTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                        "Numéro de téléphone invalide." :
                        "Invalid phone number.");
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(5, 1, _emailTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                        "Courriel invalide." :
                        "Invalid email.");
                    return false;
                }
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

            return true;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _smsTextbox.Focus();
        }
    }
}
