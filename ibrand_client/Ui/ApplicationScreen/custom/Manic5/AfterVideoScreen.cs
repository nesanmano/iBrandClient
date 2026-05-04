using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class AfterVideoScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        public AfterVideoScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            IsSaveOnThisPage = false;

            _nextArea = new Rectangle(new Point(0, 0), new Size(1920, 1080));
        }

//        protected override void MouseDownHandler(object sender, MouseEventArgs e)
//        {
//            base.MouseDownHandler(sender, e);
//            
//            if (_activated && InArea(e.X, e.Y, _nextArea))
//            {
//                End();
//            }
//
//        }

        // On termine en sauvant ce qui a été fait si on timeout sur cette page.
        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            if (_activated)
            {
                End();
            }
        }
    }
}
