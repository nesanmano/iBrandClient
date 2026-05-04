using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Tangerine
{
        class TangerineQuestionnaireScreen3 : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;




        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }
        
        public TangerineQuestionnaireScreen3(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {

            // AddTextBoxs();
            //AddKeyboard();

            _nextArea = new Rectangle(1168, 339, 109, 109);
            _cancelArea = new Rectangle(1186,0,93,75);

            Load += OnLoad;
        }

        public override void Recycle()
        {
            base.Recycle();


            if (CheckBoxZone != null)
            {
                foreach (var zone in CheckBoxZone)
                {
                    zone.Recycle();
                }
            }

         

        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(29,300,1221,240)
            {
                KeyPadding = 1
                
            };
            AddKeyboardToControls(kb);
        }


        
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x,y, _cancelArea))
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
                    Next();
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
                                if(SetAnswersToQuestionnaire())
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

            /*var pictureInfo = GetSelectedPicture();
            SaveAnswerToQuestion(1, 1, pictureInfo.Src);

            MessageDialog mBox;*/

            // Validation nom
           /* if (!string.IsNullOrWhiteSpace(_nomTextBox.Text))
            {
                SaveAnswerToQuestion(7, 1, _nomTextBox.Text.Trim());
            }
            else
            {
                if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                {
                        mBox = new MessageDialog();
                        mBox.ShowMessage("Name is mandatory.");
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Nom est requis.");
                }
                
                return false;
            }*/

           
       
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
            }*/

             // Validation téléphone 
             /*if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
             {
                 if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text.Trim()))
                 {
                     SaveAnswerToQuestion(3, 1, _phoneTextbox.Text.Trim());
                 }
               
             }
             else if(string.IsNullOrWhiteSpace(_phoneTextbox.Text))
             {
                 if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("The Phone Number is mandatory.");
                 }
                 else
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("Le numéro de téléphone est requis.");
                 }
                
                 return false;
             }
             else
             {
                 
                 if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("Phone number is invalid.");
                 }
                 else
                 {
                     mBox = new MessageDialog();
                     mBox.ShowMessage("Le numéro de téléphone est invalide.");
                 }
                
                 return false;
             }*/


            // Validation email
            /*if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                SaveAnswerToQuestion(2, 1, _emailTextbox.Text.Trim());
            }
            else if(string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Email is mandatory.");
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Le courriel est requis.");
                }
                
                return false;
            }
            else
            {
                 
                if (_activeLg.Equals("en",StringComparison.CurrentCultureIgnoreCase))
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Email is invalid.");
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Le Courriel est invalide.");
                }
                
                return false;
            }*/



             //Validation postal code
             /*if (!string.IsNullOrWhiteSpace(_postalCodeTextbox.Text) && StringUtil.IsValidCanadaPostalCode(_postalCodeTextbox.Text))
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
            //_nomTextBox.Focus();
        }
    
    }
}
