using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Momentum
{
    public class QuestionnaireGmcStampede2017 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _mailTextbox = new TextBox();
        private readonly TextBox _smsTextbox = new TextBox();

        private readonly Rectangle _nextArea;

        public QuestionnaireGmcStampede2017(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;
            
            _nextArea = new Rectangle(539, 529, 217, 94);
        }

        private void AddKeyboard()
        {
            const int width = 1186;
            const int height = 273;
            const int x = -1;
            const int y = 222;

            AddKeyboardToControls(x, y, width, height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 22);
            this.InitTextBox(_mailTextbox, new Rectangle(263, 72, 805, 35), font, 1);
            this.InitTextBox(_smsTextbox, new Rectangle(263, 148, 805, 35), font, 2);
        }

        public override void Recycle()
        {
            base.Recycle();
            
            _mailTextbox.Text = "";
            _smsTextbox.Text = "";
        }
        
        private bool SetAnswersToQuestionnaire()
        {
            // Validation mail
            if (!string.IsNullOrWhiteSpace(_mailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_mailTextbox.Text))
                {
                    //SaveAnswerToQuestion(4, 1, _mailTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Courriel non valide." : "Invalid email");
                    return false;
                }
            }

            // Validation mail
            if (!string.IsNullOrWhiteSpace(_smsTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_smsTextbox.Text))
                {
                    //SaveAnswerToQuestion(5, 1, _smsTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Téléphone non valide." : "Invalid phone");
                    return false;
                }
            }
            
            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;
            
        //    if (InArea(e.X, e.Y, _nextArea))
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
            _mailTextbox.Focus();
        }
    }
}
