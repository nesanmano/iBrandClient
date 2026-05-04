using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base
{
    public class CanvasLayer
    {
        public ImageFileTypeEnum CanvasFileType { get; set; } = ImageFileTypeEnum.Jpeg;
        public Size Size { get; set; } = new Size(0,0);
        public string FileName { get; set; }
    }
}