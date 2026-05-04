using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.OneThirdLab
{
    class OneThirdLab : AbstractPrintStationScreen
    {

        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private  TextBox _firstNameLastName;
        private  TextBox _courriel;
        private  TextBox _sms;



        private  IList<SectionSkipScreenData> _zones;

        public OneThirdLab(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            

            AddKeyboard();

            AddControls();

            _nextArea = new Rectangle(new Point(470, 540), new Size(341, 107));
            _closeArea = new Rectangle(new Point(1198, 1), new Size(80, 70));
        }


        private void AddKeyboard()
        {
            var key = new KeyboardControl(6, 190, 1268, 326)
            {
                Parent = PictureBox
            };

            key.BringToFront();
            key.Show();
        }

        private bool SetAnswersToQuestionnaire()
        {
            MessageDialog mBox;
            if (_firstNameLastName.Text.Length == 0)
            {
                mBox = new MessageDialog();
                if (!_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase))
                {
                    mBox.ShowMessage("Please enter your first and last name.");
                    return false;
                }
                
                    mBox.ShowMessage("Veuillez saisir votre prénom et votre nom de famille.");
                    return false;
                
                    
            }
            
            //SaveAnswerToQuestion(2, 1, _firstNameLastName.Text);
            

            // Validation email
            if (!string.IsNullOrWhiteSpace(_courriel.Text) && StringUtil.IsValidEmail(_courriel.Text))
            {
                //SaveAnswerToQuestion(3, 1, _courriel.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();

                if (!_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase))
                {
                    mBox.ShowMessage("You must enter a valid email.");
                    return false;
                }

                mBox.ShowMessage("Vous devez entrer une adresse courriel valide.");
                return false;
            }


            
            // Validation SMS
            if (!string.IsNullOrWhiteSpace(_sms.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_sms.Text))
                {
                    //SaveAnswerToQuestion(4, 1, _sms.Text.Trim());
                }
                else
                {
                    mBox = new MessageDialog();

                    if (!_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase))
                    {
                        mBox.ShowMessage("Phone number is invalid.");
                        return false;
                    }

                    mBox.ShowMessage("Téléphone invalide.");
                    return false;
                }
            }

            // On doit transformer l'image sélectionnée
            var baseDir = GetTerminalDirectory() + "overlay.png";

            using (Bitmap trans = FileUtil.LoadBitmap(GetSelectedPicture().Src), overlay = FileUtil.LoadBitmap(baseDir))
            {
                using (var res = trans.ResizeToSquare().Resize(1080, 1080))
                {
                    // res.AddOverlay(overlay);
                    // var photoFile = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                    //                     _screenConfig.QuestionnaireId,
                    //                     DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                    //                     DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) +
                    //                 GenerateUniqueId.GetId() + ".jpg";
                    // res.Save(ImageFormat.Jpeg, 90, photoFile);
                    //SaveAnswerToQuestion(1, 1, photoFile);
                }
            }

                

            return true;
        }

        private void AddControls()
        {
            _firstNameLastName = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(419, 33),
                Size = new Size(650, 47),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_firstNameLastName);
            _firstNameLastName.BringToFront();

            _courriel = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(419, 80),
                Size = new Size(650, 47),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_courriel);
            _courriel.BringToFront();

            _sms = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(419, 133),
                Size = new Size(650, 47),
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_sms);
            _sms.BringToFront();
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;


            if (InArea(x, y, _closeArea))
            {
                Close();
                return true;
            }

  
            if (InArea(x,y, _nextArea))
            {
                if (SetAnswersToQuestionnaire()) Next();
                return true;
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

      


        public override void Recycle()
        {
            base.Recycle();

            _firstNameLastName.Text = "";
            _courriel.Text = "";
            _sms.Text = "";
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameLastName.Focus();
        }
    }
}
