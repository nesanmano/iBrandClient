using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Clorox
{
    public class QuestionnaireScreenXmas : AbstractPrintStationScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea;

        private readonly Rectangle _pressNsealArea;
        private readonly Rectangle _clingWrapArea;
        private readonly Rectangle _foodBagsArea;
        private readonly Rectangle _foodContainersArea;
        private readonly Rectangle _wasteBagsArea;
        //private readonly Rectangle _privacyArea;
        //private readonly Rectangle _termsArea;

        private readonly Bitmap _checkmark;
        private TextBox _emailTextBox;
        private TextBox _nameTextBox;
        private TextBox _otherTextBox;

        private int _optInChecked;

        private int _pressNsealChecked;
        private int _clingWrapChecked;
        private int _foodBagsChecked;
        private int _foodContainerChecked;
        private int _wasteBagsChecked;

        public QuestionnaireScreenXmas(string backgroundImgEnSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInArea = new Rectangle(new Point(42, 296), new Size(500, 50));

            _pressNsealArea = new Rectangle(new Point(42, 199), new Size(167, 38));
            _clingWrapArea = new Rectangle(new Point(244, 199), new Size(148, 40));
            _foodBagsArea = new Rectangle(new Point(443, 199), new Size(154, 40));
            _foodContainersArea = new Rectangle(new Point(633, 199), new Size(207, 40));
            _wasteBagsArea = new Rectangle(new Point(880, 199), new Size(158, 40));

            //_privacyArea = new Rectangle(new Point(679, 288), new Size(101, 33));
            //_termsArea = new Rectangle(new Point(814, 288), new Size(160, 33));

            _nextArea = new Rectangle(new Point(569, 341), new Size(188, 68));
            _closeArea = new Rectangle(new Point(1180, 10), new Size(60, 60));
        }

        public override void Recycle()
        {
            base.Recycle();

            _nameTextBox.Text = "";
            _emailTextBox.Text = "";
            _otherTextBox.Text = "";

            _optInChecked = 0;

            _pressNsealChecked = 0;
            _clingWrapChecked = 0;
            _foodBagsChecked = 0;
            _foodContainerChecked = 0;
            _wasteBagsChecked = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(50, 305));
        //    }

        //    if (_pressNsealChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(50, 209));
        //    }

        //    if (_clingWrapChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(250, 209));
        //    }

        //    if (_foodBagsChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(450, 209));
        //    }

        //    if (_foodContainerChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(639, 209));
        //    }

        //    if (_wasteBagsChecked == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(885, 209));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1200) / 2, 436, 1200, 333);
        }

        private bool ValidateEmailAddress()
        {
            if (ValidateEmailAddress(3, _emailTextBox.Text)) return true;

            var mBox = new MessageDialog();
            mBox.ShowMessage(_activeLg.Equals("en") ? "Email address not valid." : "Le courriel n'est pas valide.");

            return false;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_nameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false; // Question 3
                }

                /*SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
                SaveAnswerToQuestion(2, 1, _nameTextBox.Text);

                SaveAnswerToQuestionCheck(4, 1, _pressNsealChecked != 0);
                SaveAnswerToQuestionCheck(4, 2, _clingWrapChecked != 0);
                SaveAnswerToQuestionCheck(4, 3, _foodBagsChecked != 0);
                SaveAnswerToQuestionCheck(4, 4, _foodContainerChecked != 0);
                SaveAnswerToQuestionCheck(4, 5, _wasteBagsChecked != 0);

                SaveAnswerToQuestionCheck(6, 1, _optInChecked != 0);

                if (_otherTextBox.Text.Length > 0)
                    SaveAnswerToQuestion(5, 1, _otherTextBox.Text);*/
                return true;
            }

            mBox.ShowMessage(_activeLg.Equals("en")
                ? "Please answer all questions."
                : "Vous devez répondre à toutes les questions.");
            return false;
        }

        private void AddTextBoxs()
        {
            _nameTextBox = new TextBox
            {
                Location = new Point(132, 89),
                Size = new Size(480, 35),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _nameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_nameTextBox);
            _nameTextBox.BringToFront();

            

            _emailTextBox = new TextBox
            {
                Location = new Point(719, 89),
                Size = new Size(480, 35),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _otherTextBox = new TextBox
            {
                Location = new Point(288, 247),
                Size = new Size(400, 35),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _otherTextBox.TextChanged += OnTextChanged;

            Controls.Add(_otherTextBox);
            _otherTextBox.BringToFront();

        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (InArea(e.X, e.Y, _closeArea))
        //    {
        //        Close();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea))
        //    {
        //        _optInChecked = _optInChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _pressNsealArea))
        //    {
        //        _pressNsealChecked = _pressNsealChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _clingWrapArea))
        //    {
        //        _clingWrapChecked = _clingWrapChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _foodBagsArea))
        //    {
        //        _foodBagsChecked = _foodBagsChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _foodContainersArea))
        //    {
        //        _foodContainerChecked = _foodContainerChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _wasteBagsArea))
        //    {
        //        _wasteBagsChecked = _wasteBagsChecked == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    /*else if (InArea(e.X, e.Y, _privacyArea))
        //    {
        //        SuspendTimer();
        //        var file = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screen_config.ProjectId, _screen_config.QuestionnaireId,
        //            DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + "policy.html";
        //        new WaiverHtmlDialog(file).ShowDialog();
        //        ResumeTimer();
        //    }
        //    else if (InArea(e.X, e.Y, _termsArea))
        //    {
        //        SuspendTimer();
        //        var file = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screen_config.ProjectId, _screen_config.QuestionnaireId,
        //            DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + "terms.html";
        //        new WaiverHtmlDialog(file).ShowDialog();
        //        ResumeTimer();
        //    }*/
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextBox.Focus();
        }
    }
}