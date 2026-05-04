using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    [PublicAPI]
    public class ImageTransformChromaKey
    {
        public int SaveToQuestion { get; set; }
        public ICondition Condition { get; set; }
        public CanvasLayer CanvasLayer { get; set; } = new CanvasLayer();
        public ImageFileLayer Frame { get; set; } = new ImageFileLayer();
        public ImageLayer PictureLayer { get; set; } = new ImageLayer();
    }
}