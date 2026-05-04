using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Carte_Hockey
{
    class CarteHockeyQuestionnaireScreen: AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _playerNumberTextbox;
        private TextBox _teamNameTextbox;



        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("check mark.png");
        }
        
        public CarteHockeyQuestionnaireScreen(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(469, 536, 329, 90);
            _cancelArea = new Rectangle(1180,3,99,80);

            Load += OnLoad;
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextbox.Text = "";
            _lastNameTextbox.Text = "";
            _playerNumberTextbox.Text = "";
            _teamNameTextbox.Text = "";
            
            if (CheckBoxZone == null) return;
            foreach (var zone in CheckBoxZone)
            {
                zone.Recycle();
            }
        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(29,230,1221,280)
            {
                
                
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {


            _firstNameTextbox = new TextBox();
            this.InitTextBox(_firstNameTextbox, new Rectangle(345, 32,589,39),new Font(FontFamily.GenericSansSerif,14));
     
            _lastNameTextbox = new TextBox();
            this.InitTextBox(_lastNameTextbox, new Rectangle(345, 74, 589, 39), new Font(FontFamily.GenericSansSerif, 14));

            _playerNumberTextbox = new TextBox();
            this.InitTextBox(_playerNumberTextbox, new Rectangle(345, 115, 589, 39), new Font(FontFamily.GenericSansSerif, 14));

            _teamNameTextbox = new TextBox();
            this.InitTextBox(_teamNameTextbox, new Rectangle(345, 155, 589, 39), new Font(FontFamily.GenericSansSerif, 14));
        }

        
        protected override bool OnMouseDownHandler(int x, int y)
        {
            //base.MouseDownHandler(sender, e);

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    /*int tmp = 0;
                    foreach (var check in CheckBoxZone.Where(data => data.Mandatory == false))
                    {
                        if (check.AnswerState == false && (check.QuestionNo == 16 || check.QuestionNo == 17))
                        {  tmp++;}               
                     
                    }
                    if (tmp > 1)
                    {
                        var mBox = new MessageDialog();
                        mBox.ShowMessage("You must select how you want to receive your video.");
                        return;
                    }               

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
                            Next(1);
                            return;
                        }
                    }*/
                    Next(1);
                }
                
                return false;
            }

            if (InArea(x, y, _cancelArea))
            {
                log.Trace("Cancel Area clicked");
                Close();
                return false;
            }

            return false;

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
            MessageDialog mBox;

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(3, 1, _firstNameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                if(GetQuestionnaireLanguage() == "English")
                    mBox.ShowMessage("First Name is mandatory.");
                else
                {
                    mBox.ShowMessage("Le prénom est requis.");
                }
                return false;
            }

            
            // Validation ville
            if (!string.IsNullOrWhiteSpace(_lastNameTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _lastNameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                if(GetQuestionnaireLanguage() == "English")
                    mBox.ShowMessage("Last Name is mandatory.");
                else
                {
                    mBox.ShowMessage("Nom de famille est requis.");
                }
                return false;
            }
           



            // Validation  address
            if (!string.IsNullOrWhiteSpace(_playerNumberTextbox.Text) && _playerNumberTextbox.Text.Trim().Length==2 )
            {
                try
                {
                    
                     int tmp = Int32.Parse(_playerNumberTextbox.Text);
                     //SaveAnswerToQuestion(2, 1,_playerNumberTextbox.Text);
                    
                }
                catch (Exception e)
                {
                     mBox = new MessageDialog();
                     if(GetQuestionnaireLanguage() == "English")
                        mBox.ShowMessage("Must be a valid player number 00 - 99.");
                     else
                     {
                         mBox.ShowMessage("Saisir un numéro de joueur valide 00 - 99.");
                     }
                     return false;
                }
                
            }
            else
            {
                mBox = new MessageDialog();
                if(GetQuestionnaireLanguage() == "English")
                    mBox.ShowMessage("Must be a valid player number 00 - 99.");
                else
                {
                    mBox.ShowMessage("Saisir un numéro de joueur valide 00 - 99.");
                }
                return false;
            }
           


            

            // Validation province
            if (!string.IsNullOrWhiteSpace(_teamNameTextbox.Text))
            {
                //SaveAnswerToQuestion(5, 1, _teamNameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                if(GetQuestionnaireLanguage() == "English")
                    mBox.ShowMessage("Team name is mandatory.");
                else
                {
                    mBox.ShowMessage("Nom d'équipe est requis.");
                }
                return false;
            }
            
            return true;
        }

        private string GetQuestionnaireLanguage()
        {
            var ans = new List<Answer>();
            GetCurrentQuestionnaire().GetQuestion(10).GetAnswers(ans);

            return ans[0].Response;
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
            _firstNameTextbox.Focus();
        }
    
    }
}
