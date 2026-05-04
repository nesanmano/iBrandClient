using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Batimo2016
{
    public class QuestionnaireBatimo2016_Generique : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        private readonly TextBox _emailTextbox = new TextBox();
        private readonly TextBox _phoneTextbox = new TextBox();
        private readonly TextBox _postalTextbox = new TextBox();
        private bool _checkState;
        private readonly Rectangle _checkArea = new Rectangle(73, 107, 572, 30);
        private readonly Point _checkLocation = new Point(612, 109);
        private readonly Bitmap _checkBitmap;
        private readonly DateTimePicker _datePicker = new DateTimePicker();

        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;
        private KeyboardControl _keyboard;

        public QuestionnaireBatimo2016_Generique(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(607, 309, 89, 44);
            _cancelArea = new Rectangle(1222, 10, 40, 40);

            var filters = new[]
            {
                new ColorSubstitutionFilter(10, Color.Black, Color.White)
            };
            var bmf = new BitmapModificationFilter(filters, 0.5f);
            _checkBitmap = ImageUtil.ModifyPicture(GetNoneLanguageBitmap("checkmark.png"),
                ColorSubstitutionFilterGroup.All, bmf);
        }

        private void AddKeyboard()
        {
            const int width = 1117;
            const int height = 275;
            const int x = 81;
            const int y = 369;

            _keyboard = AddKeyboardToControls(x,y,width, height);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);
            this.InitTextBox(_nameTextbox, new Rectangle(595, 31, 601, 31), font);
            this.InitTextBox(_emailTextbox, new Rectangle(595, 71, 601, 31), font);
            this.InitTextBox(_phoneTextbox, new Rectangle(715, 150, 481, 31), font);
            this.InitTextBox(_postalTextbox, new Rectangle(844, 209, 352, 31), font);
            _postalTextbox.MaxLength = 3;

            _datePicker.Format = DateTimePickerFormat.Custom;
            _datePicker.CustomFormat = @"yyyy-MM-dd";
            // _datePicker.MaxDate = DateTime.Today;
            _datePicker.MinDate = new DateTime(1900, 01, 01);
            _datePicker.ShowCheckBox = false;
            _datePicker.ValueChanged += OnTextChanged;
            _datePicker.Location = new Point(864, 260);
            _datePicker.Size = new Size(200, 31);
            _datePicker.CalendarFont = new Font(FontFamily.GenericMonospace, 26);
            _datePicker.Font = new Font(FontFamily.GenericSansSerif, 20);
            Controls.Add(_datePicker);
            _datePicker.BringToFront();
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();

            _checkState = false;
            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";
            _postalTextbox.Text = "";
            _nameTextbox.Text = "";
            _datePicker.Value = DateTime.Today.AddHours(-1);
        }

        ///// <inheritdoc />
        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_checkState) e.Graphics.DrawImageUnscaled(_checkBitmap, _checkLocation);
        //}

        private bool SetAnswersToQuestionnaire()
        {
            // Validation courriel
            if (!string.IsNullOrEmpty(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    SaveAnswerToQuestion(new Answer{QuestionNo = 4, Response = _emailTextbox.Text});
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") ? "Adresse courriel non valide." : "Invalid email");
                    return false;
                }
            }

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                SaveAnswerToQuestion(new Answer{QuestionNo = 3, Response = _nameTextbox.Text});
            }
            /*else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_active_lg.Equals("fr") ? "Prénom/Nom non valide." : "Invalid Name/Last Name");
                return false;
            }*/

            // Validation check
            SaveAnswerToQuestionCheck(5, 1, _checkState);

            // Validation téléphone
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    SaveAnswerToQuestion(new Answer{QuestionNo = 6, Response = _phoneTextbox.Text});
                }
            }
            /*else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_active_lg.Equals("fr") ? "Téléphone invalide" : "Invalid Phone number");
                return false;
            }*/

            // Validation Code Postal (moitié)
            if (!string.IsNullOrWhiteSpace(_postalTextbox.Text))
            {
                if (StringUtil.IsValidCanadaPostalCode(_postalTextbox.Text.Trim() + "1A1"))
                {
                    //SaveAnswerToQuestion(7, 1, _postalTextbox.Text);
                }
            }
            /* else
             {
                 var mBox = new MessageDialog();
                 mBox.ShowMessage(_active_lg.Equals("fr") ? "Première moitié de Code Postal invalide" : "Invalid postal code first hals");
                 return false;
             }*/

            // Date d'anniversaire
            //SaveAnswerToQuestion(8, 1, _datePicker.Value.ToString(_datePicker.CustomFormat));

            return true;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        FirstScreen();
        //    }
        //    else if (InArea(e.X, e.Y, _checkArea))
        //    {
        //        _checkState = !_checkState;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
        //        }
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextbox.Focus();
        }
    }
}