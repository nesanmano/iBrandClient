using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.MonsterEnergy
{
    public class HomeScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        public HomeScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(0, 0), new Size(1280, 800));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
            
        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
        //    }

        //}
    }
}
