using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    public class VS2008ToolStrip : ToolStrip {
        public VS2008ToolStrip() {
            Renderer = new VS2008ToolStripRenderer();
        }
    }
}
