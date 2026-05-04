using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Cossette
{
    public partial class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        private TextBox _showNameTextBox;

        private TextBox _firstnameTextBox;
        private TextBox _nameTextBox;

        private TextBox _emailTextBox;
        private TextBox _phoneTextBox;

        private TextBox _cityTextBox;
        private TextBox _zipCodeTextBox;
        
        private List<ScreenCheckOption> _checks;
        private Bitmap _checkBmp;

        public QuestionnaireScreen(string bkgImageEn, string bkgImageFr, ScreenConfig sc) : base(bkgImageEn, bkgImageFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;
            
            _nextArea = new Rectangle(new Point(102, 486), new Size(162, 48));
            _cancelArea = new Rectangle(new Point(1600, 6), new Size(70, 70));

            Load += (sender, args) => OnLoad(args);
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstnameTextBox.Text = "";
            _nameTextBox.Text = "";
            _emailTextBox.Text = "";
            _phoneTextBox.Text = "";
            _zipCodeTextBox.Text = "";
            _cityTextBox.Text = "";
            _showNameTextBox.Text = "";

            foreach (var sco in _checks)
            {
                sco.Recycle();
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();

            _showNameTextBox.Focus();
        }

        // ReSharper disable once RedundantAssignment
        private void AddTextBoxs()
        {
            var tabIndex = 0;

            _checks = new List<ScreenCheckOption>
            {
                new ScreenCheckOption(new Rectangle(112, 330, 249, 35), 4, 1, false, new Point(112, 330)),
                new ScreenCheckOption(new Rectangle(112, 382, 681, 35), 6, 1, true, new Point(112, 382)),
                //new ScreenCheckOption(new Rectangle(112, 434, 1308, 35), 5, 1, false, new Point(112, 434))
            };

            var font = new Font(FontFamily.GenericSansSerif, 20);
            const int lCol = 252;
            const int rCol = 1035;
            const int row1 = 189;
            const int row2 = 235;
            const int row3 = 278;
            var tSize = new Size(600, 30);

            tabIndex = InitTextBox(out _showNameTextBox, tabIndex, font, 105, 84, new Size(734, 45));
            tabIndex = InitTextBox(out _firstnameTextBox, tabIndex, font, lCol, row1, tSize);
            tabIndex = InitTextBox(out _nameTextBox, tabIndex, font, rCol, row1, tSize);
            tabIndex = InitTextBox(out _emailTextBox, tabIndex, font, lCol, row2, tSize);
            tabIndex = InitTextBox(out _phoneTextBox, tabIndex, font, rCol, row2, tSize);
            tabIndex = InitTextBox(out _zipCodeTextBox, tabIndex, font, rCol, row3, tSize);
            _zipCodeTextBox.MaxLength = 7;
            tabIndex = InitTextBox(out _cityTextBox, tabIndex, font, lCol, row3, tSize);
        }

        private int InitTextBox(out TextBox box, int tabIndex, Font font, int lCol, int row1, Size tSize)
        {
            box = new TextBox
            {
                Location = new Point(lCol, row1),
                Size = tSize,
                TabIndex = tabIndex++,
                Font = font
            };
            box.TextChanged += OnTextChanged;
            Controls.Add(box);
            box.BringToFront();
            return tabIndex;
        }
        /*
        protected override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }*/
        
        private void AddKeyboard()
        {
            AddKeyboardToControls(45,550,1580, 460);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (_checks.Any())
            {
                _checkBmp = GetNoneLanguageBitmap("checkmark.png");
            }
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkBmp != null)
        //    {
        //        foreach (var zone in _checks)
        //        {
        //            if (zone.AnswerState)
        //            {
        //                e.Graphics.DrawImageUnscaled(_checkBmp, zone.CheckLocation);
        //            }
        //        }
        //    }
        //}

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        FirstScreen();
        //    }
        //    else 
        //    {
        //        for (var i = 0; i < _checks.Count; i++)
        //        {
        //            var sco = _checks[i];
                
        //            if (!InArea(e.X, e.Y, sco.ClickZone)) continue;
        //            sco.AnswerState = !sco.AnswerState;
        //            SaveAnswerToQuestionCheck(sco.QuestionNo, sco.ChoiceNo, sco.AnswerState);
        //            Invalidate(sco.ClickZone);
        //            Refresh();
        //            break;
        //        }
        //    }
        //}
        /*
        private bool SetAnswersToQuestionnaire()
        {
            if (string.IsNullOrWhiteSpace(_showNameTextBox.Text))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ? 
                    @"You must enter name(s) for the picture." : 
                    @"Vous devez entrer un ou plusieurs noms pour l'image.", 
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveAnswerToQuestion(7, 1, _showNameTextBox.Text, 4);

            if (string.IsNullOrWhiteSpace(_firstnameTextBox.Text))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ?
                        @"You must enter your first name." :
                        @"Vous devez entrer votre prénom.",
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveAnswerToQuestion(8, 1, _firstnameTextBox.Text, 4);

            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ?
                        @"You must enter your name." :
                        @"Vous devez entrer votre nom.",
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveAnswerToQuestion(9, 1, _nameTextBox.Text, 4);

            if (!string.IsNullOrWhiteSpace(_emailTextBox.Text) && !StringUtil.IsValidEmail(_emailTextBox.Text))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ?
                        @"You must enter a valid email." :
                        @"Vous devez entrer un courriel valide.",
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveAnswerToQuestion(10, 1, _emailTextBox.Text, 4);

            if (!string.IsNullOrWhiteSpace(_phoneTextBox.Text) && !StringUtil.IsValidPhoneNumber(_phoneTextBox.Text))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ?
                        @"You must enter a valid mobile phone number." :
                        @"Vous devez entrer un numéro de téléphone portable valide.",
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveAnswerToQuestion(11, 1, _phoneTextBox.Text, 4);

            if (string.IsNullOrWhiteSpace(_cityTextBox.Text))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ?
                        @"You must enter a city." :
                        @"Vous devez entrer une ville.",
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveAnswerToQuestion(12, 1, _cityTextBox.Text, 4);

            if (string.IsNullOrWhiteSpace(_zipCodeTextBox.Text) || !StringUtil.IsValidCanadaPostalCode(_zipCodeTextBox.Text))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ?
                        @"You must enter a valid canadian zip code." :
                        @"Vous devez entrer un code postal canadien valide.",
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SaveAnswerToQuestion(13, 1, _zipCodeTextBox.Text, 4);

            if (!AllMandatoryChecks(_checks))
            {
                MsgBox.Show(this, _activeLg.Equals("en") ?
                        @"You must authorize the use of your picture for promotional purposes." :
                        @"Vous devez accepter que votre image soit utilisée à des fins promotionelles.",
                    Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }
        */
    }
}
