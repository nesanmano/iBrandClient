using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Maizex
{
    class MaizexEliteQuestionnaire : AbstractPrintStationScreen
    {

        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private TextBox _firstName;
        private TextBox _lastName;
        private TextBox _firmeEnterpise;
        private TextBox _codePostal;
        private TextBox _coorperative;
        private TextBox _courriel;
        private TextBox _sms;



        private IList<SectionSkipScreenData> _zones;

        public MaizexEliteQuestionnaire(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            AddKeyboard();

            AddControls();

            _nextArea = new Rectangle(new Point(507, 576), new Size(265, 65));
            _closeArea = new Rectangle(new Point(1197, 5), new Size(80, 51));
        }


        private void AddKeyboard()
        {
            var key = new KeyboardControl(6, 315, 1261, 262)
            {
                Parent = PictureBox
            };

            key.BringToFront();
            key.Show();
        }

        private bool SetAnswersToQuestionnaire()
        {
            MessageDialog mBox;



            //SaveAnswerToQuestion(10, 1, GetSelectedPicture().Src);

            if (_lastName.Text.Length == 0)
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Veuillez saisir votre nom de famille.");
                return false;


            }

            //SaveAnswerToQuestion(1, 1, _firstName.Text);


            if (_firstName.Text.Length == 0)
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Veuillez saisir votre prénom.");
                return false;


            }

            //SaveAnswerToQuestion(2, 1, _firstName.Text);

            if (_firmeEnterpise.Text.Length == 0)
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Veuillez saisir votre ferme ou entreprise.");
                return false;


            }

            //SaveAnswerToQuestion(3, 1, _firmeEnterpise.Text);



            //SaveAnswerToQuestion(5, 1, _coorperative.Text);





            if (_codePostal.Text.Length == 0)
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Veuillez saisir votre code postal.");
                return false;


            }

            //SaveAnswerToQuestion(4, 1, _codePostal.Text);


            // Validation email
            if (!string.IsNullOrWhiteSpace(_courriel.Text) && StringUtil.IsValidEmail(_courriel.Text))
            {
                //SaveAnswerToQuestion(7, 1, _courriel.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();

  
                mBox.ShowMessage("Veuillez saisir une adresse courriel valide.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_sms.Text) && StringUtil.IsValidPhoneNumber(_sms.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_sms.Text.Trim()))
                {
                    //SaveAnswerToQuestion(6, 1, _sms.Text.Trim());
                }

            }
            if(!string.IsNullOrWhiteSpace(_sms.Text) &&  !StringUtil.IsValidPhoneNumber(_sms.Text))
            {
                mBox = new MessageDialog();
                mBox.ShowMessage("Entrer un numéro mobile valide.");
                return false;
            }



            // On doit transformer l'image sélectionnée
            /*var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId) + "overlay.png";

            using (Bitmap trans = FileUtil.LoadBitmap(GetSelectedPicture().Src), overlay = FileUtil.LoadBitmap(baseDir))
            {
                using (var res = trans.ResizeToSquare().Resize(1080, 1080))
                {
                    res.AddOverlay(overlay);
                    var photoFile = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                                        _screenConfig.QuestionnaireId,
                                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                        DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) +
                                    GenerateUniqueId.GetId() + ".jpg";
                    res.Save(ImageFormat.Jpeg, 90, photoFile);
                    SaveAnswerToQuestion(1, 1, photoFile);
                }
            }*/



            return true;
        }

        private void AddControls()
        {

            _lastName = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(354, 3),
                Size = new Size(678, 36),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_lastName);
            _lastName.BringToFront();


            _firstName = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(354, 47),
                Size = new Size(678, 36),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_firstName);
            _firstName.BringToFront();


            

            _firmeEnterpise = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(354, 90),
                Size = new Size(678, 36),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_firmeEnterpise);
            _firmeEnterpise.BringToFront();


            _codePostal = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(354, 133),
                Size = new Size(678, 36),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_codePostal);
            _codePostal.BringToFront();

            _coorperative = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(354, 175),
                Size = new Size(678, 36),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_coorperative);
            _coorperative.BringToFront();

            _courriel = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(354, 261),
                Size = new Size(678, 36),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_courriel);
            _courriel.BringToFront();

            _sms = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(354, 218),
                Size = new Size(678, 36),
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


            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire()) Next();
                return true;
            }

            return true;
        }

      



        public override void Recycle()
        {
            base.Recycle();

            _firstName.Text = "";
            _courriel.Text = "";
            _sms.Text = "";
            _lastName.Text = "";
            _codePostal.Text = "";
            _coorperative.Text = "";
            _firmeEnterpise.Text = "";
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstName.Focus();

        }
    }
}
