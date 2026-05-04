using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class InformationPage : AbstractScreen
    {
        readonly Rectangle _nextArea;

        public InformationPage(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(531, 625), new Size(211, 82));
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
