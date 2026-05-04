using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    public class QuestionnaireBlueJays2016 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _firstNameTextbox = new TextBox();
        private bool _checkState;
        private readonly Rectangle _checkArea = new Rectangle(647, 226, 520, 59);
        private readonly Point _checkLocation = new Point(646, 226);
        private readonly Bitmap _checkBitmap;

        private readonly Rectangle _nextArea;

        public QuestionnaireBlueJays2016(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(379, 311, 522, 62);

            var filters = new[]
            {
                new ColorSubstitutionFilter(10, Color.Black, Color.White)
            };
            var bmf = new BitmapModificationFilter(filters, 0.5f);
            _checkBitmap = ImageUtil.ModifyPicture(GetNoneLanguageBitmap("checkmark.png"),
                ColorSubstitutionFilterGroup.All, bmf);
        }

        private void AddKeyboard()
        {
            const int width = 1204;
            const int height = 366;
            const int x = 36;
            const int y = 400;

            AddKeyboardToControls(x, y, width, height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            this.InitTextBox(_firstNameTextbox, new Rectangle(110, 155, 502, 37), font);
            this.InitTextBox(_nameTextbox, new Rectangle(110, 239, 502, 37), font);
            this.InitTextBox(_emailTextbox, new Rectangle(649, 155, 502, 37), font);
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState = false;
            _emailTextbox.Text = "";
            _firstNameTextbox.Text = "";
            _nameTextbox.Text = "";
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation);
        //}

        private bool SetAnswersToQuestionnaire()
        {
            // Validation courriel
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(2, 1, _emailTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Adresse courriel non valide." : "Invalid email");
                    return false;
                }
            }

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(5, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr") ? "Prénom non valide." : "Invalid First Name");
                return false;
            }

            // Validation check
            SaveAnswerToQuestionCheck(3, 1, _checkState);

            // Validation Prénom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _firstNameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr") ? "Nom non valide." : "Invalid Last Name");
                return false;
            }

            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _checkArea))
        //    {
        //        _checkState = !_checkState;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
        //        }
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextbox.Focus();
        }
    }
}