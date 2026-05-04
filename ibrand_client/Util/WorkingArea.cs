using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;

namespace NomadLogic.iBrandDesktop.Util
{
    public static class WorkingArea
    {
        public static Size Size => Container?.ClientSize ?? new Size(0, 0);

        public static Form Container { set; get; }

        public static VS2008StatusStrip StatusStrip { set; get; }
    }
}
