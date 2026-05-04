using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.PrintStation.Ui.custom.Nationwide
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea;
        private readonly Bitmap _checkmark;
        private TextBox _emailTextBox;
        private TextBox _nameTextBox;
        private int _optInChecked;
        private TextBox _zipTextBox;

        public QuestionnaireScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetBitmap("checkmark.png");

            _optInArea = new Rectangle(new Point(429, 199), new Size(433, 43));

            _nextArea = new Rectangle(new Point(535, 258), new Size(212, 63));
            _closeArea = new Rectangle(new Point(1164, 5), new Size(113, 79));
        }

        protected override Bitmap GetBitmap(string fileName)
        {
            Bitmap bitmap = null;

            var path =
                DirectoryUtil.GetLocalPrintstationMediaDir(0, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) +
                fileName;

            if (File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                bitmap = new Bitmap(stream);
                stream.Close();
            }

            return bitmap;
        }

        public override void Recycle()
        {
            base.Recycle();

            _nameTextBox.Text = "";
            _emailTextBox.Text = "";
            _zipTextBox.Text = "";

            _optInChecked = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked == 1)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(434, 213));
        //        }
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls(18,340,1239, 436);
        }

        private bool ValidateEmailAddress()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            /*var question = questionnaire.GetQuestion(3);
            var choiceText = (ChoiceEmail) question.GetChoice(1);
            var answer = new Answer(-1, question.No, choiceText.No) {Response = _emailTextBox.Text};
            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern() || _emailTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Email address not valid.");

                answer.Response = "";

                choiceText.SetAnswer(answer);

                return false;
            }*/

            return true;
        }

        private bool ValidateZipCode()
        {
            /*var questionnaire = MainForm.GetInstance().Questionnaire;

            var questionZip = questionnaire.GetQuestion(2);
            var choiceZip = (ChoiceText) questionZip.GetChoice(1);

            var answerZip = new Answer(-1, questionZip.No, choiceZip.No);
            answerZip.Response = _zipTextBox.Text;
            choiceZip.SetAnswer(answerZip);

            if (!questionZip.ValidateAnswersPattern() || _zipTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Zip code is not valid.");

                answerZip.Response = "";

                choiceZip.SetAnswer(answerZip);

                return false;
            }*/

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var mBox = new MessageDialog();

            if (_nameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0 && _zipTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

                if (!ValidateZipCode())
                {
                    return false;
                }

                var questionName = questionnaire.GetQuestion(1);
                var choiceName = (ChoiceText) questionName.GetChoice(1);

//                var answerName = new Answer(-1, questionName.No, choiceName.No);
//                answerName.Response = _nameTextBox.Text;
//                choiceName.SetAnswer(answerName);
//
//                if (_optInChecked == 1)
//                {
//                    var question = questionnaire.GetQuestion(4);
//                    var choiceCheck = (ChoiceCheck) question.GetChoice(1);
//                    var answer = new Answer(-1, question.No, 1);
//                    choiceCheck.SetAnswer(answer);
//                }

                return true;
            }

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
            _nameTextBox = new TextBox();
            _nameTextBox.Location = new Point(235, 75);
            _nameTextBox.Size = new Size(360, 50);
            _nameTextBox.TabIndex = 0;
            _nameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _nameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_nameTextBox);
            _nameTextBox.BringToFront();

            _zipTextBox = new TextBox();
            _zipTextBox.Location = new Point(754, 75);
            _zipTextBox.Size = new Size(155, 50);
            _zipTextBox.TabIndex = 1;
            _zipTextBox.MaxLength = 5;
            _zipTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _zipTextBox.TextChanged += OnTextChanged;

            Controls.Add(_zipTextBox);
            _zipTextBox.BringToFront();

            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point(177, 140);
            _emailTextBox.Size = new Size(547, 50);
            _emailTextBox.TabIndex = 2;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
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
        //    else if (InArea(e.X, e.Y, _optInArea))
        //    {
        //        if (_optInChecked == 0)
        //        {
        //            _optInChecked = 1;
        //        }
        //        else
        //        {
        //            _optInChecked = 0;
        //        }

        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextBox.Focus();
        }
    }
}