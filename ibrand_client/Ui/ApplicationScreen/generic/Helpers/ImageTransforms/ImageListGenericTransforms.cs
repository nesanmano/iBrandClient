using System.Collections.Generic;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    public class ImageListGenericTransforms
    {
        public IList<GenericImageTransform> Transforms { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}