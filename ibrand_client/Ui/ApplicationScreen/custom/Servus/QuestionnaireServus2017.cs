using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Servus
{
    public class QuestionnaireServus2017 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _firstNameTextbox = new TextBox();
        private readonly TextBox _lastNameTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _postalCodeTextbox = new TextBox();
        private bool _checkState1;
        private readonly Rectangle _checkArea1 = new Rectangle(210, 353, 738, 75);
        private readonly Point _checkLocation1 = new Point(215, 350);
        private readonly Bitmap _checkBitmap;

        private readonly Rectangle _nextArea;
        private readonly Rectangle _detailArea;

        public QuestionnaireServus2017(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _detailArea = new Rectangle(950, 386, 379, 45);
            _nextArea = new Rectangle(645, 863, 389, 102);

            _checkBitmap = GetNoneLanguageBitmap("checkmark.png");
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(-1, 460, 1600, 355);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 22);
            this.InitTextBox(_firstNameTextbox, new Rectangle(473, 37, 1004, 50), font, 1);
            this.InitTextBox(_lastNameTextbox, new Rectangle(473, 114, 1004, 50), font, 2);
            this.InitTextBox(_emailTextbox, new Rectangle(376, 189, 1100, 50), font, 3);
            this.InitTextBox(_postalCodeTextbox, new Rectangle(532, 265, 945, 50), font, 4);
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState1 = false;
            _firstNameTextbox.Text = "";
            _emailTextbox.Text = "";
            _lastNameTextbox.Text = "";
            _postalCodeTextbox.Text = "";
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState1) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation1);
        //}

        private bool SetAnswersToQuestionnaire()
        {
            // Validation prénom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _firstNameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your First name.");
                return false;
            }

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_lastNameTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _lastNameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your last name.");
                return false;
            }

            // Validation courriel
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(3, 1, _emailTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Adresse courriel non valide." : "Invalid email");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your email.");
                return false;
            }

            // Validation Code Postal
            if (!string.IsNullOrWhiteSpace(_postalCodeTextbox.Text))
            {
                if (StringUtil.IsValidCanadaPostalCode(_postalCodeTextbox.Text) || StringUtil.IsValidUSPostalCode(_postalCodeTextbox.Text))
                {
                    //SaveAnswerToQuestion(4, 1, _postalCodeTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Code postal non valide." : "Invalid postal code");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your postal code.");
                return false;
            }

            // Validation check
            if (_checkState1)
            {
                SaveAnswerToQuestionCheck(5, 1, _checkState1);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must agree to the disclaimer.");
                return false;
            }

            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _checkArea1))
        //    {
        //        _checkState1 = !_checkState1;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next(2);
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _detailArea))
        //    {
        //        Next(1);
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextbox.Focus();
        }
    }
}
