using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.QDA
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;
        private readonly Rectangle _optInFutureArea1;
        private readonly Rectangle _optInFutureArea2;
        private readonly Rectangle _optInPastArea1;
        private readonly Rectangle _optInPastArea2;
        private readonly Bitmap _checkmark;
        private TextBox _yearTextBox;
        private TextBox _nbrAttendeesTextBox;
        private TextBox _eventNameTextBox;
        private int _optInFuture;
        private int _optInPast;

        public QuestionnaireScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _optInFutureArea1 = new Rectangle(973, 28, 84, 46);
            _optInFutureArea2 = new Rectangle(1071, 28, 84, 46);
            _optInPastArea1 = new Rectangle(973, 80, 84, 46);
            _optInPastArea2 = new Rectangle(1071, 80, 84, 46);

            _nextArea = new Rectangle(544, 314, 196, 51);
            _cancelArea = new Rectangle(1220, 13, 45, 45);
        }

        public override void Recycle()
        {
            base.Recycle();

            _yearTextBox.Text = "";
            _nbrAttendeesTextBox.Text = "";
            _eventNameTextBox.Text = "";

            _optInFuture = 0;
            _optInPast = 0;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInFuture == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, _optInFutureArea1.Location);
        //    }
        //    else if (_optInFuture == 2)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, _optInFutureArea2.Location);
        //    }

        //    if (_optInPast == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, _optInPastArea1.Location);
        //    }
        //    else if (_optInPast == 2)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, _optInPastArea2.Location);
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1177) / 2, 400, 1177,353);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_optInFuture == 0)
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "You must tell us if you consider to host in Québec City in the future." :
                    "Vous devez indiquez si vous considérez la cité de Québec pour un événement futur.");
                return false;
            }

            if (_optInPast == 0)
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "You must tell us if you has an event in the past in Québec." :
                    "Vous devez indiquez si vous avez déjà tenu un événement dans la province de Québec.");
                return false;
            }

            //SaveAnswerToQuestion(9, 1, _optInFuture);
            //SaveAnswerToQuestion(10, 1, _optInPast);

            if (_optInFuture != 1) return true;

            {
                if (string.IsNullOrWhiteSpace(_eventNameTextBox.Text))
                {
                    var mBox = new MessageDialog();
                    var lg = GetCurrentQuestionnaire().LanguageCurrent;
                    mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "You must tell us the name of the event."
                        : "Vous devez indiquez le nom de l'événement.");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(_yearTextBox.Text))
                {
                    var mBox = new MessageDialog();
                    var lg = GetCurrentQuestionnaire().LanguageCurrent;
                    mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "You must tell us which year the event will be held."
                        : "Vous devez indiquez l'année où aura lieu l'événement.");
                    return false;
                }

                int tmp;
                if (!int.TryParse(_yearTextBox.Text, out tmp) || tmp < 2016 || tmp > 2100)
                {
                    var mBox = new MessageDialog();
                    var lg = GetCurrentQuestionnaire().LanguageCurrent;
                    mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "You must enter a valid year between 2016 and 2100."
                        : "Vous devez entrer une année valide entre 2016 et 2100.");
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(_nbrAttendeesTextBox.Text))
                {
                    var mBox = new MessageDialog();
                    var lg = GetCurrentQuestionnaire().LanguageCurrent;
                    mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "You must tell us how many people will probably attend."
                        : "Vous devez indiquez le nombre de participants prévu.");
                    return false;
                }

                if (!int.TryParse(_nbrAttendeesTextBox.Text, out tmp) || tmp < 1)
                {
                    var mBox = new MessageDialog();
                    var lg = GetCurrentQuestionnaire().LanguageCurrent;
                    mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "For attendees, you must enter a number higher than zero."
                        : "Pour les participants, vous devez entrer un nombre valide supérieur à zéro.");
                    return false;
                }
            }

            //SaveAnswerToQuestion(13, 1, _nbrAttendeesTextBox.Text);
            //SaveAnswerToQuestion(12, 1, _yearTextBox.Text);
            //SaveAnswerToQuestion(11, 1, _eventNameTextBox.Text);

            return true;
        }

        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            var index = 0;

            SetTextBox(out _eventNameTextBox, ++index, 782, 131, 372, 34);
            SetTextBox(out _yearTextBox, ++index, 782, 184, 372, 34);
            SetTextBox(out _nbrAttendeesTextBox, ++index, 782, 237, 372, 34);
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
        //    else if (InArea(e.X, e.Y, _optInFutureArea1))
        //    {
        //        _optInFuture = 1;
        //        _eventNameTextBox.Enabled = true;
        //        _yearTextBox.Enabled = true;
        //        _nbrAttendeesTextBox.Enabled = true;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInFutureArea2))
        //    {
        //        _optInFuture = 2;
        //        _eventNameTextBox.Text = "";
        //        _eventNameTextBox.Enabled = false;
        //        _yearTextBox.Text = "";
        //        _yearTextBox.Enabled = false;
        //        _nbrAttendeesTextBox.Text = "";
        //        _nbrAttendeesTextBox.Enabled = false;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInPastArea1))
        //    {
        //        _optInPast = 1;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInPastArea2))
        //    {
        //        _optInPast = 2;

        //        Refresh();
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _eventNameTextBox.Focus();
        }
    }
}