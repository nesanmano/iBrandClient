using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Stoli
{
    public class QuestionnaireStoli : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _ageTextbox = new TextBox();
        private readonly TextBox _nameTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private ComboBox _poseComboBox;
        private bool _checkState1;
        private bool _checkState2;
        private readonly Rectangle _checkArea1 = new Rectangle(100, 385, 980, 77);
        private readonly Rectangle _checkArea2 = new Rectangle(100, 495, 980, 81);
        private readonly Point _checkLocation1 = new Point(115, 404);
        private readonly Point _checkLocation2 = new Point(115, 508);
        private readonly Bitmap _checkBitmap;

        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        public QuestionnaireStoli(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _cancelArea = new Rectangle(1608, 15, 61, 61);
            _nextArea = new Rectangle(1279, 492, 227, 77);

            _checkBitmap = GetNoneLanguageBitmap("check.png");
        }

        private void AddKeyboard()
        {
            const int width = 1624;
            const int height = 458;
            const int x = 30;
            const int y = 573;

            AddKeyboardToControls(x, y, width, height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 22);
            this.InitTextBox(_ageTextbox, new Rectangle(355, 61, 502, 45), font);

            this.InitTextBox(_nameTextbox, new Rectangle(382, 147, 502, 45), font);
            this.InitTextBox(_emailTextbox, new Rectangle(789, 230, 502, 45), font);

            _poseComboBox = new ComboBox
            {
                Location = new Point(984, 313),
                Size = new Size(405, 36),
                TabIndex = 4,
                Font = font
                ,
            };


            _poseComboBox.Items.AddRange(new object[]
             {
                "THE Eleganza Extravaganza"
                ,"THE Fierce Fashionista"
                ,"THE Pride Pose",
                "THE Rainbow Realness",
                "THE Vodka Voguer",
                "THE Werk n' Twerk",
                "None"
             });

            _poseComboBox.SelectedIndex = -1;

            _poseComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _poseComboBox.TextChanged += OnTextChanged;
            //_yesNoComboBox.SelectedIndexChanged += OnSelectionMade;
            Controls.Add(_poseComboBox);
            _poseComboBox.BringToFront();

           
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState1 = false;
            _checkState2 = false;
            _ageTextbox.Text = "";
            _emailTextbox.Text = "";
            _nameTextbox.Text = "";
            _poseComboBox.SelectedIndex = -1;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState1) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation1);

        //    if (_checkState2) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation2);
        //}

        private bool SetAnswersToQuestionnaire()
        {
            // Validation age
            if (!string.IsNullOrWhiteSpace(_ageTextbox.Text))
            {
                if (StringUtil.IsInteger(_ageTextbox.Text))
                {
                    //SaveAnswerToQuestion(6, 1, _ageTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Age non valide." : "Invalid Age");
                    return false;
                }
            }

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(7, 1, _nameTextbox.Text.Trim());
            }
            

            // Validation courriel
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(8, 1, _emailTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Adresse courriel non valide." : "Invalid email");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your email.");
                return false;
            }
            // Validation Pose
            if (_poseComboBox.SelectedIndex != -1)
            {
                //SaveAnswerToQuestion(9, 1, _poseComboBox.SelectedIndex + 1); // zero based, donc on augmente de un.
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must select a Stoli pose.");
                return false;
            }

            // Validation check
            SaveAnswerToQuestionCheck(10, 1, _checkState1);

            SaveAnswerToQuestionCheck(11, 1, _checkState2);


            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _checkArea1))
        //    {
        //        _checkState1 = !_checkState1;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _checkArea2))
        //    {
        //        _checkState2 = !_checkState2;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        FirstScreen();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _ageTextbox.Focus();
        }
    }
}
