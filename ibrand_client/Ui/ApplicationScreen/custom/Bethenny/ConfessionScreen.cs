using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class ConfessionScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _restoreArea;

        public ConfessionScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(669, 656), new Size(615, 240));
            _restoreArea = new Rectangle(new Point(0, 0), new Size(204, 115));
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _nextArea))
            {
                Next();
            }
            else if (InArea(e.X, e.Y, _restoreArea))
            {
                var mainFormUtil = MainFormUtil.GetInstance();

                if (mainFormUtil.IsMaximized)
                    mainFormUtil.Restore();
                else
                    mainFormUtil.Maximize();
            }
        }
    }
}
