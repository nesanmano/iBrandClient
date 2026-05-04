using System.Collections.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    public class ImageListTransformOptionsBaseBackground
    {
        public IList<BaseBackgroundTransform> BasePictureTransforms { get; set; } = new List<BaseBackgroundTransform>();
    }
}