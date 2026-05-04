using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class OptInScreen : AbstractScreen
    {
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        readonly Rectangle _acceptArea;
        readonly Rectangle _refuseArea;

        readonly Bitmap _consens;
        readonly Bitmap _refuse;

        public OptInScreen(string backgroundImgSrc, ScreenConfig sc)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _acceptArea = new Rectangle(new Point(((1920) / 2) - 10 - 327,  708), new Size(327, 97));
            _refuseArea = new Rectangle(new Point(((1920) / 2) + 10,        708), new Size(327, 97));

            _consens = GetNoneLanguageBitmap("consens.jpg");
            _refuse = GetNoneLanguageBitmap("refuse.jpg");
        }

//        public new void PaintEvent(object sender, PaintEventArgs e)
//        {
//            base.PaintEvent(sender, e);
//
//            e.Graphics.DrawImageUnscaled(_consens, _acceptArea);
//            e.Graphics.DrawImageUnscaled(_refuse, _refuseArea);
//        }

/*
        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _acceptArea))
            {

                if (!CanonCameraManager.GetInstance().IsCameraConnected())
                {
                    MessageBox.Show(this, @"La caméra n'est pas connectée.", @"iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                Next();
            }
            else if (InArea(e.X, e.Y, _refuseArea))
            {
                Back(1);
            }
        }
*/
    }
}
