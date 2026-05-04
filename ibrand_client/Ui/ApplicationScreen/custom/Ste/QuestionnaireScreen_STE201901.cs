using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Ste
{
    // ReSharper disable once InconsistentNaming
    class QuestionnaireScreen_STE201901 : AbstractScreen
    {
        private readonly TextBox _customTextbox = new TextBox();

        private readonly Rectangle _nextArea = new Rectangle(590, 892, 500, 90);
        private readonly Rectangle _cancelArea = new Rectangle(1325, 33, 336, 112);

        public QuestionnaireScreen_STE201901(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();
        }


        private void AddKeyboard()
        {
            AddKeyboardToControls<KeyboardControl>(-1, 299, 1600, 573);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 30);
            
            this.InitTextBox(_customTextbox, new Rectangle(492, 220, 970, 50), font, 2);
        }


        public override void Recycle()
        {
            base.Recycle();

            _customTextbox.Text = "";
        }

        private bool SetAnswersToQuestionnaire()
        {
            // Validation
            if (!string.IsNullOrWhiteSpace(_customTextbox.Text))
            {
                //SaveAnswerToQuestion(9, 1, _customTextbox.Text.Trim().ToUpperInvariant());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Vous devez indiquez votre motivation!" : "You must enter your motivation!");
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
            _customTextbox.Focus();
        }
    }
}
