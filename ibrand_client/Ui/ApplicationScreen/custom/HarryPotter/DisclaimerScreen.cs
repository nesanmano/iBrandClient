using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class DisclaimerScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _closeArea;

        public DisclaimerScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(531, 625), new Size(211, 82));
            _closeArea = new Rectangle(new Point(1190, 19), new Size(67, 67));

        }

        /*
        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _nextArea))
            {
                    Next();
            }
            else if (InArea(e.X, e.Y, _closeArea))
            {
                Back();
            }
        }
        */
    }
}
