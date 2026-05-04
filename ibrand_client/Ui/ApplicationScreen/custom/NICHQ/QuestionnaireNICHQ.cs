using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.NICHQ
{
    /// <summary>
    /// Questionnaire du projet NIC-2017-01
    /// </summary>
    public class QuestionnaireNICHQ : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _stateTextbox = new TextBox();
        private readonly TextBox _titleTextbox = new TextBox();
        private bool _checkState;
        private readonly Rectangle _checkArea = new Rectangle(65, 332, 1571, 95);
        private readonly Point _checkLocation = new Point(64, 318);
        private readonly Bitmap _checkBitmap;
        
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        /// <summary>
        /// Questionnaire principal
        /// </summary>
        /// <param name="backgroundImageSrc">Fond en anglais</param>
        /// <param name="sc">Paramètres de configuration.</param>
        public QuestionnaireNICHQ(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(734, 430, 228, 67);
            _cancelArea = new Rectangle(1611, 8, 59, 58);
            
            _checkBitmap = GetNoneLanguageBitmap("checkmark.png");
        }

        private void AddKeyboard()
        {
            const int width = 1600;
            const int height = 427;
            const int x = 80;
            const int y = 520;

            AddKeyboardToControls(x, y, width, height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            this.InitTextBox(_nameTextbox, new Rectangle(173, 181, 634, 52), font);
            this.InitTextBox(_emailTextbox, new Rectangle(173, 253, 634, 52), font);
            this.InitTextBox(_stateTextbox, new Rectangle(947, 181, 634, 52), font);
            this.InitTextBox(_titleTextbox, new Rectangle(1191, 253, 392, 52), font);
            _titleTextbox.MaxLength = 3;
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            _checkState = false;
            _emailTextbox.Text = "";
            _stateTextbox.Text = "";
            _titleTextbox.Text = "";
            _nameTextbox.Text = "";
        }

        ///// <inheritdoc />
        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation);
        //}

        private bool SetAnswersToQuestionnaire()
        {
            // Validation courriel
            if (!string.IsNullOrEmpty(_emailTextbox.Text) && StringUtil.IsValidEmail(_emailTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _emailTextbox.Text);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Invalid Email");
                return false;
            }

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _nameTextbox.Text);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Invalid name");
                return false;
            }
            
            // Validation check
            if (_checkState) { SaveAnswerToQuestionCheck(5, 1, _checkState); }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Please give your consent to continue.");
                return false;
            }

            // Validation state
            if (!string.IsNullOrWhiteSpace(_stateTextbox.Text))
            {
                //SaveAnswerToQuestion(3, 1, _stateTextbox.Text);
            }

            // Validation title
            if (!string.IsNullOrWhiteSpace(_titleTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _titleTextbox.Text);
            }

            return true;
        }

        ///// <inheritdoc />
        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        FirstScreen();
        //    }
        //    else if (InArea(e.X, e.Y, _checkArea))
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

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextbox.Focus();
        }
    }




}