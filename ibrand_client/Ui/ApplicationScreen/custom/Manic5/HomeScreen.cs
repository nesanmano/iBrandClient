using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class HomeScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        public HomeScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            _nextArea = new Rectangle(new Point(0, 0), new Size(1920, 1080));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
        //    }
        //}
    }
}
