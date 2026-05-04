using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    // ReSharper disable once InconsistentNaming
    public class Questionnaire_Dia201901 : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _nameTextbox = new TextBox();
        
        private bool _checkState1;
        private bool _checkState2;

        private readonly Rectangle _checkArea1 = new Rectangle(187, 369, 685, 49);
        private readonly Rectangle _checkArea2 = new Rectangle(187, 430, 685, 57);

        private readonly Bitmap _checkBitmap;
        private readonly Rectangle _nextArea = new Rectangle(528,1000,344,73);
        private readonly Rectangle _declineArea = new Rectangle(187,1000,341,73);

        public Questionnaire_Dia201901(string backgroundImageEnSrc, string backgroundImageFrSrc, ScreenConfig sc)
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();
            
            _checkBitmap = GetNoneLanguageBitmap("selection_blanc.png");
        }

        private void AddKeyboard()
        {
            var key = new UsaKeyboardControl(-1, 1077, 1000, 381)
            {
                KeyPadding = -5
            };
            key.CustomLabelInfo.Font = StaticCustomFontRepository.CreateCustomFont(new FontDefinitionInfo
            {
                Size = 25
            });

            AddKeyboardToControls(key);
        }

        private void AddTextBoxs()
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);
            this.InitTextBox(_nameTextbox, new Rectangle(202, 296, 151, 28), font, 1);
        }

        public override void Recycle()
        {
            base.Recycle();

            _checkState1 = false;
            _checkState2 = false;
            _nameTextbox.Text = "";
        }
        
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_checkState1) DrawImage(g, _checkBitmap, _checkArea1.Location);
            if (_checkState2) DrawImage(g, _checkBitmap, _checkArea2.Location);
        }

        private bool SetAnswersToQuestionnaire()
        {
            // Validation prénom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(4, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? 
                    "Vous devez entrez votre nom." : 
                    "You must enter your name.");
                return false;
            }

            if (!_checkState1)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Vous devez acceptez l'impression." :
                    "You must accept to print your picture.");
                return false;
            }

            SaveAnswerToQuestionCheck(6, 1, _checkState1);

            SaveAnswerToQuestionCheck(7, 1, _checkState2);

            return true;
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _checkArea1))
            {
                _checkState1 = !_checkState1;
                Refresh();
                return false;
            }

            if (InArea(x, y, _checkArea2))
            {
                _checkState2 = !_checkState2;
                Refresh();
                return false;
            }

            if (InArea(x, y, _declineArea))
            {
                Next();
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
                if (!SetAnswersToQuestionnaire()) return false;
                Next(AcceptStep);
                return false;
            }

            return true;
        }

        public int AcceptStep { get; set; } = 2;

        public override void SetActivated()
        {
            base.SetActivated();
            _nameTextbox.Focus();
        }
    }
}
