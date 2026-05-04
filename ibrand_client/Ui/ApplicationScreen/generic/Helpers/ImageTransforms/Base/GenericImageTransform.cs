using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base
{
    public class GenericImageTransform
    {
        public int SaveResultToQuestionNo { get; set; }

        public ImageFileTypeEnum ResultFileType { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}