using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.BNC
{
    class BNCQuestionnaireScreen : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        private TextBox _phoneTextbox;


        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            AddTextBoxs();
            AddKeyboard();
        }

        public BNCQuestionnaireScreen(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {

            _nextArea = new Rectangle(626, 896, 411, 105);
            _cancelArea = new Rectangle(1536, 1, 138, 138);

            Load += OnLoad;
        }

        public override void Recycle()
        {
            base.Recycle();

            _phoneTextbox.Text = "";

            if (CheckBoxZone == null) return;
            foreach (var zone in CheckBoxZone)
            {
                zone.Recycle();
            }
        }

        private void AddKeyboard()
        {
            var kb = new NumbersKeyboardControl(578, 365, 517, 514)
            {
                KeyPadding = 1
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {

            _phoneTextbox = new TextBox();

            this.InitTextBox(out _phoneTextbox, new Rectangle(488,294,926,50), new Font(FontFamily.GenericSansSerif, 18));
        }


        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _cancelArea))
            {
                Close();
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                    return true;
                }
                return false;
            }

            if (CheckBoxZone != null)
            {

                foreach (var zone in CheckBoxZone.Where(data => InArea(x, y, data.Position)))
                {
                    //var zone = _zonesCheckBox[i];
                    if (!InArea(x, y, zone.Position)) continue;
                    zone.Toggle();
                    zone.SaveToAllModels(GetCurrentQuestionnaire(), zone.AnswerState);
                    Refresh();
                    return true;
                }
            }



            if (ZoneList != null)
            {
                foreach (var data in ZoneList.Where(data => InArea(x, y, data.Zone)))
                {
                    if (!data.SkipValidation && !AllMandatoryChecks(CheckBoxZone))
                    {
                        var mBox = new MessageDialog();
                        mBox.ShowMessage("Mandatory check boxes are not ticked");
                        return true;
                    }

                    if (data.PageSkip == 2)
                    {
                        if (SetAnswersToQuestionnaire())
                            Next(data.PageSkip);

                    }
                    else
                    {
                        Next(data.PageSkip);
                    }

                    return true;
                }
            }

            return true;

        }

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

            var pictureInfo = GetSelectedPicture();
            //SaveAnswerToQuestion(1, 1, pictureInfo.Src);

            MessageDialog mBox;


            // Validation  address
            /* if (!string.IsNullOrWhiteSpace(_prenomTextBox.Text))
             {
                 SaveAnswerToQuestion(8, 1, _prenomTextBox.Text.Trim());
             }
             else
             {
                 if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("First name is mandatory.");
                 }
                 else
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("Prénom est requis.");
                 }

                 return false;
             }
             */
            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text.Trim()))
                {
                    //SaveAnswerToQuestion(3, 1, _phoneTextbox.Text.Trim());
                }
            }
            else if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && !StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
            {
                mBox = new MessageDialog();
                mBox.ShowMessage("Le numéro de téléphone est invalide.");

                return false;
            }     
            else if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && !StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
            {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Le numéro de téléphone est invalide.");

                    return false;
            }


            //Validation postal code
            /* if (!string.IsNullOrWhiteSpace(_postalCodeTextbox.Text) && StringUtil.IsValidCanadaPostalCode(_postalCodeTextbox.Text))
            {
                SaveAnswerToQuestion(9, 1, _postalCodeTextbox.Text.Trim());
            }
            else if(string.IsNullOrWhiteSpace(_postalCodeTextbox.Text))
            {
                if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Postal code is mandatory.");
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Le code postal est requis.");
                }
                
                return false;
            }
             else
             {
                 
                 if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("Postal code is invalid.");
                 }
                 else
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("Le code postal est invalide.");
                 }
                
                 return false;
             }*/





            return true;
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            base.OnPaintEvent(g);

            PaintCheckBoxList(g, CheckBoxZone, _checkmark);

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
            _phoneTextbox.Focus();
        }

    }
}
