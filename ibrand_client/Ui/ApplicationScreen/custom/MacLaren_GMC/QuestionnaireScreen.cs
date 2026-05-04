using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.MacLaren_GMC
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;

        private TextBox _emailTextBox1;
        private TextBox _phoneTextBox1;
        private TextBox _emailTextBox2;
        private TextBox _phoneTextBox2;
        private TextBox _emailTextBox3;
        private TextBox _phoneTextBox3;


        public QuestionnaireScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();


            _nextArea = new Rectangle(new Point(544, 480), new Size(130, 82));

        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextBox1.Text = "";
            _phoneTextBox1.Text = "";
            _emailTextBox2.Text = "";
            _phoneTextBox2.Text = "";
            _emailTextBox3.Text = "";
            _phoneTextBox3.Text = "";

        }


        private void AddTextBoxs()
        {
            _emailTextBox1 = new TextBox();
            _emailTextBox1.Location = new Point(234, 75);
            _emailTextBox1.Size = new Size(550, 50);
            _emailTextBox1.TabIndex = 0;
            _emailTextBox1.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox1.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox1);
            _emailTextBox1.BringToFront();

            _phoneTextBox1 = new TextBox();
            _phoneTextBox1.Location = new Point(925, 75);
            _phoneTextBox1.Size = new Size(270, 50);
            _phoneTextBox1.TabIndex = 1;
            _phoneTextBox1.Font = new Font(FontFamily.GenericSansSerif, 23);
            _phoneTextBox1.TextChanged += OnTextChanged;

            Controls.Add(_phoneTextBox1);
            _phoneTextBox1.BringToFront();

            _emailTextBox2 = new TextBox();
            _emailTextBox2.Location = new Point(234, 150);
            _emailTextBox2.Size = new Size(550, 50);
            _emailTextBox2.TabIndex = 2;
            _emailTextBox2.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox2.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox2);
            _emailTextBox2.BringToFront();

            _phoneTextBox2 = new TextBox();
            _phoneTextBox2.Location = new Point(926, 150);
            _phoneTextBox2.Size = new Size(270, 50);
            _phoneTextBox2.TabIndex = 3;
            _phoneTextBox2.Font = new Font(FontFamily.GenericSansSerif, 23);
            _phoneTextBox2.TextChanged += OnTextChanged;

            Controls.Add(_phoneTextBox2);
            _phoneTextBox2.BringToFront();

            _emailTextBox3 = new TextBox();
            _emailTextBox3.Location = new Point(234, 241);
            _emailTextBox3.Size = new Size(550, 50);
            _emailTextBox3.TabIndex = 4;
            _emailTextBox3.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox3.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox3);
            _emailTextBox3.BringToFront();

            _phoneTextBox3 = new TextBox();
            _phoneTextBox3.Location = new Point(926, 241);
            _phoneTextBox3.Size = new Size(270, 50);
            _phoneTextBox3.TabIndex = 5;
            _phoneTextBox3.Font = new Font(FontFamily.GenericSansSerif, 23);
            _phoneTextBox3.TextChanged += OnTextChanged;

            Controls.Add(_phoneTextBox3);
            _phoneTextBox3.BringToFront();

        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
           
           
        //}

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_emailTextBox1.Text.Length > 0 || _phoneTextBox1.Text.Length > 0)
            {
               
                if (_emailTextBox1.Text.Length > 0  && !StringUtil.IsValidEmail(_emailTextBox1.Text))
                {
                    mBox.ShowMessage("Email address 1 not valid.");
                    return false;
                }

                if (_phoneTextBox1.Text.Length > 0 && !StringUtil.IsValidPhoneNumber(_phoneTextBox1.Text))
                {
                    mBox.ShowMessage("Phone number 1 not valid.");
                    return false;
                }

                /*if (_emailTextBox1.Text.Length > 0)
                    SaveAnswerToQuestion(6, 1, _emailTextBox1.Text);

                if (_phoneTextBox1.Text.Length > 0)
                    SaveAnswerToQuestion(7, 1, _phoneTextBox1.Text);*/


                if (_emailTextBox2.Text.Length > 0 && !StringUtil.IsValidEmail(_emailTextBox2.Text))
                {
                    mBox.ShowMessage("Email address 2 not valid.");
                    return false;
                }

                if (_phoneTextBox2.Text.Length > 0 && !StringUtil.IsValidPhoneNumber(_phoneTextBox2.Text))
                {
                    mBox.ShowMessage("Phone number 2 not valid.");
                    return false;
                }

//                if (_emailTextBox2.Text.Length > 0)
//                    SaveAnswerToQuestion(8, 1, _emailTextBox2.Text);
//
//                if (_phoneTextBox2.Text.Length > 0)
//                    SaveAnswerToQuestion(9, 1, _phoneTextBox2.Text);

                //
                if (_emailTextBox3.Text.Length > 0 && !StringUtil.IsValidEmail(_emailTextBox3.Text))
                {
                    mBox.ShowMessage("Email address 3 not valid.");
                    return false;
                }

                if (_phoneTextBox3.Text.Length > 0 && !StringUtil.IsValidPhoneNumber(_phoneTextBox3.Text))
                {
                    mBox.ShowMessage("Phone number 3 not valid.");
                    return false;
                }

//                if (_emailTextBox3.Text.Length > 0)
//                    SaveAnswerToQuestion(10, 1, _emailTextBox3.Text);
//
//                if (_phoneTextBox3.Text.Length > 0)
//                    SaveAnswerToQuestion(11, 1, _phoneTextBox3.Text);

                return true;
            }

            mBox.ShowMessage("You must enter at least one email address or phone number.");

            return false;
        }
    }
}
