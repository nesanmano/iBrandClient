using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public partial class EmailScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;

        private TextBox _emailTextBox;

        public EmailScreen(string backgroundImgSrc, ScreenConfig sc)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            //InitializeComponent();
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(782, 390), new Size(354, 100));
        }

        public override void Recycle()
        {
            base.Recycle();
            _emailTextBox.Text = "";
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls((1920 - 1500)/2, 1080 - 500, 1500, 500);
        }

        private bool SetAnswersToQuestionnaire()
        {
            var email = _emailTextBox.Text;

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            var question = questionnaire.GetQuestion(4);
            var choice = question.GetChoice(1);
//            var answer = new Answer(-1, question.No, choice.No) {Response = email};
//            choice.SetAnswer(answer);

            if (string.IsNullOrWhiteSpace(email)) return true;
            if (question.ValidateAnswersPattern()) return true;

            MessageBox.Show(this, @"Le courriel est non-valide.", @"iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            return false;
        }

        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _emailTextBox = new TextBox
            {
                Location = new Point(259, 269),
                Size = new Size(1389, 68),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 30)
            };
            _emailTextBox.TextChanged += OnTextChanged;
            _emailTextBox.TextAlign = HorizontalAlignment.Left;

            Controls.Add(_emailTextBox);
        }

        /*
        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (!InArea(e.X, e.Y, _nextArea)) return;
            
            if (SetAnswersToQuestionnaire())
            {
                Next();
            }
        }
        */

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextBox.Text = "";
            _emailTextBox.BringToFront();
            _emailTextBox.Focus();
        }
    }
}
