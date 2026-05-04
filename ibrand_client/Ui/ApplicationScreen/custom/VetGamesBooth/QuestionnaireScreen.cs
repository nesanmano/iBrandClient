using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.VetGamesBooth
{
    public class QuestionnaireScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        private TextBox _emailTextBox;
        private TextBox _nameTextBox;
        private TextBox _clinicTextBox;

        public QuestionnaireScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(new Point(210, 1025), new Size(200, 65));
        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextBox.Text = "";
            _nameTextBox.Text = "";
            _clinicTextBox.Text = "";
        }


        private void AddKeyboard()
        {
            AddKeyboardToControls(0, 1320, Width, 600);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_nameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0 && _clinicTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress(_screenConfig.QuestionEmailNo.Value, _emailTextBox.Text))
                {
                    return false;
                }

                //SaveAnswerToQuestion(2, 1, _nameTextBox.Text);
                //SaveAnswerToQuestion(3, 1, _clinicTextBox.Text);

                return true;
            }
            var mBox = new MessageDialog();
            mBox.ShowMessage("Please answer all questions.");
            return false;
        }

        public void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _emailTextBox = new TextBox();
            _emailTextBox.BorderStyle = BorderStyle.None;
            _emailTextBox.Location = new Point(420, 915);
            _emailTextBox.Size = new Size(500, 40);
            _emailTextBox.TabIndex = 0;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 24);
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _nameTextBox = new TextBox();
            _nameTextBox.BorderStyle = BorderStyle.None;
            _nameTextBox.Location = new Point(420, 615);
            _nameTextBox.Size = new Size(500, 40);
            _nameTextBox.TabIndex = 1;
            _nameTextBox.Font = new Font(FontFamily.GenericSansSerif, 24);
            _nameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_nameTextBox);
            _nameTextBox.BringToFront();

            _clinicTextBox = new TextBox();
            _clinicTextBox.BorderStyle = BorderStyle.None;
            _clinicTextBox.Location = new Point(420, 765);
            _clinicTextBox.Size = new Size(500, 40);
            _clinicTextBox.TabIndex = 2;
            _clinicTextBox.Font = new Font(FontFamily.GenericSansSerif, 24);
            _clinicTextBox.TextChanged += OnTextChanged;

            Controls.Add(_clinicTextBox);
            _clinicTextBox.BringToFront();
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
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextBox.Focus();
        }
    }
}
