using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.App
{
    // ReSharper disable once InconsistentNaming
    public class Questionnaire_APP201801 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _nameTextbox = new TextBox();

        private readonly Rectangle _nextArea = new Rectangle(681, 896, 317, 84);
        private readonly Rectangle _policyArea = new Rectangle(254, 308, 155, 24);
        private readonly Rectangle _cancelArea = new Rectangle(1448, 35, 190, 73);

        private Bitmap _checkmark;

        private readonly IList<ScreenCheckOption> _checks = new List<ScreenCheckOption>
        {
            new ScreenCheckOption(new Rectangle(81, 291, 33, 33), 4, 1, false, new Point(85, 295))
        };

        public Questionnaire_APP201801(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc)
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
            AddKeyboardToControls<KeyboardControl>(-1, 381, 1587, 478);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);
            this.InitTextBox(_emailTextbox, new Rectangle(314, 230, 1200, 30), font, 2);
            this.InitTextBox(_nameTextbox, new Rectangle(314, 182, 1200, 30), font, 1);
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

            _emailTextbox.Text = "";
            _nameTextbox.Text = "";

            RecycleCheckBoxs(_checks);
        }
        
        private bool SetAnswersToQuestionnaire()
        {
            if (!AllMandatoryChecks(_checks))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Vous devez cochez que vous avez 18 ans ou plus et acceptez l'utilisation de votre image à des fins promotionelles." :
                    "You must check that you are 18 and above and accept the use of your image for promotional use.");
                return false;
            }
            
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _emailTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Courriel invalide." :
                    "Invalid email.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Veuillez entrez votre nom." :
                    "Please enter your name.");
                return false;
            }

            return true;
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _policyArea))
            {
                Next(1);
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next(2);
                }
                return false;
            }

            return EvaluateCheckBoxsMouseDown(x, y, _checks);
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextbox.Focus();
        }
    }
}
