using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Hewitt
{
    public class QuestionnaireScreen : AbstractScreen
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
        private readonly Rectangle _checkArea = new Rectangle(47, 514, 745, 70);
        private readonly Point _checkLocation = new Point(50, 518);

        private ComboBox _buyRentNoComboBox;

        private ComboBox _whenComboBox;

        private readonly Bitmap _checkBitmap;

        private readonly Rectangle _nextArea;

        public QuestionnaireScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(290, 708, 220, 61);

            _checkBitmap = GetNoneLanguageBitmap("check.png");
        }

        private void AddKeyboard()
        {
            const int width = 770;
            const int height = 487;
            const int x = 22;
            const int y = 767;

            AddKeyboardToControls(x, y, width, height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            this.InitTextBox(_nameTextbox, new Rectangle(118, 257, 601, 31), font);
            this.InitTextBox(_firstNameTextbox, new Rectangle(143, 299, 577, 31), font);
            this.InitTextBox(_cellPhoneTextbox, new Rectangle(250, 341, 470, 31), font);
            this.InitTextBox(_emailTextbox, new Rectangle(152, 383, 568, 31), font);
            this.InitTextBox(_companyTextbox, new Rectangle(175, 425, 545, 31), font);
            this.InitTextBox(_cityTextbox, new Rectangle(115, 467, 605, 31), font);

            _buyRentNoComboBox = new ComboBox
            {
                Location = new Point(515, 595),
                Size = new Size(105, 36),
                TabIndex = 6,
                Font = new Font(FontFamily.GenericSansSerif, 14)
                ,
            };
            _buyRentNoComboBox.Items.AddRange(new object[]
             {
                "Acheter"
                ,"Louer"
                ,"Non"
             });

            _buyRentNoComboBox.SelectedIndex = -1;

            _buyRentNoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _buyRentNoComboBox.TextChanged += OnTextChanged;
            //_yesNoComboBox.SelectedIndexChanged += OnSelectionMade;
            Controls.Add(_buyRentNoComboBox);
            _buyRentNoComboBox.BringToFront();

            _whenComboBox = new ComboBox
            {
                Location = new Point(248, 635),
                Size = new Size(200, 36),
                TabIndex = 6,
                Font = new Font(FontFamily.GenericSansSerif, 14)
                ,
            };
            _whenComboBox.Items.AddRange(new object[]
             {
                "0-3 Mois"
                ,"6-12 Mois"
                ,"12 mois et +"
             });

            _whenComboBox.SelectedIndex = -1;

            _whenComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _whenComboBox.TextChanged += OnTextChanged;
            //_yesNoComboBox.SelectedIndexChanged += OnSelectionMade;
            Controls.Add(_whenComboBox);
            _whenComboBox.BringToFront();
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState = false;
            _emailTextbox.Text = "";
            _firstNameTextbox.Text = "";
            _nameTextbox.Text = "";
            _cellPhoneTextbox.Text = "";
            _companyTextbox.Text = "";
            _cityTextbox.Text = "";
            _buyRentNoComboBox.SelectedIndex = -1;
            _whenComboBox.SelectedIndex = -1;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation);
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

            // Validation Louer équipement
            if (_buyRentNoComboBox.SelectedIndex != -1)
            {
                //SaveAnswerToQuestion(8, 1, _buyRentNoComboBox.SelectedIndex + 1); // zero based, donc on augmente de un.

                if (_buyRentNoComboBox.SelectedIndex != 2)
                {
                     //if (_whenComboBox.SelectedIndex != -1)
                        //SaveAnswerToQuestion(9, 1, _whenComboBox.SelectedIndex + 1); // zero based, donc on augmente de un.
                    //else
                        {
                        var mBox = new MessageDialog();
                        mBox.ShowMessage("Vous devez choisir quand vous pensez acheter ou louer.");
                        return false;
                        }
                }
               
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Vous devez sélectionner si vous pensez acheter ou louer un équipement.");
                return false;
            }

            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _checkArea))
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
