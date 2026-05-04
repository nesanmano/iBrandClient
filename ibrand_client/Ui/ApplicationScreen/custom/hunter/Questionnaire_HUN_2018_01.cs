using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.hunter
{
    // ReSharper disable once InconsistentNaming
    internal class Questionnaire_HUN_2018_01 : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TextBox _phoneTextbox;
        private TextBox _emailTextbox;
        private Rectangle _next = new Rectangle(733, 728, 214, 104);

        public Questionnaire_HUN_2018_01(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();
        }

        public override void Recycle()
        {
            base.Recycle();

            FormUtil.SafeCall(this, () => { 
                _phoneTextbox.Text = "";
                _emailTextbox.Text = "";
            });
        }

        private void AddKeyboard()
        {
            var kb = new UsaKeyboardControl(-1, 302, 1520, 392);
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {
            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(340, 172, 1000, 40), new Font(FontFamily.GenericSansSerif, 14), 3);

            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(340, 233, 1000, 40), new Font(FontFamily.GenericSansSerif, 14), 4);
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _next))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                }
                return false;
            }

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            MessageDialog mBox;

            
            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    //SaveAnswerToQuestion(3, 1, _phoneTextbox.Text.Trim());
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase)
                        ? "Numéro de téléphone invalide."
                        : "Invalid phone number.");
                    return false;
                }
            }

            // Validation courriel 
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(1, 1, _emailTextbox.Text.Trim());
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                        "Courriel invalide." :
                        "Invalid email.");
                    return false;
                }
            }

            return true;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _phoneTextbox.Focus();
        }
    }
}
