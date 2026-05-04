using System.Drawing;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Iris
{
    public class HomeScreen : AbstractScreen
    {
        public event LanguageSelectionEventHandler OnLanguageSelection;

        readonly Rectangle _nextAreaFr;
        readonly Rectangle _nextAreaEn;

        public HomeScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            _nextAreaFr = new Rectangle(new Point(505, 935), new Size(505, 190));
            _nextAreaEn = new Rectangle(new Point(1145, 935), new Size(505, 190));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (InArea(e.X, e.Y, _nextAreaFr))
        //    {
        //        OnLanguageSelection?.Invoke(Constants.FR);
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _nextAreaEn))
        //    {
        //        OnLanguageSelection?.Invoke(Constants.EN);
        //        Next();
        //    }
        //}
    }
}
