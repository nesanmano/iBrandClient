using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.PublicisStarTrek
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _emailTextbox;
        private TextBox _postalCodeTextbox;
        private TextBox _phoneTextbox;
        private bool _checkOptIn;
        private readonly Rectangle _checkZone;
        private ComboBox _ageCombo;
        private readonly Bitmap _checkBmp;
        private readonly Point _checkPos;

        public QuestionnaireScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(new Point(358, 852), new Size(182, 62));
            _checkZone = new Rectangle(66, 664, 766, 112);
            _checkPos = new Point(57, 648);

            var filter = new BitmapModificationFilter(new [] { new ColorSubstitutionFilter(10, Color.Black, Color.Crimson) }, null);

            using (var tmpBmp = GetNoneLanguageBitmap("checkmark.png"))
            {
                _checkBmp = ImageUtil.ModifyPicture(tmpBmp, ColorSubstitutionFilterGroup.Icon, filter);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextbox.Text = "";
            _lastNameTextbox.Text = "";
            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";
            _postalCodeTextbox.Text = "";
            _ageCombo.SelectedIndex = -1;
            _checkOptIn = false;
        }
        
        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 834) / 2, 952, 834, 466);
            
        }

        private void AddTextBoxs()
        {
            var boxSize = new Size(507, 36);
            const int xPos = 320;

            _firstNameTextbox = new TextBox
            {
                Location = new Point(xPos, 424),
                Size = boxSize,
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _firstNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_firstNameTextbox);
            _firstNameTextbox.BringToFront();

            _lastNameTextbox = new TextBox
            {
                Location = new Point(xPos, 472),
                Size = boxSize,
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _lastNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_lastNameTextbox);
            _lastNameTextbox.BringToFront();

            _emailTextbox = new TextBox
            {
                Location = new Point(xPos, 567),
                Size = boxSize,
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _emailTextbox.TextChanged += OnTextChanged;
            Controls.Add(_emailTextbox);
            _emailTextbox.BringToFront();

            _postalCodeTextbox = new TextBox
            {
                Location = new Point(xPos, 518),
                Size = boxSize,
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _postalCodeTextbox.TextChanged += OnTextChanged;
            Controls.Add(_postalCodeTextbox);
            _postalCodeTextbox.BringToFront();

            _phoneTextbox = new TextBox
            {
                Location = new Point(xPos, 615),
                Size = boxSize,
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _phoneTextbox.TextChanged += OnTextChanged;
            Controls.Add(_phoneTextbox);
            _phoneTextbox.BringToFront();

            _ageCombo = new ComboBox
            {
                Location = new Point(xPos, 311),
                Size = boxSize,
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 14)
                ,
            };
            if (_activeLg.Equals(Constants.EN))
            {
                SetComboBoxEnglish();
            }
            else
            {
                SetComboBoxFrench();
            }
            _ageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            _ageCombo.TextChanged += OnTextChanged;
            _ageCombo.SelectedIndexChanged += OnSelectionMade;
            Controls.Add(_ageCombo);
            _ageCombo.BringToFront();
        }

        private void OnSelectionMade(object sender, EventArgs e)
        {
            if (_ageCombo.SelectedIndex == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                    ? "Please ask your parent or guardian to accompany you to participate in the Canada Post transporter experience."
                    : "Veuillez demander à un parent ou à un tuteur légal de vous accompagner pour participer à l’« Expérience téléporteur » de Postes Canada.");
                _phoneTextbox.Enabled = false;
                _lastNameTextbox.Enabled = false;
                _firstNameTextbox.Enabled = false;
                _postalCodeTextbox.Enabled = false;
                _emailTextbox.Enabled = false;
                _checkOptIn = false;
                Refresh();
            }
            else
            {
                _phoneTextbox.Enabled = true;
                _lastNameTextbox.Enabled = true;
                _firstNameTextbox.Enabled = true;
                _postalCodeTextbox.Enabled = true;
                _emailTextbox.Enabled = true;
                Refresh();
            }
        }

        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            if (language.Equals(Constants.EN))
            {
                SetComboBoxEnglish();
            }
            else
            {
                SetComboBoxFrench();
            }
            //pictureBox.Invalidate();
            return true;
        }

        private void SetComboBoxEnglish()
        {
            _ageCombo.Items.Clear();
            _ageCombo.Items.AddRange(new object[]
            {
                "Under 13"
                ,"13 – 18"
                ,"19+"
            });
            _ageCombo.SelectedIndex = -1;
        }

        private void SetComboBoxFrench()
        {
            _ageCombo.Items.Clear();
            _ageCombo.Items.AddRange(new object[]
            {
                "Moins de 13 ans"
                ,"De 13 à 18 ans"
                ,"19 ans ou plus"
            });
            _ageCombo.SelectedIndex = -1;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea) && _ageCombo.SelectedIndex != 0)
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _checkZone))
        //    {
        //        var choice = _ageCombo.SelectedIndex;
        //        switch (choice)
        //        {
        //            case 0:
        //                return;
        //            case 1:
        //                var mBox = new MessageDialog();
        //                mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
        //                    ? "You must be adult to conset to receive sollicitation."
        //                    : "Vous devez être adulte pour consentir à recevoir de la sollicitation.");
        //                break;
        //            default:
        //                _checkOptIn = !_checkOptIn;
        //                Refresh();
        //                break;
        //        }
        //    }
        //}

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkOptIn)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkBmp, _checkPos);
        //    }
        //}

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_emailTextbox.Text.Length > 0 && _firstNameTextbox.Text.Length > 0 && !(_ageCombo.SelectedIndex < 0))
            {

                if (!StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ? "Invalid email." : "Courriel Invalide");
                    return false;
                }

                if (_postalCodeTextbox.Text.Length > 0 && !StringUtil.IsValidCanadaPostalCode(_postalCodeTextbox.Text))
                {
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "Invalid postal code"
                        : "Code postal invalide");
                    return false;
                }

                if (_phoneTextbox.Text.Length > 0 && !StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "Invalid phone number"
                        : "Téléphone invalide");
                    return false;
                }

//                /* Questions obligatoires */
//                SaveAnswerToQuestion(6, 1, _firstNameTextbox.Text);
//                SaveAnswerToQuestion(9, 1, _emailTextbox.Text);
//                SaveAnswerToQuestion(5, 1, _ageCombo.SelectedIndex + 1); // zero based, donc on augmente de un.
//
//                /* Question facultatives */
//                SaveAnswerToQuestion(8, 1, _postalCodeTextbox.Text);
//                SaveAnswerToQuestion(7, 1, _lastNameTextbox.Text);
//                SaveAnswerToQuestion(10, 1, _phoneTextbox.Text);
//                SaveAnswerToQuestionCheck(11, 1, _checkOptIn);

                return true;
            }

            mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ? "You must answer all mandatory questions." : "Vous devez répondre à toutes les questions obligatoires.");

            return false;
        }
    }
}
