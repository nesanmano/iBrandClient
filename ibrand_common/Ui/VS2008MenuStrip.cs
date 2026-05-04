using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    public class Vs2008MenuStrip : MenuStrip {
        public Vs2008MenuStrip() {
            Renderer = new VS2008MenuRenderer();
        }
    }

    public class Vs2008ContextMenuStrip : ContextMenuStrip {
        public Vs2008ContextMenuStrip() {
            Renderer = new VS2008MenuRenderer();
        }
    }
}
