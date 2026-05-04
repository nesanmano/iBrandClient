using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class OptInScreenVideo : AbstractScreen
    {
        readonly Rectangle _accept;
        readonly Rectangle _refuse;

        readonly Bitmap _consensBmp;
        readonly Bitmap _refuseBmp;

        public OptInScreenVideo(string backgroundImgSrc, ScreenConfig sc)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _accept = new Rectangle(new Point(((1920) / 2) - 10 - 327, 900), new Size(327, 97));
            _refuse = new Rectangle(new Point(((1920) / 2) + 10, 900), new Size(327, 97));

            _consensBmp = GetNoneLanguageBitmap("consens.jpg");
            _refuseBmp = GetNoneLanguageBitmap("refuse.jpg");
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    e.Graphics.DrawImageUnscaled(_consensBmp, _accept);
        //    e.Graphics.DrawImageUnscaled(_refuseBmp, _refuse);
        //}

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _accept))
        //    {
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _refuse))
        //    {
        //        End();
        //    }
        //}

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
