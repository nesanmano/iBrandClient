using System.Collections.Generic;
using System.Drawing;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.XmlTransformation.image;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Représente les commandes de base pour une SubmitAction qui possède un transformateur d'image en plus des bassins de transformation de base.
    /// Un PictureTransformationController n'est pas nécessaire. Si la transformation est simple ou requiert d'être fait d'une façon qu'un
    /// contrôlleur ne permet pas, alors la méthode TransformPicture doit être surchargée pour contenir ce processus de façon synchrone.
    /// </summary>
    public abstract class AbstractSubmitActionPictureTransformation : AbstractSubmitAction, ISubmitActionPictureTransformation
    {
        /// <inheritdoc />
        protected override void AdditionalInitOperations()
        {
            if (PictureTransformationController != null)
            {
                PictureTransformationController.Dispose();
                PictureTransformationController = null;
            }

            PictureTransformationController = new TransformationsController();
            var baseFile = DirectoryUtil.GetLocalIbrandTerminalDir(TerminalInfo) + "picture_transformation_config.xml";
            PictureTransformationController.Initialize(baseFile);
        }

        /// <inheritdoc />
        public virtual IList<Bitmap> TransformPictures(IRespondent resp, bool preview)
        {
            return PictureTransformationController.TransformPictures(resp, preview);
        }

        /// <inheritdoc />
        public virtual TransformationsController PictureTransformationController { get; set; }

        /// <inheritdoc />
        public string PictureTransformationConfigFile => _configData["picture_transformation"];
    }
}
