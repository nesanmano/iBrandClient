using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.iBrand;
using NomadLogic.iBrand.Common;
using NomadLogic.iBrand.Common.Ui.Keyboard;
using NomadLogic.iBrand.Ui;
using NomadLogic.iBrand.Ui.ApplicationScreen;
using NomadLogic.iBrand.Ui.Choice;

namespace NomadLogic.PrintStation.Ui.custom.Dulcolax
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _waiverArea;
        private readonly Bitmap _checkmark;
        private KeyboardForm _keyboard;
        private TextBox _emailTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private int _optInChecked1;

        public QuestionnaireScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetBitmap("checkmark.png");

            _waiverArea = new Rectangle(new Point(900, 190), new Size(180, 45));

            _optInArea1 = new Rectangle(new Point(263, 190), new Size(641, 45));

            _nextArea = new Rectangle(new Point(566, 250), new Size(200, 75));
            _closeArea = new Rectangle(new Point(1180, 5), new Size(113, 94));
        }

        protected override Bitmap GetBitmap(string fileName)
        {
            Bitmap bitmap = null;

            var path = Constants.APP_DIR + "ui" + Path.DirectorySeparatorChar + "dulcolax" + Path.DirectorySeparatorChar +
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

            _firstNameTextBox.Text = "";
            _lastNameTextBox.Text = "";
            _emailTextBox.Text = "";

            _optInChecked1 = 0;
        }

        public override void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (_optInChecked1 == 1)
            {
                if (_checkmark != null)
                {
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(282, 207));
                }
            }
        }

        private void AddKeyboard()
        {
            _keyboard = new KeyboardForm(1239, 405);

            _keyboard.Location = new Point(0, 372);
            _keyboard.TopLevel = false;
            Controls.Add(_keyboard);

            _keyboard.BringToFront();
            _keyboard.Show();
        }

        private bool ValidateEmailAddress()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var question = questionnaire.GetQuestion(3);
            var choiceText = (ChoiceText) question.GetChoice(1);
            var answer = new Answer(-1, question.NumberInt, choiceText.No);
            answer.Response = _emailTextBox.Text;
            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern() || _emailTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Email address not valid.");

                answer.Response = "";

                choiceText.SetAnswer(answer);

                return false;
            }

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var mBox = new MessageDialog();

            if (_firstNameTextBox.Text.Length > 0 && _lastNameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }


                if (_optInChecked1 == 0)
                {
                    mBox.ShowMessage("You must agree to the photo release and waiver.");
                    return false;
                }

                var questionName = questionnaire.GetQuestion(1);
                var choiceName = (ChoiceText) questionName.GetChoice(1);

                var answerName = new Answer(-1, questionName.NumberInt, choiceName.No);
                answerName.Response = _firstNameTextBox.Text;
                choiceName.SetAnswer(answerName);

                var questionLastName = questionnaire.GetQuestion(2);
                var choiceLastName = (ChoiceText) questionLastName.GetChoice(1);

                var answerLastName = new Answer(-1, questionLastName.NumberInt, choiceLastName.No);
                answerLastName.Response = _lastNameTextBox.Text;
                choiceLastName.SetAnswer(answerLastName);

                if (_optInChecked1 == 1)
                {
                    var question = questionnaire.GetQuestion(4);
                    var choiceCheck = (ChoiceCheck) question.GetChoice(1);
                    var answer = new Answer(-1, question.NumberInt, 1);
                    choiceCheck.SetAnswer(answer);
                }

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
            _firstNameTextBox = new TextBox();
            _firstNameTextBox.Location = new Point(250, 71);
            _firstNameTextBox.Size = new Size(420, 50);
            _firstNameTextBox.TabIndex = 0;
            _firstNameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _lastNameTextBox = new TextBox();
            _lastNameTextBox.Location = new Point(819, 71);
            _lastNameTextBox.Size = new Size(360, 50);
            _lastNameTextBox.TabIndex = 1;
            _lastNameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();

            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point(212, 133);
            _emailTextBox.Size = new Size(460, 50);
            _emailTextBox.TabIndex = 3;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();
        }

        public override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _closeArea))
            {
                Close();
            }
            else if (InArea(e.X, e.Y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                    Next();
            }
            else if (InArea(e.X, e.Y, _waiverArea))
            {
                SuspendTimer();
                var release = GetBitmap("agreement.jpg");
                if (release != null)
                {
                    new WaiverDialog(release).ShowDialog();
                    release.Dispose();
                }

                ResumeTimer();
            }
            else if (InArea(e.X, e.Y, _optInArea1))
            {
                if (_optInChecked1 == 0)
                {
                    _optInChecked1 = 1;
                }
                else
                {
                    _optInChecked1 = 0;
                }

                Refresh();
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextBox.Focus();
        }
    }
}