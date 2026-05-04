using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.NomadLogic
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _emailTextbox;
        private TextBox _phoneTextbox;
        private bool _checkOptIn;
        private readonly Rectangle _checkZone;
        
        private readonly Bitmap _checkBmp;
        private readonly Point _checkPos;

        public QuestionnaireScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(132, 461, 342, 81);
            _checkZone = new Rectangle(158, 422, 969, 40);
            _checkPos = new Point(147, 407);

            _checkBmp = GetNoneLanguageBitmap("checkmark.png");
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextbox.Text = "";
            _lastNameTextbox.Text = "";
            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";
            _checkOptIn = false;
        }
        
        private void AddKeyboard()
        {
            AddKeyboardToControls<KeyboardControl>(-1, 640, 1600, 375);
        }

        private void AddTextBoxs()
        {
            var boxSize = new Size(488, 34);
            const int X_POS1 = 320;
            const int X_POS2 = 1090;
            const int Y_ROW1 = 325;
            const int Y_ROW2 = 385;

            _firstNameTextbox = new TextBox
            {
                Location = new Point(X_POS1, Y_ROW1),
                Size = boxSize,
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _firstNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_firstNameTextbox);
            _firstNameTextbox.BringToFront();

            _lastNameTextbox = new TextBox
            {
                Location = new Point(X_POS2, Y_ROW1),
                Size = boxSize,
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _lastNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_lastNameTextbox);
            _lastNameTextbox.BringToFront();

            _emailTextbox = new TextBox
            {
                Location = new Point(X_POS1, Y_ROW2),
                Size = boxSize,
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _emailTextbox.TextChanged += OnTextChanged;
            Controls.Add(_emailTextbox);
            _emailTextbox.BringToFront();

            _phoneTextbox = new TextBox
            {
                Location = new Point(X_POS2, Y_ROW2),
                Size = boxSize,
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _phoneTextbox.TextChanged += OnTextChanged;
            Controls.Add(_phoneTextbox);
            _phoneTextbox.BringToFront();
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _checkZone))
            {
                _checkOptIn = !_checkOptIn;
                Refresh();
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                }

                return false;
            }

            return true;
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_checkOptIn)
            {
                DrawImage(g, _checkBmp, _checkPos);
            }
        }

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_firstNameTextbox.Text.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && !StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ? "Invalid email." : "Courriel Invalide");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && !StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "Invalid phone number"
                        : "Téléphone invalide");
                    return false;
                }

//                /* Questions obligatoires */
//                SaveAnswerToQuestion(6, 1, _firstNameTextbox.Text);
//
//                /* Question facultatives */
//                SaveAnswerToQuestion(2, 1, _emailTextbox.Text);
//                SaveAnswerToQuestion(7, 1, _lastNameTextbox.Text);
//                SaveAnswerToQuestion(1, 1, _phoneTextbox.Text);
//                SaveAnswerToQuestionCheck(8, 1, _checkOptIn);

                return true;
            }

            mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ? 
                "You must answer all mandatory questions." : 
                "Vous devez répondre à toutes les questions obligatoires.");

            return false;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            _emailTextbox.Text = GetTextAnswerFromQuestion(2) ?? "";
            _firstNameTextbox.Text = GetTextAnswerFromQuestion(6) ?? "";
            _lastNameTextbox.Text = GetTextAnswerFromQuestion(7) ?? "";
            if (GetTextAnswerFromQuestion(1) != null && StringUtil.IsValidPhoneNumber(GetTextAnswerFromQuestion(1)))
            {            
                _phoneTextbox.Text = GetTextAnswerFromQuestion(1);
            }
            else
            {
                _phoneTextbox.Text = "";
            }
        }
    }
}
