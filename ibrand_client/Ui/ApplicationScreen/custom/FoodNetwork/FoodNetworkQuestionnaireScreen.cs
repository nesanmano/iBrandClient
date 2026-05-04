using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.FoodNetwork
{
    class FoodNetworkQuestionnaireScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;
        private readonly DateTimePicker _datePicker = new DateTimePicker();

        private TextBox _fullNameTextbox;
        private TextBox _addresseTextbox;
        private TextBox _cityTextbox;
        private TextBox _provinceTextbox;
        private TextBox _postalCodeTextbox;
        private TextBox _emailTextbox;
        private TextBox _phoneTextbox;


        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark3.png");
        }

        public FoodNetworkQuestionnaireScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            //_nextArea = new Rectangle(585, 721, 113, 56);
            _cancelArea = new Rectangle(1470, 0, 207, 207);

            Load += OnLoad;
        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";

            if (CheckBoxZone != null)
            {
                foreach (var zone in CheckBoxZone)
                {
                    zone.Recycle();
                }
            }

            _datePicker.Value = DateTime.Today.AddHours(-1);

        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(167, 567, 1336, 455)
            {
                KeyPadding = 3

            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {        
            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(290, 179, 471, 48), new Font(FontFamily.GenericSansSerif, 14));

            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(290, 241, 466, 48), new Font(FontFamily.GenericSansSerif, 14));
        }

/*
        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _cancelArea))
            {
                FirstScreen();
            }
            else
            {
                if (SetAnswersToQuestionnaire())
                {
                    /*int tmp = 0;
                    foreach (var check in CheckBoxZone.Where(data => data.Mandatory == false))
                    {
                        if (check.AnswerState == false && (check.QuestionNo == 16 || check.QuestionNo == 17))
                        { tmp++; }

                    }
                    if (tmp > 1)
                    {
                        var mBox = new MessageDialog();
                        mBox.ShowMessage("You must select how you want to receive your video.");
                        return;
                    }#1#

                    if (ZoneList != null)
                    {
                        foreach (var data in ZoneList.Where(data => InArea(e.X, e.Y, data.Zone)))
                        {
                            if (!data.SkipValidation && !AllMandatoryChecks(CheckBoxZone))
                            {
                                var mBox = new MessageDialog();
                                mBox.ShowMessage(Resources.iBrand.NotAllMandatoryCheckEnabled);
                                return;
                            }
                            var steps = data.PageChangeOptions.Evaluate(GetCurrentRespondentWithAnswers());

                            if (steps == null)
                            {
                                log.Warn("No page change options evaluated to true. Please double check page change conditions. Using next page (1).");
                            }

                            Next(steps?.Step ?? 1);
                            return;
                        }
                    }

                    if (CheckBoxZone != null)
                    {

                        foreach (var zone in CheckBoxZone.Where(data => InArea(e.X, e.Y, data.ClickZone)))
                        {
                            //var zone = _zonesCheckBox[i];
                            if (!InArea(e.X, e.Y, zone.ClickZone)) continue;
                            zone.AnswerState = !zone.AnswerState;
                            SaveAnswerToQuestionCheck(zone.QuestionNo, zone.ChoiceNo, zone.AnswerState);
                            Refresh();
                            return;
                        }
                    }
                }
            }
        }
*/

        /*
        protected override void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (CheckBoxZone != null && _checkmark != null)
            {
                foreach (var zone in CheckBoxZone)
                {
                    if (zone.AnswerState)
                    {
                        e.Graphics.DrawImageUnscaled(_checkmark, zone.CheckLocation);
                    }
                }
            }
        }
        */

        /// <summary>
        /// Assigne ou va chercher un tableau de définition de zones.
        /// Cette valeur ne peut jamais être vide ou nulle, et dans cette situation on lui assignera une valeur 
        /// par défaut (Tout l'écran, va à la prochaine page) la prochaine fois qu'on demandera sa valeur.
        /// </summary>
        [PublicAPI]
        public IList<SectionSkipScreenData> ZoneList
        {
            set => _zones = value;
            get => _zones ?? (_zones = new List<SectionSkipScreenData>
                       {
                           new SectionSkipScreenData
                           {
                               Zone = new Rectangle(new Point(0, 0), new Size(PictureBox.Image.Width, PictureBox.Image.Height)),
                               PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                           }
                       }
                   );
        }

        private bool SetAnswersToQuestionnaire()
        {
            MessageDialog mBox;

            
            // Validation email
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _emailTextbox.Text.Trim());
            }
            /*else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage("Email Address Invalid.");
                return false;
            }*/

            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && StringUtil.IsValidPhoneNumber(_phoneTextbox.Text.Trim()))
            {
                
                    //SaveAnswerToQuestion(3, 1, _phoneTextbox.Text.Trim());
                

            }
            /*else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage("Phone number Invalid.");
                return false;
            }*/

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextbox.Focus();
        }

    }
}
