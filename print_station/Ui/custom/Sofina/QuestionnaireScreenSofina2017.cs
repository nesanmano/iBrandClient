using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Ui;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Sofina
{
    public class QuestionnaireScreenSofina2017 : AbstractPrintStationScreen
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

        public QuestionnaireScreenSofina2017(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            _optInArea1 = new Rectangle(38, 143, 1073, 43);
            _optInArea2 = new Rectangle(38, 204, 917, 43);

            _nextArea = new Rectangle(538, 292, 204, 44);
            _cancelArea = new Rectangle(1221, 11, 45, 45);
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();

            _phoneTextBox.Text = "";
            _emailTextBox.Text = "";

            _optInChecked1 = false;
            _optInChecked2 = false;
        }
        
        ///// <inheritdoc/>
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
            AddKeyboardToControls(-1, 351, 984,427);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (!string.IsNullOrWhiteSpace(_phoneTextBox.Text))
            {
                if (!ValidatePhoneNumber(3, 1, _phoneTextBox.Text))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(_emailTextBox.Text))
            {
                if (!ValidateEmailAddress(4, _emailTextBox.Text))
                {
                    return false;
                }
            }

            if (!_optInChecked2)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Consentement pour matériel promotionel obligatoire." :  "Opt-in for promotional material is mandatory.");
                return false;
            }

            SaveAnswerToQuestionCheck(5, 1, _optInChecked1);
            SaveAnswerToQuestionCheck(6, 1, _optInChecked2);

            return true;
        }

        /// <inheritdoc/>
        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _phoneTextBox = new TextBox
            {
                Location = new Point(400, 43),
                Size = new Size(700, 45),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _phoneTextBox.TextChanged += OnTextChanged;

            Controls.Add(_phoneTextBox);
            _phoneTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(400, 92),
                Size = new Size(700, 45),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();
        }

        ///// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();
            _phoneTextBox.Focus();
        }
    }
}
