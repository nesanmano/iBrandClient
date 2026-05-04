using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.ParcCanada
{
    public class OptInScreen : AbstractScreen
    {
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        int _q2Option = -1;
        
        readonly Rectangle _q2C1Area;
        readonly Rectangle _q2C2Area;

        readonly Bitmap _checkmark;

        public OptInScreen(string backgroundImgEnSrc, string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;
            PictureBox.Paint += PaintEvent;

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            
            _q2C1Area = new Rectangle(new Point(650, 848), new Size(265, 42));
            _q2C2Area = new Rectangle(new Point(1050, 848), new Size(265, 42));
        }

        public override void Recycle()
        {
            base.Recycle();
            _q2Option = -1;
        }

        public new void PaintEvent(object sender, PaintEventArgs e)
        {
            //base.PaintEvent(sender, e);
            if (_q2Option == 1)
            {
                if (_checkmark != null)
                {
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(695, 1008));
                }
            }
            else if (_q2Option == 2)
            {
                if (_checkmark != null)
                {
                    e.Graphics.DrawImageUnscaled(_checkmark, new Point(1120, 1008));
                }
            }
        }

        private void SetAnswersToQuestionnaire()
        {
//            if (_q2Option <= 0) return;
//            var answer = new Answer(QuestionTypeEnum.Radio, 2, 1, -1);
//            SaveAnswerToQuestion(2,1,answer);
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (InArea(e.X, e.Y, _q2C1Area))
        //    {
        //        _q2Option = 1;
        //        CheckForNext();
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _q2C2Area))
        //    {
        //        _q2Option = 2;
        //        CheckForNext();
        //        Refresh();
        //    }
        //}

        private void CheckForNext()
        {
            if (_q2Option == 1) {
                SetAnswersToQuestionnaire();
                Next();
            }
            else if (_q2Option == 2)
            {
                Back(2);
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _q2Option = -1;
        }
    }
}
