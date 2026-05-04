using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc06Mail : AbstractPrintStationScreen
    {
        readonly Rectangle _previousArea;
        
        private TextBox _mailTextBox;

        public Rbc06Mail(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;
            
            _previousArea = new Rectangle(new Point(543, 287), new Size(186, 76));
        }

        public override void Recycle()
        {
            base.Recycle();

            _mailTextBox.Text = "";
        }


        private void AddKeyboard()
        {
            AddKeyboardToControls(100, 385, 1160, 350);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_mailTextBox.Text.Length <= 0) return true;

            if (StringUtil.IsValidEmail(_mailTextBox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _mailTextBox.Text);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Invalid email address.");
                return false;
            }

            return true;
        }

        private void AddTextBoxs()
        {
            _mailTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(190, 185),
                Size = new Size(955, 40),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _mailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_mailTextBox);
            _mailTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (!InArea(e.X, e.Y, _previousArea)) return;

        //    if (SetAnswersToQuestionnaire())
        //    {
        //        Back(1);
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _mailTextBox.Focus();
        }
    }
}
