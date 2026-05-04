using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.ParcCanada
{
    public partial class EmailScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;

        private TextBox _emailTextBox;

        public EmailScreen(string backgroundImgEnSrc, string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //InitializeComponent();
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(1286, 330), new Size(127, 65));
            _backArea = new Rectangle(new Point(40, 980), new Size(86, 86));
        }

        public override void Recycle()
        {
            base.Recycle();
            _emailTextBox.Text = "";
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls((1920 - 1500) / 2, 1080 - 600, 1500, 500);
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            var question = questionnaire.GetQuestion(3);
            var choiceText = (ChoiceText)question.GetChoice(1);
//            var answer = new Answer(-1, question.No, choiceText.No);
//            answer.Response = _emailTextBox.Text;
//            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern())
            {
                if (_activeLg.Equals(Constants.FR))
                {
                    MessageBox.Show(this, "Le courriel est non-valide.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    MessageBox.Show(this, "Email is not valid.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                return false;
            }

            return true;
        }

        public void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point((1920 - 600) / 2, 330);
            _emailTextBox.Size = new Size(600, 40);
            _emailTextBox.TabIndex = 0;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 20);
            _emailTextBox.TextChanged += OnTextChanged;
            _emailTextBox.TextAlign = HorizontalAlignment.Center;

            Controls.Add(_emailTextBox);
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (_emailTextBox.Text != null && !_emailTextBox.Text.Equals(""))
        //        {
        //            if (!CanonCameraManager.GetInstance().IsCameraConnected())
        //            {
        //                MessageBox.Show(this, "Camera is not connected.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        //                return;
        //            }

        //            if (SetAnswersToQuestionnaire())
        //            {
        //                Next();
        //            }
        //        }

        //    }
        //    else if (InArea(e.X, e.Y, _backArea))
        //    {
        //        Back(); // Retour au debut.
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextBox.Text = "";
            _emailTextBox.BringToFront();
            _emailTextBox.Focus();
        }
    }
}
