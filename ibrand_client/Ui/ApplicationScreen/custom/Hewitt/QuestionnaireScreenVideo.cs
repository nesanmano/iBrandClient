using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Hewitt
{
    public class QuestionnaireScreenVideo : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        
        private readonly TextBox _firstNameTextbox = new TextBox();

        private readonly TextBox _cellPhoneTextbox = new TextBox();

        private readonly TextBox _emailTextbox = new TextBox();

        private readonly TextBox _companyTextbox = new TextBox();

        private readonly TextBox _cityTextbox = new TextBox();

        private bool _checkState;
        private readonly Rectangle _checkArea = new Rectangle(52, 378, 945, 35);
        private readonly Point _checkLocation = new Point(53, 380);

        private bool _checkState2;
        private readonly Rectangle _checkArea2 = new Rectangle(53, 542, 1167, 56);
        private readonly Point _checkLocation2 = new Point(53, 544);

        private ComboBox _yesNoComboBox;

        private ComboBox _datePicker;
        
        private readonly Bitmap _checkBitmap;

        private readonly Rectangle _nextArea = new Rectangle(543, 670, 258, 51);

        public QuestionnaireScreenVideo(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();

            //PictureBox.MouseClick += MouseDownHandler;
            
            _checkBitmap = GetNoneLanguageBitmap("checkmark.png");
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            this.InitTextBox(_nameTextbox, new Rectangle(300, 84, 582, 35), font, 1);
            this.InitTextBox(_firstNameTextbox, new Rectangle(300, 131, 582, 35), font, 2);
            this.InitTextBox(_cellPhoneTextbox, new Rectangle(300, 176, 582, 35), font, 3);
            this.InitTextBox(_emailTextbox, new Rectangle(300, 224, 582, 35), font, 4);
            this.InitTextBox(_companyTextbox, new Rectangle(300, 270, 582, 35), font, 5);
            this.InitTextBox(_cityTextbox, new Rectangle(300, 315, 582, 35), font, 6);

            _yesNoComboBox = new ComboBox
            {
                Location = new Point(615, 436),
                Size = new Size(197, 35),
                TabIndex = 7,
                Font = new Font(FontFamily.GenericSansSerif, 14),
            };
            _yesNoComboBox.Items.AddRange(new object[]
             {
                "Acheter"
                ,"Louer"
                ,"Non"
             });

            _yesNoComboBox.SelectedIndex = -1;

            _yesNoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _yesNoComboBox.TextChanged += OnTextChanged;
            //_yesNoComboBox.SelectedIndexChanged += OnSelectionMade;
            Controls.Add(_yesNoComboBox);
            _yesNoComboBox.BringToFront();

            ///////****

            _datePicker = new ComboBox
            {
                Location = new Point(292, 482),
                Size = new Size(494, 35),
                TabIndex = 8,
                Font = new Font(FontFamily.GenericSansSerif, 14),
            };
            _datePicker.Items.AddRange(new object[]
             {
                "0-3 mois"
                ,"6-12 mois"
                ,"12 mois et +"
             });

            _datePicker.SelectedIndex = -1;

            _datePicker.DropDownStyle = ComboBoxStyle.DropDownList;
            _datePicker.TextChanged += OnTextChanged;
            //_yesNoComboBox.SelectedIndexChanged += OnSelectionMade;
            Controls.Add(_datePicker);
            _datePicker.BringToFront();
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState = false;
            _checkState2 = false;
            _emailTextbox.Text = "";
            _firstNameTextbox.Text = "";
            _nameTextbox.Text = "";
            _cellPhoneTextbox.Text = "";
            _companyTextbox.Text = "";
            _cityTextbox.Text = "";
            _datePicker.SelectedIndex = -1;
            _yesNoComboBox.SelectedIndex = -1;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation);
        //    if (_checkState2) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation2);
        //}

        private bool SetAnswersToQuestionnaire()
        {

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Nom non valide.");
                return false;
            }

            // Validation Prénom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _firstNameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Prénom non valide.");
                return false;
            }

            // Validation téléphone cellulaire
            if (!string.IsNullOrWhiteSpace(_cellPhoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_cellPhoneTextbox.Text))
                {
                    //SaveAnswerToQuestion(3, 1, _cellPhoneTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Téléphone non valide.");
                    return false;
                }
            }
           

            // Validation courriel
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(4, 1, _emailTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Adresse courriel non valide.");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Adresse courriel non valide.");
                return false;
            }

            // Validation Compagnie
            if (!string.IsNullOrWhiteSpace(_companyTextbox.Text))
            {
                //SaveAnswerToQuestion(5, 1, _companyTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Compagnie non valide.");
                return false;
            }


            // Validation ville
            if (!string.IsNullOrWhiteSpace(_cityTextbox.Text))
            {
                //SaveAnswerToQuestion(6, 1, _cityTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Ville non valide.");
                return false;
            }

            SaveAnswerToQuestionCheck(7, 1, _checkState);
            SaveAnswerToQuestionCheck(14, 1, _checkState);

            // Validation Louer équipement
            if (_yesNoComboBox.SelectedIndex != -1)
            {
                //SaveAnswerToQuestion(8, 1, _yesNoComboBox.SelectedIndex + 1); // zero based, donc on augmente de un.

                if (_yesNoComboBox.SelectedIndex != 0) return true;

                //if (_datePicker.SelectedIndex != -1)
                //    SaveAnswerToQuestion(9, 1, _datePicker.SelectedIndex + 1);
                //else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Vous devez choisir un interval de date.");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Vous devez sélectionner si vous pensez acheter un équipement.");
                return false;
            }

            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _checkArea2))
        //    {
        //        _checkState2 = !_checkState2;
        //        Refresh();
        //    } else if (InArea(e.X, e.Y, _checkArea))
        //    {
        //        _checkState = !_checkState;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
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
