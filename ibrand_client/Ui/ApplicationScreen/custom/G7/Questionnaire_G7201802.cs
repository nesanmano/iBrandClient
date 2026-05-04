using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.G7
{
    // ReSharper disable once InconsistentNaming
    public class Questionnaire_G7201802 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _smsTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _firstNameTextbox = new TextBox();
        private readonly TextBox _lastNameTextbox = new TextBox();

        private readonly Rectangle _nextArea = new Rectangle(489, 377, 298, 64);
        private readonly Rectangle _disclaimerArea = new Rectangle(745, 325, 235, 41);

        private Bitmap _checkmark;

        private readonly IList<ScreenCheckOption> _checks = new List<ScreenCheckOption>
        {
            new ScreenCheckOption(new Rectangle(83, 187, 193, 40),  6, 1, false),
            new ScreenCheckOption(new Rectangle(83, 239, 1122, 40), 7, 1, false),
            new ScreenCheckOption(new Rectangle(83, 291, 659, 72),  8, 1, true)
        };

        public Questionnaire_G7201802(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();
        }

        protected override void OnLoad()
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls<KeyboardControl>(25, 454, 1059, 303);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);
            this.InitTextBox(_smsTextbox,       new Rectangle(822, 135, 425, 35), font, 1);
            this.InitTextBox(_emailTextbox,     new Rectangle(230, 135, 425, 35), font, 2);
            this.InitTextBox(_firstNameTextbox, new Rectangle(230, 090, 425, 35), font, 3);
            this.InitTextBox(_lastNameTextbox,  new Rectangle(822, 090, 425, 35), font, 4);
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, _checks, _checkmark);
        }

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }

        public override void Recycle()
        {
            base.Recycle();

            _smsTextbox.Text = "";
            _emailTextbox.Text = "";
            _firstNameTextbox.Text = "";
            _lastNameTextbox.Text = "";

            foreach (var zone in _checks)
            {
                zone.Recycle();
            }
        }
        
        private bool SetAnswersToQuestionnaire()
        {
            if (!AllMandatoryChecks(_checks))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.iBrand.NotAllMandatoryCheckEnabled);
                return false;
            }

            // Validation sms
            if (!string.IsNullOrWhiteSpace(_smsTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_smsTextbox.Text))
                {
                    //SaveAnswerToQuestion(5, 1, _smsTextbox.Text.Trim());
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
                    //SaveAnswerToQuestion(4, 1, _emailTextbox.Text.Trim());
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

            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _firstNameTextbox.Text.Trim());
            }

            if (!string.IsNullOrWhiteSpace(_lastNameTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _lastNameTextbox.Text.Trim());
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
                    Next(2);
                }
                return false;
            }

            if (InArea(x, y, _disclaimerArea))
            {
                Next(1);
                return false;
            }

            return EvaluateCheckBoxsMouseDown(x, y, _checks);
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextbox.Focus();
        }
    }
}
