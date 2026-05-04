using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _backArea;
        private readonly Rectangle _nextArea;

        private readonly Rectangle _masculinArea;
        private readonly Rectangle _femininArea;

        private TextBox _ageTextBox;
        private TextBox _codePostalTextBox;

        private readonly Bitmap _checkmark;

        private bool _masculinChecked;
        private bool _femininChecked;
        
        public QuestionnaireScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _masculinArea = new Rectangle(new Point(208, 169), new Size(228, 45));
            _femininArea = new Rectangle(new Point(208, 227), new Size(228, 45));

            _nextArea = new Rectangle(new Point(1095, 304), new Size(180, 118));
            _backArea = new Rectangle(new Point(9, 302), new Size(188, 118));
        }

        public override void Recycle()
        {
            base.Recycle();

            _ageTextBox.Text = "";
            _codePostalTextBox.Text = "";

            _masculinChecked = false;
            _femininChecked = false;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_masculinChecked)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(219, 173));
        //    }
        //    else if (_femininChecked)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(219, 228));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1239) / 2, 420, 1239, 365);
        }

        private void AddTextBoxs()
        {
            _ageTextBox = new TextBox();
            _ageTextBox.Location = new Point(850, 164);
            _ageTextBox.Size = new Size(180, 50);
            _ageTextBox.TabIndex = 0;
            _ageTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _ageTextBox.MaxLength = 2;
            _ageTextBox.TextChanged += OnTextChanged;

            Controls.Add(_ageTextBox);
            _ageTextBox.BringToFront();

            _codePostalTextBox = new TextBox();
            _codePostalTextBox.Location = new Point(850, 231);
            _codePostalTextBox.Size = new Size(180, 50);
            _codePostalTextBox.TabIndex = 1;
            _codePostalTextBox.Font = new Font(FontFamily.GenericSansSerif, 23);
            _codePostalTextBox.TextChanged += OnTextChanged;

            Controls.Add(_codePostalTextBox);
            _codePostalTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _backArea))
        //    {
        //        Back();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _masculinArea))
        //    {
        //        _masculinChecked = true;
        //        _femininChecked = false;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _femininArea))
        //    {
        //        _masculinChecked = false;
        //        _femininChecked = true;

        //        Refresh();
        //    }
           
        //}

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_ageTextBox.Text.Length > 0 && _codePostalTextBox.Text.Length > 0 && (_masculinChecked  || _femininChecked))
            {
               
                if (!StringUtil.IsValidCanadaPostalCode(_codePostalTextBox.Text))
                {
                    mBox.ShowMessage("Le code postal est non valide.");
                    return false;
                }

                if (!StringUtil.IsInteger(_ageTextBox.Text))
                {
                    mBox.ShowMessage("L'age est non valide.");
                    return false;
                }
//
//                if (_masculinChecked)
//                    SaveAnswerToQuestion(2, 1, 1);
//                else
//                    SaveAnswerToQuestion(2, 1, 2);
//
//                SaveAnswerToQuestion(3, 1, _ageTextBox.Text);
//
//                SaveAnswerToQuestion(4, 1, _codePostalTextBox.Text);

                return true;
            }

            mBox.ShowMessage("Vous devez répondre à toutes les questions.");

            return false;
        }
    }
}
