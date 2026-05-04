using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class QuestionnaireScreen1 : AbstractScreen
    {
        private readonly Rectangle _nextArea;

        private TextBox _firstnameTextBox;
        private TextBox _nameTextBox;
        private TextBox _emailTextBox;
        private TextBox _phoneTextBox;
        private TextBox _zipCodeTextBox;
        private TextBox _stateTextBox;
        private TextBox _cityTextBox;
        private TextBox _addressTextBox;
        private TextBox _apartmentTextBox;
        private readonly PostalCodeDatabase _postalCodeDatabase;

        public QuestionnaireScreen1(string backgroundImgEnSrc)
#pragma warning disable 618
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
#pragma warning restore 618
        {
            AddTextBoxs();
            AddKeyboard();

            PictureBox.MouseDown += MouseDownHandler;

            _postalCodeDatabase = new PostalCodeDatabase(ScreensControllerAccessor.ScreensController.QuestionnaireDir + "us_zip_code.db3");

            _nextArea = new Rectangle(new Point(756, 535), new Size(404, 133));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstnameTextBox.Text = "";
            _nameTextBox.Text = "";
            _emailTextBox.Text = "";
            _phoneTextBox.Text = "";
            _zipCodeTextBox.Text = "";
            _stateTextBox.Text = "";
            _cityTextBox.Text = "";
            _addressTextBox.Text = "";
            _apartmentTextBox.Text = "";
        }

        public override void SetActivated()
        {
            base.SetActivated();

            _firstnameTextBox.Focus();
        }

        private void AddTextBoxs()
        {
            _firstnameTextBox = new TextBox
            {
                Location = new Point(346, 217),
                Size = new Size(537, 70),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _firstnameTextBox.TextChanged += OnTextChanged;
            _firstnameTextBox.Focus();

            Controls.Add(_firstnameTextBox);
            _firstnameTextBox.BringToFront();


            _nameTextBox = new TextBox
            {
                Location = new Point(346, 289),
                Size = new Size(537, 70),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _nameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_nameTextBox);
            _nameTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(346, 360),
                Size = new Size(537, 70),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _phoneTextBox = new TextBox
            {
                Location = new Point(346, 429),
                Size = new Size(537, 70),
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _phoneTextBox.TextChanged += OnTextChanged;

            Controls.Add(_phoneTextBox);
            _phoneTextBox.BringToFront();

            _zipCodeTextBox = new TextBox
            {
                Location = new Point(1145, 217),
                Size = new Size(250, 70),
                TabIndex = 4,
                MaxLength = 5,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _zipCodeTextBox.TextChanged += OnZipCodeTextChanged;

            Controls.Add(_zipCodeTextBox);
            _zipCodeTextBox.BringToFront();

            _stateTextBox = new TextBox
            {
                Location = new Point(1590, 217),
                Size = new Size(210, 70),
                TabIndex = 5,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _stateTextBox.TextChanged += OnTextChanged;

            Controls.Add(_stateTextBox);
            _stateTextBox.BringToFront();

            _cityTextBox = new TextBox
            {
                Location = new Point(1030, 289),
                Size = new Size(769, 70),
                TabIndex = 6,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _cityTextBox.TextChanged += OnTextChanged;

            Controls.Add(_cityTextBox);
            _cityTextBox.BringToFront();

            _addressTextBox = new TextBox
            {
                Location = new Point(1263, 360),
                Size = new Size(537, 70),
                TabIndex = 7,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _addressTextBox.TextChanged += OnTextChanged;

            Controls.Add(_addressTextBox);
            _addressTextBox.BringToFront();

            _apartmentTextBox = new TextBox
            {
                Location = new Point(1263, 429),
                Size = new Size(137, 70),
                TabIndex = 8,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _apartmentTextBox.TextChanged += OnTextChanged;

            Controls.Add(_apartmentTextBox);
            _apartmentTextBox.BringToFront();
        }

        /*
        protected override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }*/

        public void OnZipCodeTextChanged(object sender, EventArgs e)
        {
            //Look for State and City
            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(8);

            var choiceText = (ChoiceText)question.GetChoice(1);
            /*
            var answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No)
            {
                Response = _zipCodeTextBox.Text
            };
            


            choiceText.SetAnswer(answer);
            */
            if (question.ValidateAnswersPattern())
            {
                var zipEntry = _postalCodeDatabase.GetZipCodeEntry(_zipCodeTextBox.Text);
                
                if (zipEntry != null)
                {
                    _stateTextBox.Text = zipEntry.State;
                    _cityTextBox.Text = zipEntry.City;
                }
            }

            if (_activated)
                ResetTimer();
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(75,675,1776, 378);
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!InArea(e.X, e.Y, _nextArea)) return;

            if (_firstnameTextBox.Text != null && _firstnameTextBox.Text.Equals("!@#"))
            {
                var mainFormUtil = MainFormUtil.GetInstance();
                mainFormUtil.Restore();
                return;
            }
            if (_firstnameTextBox.Text != null && _firstnameTextBox.Text.Equals("configuration"))
            {
                var config = new ConfigurationDlg();
                config.Owner = MainFormUtil.GetInstance().MainForm;
                config.Show();
                return;
            }
            if (SetAnswersToQuestionnaire())
                Next();
        }

        private bool SetAnswersToQuestionnaire()
        {
            /*var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            var question = questionnaire.GetQuestion(2);

            var choiceText = (ChoiceText)question.GetChoice(1);
            var answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No)
            {
                Response = _firstnameTextBox.Text
            };


            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your firstame.");
                return false;
            }

            question = questionnaire.GetQuestion(3);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No);

            answer.Response = _nameTextBox.Text;

            choiceText.SetAnswer(answer);
          
            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must enter your name.");
                return false;
            }

            question = questionnaire.GetQuestion(4);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No);

            answer.Response = _emailTextBox.Text;

            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("You must enter your email address.");
                return false;
            }

            if (!question.ValidateAnswersPattern())
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Email is not valid.");
                return false;
            }

            //
            question = questionnaire.GetQuestion(5);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No);

            answer.Response = _phoneTextBox.Text;

            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("You must enter your phone number.");
                return false;
            }

            if (!question.ValidateAnswersPattern())
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Phone number is not valid.");
                return false;
            }

            //
            question = questionnaire.GetQuestion(8);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No);

            answer.Response = _zipCodeTextBox.Text;

            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("You must enter your zip code.");
                return false;
            }

            if (!question.ValidateAnswersPattern())
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Zip code is not valid.");
                return false;
            }

            //
            question = questionnaire.GetQuestion(9);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No);

            answer.Response = _cityTextBox.Text;

            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("You must enter your city.");
                return false;
            }

            //
            question = questionnaire.GetQuestion(10);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No) {Response = _stateTextBox.Text};


            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("You must enter your state.");
                return false;
            }

            //
            question = questionnaire.GetQuestion(6);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No) {Response = _addressTextBox.Text};


            choiceText.SetAnswer(answer);

            if (!question.ValidateAnswers())
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("You must enter your street address.");
                return false;
            }

            //
            question = questionnaire.GetQuestion(7);

            choiceText = (ChoiceText)question.GetChoice(1);
            answer = new Answer(QuestionTypeEnum.Text, question.No, choiceText.No)
            {
                Response = _apartmentTextBox.Text
            };


            choiceText.SetAnswer(answer);*/
         
            return true;
        }
    }
}
