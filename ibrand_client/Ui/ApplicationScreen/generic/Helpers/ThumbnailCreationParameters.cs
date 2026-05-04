using System.Drawing;
using JetBrains.Annotations;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    ///     Structure pour la création d'un thumbnail à partir d'un vidéo.
    /// </summary>
    [PublicAPI]
    public class ThumbnailCreationParameters
    {
        /// <summary>
        ///     Temps, en secondes, où prendre le frame à extraire.
        /// </summary>
        public float ThumbnailTime { get; set; } = 3.0f;

        /// <summary>
        ///     Indique la taille finale du thumbnail. Va faire un élaguage pour enlever le moins possible à cette nouvelle taille.
        /// </summary>
        public Size? CropSize { get; set; }

        /// <summary>
        ///     Indique si on doit utiliser un fichier de overlay par dessus la trame extraite. Mis par-dessus le résultat après redimensionnement.
        /// </summary>
        public string OverlayFile { get; set; }

        /// <summary>
        ///     Numéro de la question où mettre le Thumbnail
        /// </summary>
        public int QuestionThumbnailNo { get; set; }
    }
}