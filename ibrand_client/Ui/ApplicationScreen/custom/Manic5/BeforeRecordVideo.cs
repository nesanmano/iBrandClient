using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class BeforeRecordVideo : AbstractScreen
    {
        readonly Rectangle _nextArea;

        int _selectedBackground = -1;
        readonly Bitmap _checkmark;

        readonly Rectangle _q1;
        readonly Rectangle _q2;
        readonly Rectangle _q3;

        public BeforeRecordVideo(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _nextArea = new Rectangle(new Point(786, 922), new Size(350, 95));

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _q1 = new Rectangle(new Point(26, 600), new Size(1022, 61));
            _q2 = new Rectangle(new Point(26, 681), new Size(1772, 93));
            _q3 = new Rectangle(new Point(26, 807), new Size(1033, 58));
        }

        /*public new void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            switch (_selectedBackground)
            {
                case 1:
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(40, 605));
                    break;
                case 2:
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(40, 713));
                    break;
                case 3:
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(40, 818));
                    break;
            }
        }*/

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
                {
                    Next();
                }
            }
            else if (InArea(e.X, e.Y, _q1))
            {
                _selectedBackground = 1;
                Refresh();
            }
            else if (InArea(e.X, e.Y, _q2))
            {
                _selectedBackground = 2;
                Refresh();
            }
            else if (InArea(e.X, e.Y, _q3))
            {
                _selectedBackground = 3;
                Refresh();
            }
        }
*/

        // On termine en sauvant ce qui a été fait si on timeout sur cette page.
        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            if (_activated)
            {
                End();
            }
        }

        private bool SetAnswerToQuestionnaire()
        {

            if (_selectedBackground == -1)
            {
                return false;
            }

            //var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            //var question = questionnaire.GetQuestion(11);
            //var choiceCombo = (ChoiceCombo)question.GetChoice(1);
            //choiceCombo.SetAnswer(_selectedBackground);

            return true;
        }
    }
}
