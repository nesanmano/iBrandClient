using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Agropur
{
    public class Agropur2018Questionnaire : AbstractPrintStationScreen
    {

        private readonly Rectangle _cancelArea;
        private readonly Rectangle _nextArea;
        private TextBox _age;
        private TextBox _courriel;
        private TextBox _sms;

        private IList<ScreenCheckOption> _checks;
        private Bitmap _checkBmp;

        public Agropur2018Questionnaire(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            AddKeyboard();

            AddControls();

            _checks = new List<ScreenCheckOption>
            {
                new ScreenCheckOption(new Rectangle(478, 190,92,23), 4, 1, false, new Point(480, 188)), // email
                new ScreenCheckOption(new Rectangle(34, 63, 861, 44), 6, 1, false, new Point(37, 60)), // info
                new ScreenCheckOption(new Rectangle(609,190,69,23), 7, 1, false, new Point(657, 188)) // info
            };

            _nextArea = new Rectangle(570,230,138,42);
            _cancelArea = new Rectangle(1215, 0,62,63);

            _checkBmp = GetNoneLanguageBitmap("checkmark.png");
        }

        private void AddKeyboard()
        {
            var key = new KeyboardControl(-1, 289, 1215, 287)
            {
                Parent = PictureBox
            };

            key.BringToFront();
            key.Show();
        }

        private bool SetAnswersToQuestionnaire()
        {
            // Validation email
            if (!string.IsNullOrWhiteSpace(_courriel.Text) && StringUtil.IsValidEmail(_courriel.Text))
            {
                //SaveAnswerToQuestion(2, 1, _courriel.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Veuillez saisir une adresse courriel valide.");
                return false;
            }

            // SMS
            if (!string.IsNullOrWhiteSpace(_sms.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_sms.Text.Trim()))
                {
                    //SaveAnswerToQuestion(3, 1, _sms.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Entrer un numéro mobile valide.");
                    return false;
                }
            }

            // AGE
            if (!string.IsNullOrWhiteSpace(_age.Text))
            {
                if (int.TryParse(_age.Text.Trim(), out var age) && age > 0)
                {
                    //SaveAnswerToQuestion(5, 1, age.ToString());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Entrer un age valide");
                    return false;
                }
            }

            return true;
        }

        private void AddControls()
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);

            _age = new TextBox();
            this.InitTextBox(_age, new Rectangle(222,154,486,20), font, 3);

            _courriel = new TextBox();
            this.InitTextBox(_courriel, new Rectangle(204, 33, 504, 21), font, 1);

            _sms = new TextBox();
            this.InitTextBox(_sms, new Rectangle(313,117,395,20), font, 2);
        }

        protected override void OnPaintEvent(Graphics g)
        {
            PaintCheckBoxList(g, _checks, _checkBmp);
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;


            if (InArea(x, y, _cancelArea))
            {
                Close();
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                }
                return false;
            }

            return EvaluateCheckBoxsMouseDown(x, y, _checks);
        }

        public override void Recycle()
        {
            base.Recycle();

            _courriel.Text = "";
            _sms.Text = "";
            _age.Text = "";

            RecycleCheckBoxs(_checks);
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _courriel.Focus();
        }
    }
}
