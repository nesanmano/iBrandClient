using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;
// ReSharper disable UnusedMember.Global

#pragma warning disable 618

namespace NomadLogic.PrintStation.Ui.custom.Mosaique
{
    // ReSharper disable once InconsistentNaming
    class QuestionnaireScreen_MAS201901_info : AbstractPrintStationScreen
    {
        private TextBox _emailTextbox;

        private readonly List<ScreenCheckOption> _checks = new List<ScreenCheckOption>
        {
            new ScreenCheckOption
            {
                CheckLocation = new Point(163, 378),
                Position = new Rectangle(163, 378, 1224, 123),
                DataModels = new List<DataModelBase>{
                    new DataModelQuestionnaire
                {
                    Identifier = new QuestionIdentifier(6)
                }},
                Mandatory = true
            }
        };

        private readonly Rectangle _nextArea = new Rectangle(591, 894, 496, 84);
        private readonly Rectangle _cancelArea = new Rectangle(1580, 0, 100, 100);
        private Bitmap _checkmark;

        public QuestionnaireScreen_MAS201901_info(string backgroundImageEnSrc, string backgroundImageFrSrc, PrintStationScreenConfig sc)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc) { }

        
        protected override void OnLoad()
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            AddTextBoxs();
            AddKeyboard();
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(new KeyboardEmail(-1, 500, 1530, 375)
            {
                CustomLabelInfo = new CustomPaintLabelInfo
                {
                    Font = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold)
                }
            });
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 20);
            
            this.InitTextBox(out _emailTextbox, new Rectangle(350, 260, 1050, 40), font);
        }
        
        public override void Recycle()
        {
            base.Recycle();
            
            RecycleCheckBoxs(_checks);

            _emailTextbox.Text = "";
        }

        private bool SetAnswersToQuestionnaire()
        {
            // Validation Courriel
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(4, 1, _emailTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Courriel invalide." : "Invalid email.");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Vous devez indiquez votre courriel." : "You must enter your email.");
                return false;
            }

            if (!AllMandatoryChecks(_checks))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "VOus devez accepter la notice pour continuer." : "You must agree with the notice to continue.");
                return false;
            }

            return true;
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                }
                return false;
            }

            if (InArea(x, y, _cancelArea))
            {
                FirstScreen();
                return false;
            }

            if (!EvaluateCheckBoxsMouseDown(x, y, _checks)) return false;

            return true;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextbox.Focus();
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, _checks, _checkmark);
        }

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }
    }
}
