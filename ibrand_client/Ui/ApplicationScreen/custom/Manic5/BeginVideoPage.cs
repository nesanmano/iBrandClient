using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class BeginVideoPage : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _end;
        
        public BeginVideoPage(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(582, 896), new Size(350, 98));
            _end = new Rectangle(new Point(964, 896), new Size(350, 98));
        }

        /*protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _nextArea))
            {
                Next();
            }
            else if (InArea(e.X, e.Y, _end)) 
            {
                End();
            }
        }*/

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
