using System.Collections;

namespace NomadLogic.Common.Ui
{
    public class ColorButtonDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        protected override void PostFilterProperties(IDictionary properties)
        {
            properties.Remove("AllowDrop");
            properties.Remove("BackColor");
            properties.Remove("BackgroundImage");
            properties.Remove("ContextMenu");
            properties.Remove("FlatStyle");
            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageList");
            properties.Remove("TextAlign");
            properties.Remove("Enabled");
        }
    }
}
