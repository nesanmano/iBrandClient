using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.BestBuy
{
    public class HomeScreen : AbstractScreen
    {
        readonly Rectangle _clickHereArea;


        public HomeScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _clickHereArea = new Rectangle(new Point(0, 0), new Size(1280, 800));

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
