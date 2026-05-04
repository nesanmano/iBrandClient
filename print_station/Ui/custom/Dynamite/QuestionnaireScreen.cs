using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.PrintStation.Services;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Dynamite
{
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const int MINIMUM_PRINT_WAIT_TIME = 500;

        private readonly Rectangle _cancelArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _printArea;
        private readonly Rectangle _optInArea;
        private readonly Point _checkPoint;
        private readonly Bitmap _checkmark;
        private readonly Bitmap _printDisabled;
        private TextBox _emailTextBox;
        private TextBox _postalCodeTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private bool _optInChecked;
        private bool _printEnabled;
        private string _printFileName;
        private string _photoFileName;

        public QuestionnaireScreen(string backgroundImgEnSrc, string backgroundImageSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            _printDisabled = GetNoneLanguageBitmap("print_button_inactive.png");

            _optInArea = new Rectangle(29, 219, 821, 45);
            _checkPoint = new Point(30, 220);

            _nextArea = new Rectangle(664, 285, 91, 91);
            _cancelArea = new Rectangle(1204, 17, 54, 53);
            _printArea = new Rectangle(515, 277, 109, 107);
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextBox.Text = "";
            _lastNameTextBox.Text = "";
            _emailTextBox.Text = "";
            _postalCodeTextBox.Text = "";

            _optInChecked = false;
            _printEnabled = false;
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, _checkPoint);
        //    }

        //    if (!_printEnabled)
        //    {
        //        e.Graphics.DrawImageUnscaled(_printDisabled, _printArea);
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1220) / 2, 384, 1220,393);
        }

        private bool ValidateEmailAddress(bool showMessages)
        {
            if (!StringUtil.IsValidEmail(_emailTextBox.Text))
            {
                if (!showMessages) return false;
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.printstation.InvalidEmail);
                return false;
            }

            //SaveAnswerToQuestion(3, 1, _emailTextBox.Text);

            return true;
        }

        private bool ValidatePostalCode(bool showMessages)
        {
            if (!StringUtil.IsValidCanadaPostalCode(_postalCodeTextBox.Text))
            {
                if (!showMessages) return false;
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.printstation.InvalidPostalCodeCanada);
                return false;
            }

            //SaveAnswerToQuestion(4, 1, _postalCodeTextBox.Text);

            return true;
        }

        private bool SetAnswersToQuestionnaire(bool showMessages)
        {
            var error = false;

            if (string.IsNullOrWhiteSpace(_firstNameTextBox.Text) || string.IsNullOrWhiteSpace(_lastNameTextBox.Text))
            {
                if (!showMessages) return false;
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.printstation.InvalidPersonName);
                return false;
            }

            error |= !ValidateEmailAddress(showMessages);
            if (error) return false;

            error |= !ValidatePostalCode(showMessages);
            if (error) return false;

            //SaveAnswerToQuestion(1, 1, _firstNameTextBox.Text);
            //SaveAnswerToQuestion(2, 1, _lastNameTextBox.Text);

            SaveAnswerToQuestionCheck(5, 1, _optInChecked);

            return true;
        }

        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
            {
                ResetTimer();
            }

            _printEnabled = SetAnswersToQuestionnaire(false);
            Refresh();
        }

        private void AddTextBoxs()
        {
            _firstNameTextBox = new TextBox
            {
                Location = new Point(238, 45),
                Size = new Size(536, 30),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _firstNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_firstNameTextBox);
            _firstNameTextBox.BringToFront();

            _lastNameTextBox = new TextBox
            {
                Location = new Point(213, 87),
                Size = new Size(561, 30),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _lastNameTextBox.TextChanged += OnTextChanged;

            Controls.Add(_lastNameTextBox);
            _lastNameTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(213, 132),
                Size = new Size(561, 30),
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _postalCodeTextBox = new TextBox
            {
                Location = new Point(297, 175),
                Size = new Size(477, 30),
                TabIndex = 4,
                MaxLength = 7,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _postalCodeTextBox.TextChanged += OnTextChanged;

            Controls.Add(_postalCodeTextBox);
            _postalCodeTextBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        Close();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire(true))
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _printArea))
        //    {
        //        if (!_printEnabled) return;

        //        SuspendTimer();

        //        var t = new Thread(SendPrint);
        //        t.SetApartmentState(ApartmentState.STA);
        //        t.Start();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea))
        //    {
        //        _optInChecked = !_optInChecked;

        //        Refresh();
        //    }
        //}

        private void SendPrint()
        {
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var database = ServicesContainer.GetService<PrintStationDatabase>();

                var picture = MainForm.GetInstance().SelectedPicture;

                database.AddPrintCount(picture.ServerPostId);

                if (PrintUtil.Print(_printFileName, null, this))
                {
                    if (_screenConfig.QuestionNumberOfPrintNo != null)
                    {
                        IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value, 1);
                    }

                    NewPrintEvent.GetInstance().NewItemEvent.Set();
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error during print sequence.");
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds < MINIMUM_PRINT_WAIT_TIME)
            {
                Thread.Sleep(MINIMUM_PRINT_WAIT_TIME - (int)watch.ElapsedMilliseconds);
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextBox.Focus();

            var picture = GetSelectedPicture();

            _photoFileName = picture.Src;
            //SaveAnswerToQuestion(6, 1, _photoFileName);
            
            _printFileName = GetTerminalDirectoryDataFile(GenerateUniqueId.GetId() + ".jpg",
                        // On utilise le EventID et le Feature ID comme équivalence au projet/questionnaire. 
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                        DirectoryUtil.ProjectDataDirectoriesModEnum.print);

            if (!File.Exists(_printFileName))
            {
                var result = PrintStationUtil.CreateFramedPhoto(picture.Src, picture, _printFileName);
                result.Save(95, _printFileName);
                result.Dispose();
            }

            //SaveAnswerToQuestion(7, 1, _printFileName);
        }
    }
}