using System.Collections.Generic;
using JetBrains.Annotations;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    [PublicAPI]
    public class ImageListTransformOptionsChroma
    {
        public IList<ImageTransformChromaKey> Transforms { get; set; } = new List<ImageTransformChromaKey>();

    }
}