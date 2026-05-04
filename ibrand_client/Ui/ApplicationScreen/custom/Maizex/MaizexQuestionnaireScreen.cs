
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Maizex
{
    class MaizexQuestionnaireScreen : AbstractScreen
    {
        private readonly TextBox _smsTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _firstNameTextbox = new TextBox();
        private readonly TextBox _lastNameTextbox = new TextBox();

        private readonly Rectangle _nextArea = new Rectangle(533, 739, 205, 53);

        private Bitmap _checkmark;

        private readonly IList<ScreenCheckOption> _checks = new List<ScreenCheckOption>
        {
         
        };

        public MaizexQuestionnaireScreen(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc)
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
            AddKeyboardToControls<KeyboardControl>(74, 408, 1135, 261);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);
            this.InitTextBox(_smsTextbox, new Rectangle(339, 329, 651, 25), font, 2);
            this.InitTextBox(_emailTextbox, new Rectangle(339, 377, 651, 25), font, 3);
            this.InitTextBox(_firstNameTextbox, new Rectangle(339, 284, 651, 25), font, 1);

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

            RecycleCheckBoxs(_checks);
        }

        private bool SetAnswersToQuestionnaire()
        {
            MessageDialog mBox;
            // Validation nom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _firstNameTextbox.Text.Trim());
            }

            // Validation email
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _emailTextbox.Text.Trim());
            }
            else if(!string.IsNullOrWhiteSpace(_emailTextbox.Text) && !StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                mBox = new MessageDialog();
                mBox.ShowMessage("Entrer un courriel valide.");
                return false;
            }


            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_smsTextbox.Text) && StringUtil.IsValidPhoneNumber(_smsTextbox.Text.Trim()))
            {

                //SaveAnswerToQuestion(3, 1, _smsTextbox.Text.Trim());


            }
            else if(!string.IsNullOrWhiteSpace(_smsTextbox.Text) && ! StringUtil.IsValidPhoneNumber(_smsTextbox.Text.Trim()))
            {
                mBox = new MessageDialog();
                mBox.ShowMessage("Entrer un numéro de téléphone valide.");
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

            return EvaluateCheckBoxsMouseDown(x, y, _checks);
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextbox.Focus();
        }



    }
}
