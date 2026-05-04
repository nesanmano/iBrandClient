using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class BeforeRecordVideoPage : AbstractScreen
    {
        readonly Rectangle _nextArea;

        public BeforeRecordVideoPage(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(558, 544), new Size(186, 74));
        }
//
//        protected override void MouseDownHandler(object sender, MouseEventArgs e)
//        {
//            base.MouseDownHandler(sender, e);
//
//            if (InArea(e.X, e.Y, _nextArea))
//            {
//                Next();
//            }
//        }
    }
}
