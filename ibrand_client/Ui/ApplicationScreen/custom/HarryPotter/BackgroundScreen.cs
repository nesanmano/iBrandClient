using System.Drawing;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class BackgroundScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        readonly Rectangle _hagrigHutArea;
        readonly Rectangle _hogwartsArea;
        readonly Rectangle _trainArea;

        readonly Bitmap _checkmark;

        int _selectedBackground = -1;
        
        public BackgroundScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _nextArea = new Rectangle(new Point(533, 632), new Size(201, 82));

            _hagrigHutArea = new Rectangle(new Point(158, 177), new Size(265, 405));
            _hogwartsArea = new Rectangle(new Point(509, 177), new Size(265, 405));
            _trainArea = new Rectangle(new Point(858, 177), new Size(265, 405));
        }

        public override void Recycle()
        {
            base.Recycle();

            _selectedBackground = -1;
        }

        /*protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);
            
            if (InArea(e.X, e.Y, _nextArea))
            {
                if (SetAnswerToQuestionnaire())
                    Next();
            }           
            else if (InArea(e.X, e.Y, _hagrigHutArea))
            {
                _selectedBackground = 1;
                Refresh();
            }
            else if (InArea(e.X, e.Y, _hogwartsArea))
            {
                _selectedBackground = 2;
                Refresh();
            }
            else if (InArea(e.X, e.Y, _trainArea))
            {
                _selectedBackground = 3;
                Refresh();
            }

        }*/

        private bool SetAnswerToQuestionnaire()
        {

            if (_selectedBackground == -1)
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("You must select a background");

                return false;
            }
            
            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            //var question = questionnaire.GetQuestion(1);
            //var choiceCombo = (ChoiceCombo)question.GetChoice(1);
            //choiceCombo.SetAnswer(_selectedBackground);
          
            return true;
        }

        /*public new void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (_selectedBackground == 1)
            {
                if (_checkmark != null)
                {
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(275, 545));
                }
            }
            else if (_selectedBackground == 2)
            {
                if (_checkmark != null)
                {
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(621, 545));
                }
            }
            else if (_selectedBackground == 3)
            {
                if (_checkmark != null)
                {
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(984, 545));
                }
            }
        }*/
    }
}
