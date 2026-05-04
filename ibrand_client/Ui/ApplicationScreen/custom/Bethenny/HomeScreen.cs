using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class HomeScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        public HomeScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //InitializeComponent();
            PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(0, 0), new Size(1920, 1080));
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
