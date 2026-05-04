using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// Options pour regrouper les images prises (ou légèrement modifiées) en un seul fichier ZIP, pour traitement
    /// ultérieur, ou pour un affichage via site web, ce qui conserve la qualité de l'image originale.
    [PublicAPI]
    public class ImageListTransformOptionsBundle
    {
        /// Numéro de la question où sauvegarder la réponse. Si cette valeur est nulle (elle l'est par défaut),
        /// cela implique que la transformation voulue n'aura pas lieu et que l'invocation soit considérée comme "no-op".
        public int? SaveToQuestion { get; set; }
        /// Redimensionne les images avant de les grouper dans un seul fichier. Conserve le ratio de pixel en élaguant l'image.
        public Size? NewSize { get; set; }
        /// Qualité de sauvegarde des fichiers, ce qui baisse leur taille en octets. On peut baisser à 80 environ si on veut sauver sur l'espace disque.
        /// N'est pris en compte que si on redimensionne les images. Si on veut réduire leur poid sur disque sans toucher à
        /// leur dimensions, on peut donner un "redimensionnement" égal à leur taille d'origine. (1920x1280 pour un fichier Canon en format S2)
        public int JpegSaveQuality { get; set; } = 90;

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
        public IImageSelector OverlaySelector { get; set; }
    
    }
}