using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    public class VS2008ToolStripContainer : ToolStripContainer {
        public VS2008ToolStripContainer() {
            TopToolStripPanel.Paint += new PaintEventHandler(TopToolStripPanel_Paint);
            TopToolStripPanel.SizeChanged += new EventHandler(TopToolStripPanel_SizeChanged);
        }

        void TopToolStripPanel_SizeChanged(object sender, EventArgs e) {
            Invalidate();
        }

        void TopToolStripPanel_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            var rect = new Rectangle(0, 0, Width, FindForm().Height);
            using (LinearGradientBrush b = new LinearGradientBrush(
                rect, ClsColor.clrHorBG_GrayBlue, ClsColor.clrHorBG_White, LinearGradientMode.Horizontal)) {
                g.FillRectangle(b, rect);
            }
        }
    }
}
