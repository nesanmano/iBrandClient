using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    // ReSharper disable once InconsistentNaming
    public class Questionnaire_Dia201801_C : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        
        private bool _checkState1;
        private bool _checkState2;
        
        private readonly Rectangle _checkArea1 = new Rectangle(188, 369, 725, 52);
        private readonly Rectangle _checkArea2 = new Rectangle(188, 445, 725, 535);
        
        private readonly Bitmap _checkBitmap;
        private readonly Rectangle _nextArea = new Rectangle(527, 998, 342, 77);
        private readonly Rectangle _declineArea = new Rectangle(188, 998, 342, 77);

        public Questionnaire_Dia201801_C(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _checkBitmap = GetNoneLanguageBitmap("check_mark_orange.png");
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls<UsaKeyboardControl>(-1, 1100, 868, 320);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);
            this.InitTextBox(_nameTextbox, new Rectangle(212, 296, 151, 28), font, 1);
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState1 = false;
            _checkState2 = false;
            _nameTextbox.Text = "";
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState1) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkArea1.Location);
        //    if (_checkState2) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkArea2.Location);
        //}

        private bool SetAnswersToQuestionnaire()
        {
            // Validation prénom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(7, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? 
                    "Vous devez entrez votre nom." : 
                    "You must enter your name.");
                return false;
            }

            SaveAnswerToQuestionCheck(3, 1, _checkState1);
            SaveAnswerToQuestionCheck(4, 1, _checkState2);

            if (_checkState1 || _checkState2) return true;
            
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? 
                    "Vous devez choisir au moins une méthode d'envoi." : 
                    "You must choose at least one delivery method.");
                return false;
            }
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
        //    else if (InArea(e.X, e.Y, _checkArea2))
        //    {
        //        _checkState2 = !_checkState2;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _declineArea))
        //    {
        //        Next(1);
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next(2);
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
