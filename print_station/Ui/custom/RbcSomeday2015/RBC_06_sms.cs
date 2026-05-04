using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc06Sms : AbstractPrintStationScreen
    {
        readonly Rectangle _previousArea;
        
        private TextBox _smsTextBox;

        public Rbc06Sms(string backgroundImgEnSrc, PrintStationScreenConfig sc)
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

            _smsTextBox.Text = "";
        }


        private void AddKeyboard()
        {

            AddKeyboardToControlsNumeric(415,365,470, 380);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_smsTextBox.Text.Length <= 0) return true;

            if (StringUtil.IsValidPhoneNumber(_smsTextBox.Text))
            {
                //SaveAnswerToQuestion(8, 1, _smsTextBox.Text);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Invalid phone number.");
                return false;
            }

            return true;
        }

        private void AddTextBoxs()
        {
            _smsTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(194, 185),
                Size = new Size(955, 40),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _smsTextBox.TextChanged += OnTextChanged;

            Controls.Add(_smsTextBox);
            _smsTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (!InArea(e.X, e.Y, _previousArea)) return;

        //    if (SetAnswersToQuestionnaire())
        //    {
        //        Back(2);
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _smsTextBox.Focus();
        }
    }
}
