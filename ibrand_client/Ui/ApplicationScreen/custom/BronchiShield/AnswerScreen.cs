using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.BronchiShield
{
    public class AnswerScreen : AbstractScreen
    {
        readonly Rectangle _home;

        public AnswerScreen(string backgroundImgSrc)
            : base(backgroundImgSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _home = new Rectangle(new Point(30, 1700), new Size(420, 120));

            InactivityTimerInterval = 15 * 1000;
        }

        /*
        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _home))
            {
                GoTo(1);
            }

        }
        */

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            GoTo(1);
        }
    }
}
