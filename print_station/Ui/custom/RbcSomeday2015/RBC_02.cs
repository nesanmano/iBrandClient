using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc02 : AbstractPrintStationScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _previousArea;

        private TextBox _lastNameTextBox;
        private TextBox _nameTextBox;

        public Rbc02(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(new Point(650, 350), new Size(180, 65));
            _previousArea = new Rectangle(new Point(450, 350), new Size(180, 65));
        }

        public override void Recycle()
        {
            base.Recycle();
            
            _nameTextBox.Text = "";
            _lastNameTextBox.Text = "";
        }


        private void AddKeyboard()
        {
            AddKeyboardToControls(100,425,1160, 310);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_nameTextBox.Text.Length > 0 && _lastNameTextBox.Text.Length > 0)
            {

                //SaveAnswerToQuestion(3, 1, _nameTextBox.Text);
                //SaveAnswerToQuestion(4, 1, _lastNameTextBox.Text);

                return true;
            }
            var mBox = new MessageDialog();
            mBox.ShowMessage("Please answer all questions.");
            return false;
        }

        private void AddTextBoxs()
        {
            _nameTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(245, 190),
                Size = new Size(940, 40),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _nameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_nameTextBox);
            _nameTextBox.BringToFront();

            _lastNameTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(245, 255),
                Size = new Size(940, 40),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _previousArea))
        //    {
        //        Close();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextBox.Focus();
        }
    }
}
