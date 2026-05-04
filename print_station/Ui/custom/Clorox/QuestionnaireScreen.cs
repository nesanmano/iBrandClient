using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Clorox
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _optInArea2;
        private readonly Bitmap _checkmark;
        private TextBox _emailTextBox;
        private TextBox _firstNameTextBox;
        private ComboBox _productsCombo;
        private int _optInChecked1;
        private int _optInChecked2;

        public QuestionnaireScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInArea1 = new Rectangle(new Point(40, 102), new Size(608, 40));
            _optInArea2 = new Rectangle(new Point(40, 210), new Size(1163, 40));

            _nextArea = new Rectangle(new Point(569, 344), new Size(180, 60));
            _closeArea = new Rectangle(new Point(1180, 10), new Size(55, 55));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _emailTextBox.Text = "";
            _productsCombo.SelectedIndex = 0;

            _optInChecked1 = 0;
            _optInChecked2 = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked1 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(614, 108));
        //    }

        //    if (_optInChecked2 == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(46, 220));
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
            var questionnaire = GetCurrentQuestionnaire();

            var mBox = new MessageDialog();

            if (_firstNameTextBox.Text.Length > 0 && _emailTextBox.Text.Length > 0 && _productsCombo.SelectedIndex != 0)
            {
                if (!ValidateEmailAddress())
                {
                    return false; // Question 3
                }

//                SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
//                SaveAnswerToQuestion(2, 1, _firstNameTextBox.Text);
//                SaveAnswerToQuestionCheck(4, 1, _optInChecked1 != 0);
//                SaveAnswerToQuestionCheck(5, 1, _optInChecked2 != 0);
//                SaveAnswerToQuestion(6, 1, _productsCombo.SelectedIndex);

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
                Location = new Point(124, 55),
                Size = new Size(480, 35),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(716, 55),
                Size = new Size(460, 35),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 16)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _productsCombo = new ComboBox
            {
                Location = new Point(1034, 155),
                Size = new Size(210, 40),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 16),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _productsCombo.Items.Add("Select one..");
            _productsCombo.Items.Add("Laundry Detergent");
            _productsCombo.Items.Add("Dish Products");
            _productsCombo.Items.Add("Cleaners");
            _productsCombo.Items.Add("None");
            _productsCombo.SelectedIndex = 0;

            Controls.Add(_productsCombo);
            _productsCombo.BringToFront();
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
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextBox.Focus();
        }
    }
}