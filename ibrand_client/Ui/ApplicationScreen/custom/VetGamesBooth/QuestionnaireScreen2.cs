using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.VetGamesBooth
{
    public class QuestionnaireScreen2 : AbstractScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _yesArea;
        private readonly Rectangle _noArea;

        private TextBox _productTextBox;
        private bool? _vaccinate;
        private readonly Bitmap _select;

        public QuestionnaireScreen2(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.Paint += PaintEvent;
            SkipSubmitActions = true;

            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _select = GetNoneLanguageBitmap("check.png");
            _nextArea = new Rectangle(new Point(216, 1025), new Size(205, 67));
            _yesArea = new Rectangle(new Point(514, 716), new Size(225, 85));
            _noArea = new Rectangle(new Point(783, 716), new Size(225, 85));
        }

        public override void Recycle()
        {
            base.Recycle();

            _productTextBox.Text = "";
            _vaccinate = null;
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(0,1320,Width, 600);
        }

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_productTextBox.Text.Length > 0 && _vaccinate != null)
            {
                //SaveAnswerToQuestion(6, 1, _productTextBox.Text);
                //SaveAnswerToQuestion(5, 1, _vaccinate.Value ? 1 : 2); //yes=1, no=2

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
            _productTextBox = new TextBox();
            _productTextBox.BorderStyle = BorderStyle.None;
            _productTextBox.Location = new Point(232, 942);
            _productTextBox.Size = new Size(680, 40);
            _productTextBox.TabIndex = 0;
            _productTextBox.Font = new Font(FontFamily.GenericSansSerif, 24);
            _productTextBox.TextChanged += OnTextChanged;

            Controls.Add(_productTextBox);
            _productTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _yesArea))
        //    {
        //        _vaccinate = true;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _noArea))
        //    {
        //        _vaccinate = false;
        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _productTextBox.Focus();
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_vaccinate != null)
        //    {
        //        Point p;

        //        if (_vaccinate.Value)
        //        {
        //            p = new Point(530, 704);
        //        }
        //        else
        //        {
        //            p = new Point(810, 704);
        //        }

        //        e.Graphics.DrawImageUnscaled(_select, p);
        //    }
        //}
    }
}
