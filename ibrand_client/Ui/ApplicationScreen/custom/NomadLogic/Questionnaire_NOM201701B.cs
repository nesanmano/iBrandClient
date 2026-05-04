using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.NomadLogic
{
    // ReSharper disable once InconsistentNaming
    internal class Questionnaire_NOM201701B : AbstractScreen
    {
        //private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _phoneTextbox;
        private TextBox _emailTextbox;
        private Rectangle _next = new Rectangle(265, 437, 166, 62);

        public Questionnaire_NOM201701B(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();
        }

        public override void Recycle()
        {
            base.Recycle();

            FormUtil.SafeCall(this, () => { 
                _firstNameTextbox.Text = "";
                _lastNameTextbox.Text = "";
                _phoneTextbox.Text = "";
                _emailTextbox.Text = "";
            });
        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(-1, 519, 1209, 246)
            {
                KeyPadding = -5
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {
            _firstNameTextbox = new TextBox();
            this.InitTextBox(_firstNameTextbox, new Rectangle(342, 329, 285, 30), new Font(FontFamily.GenericSansSerif, 14), 1);

            _lastNameTextbox = new TextBox();
            this.InitTextBox(_lastNameTextbox, new Rectangle(779, 329, 241, 30), new Font(FontFamily.GenericSansSerif, 14), 2);

            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(468, 368, 159, 30), new Font(FontFamily.GenericSansSerif, 14), 3);

            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(718, 368, 302, 30), new Font(FontFamily.GenericSansSerif, 14), 4);
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

            // Validation prénom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(3, 1, _firstNameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? 
                    "Prénom invalide." :
                    "Invalid First name.");
                return false;
            }

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_lastNameTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _lastNameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Nom de famille invalide." :
                    "Invalid Last name.");
                return false;
            }

            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _phoneTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Numéro de téléphone invalide." :
                    "Invalid phone number.");
                return false;
            }

            // Validation courriel 
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _emailTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Courriel invalide." :
                    "Invalid email.");
                return false;
            }

            return true;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextbox.Focus();
        }
    }
}
