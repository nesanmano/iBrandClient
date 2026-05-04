using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class PressToStartScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        public PressToStartScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(327, 447), new Size(1344, 372));
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _nextArea))
            {
                Next();
            }
        }
             
    }
}
