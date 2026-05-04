using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base
{
    public class BaseBackgroundTransform
    {
        public int SaveToQuestionNo { get; set; }
        public ImageFileLayer Frame { get; set; } = new ImageFileLayer();
        public CanvasLayer Canvas { get; set; } = new CanvasLayer();
        public ImageLayer BaseBackground { get; set; } = new ImageLayer();
        public ImageLayer Picture { get; set; } = new ImageLayer();
        public IList<ImageFileLayer> Overlays { get; set; } = new List<ImageFileLayer>();
    }
}