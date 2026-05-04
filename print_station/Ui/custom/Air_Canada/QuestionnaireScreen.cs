using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.PrintStation.Ui.custom.Air_Canada
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Bitmap _dot;
        private readonly Rectangle _englishArea;
        private readonly Rectangle _frenchArea;
        private readonly Rectangle _nextArea;
        private KeyboardControl _keyboard;
        private TextBox _emailTextBox;
        private int _englishSelected;
        private TextBox _firstNameTextBox;
        private int _frenchSelected;
        private TextBox _nameTextBox;
        private ComboBox _quoteComboBox;

        public QuestionnaireScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            AddTextBoxs();
            AddKeyboard();

            _dot = GetNoneLanguageBitmap("dot.png");

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(new Point(445, 323), new Size(310, 63));
            _closeArea = new Rectangle(new Point(1122, 5), new Size(75, 75));

            _frenchArea = new Rectangle(new Point(902, 170), new Size(65, 30));

            _englishArea = new Rectangle(new Point(979, 170), new Size(65, 30));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _nameTextBox.Text = "";
            _emailTextBox.Text = "";
            _quoteComboBox.SelectedIndex = 0;

            _frenchSelected = 0;
            _englishSelected = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_frenchSelected == 1)
        //    {
        //        if (_dot != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_dot, new Point(944, 181));
        //        }
        //    }

        //    if (_englishSelected == 1)
        //    {
        //        if (_dot != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_dot, new Point(1016, 181));
        //        }
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls(18,388,1172, 398);
        }

        private bool ValidateEmailAddress()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            /*var question = questionnaire.GetQuestion(4);
            var choiceText = (ChoiceEmail) question.GetChoice(1);
            var answer = new Answer(-1, question.No, choiceText.No);
            answer.Response = _emailTextBox.Text;
            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswersPattern() || _emailTextBox.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Email address not valid. / L'adresse courriel est non-valide.");

                answer.Response = "";

                choiceText.SetAnswer(answer);

                return false;
            }*/

            return true;
        }

        private bool SetAnswersToQuestionnaire()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var mBox = new MessageDialog();

            if (_nameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0 && _firstNameTextBox.Text.Length > 0 &&
                (_frenchSelected == 1 || _englishSelected == 1) && _quoteComboBox.SelectedIndex != 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false;
                }

                var questionLanguage = questionnaire.GetQuestion(1);
                var choiceLanguage = (ChoiceCombo) questionLanguage.GetChoice(1);
                /*
                if (_frenchSelected == 1)
                    choiceLanguage.SetAnswer(1);
                else
                    choiceLanguage.SetAnswer(2);
                */

                var questionFirstName = questionnaire.GetQuestion(2);
                var choiceFirstName = (ChoiceText) questionFirstName.GetChoice(1);

                /*var answerFirstname = new Answer(-1, questionFirstName.No, choiceFirstName.No);
                answerFirstname.Response = _firstNameTextBox.Text;
                choiceFirstName.SetAnswer(answerFirstname);


                var questionName = questionnaire.GetQuestion(3);
                var choiceName = (ChoiceText) questionName.GetChoice(1);

                var answerName = new Answer(-1, questionName.No, choiceName.No);
                answerName.Response = _nameTextBox.Text;
                choiceName.SetAnswer(answerName);

                var questionQuote = questionnaire.GetQuestion(5);
                var choiceQuote = (ChoiceCombo) questionQuote.GetChoice(1);

                choiceQuote.SetAnswer(_quoteComboBox.SelectedIndex);*/

                return true;
            }

            mBox.ShowMessage("Please answer all questions. Vous devez répondre à toutes les questions.");
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
            _firstNameTextBox.Location = new Point(298, 96);
            _firstNameTextBox.Size = new Size(400, 50);
            _firstNameTextBox.TabIndex = 0;
            _firstNameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _nameTextBox = new TextBox();
            _nameTextBox.Location = new Point(823, 96);
            _nameTextBox.Size = new Size(300, 50);
            _nameTextBox.TabIndex = 1;
            _nameTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _nameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_nameTextBox);
            _nameTextBox.BringToFront();

            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point(298, 165);
            _emailTextBox.Size = new Size(400, 50);
            _emailTextBox.TabIndex = 2;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _quoteComboBox = new ComboBox();
            _quoteComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _quoteComboBox.Location = new Point(298, 240);
            _quoteComboBox.Size = new Size(830, 50);
            _quoteComboBox.TabIndex = 3;
            _quoteComboBox.Font = new Font(FontFamily.GenericSansSerif, 17);
            _quoteComboBox.TextChanged += OnTextChanged;

            InitQuoteComboBox();

            Controls.Add(_quoteComboBox);
            _quoteComboBox.BringToFront();

            _quoteComboBox.SelectedIndex = 0;
        }

        private void InitQuoteComboBox()
        {
            var questionnaire = MainForm.GetInstance().Questionnaire;

            var question = questionnaire.GetQuestion(5);
            var choice = (ChoiceCombo) question.GetChoice(1);

            //choice.PopulateComboBox(_quoteComboBox);
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
        //    else if (InArea(e.X, e.Y, _frenchArea))
        //    {
        //        _frenchSelected = 1;
        //        _englishSelected = 0;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _englishArea))
        //    {
        //        _englishSelected = 1;
        //        _frenchSelected = 0;

        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextBox.Focus();
        }
    }
}