using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;


namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Simple_Project
{
    class SimpleProjectQuestionnaire : AbstractScreen
    {

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly TextBox _smsTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _firstNameTextbox = new TextBox();
        private readonly TextBox _lastNameTextbox = new TextBox();


        private readonly Rectangle? _sms;
        private readonly Rectangle? _email;
        private readonly Rectangle? _firstName;
        private readonly Rectangle? _lastName;
        private readonly Rectangle? _keyboard;

        private readonly Rectangle _nextArea = new Rectangle(530, 600, 887, 96);

        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        private bool _printFinished;

        public bool Print { get; set; }

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        public SimpleProjectQuestionnaire(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc,Rectangle? sms,
            Rectangle? email, Rectangle? firstName,Rectangle? lastName,Rectangle? keyboard)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            _sms = sms;
            _email = email;
            _firstName = firstName;
            _lastName = lastName;
            _keyboard = keyboard;
            AddTextBoxs();
            AddKeyboard();
            
        }
        
        protected override void OnLoad()
        {
            _checkmark = GetNoneLanguageBitmap("check2.png");
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls<KeyboardControl>(_keyboard.GetValueOrDefault().X, _keyboard.GetValueOrDefault().Y, _keyboard.GetValueOrDefault().Width, _keyboard.GetValueOrDefault().Height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            if(_sms!= null)
                this.InitTextBox(_smsTextbox,_sms.GetValueOrDefault(), font, 2,4);
            if(_email!= null)
                this.InitTextBox(_emailTextbox,_email.GetValueOrDefault(), font, 3,3);
            if(_firstName!= null)
                this.InitTextBox(_smsTextbox,_firstName.GetValueOrDefault(), font, 4);
            if(_lastName!= null)
                this.InitTextBox(_emailTextbox,_lastName.GetValueOrDefault(), font, 5);
          

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

        public override void Recycle()
        {
            base.Recycle();

            _smsTextbox.Text = "";
            _emailTextbox.Text = "";
            _firstNameTextbox.Text = "";
            _lastNameTextbox.Text = "";

            if (CheckBoxZone == null) return;
            foreach (var zone in CheckBoxZone)
            {
                zone.Recycle();
            }

        }

        private bool SetAnswersToQuestionnaire()
        {
            MessageDialog mBox;

            // Validation email
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text) && StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _emailTextbox.Text.Trim());
            }
            /*else if(!GetCurrentQuestionnaire().GetQuestion(3).HasDefaultAnswer() && (string.IsNullOrWhiteSpace(_emailTextbox.Text) || !StringUtil.IsValidEmail(_emailTextbox.Text)))
            {
                mBox = new MessageDialog();
                if(GetCurrentQuestionnaire().Language.Equals("en"))
                {
                    
                    mBox.ShowMessage("Enter a valid email.");
                    return false;
                }
                mBox.ShowMessage("Entrez un courriel valide.");
                 return false;
            }*/


            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_smsTextbox.Text) && StringUtil.IsValidPhoneNumber(_smsTextbox.Text.Trim()))
            {

                //SaveAnswerToQuestion(2, 1, _smsTextbox.Text.Trim());


            }
            /*else if(!GetCurrentQuestionnaire().GetQuestion(4).HasDefaultAnswer() && ( string.IsNullOrWhiteSpace(_smsTextbox.Text) || ! StringUtil.IsValidPhoneNumber(_smsTextbox.Text.Trim())))
            {
                mBox = new MessageDialog();
                if(GetCurrentQuestionnaire().Language.Equals("fr"))
                {
                    mBox.ShowMessage("Entrez un numéro de téléphone valide.");
                    return false;
                }
                mBox.ShowMessage("Enter a valid phone number.");
                return false;
            }*/

            

            return true;

        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    
                    /*if(Print)
                    {
                        var t = new Thread(SendPrint);
                        t.SetApartmentState(ApartmentState.STA);
                        t.Start();
                    }*/
                    Next(1);

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
            
          

            if (_zones != null)
            {
                foreach (var data in _zones.Where(data => InArea(x, y, data.Zone)))
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

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextbox.Focus();
            //PreloadCheckBoxDefaultAnswers(CheckBoxZone);
            AddTextBoxs();
        }


        private void SendPrint()
        {
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                
                var database = ServicesContainer.GetService<IbrandDatabase>();

                //List<Answer> ans = new List<Answer>();
                //GetCurrentQuestionnaire().GetQuestion(2).GetAnswers(ans);

                //var picture = ans[0].Response;

                //database.AddPrintCount(picture.ServerPostId);

                //SaveAnswerToQuestion(13,1,new Answer(4,13,1,-1,_nbImpression.ToString()));

                string fileToPrint = "";

                /*if (_printFileName != null)
                {
                    fileToPrint = _printFileName;
                }*/
                if (_screenConfig.QuestionPictureToPrintNo.HasValue)
                {
                    fileToPrint = GetTextAnswerFromQuestion(_screenConfig.QuestionPictureToPrintNo.Value);
                }
                /*else
                {
                    fileToPrint = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                        _screenConfig.QuestionnaireId,
                        // On utilise le EventID et le Feature ID comme équivalence au projet/questionnaire. 
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                        DirectoryUtil.ProjectDataDirectoriesModEnum.print) + StringUtil.GetFileName(picture.Src);*/

                    /*if (!File.Exists(fileToPrint))
                    {
                        var result = PrintStationUtil.CreateFramedPhoto(picture.Src, picture, fileToPrint);
                        result.Save(95, fileToPrint);
                        result.Dispose();
                    }*/
                //}

                if (PrintUtil.Print(fileToPrint, null, this))
                {
                    if (_screenConfig.QuestionNumberOfPrintNo != null)
                    {
                        IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value, 1);
                    }

                    NewPrintEvent.GetInstance().NewItemEvent.Set();
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during print sequence.");
            }

            watch.Stop();
            _printFinished = true;
            if (watch.ElapsedMilliseconds < MINIMUM_PRINT_WAIT_TIME)
            {
                Thread.Sleep(MINIMUM_PRINT_WAIT_TIME - (int)watch.ElapsedMilliseconds);
            }
            //SetSocialBlock(false);
        }


        private string GetQuestionnaireLanguage()
        {
            var ans = new List<Answer>();
            GetCurrentQuestionnaire().GetQuestion(1).GetAnswers(ans);

            return ans[0].Response;
        }


    }
}
