using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Multimedia.XmlTransformation.image;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Méthodes que doit implémenter une SubmitAction pour traiter la transformation d'images.
    /// </summary>
    [PublicAPI]
    public interface ISubmitActionPictureTransformation {
        
        /// <summary>
        /// Exécute la transformation photo de façon synchrone.
        /// </summary>
        /// <param name="resp">Le répondant à transformer. Requiert que ce dernier ait ses réponses d'assignées.</param>
        /// <param name="preview">Indique si la transformation est pour un prévisionnement.</param>
        /// <returns>Le Bitmap résultant de la transformation.</returns>
        IList<Bitmap> TransformPictures(IRespondent resp, bool preview);

        /// <summary>
        /// Le contrôlleur de transformation d'image associé à cette instance.
        /// </summary>
        TransformationsController PictureTransformationController { get; set; }

        /// <summary>
        /// Va chercher la configuration qui représente le fichier de transformation utilisé par le contrôlleur de transformation.
        /// </summary>
        string PictureTransformationConfigFile { get; }
    }
}