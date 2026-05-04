using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.DfcCinephoto
{
    public class QuestionnaireScreen : AbstractScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _emailTextbox;
        private ComboBox _eventDates;

        public QuestionnaireScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();
            
            _nextArea = new Rectangle(new Point(580, 249), new Size(117, 48));
            _cancelArea = new Rectangle(new Point(1226, 7), new Size(40, 40));
        }

        public override void Recycle()
        {
            base.Recycle();

            _firstNameTextbox.Text = "";
            _lastNameTextbox.Text = "";
            _emailTextbox.Text = "";
            // On ne recycle pas le "event and location" combobox
        }
        
        private void AddKeyboard()
        {
            AddKeyboardToControls((Size.Width - 1210) / 2, 312, 1210, 342);
        }

        private void AddTextBoxs()
        {
            _firstNameTextbox = new TextBox
            {
                Location = new Point(510, 81),
                Size = new Size(651, 40),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            _firstNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_firstNameTextbox);
            _firstNameTextbox.BringToFront();

            _lastNameTextbox = new TextBox
            {
                Location = new Point(510, 138),
                Size = new Size(651, 40),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            _lastNameTextbox.TextChanged += OnTextChanged;
            Controls.Add(_lastNameTextbox);
            _lastNameTextbox.BringToFront();

            _emailTextbox = new TextBox
            {
                Location = new Point(510, 193),
                Size = new Size(651, 40),
                TabIndex = 2,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            _emailTextbox.TextChanged += OnTextChanged;
            Controls.Add(_emailTextbox);
            _emailTextbox.BringToFront();

            _eventDates = new ComboBox
            {
                Location = new Point(510, 25),
                Size = new Size(651, 40),
                TabIndex = 3,
                Font = new Font(FontFamily.GenericSansSerif, 14)
            };
            _eventDates.Items.AddRange(new object[]
            {
                "1. Blue Nose Marathon, Halifax (May 20-22, 2016)"
                ,"2. MultiSport Triathlon, Woodstock (June 11, 2016)"
                ,"3. MultiSport Triathlon, Welland (June 25 & 26, 2016)"
                ,"4. MultiSport Triathlon, Belwood (July 9, 2016)"
                ,"5. MultiSport Triathlon, Gravenhurst (July 16 & 17, 2016)"
                ,"6. MultiSport Triathlon, Kingston (July 31, 2016)"
                ,"7. MultiSport Triathlon, Bracebridge (August 6 & 7, 2016)"
                ,"8. MultiSport Triathlon, Toronto (August 20 & 21, 2016)"
                ,"9. MultiSport Triathlon, Wasaga (August 27 & 28, 2016)"
                ,"10. MultiSport Triathlon, Georgina (September 10 & 11, 2016)"
                ,"11. Barrelman Race, Niagara (September 18, 2016)"
                ,"12. The County Marathon, Picton (October 2, 2016)"
                ,"13. PEI Marathon, Charlottetown (October 14 -16, 2016)"
                ,"14. Legs for Literacy Marathon, Moncton (October 22 & 23, 2016) "
                ,"15.GTHL – RwM Weekend, Toronto (October 29 & 30, 2016)"
                ,"16.GTHL – RwM Weekend, Toronto (November 12 & 13, 2016)"
            });
            _eventDates.TextChanged += OnTextChanged;
            Controls.Add(_eventDates);
            _eventDates.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        FirstScreen();
        //    }
        //}

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            if (_emailTextbox.Text.Length > 0 && _firstNameTextbox.Text.Length > 0 && _lastNameTextbox.Text.Length > 0 && !(_eventDates.SelectedIndex < 0))
            {

                if (!StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    mBox.ShowMessage("Invalid email.");
                    return false;
                }

//                SaveAnswerToQuestion(7, 1, _firstNameTextbox.Text);
//                SaveAnswerToQuestion(8, 1, _lastNameTextbox.Text);
//                SaveAnswerToQuestion(9, 1, _emailTextbox.Text);
//                SaveAnswerToQuestion(10, 1, _eventDates.SelectedIndex + 1); // zero based, donc on augmente de un.

                return true;
            }

            mBox.ShowMessage("You must answer all questions.");

            return false;
        }
    }
}
