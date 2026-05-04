using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.LoyaltyOne
{
    [PublicAPI]
    public class QuestionnaireScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        
        private readonly TextBox _firstNameTextbox = new TextBox();

        private ComboBox _departmentCombo;

        private readonly Rectangle _nextArea;

        public QuestionnaireScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(506, 849, 669, 114);
        }

        private void AddKeyboard()
        {
            const int WIDTH = 1564;
            const int HEIGHT = 312;
            const int X = 60;
            const int Y = 425;

            AddKeyboardToControls(X, Y, WIDTH, HEIGHT);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            this.InitTextBox(_firstNameTextbox, new Rectangle(465, 303, 400, 50), font, 1);
            this.InitTextBox(_nameTextbox, new Rectangle(465, 368, 400, 50), font, 2);

            _departmentCombo = new ComboBox
            {
                Location = new Point(1163, 303),
                Size = new Size(400, 50),
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _departmentCombo.Items.AddRange(new object[]
             {
                "AIR MILES"
                ,"Member Experience"
                ,"Talent and Workplace"
                 ,"Finance"
                 ,"Corporate Affairs and Legal"
                 ,"BT"
                 ,"Global Solutions"
                 ,"BrandLoyalty"
                 ,"Precima"
                 ,"Zero Gravity Labs"
                 ,"Other"
             });

            _departmentCombo.SelectedIndex = -1;
            _departmentCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            _departmentCombo.TextChanged += OnTextChanged;
            Controls.Add(_departmentCombo);
            _departmentCombo.BringToFront();
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextbox.Text = "";
            _nameTextbox.Text = "";
            _departmentCombo.SelectedIndex = -1;
        }

        private bool SetAnswersToQuestionnaire()
        {

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Invalid Last Name.");
                return false;
            }

            // Validation Prénom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _firstNameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Invalid First name.");
                return false;
            }

            if (_departmentCombo.SelectedIndex < 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must select a department.");
                return false;
            }
            else
            {
                //SaveAnswerToQuestion(3, 1, _departmentCombo.SelectedIndex + 1);
            }

            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _nextArea))
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
            _firstNameTextbox.Focus();
        }
    }
}
