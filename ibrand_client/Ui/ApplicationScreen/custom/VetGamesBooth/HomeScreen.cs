using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.VetGamesBooth
{
    public class HomeScreen : AbstractScreen
    {
        readonly Rectangle _clickHereArea;

        public HomeScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _clickHereArea = new Rectangle(new Point(0, 0), new Size(1080, 1920));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _clickHereArea))
        //    {
        //        Next();
        //    }

        //}
    }
}
