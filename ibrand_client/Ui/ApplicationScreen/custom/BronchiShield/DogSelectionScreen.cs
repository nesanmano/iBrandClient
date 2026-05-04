using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.BronchiShield
{
    public class DogSelectionScreen : AbstractScreen
    {
        readonly Rectangle _dog1;
        readonly Rectangle _dog2;
        readonly Rectangle _dog3;
        readonly Rectangle _dog4;
        readonly Rectangle _dog5;
        readonly Rectangle _dog6;
        readonly Rectangle _nextGame;
        
        public DogSelectionScreen(string backgroundImgSrc)
            : base(backgroundImgSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _dog1 = new Rectangle(new Point(168, 432), new Size(325, 410));
            _dog2 = new Rectangle(new Point(648, 432), new Size(325, 410));
            _dog3 = new Rectangle(new Point(168, 879), new Size(325, 410));
            _dog4 = new Rectangle(new Point(648, 879), new Size(325, 410));
            _dog5 = new Rectangle(new Point(168, 1335), new Size(325, 410));
            _dog6 = new Rectangle(new Point(648, 1335), new Size(325, 410));
            _nextGame = new Rectangle(new Point(828, 1737), new Size(325, 410));
        }
/*

        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _dog1))
            {
                Next();
            }
            else if (InArea(e.X, e.Y, _dog2))
            {
                Next(4);
            }
            else if (InArea(e.X, e.Y, _dog3))
            {
                Next(7);
            }
            else if (InArea(e.X, e.Y, _dog4))
            {
                Next(10);
            }
            else if (InArea(e.X, e.Y, _dog5))
            {
                Next(13);
            }
            else if (InArea(e.X, e.Y, _dog6))
            {
                Next(16);
            }
            else if (InArea(e.X, e.Y, _nextGame))
            {
                GoTo(20);
            }
        }*/
    }
}
