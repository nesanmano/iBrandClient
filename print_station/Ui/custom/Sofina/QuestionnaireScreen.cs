using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Sofina
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _optInArea2;
        private readonly Bitmap _checkmark;
        private TextBox _emailTextBox;
        private TextBox _phoneTextBox;
        private bool _optInChecked1;
        private bool _optInChecked2;

        public QuestionnaireScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInArea1 = new Rectangle(38, 143, 873, 43);
            _optInArea2 = new Rectangle(38, 205, 873, 43);
            
            _nextArea = new Rectangle(571, 292, 136, 46);
            _cancelArea = new Rectangle(1219, 10, 46, 48);
        }

        public override void Recycle()
        {
            base.Recycle();

            _phoneTextBox.Text = "";
            _emailTextBox.Text = "";

            _optInChecked1 = false;
            _optInChecked2 = false;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(39, 145));
        //    }

        //    if (_optInChecked2)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(38, 206));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1186)/2, 351, 1186,649);
        }
        
        private bool SetAnswersToQuestionnaire()
        {
            if (!ValidatePhoneNumber(2, 1, _phoneTextBox.Text))
            {
                return false;
            }

            if (!ValidateEmailAddress(3, _emailTextBox.Text))
            {
                return false;
            }

            if (!_optInChecked2)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Opt-in for promotional material is mandatory.");
                return false;
            }

            SaveAnswerToQuestionCheck(4, 1, _optInChecked1);
            SaveAnswerToQuestionCheck(5, 1, _optInChecked2);

            return true;
        }

        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _phoneTextBox = new TextBox
            {
                Location = new Point(322, 46),
                Size = new Size(715, 39),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _phoneTextBox.TextChanged += OnTextChanged;

            Controls.Add(_phoneTextBox);
            _phoneTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(322, 91),
                Size = new Size(715, 39),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (InArea(e.X, e.Y, _cancelArea))
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
        //        _optInChecked1 = !_optInChecked1;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea2))
        //    {
        //        _optInChecked2 = !_optInChecked2;

        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _phoneTextBox.Focus();
        }
    }
}