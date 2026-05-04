using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Clorox
{
    public class QuestionnaireScreen2 : AbstractPrintStationScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _optInArea2;
        private readonly Rectangle _optInArea3;
        private readonly Rectangle _optInArea4;
        private readonly Rectangle _privacyArea;
        private readonly Rectangle _termsArea;

        private readonly Bitmap _checkmark;
        private KeyboardControl _keyboard;
        private TextBox _emailTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;

        private int _optInChecked1;
        private int _optInChecked2;
        private int _optInChecked3;
        private int _optInChecked4;

        public QuestionnaireScreen2(string backgroundImgEnSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInArea1 = new Rectangle(new Point(30, 114), new Size(1222, 50));
            _optInArea2 = new Rectangle(new Point(30, 175), new Size(984, 33));
            _optInArea3 = new Rectangle(new Point(30, 214), new Size(977, 33));
            _optInArea4 = new Rectangle(new Point(30, 254), new Size(1048, 35));

            _privacyArea = new Rectangle(new Point(679, 288), new Size(101, 33));
            _termsArea = new Rectangle(new Point(814, 288), new Size(160, 33));

            _nextArea = new Rectangle(new Point(569, 344), new Size(180, 60));
            _closeArea = new Rectangle(new Point(1180, 10), new Size(60, 60));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _emailTextBox.Text = "";
            _lastNameTextBox.Text = "";


            _optInChecked1 = 0;
            _optInChecked2 = 0;
            _optInChecked3 = 0;
            _optInChecked4 = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked1 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(32, 119));
        //    }

        //    if (_optInChecked2 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(32, 179));
        //    }

        //    if (_optInChecked3 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(32, 218));
        //    }

        //    if (_optInChecked4 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(32, 257));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1200) / 2, 436, 1200,333);
        }

        private bool ValidateEmailAddress()
        {
            if (ValidateEmailAddress(4, _emailTextBox.Text)) return true;

            var mBox = new MessageDialog();
            mBox.ShowMessage(_activeLg.Equals("en") ? "Email address not valid." : "Le courriel n'est pas valide.");

            return false;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = GetCurrentQuestionnaire();

            var mBox = new MessageDialog();

            if (_firstNameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0 && _lastNameTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false; // Question 4
                }

//                SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
//                SaveAnswerToQuestion(2, 1, _firstNameTextBox.Text);
//                SaveAnswerToQuestion(3, 1, _lastNameTextBox.Text);
//                SaveAnswerToQuestionCheck(5, 1, _optInChecked1 != 0);
//                SaveAnswerToQuestionCheck(6, 1, _optInChecked2 != 0);
//                SaveAnswerToQuestionCheck(7, 1, _optInChecked3 != 0);
//                SaveAnswerToQuestionCheck(8, 1, _optInChecked4 != 0);

                return true;
            }

            mBox.ShowMessage(_activeLg.Equals("en")
                ? "Please answer all questions."
                : "Vous devez répondre à toutes les questions.");
            return false;
        }

        private void AddTextBoxs()
        {
            _firstNameTextBox = new TextBox
            {
                Location = new Point(150, 31),
                Size = new Size(480, 35),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _lastNameTextBox = new TextBox
            {
                Location = new Point(756, 31),
                Size = new Size(400, 35),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(102, 75),
                Size = new Size(480, 35),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

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
        //    else if (InArea(e.X, e.Y, _optInArea1))
        //    {
        //        _optInChecked1 = _optInChecked1 == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea2))
        //    {
        //        _optInChecked2 = _optInChecked2 == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea3))
        //    {
        //        _optInChecked3 = _optInChecked3 == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea4))
        //    {
        //        _optInChecked4 = _optInChecked4 == 0 ? 1 : 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _privacyArea))
        //    {
        //        SuspendTimer();
        //        var file = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId,
        //            DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + "policy.html";
        //        new WaiverHtmlDialog(file).ShowDialog();
        //        ResumeTimer();
        //    }
        //    else if (InArea(e.X, e.Y, _termsArea))
        //    {
        //        SuspendTimer();
        //        var file = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId,
        //            DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + "terms.html";
        //        new WaiverHtmlDialog(file).ShowDialog();
        //        ResumeTimer();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextBox.Focus();
        }
    }
}