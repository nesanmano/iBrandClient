using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// Options pour créer un fichier GIF utilisant la séquence d'images. Les options les plus communes sont prises en compte.
    [PublicAPI]
    public class ImageListTransformOptionsGif
    {
        /// Numéro de la question où sauvegarder la réponse. Si cette valeur est nulle (elle l'est par défaut),
        /// cela implique que la transformation voulue n'aura pas lieu et que l'invocation soit considérée comme "no-op".
        public int? SaveToQuestion { get; set; }
        /// Indique si le Gif internalise le fait qu'il doit se répéter à l'infini. Vrai par défaut.
        public bool Repeat { get; set; } = true;
        /// Indique si on ajoute les trames intermédiaires pour faire un effet boomerang.
        /// PAr exemple, donne si actif: 1-2-3-4-3-2-1? (le dernier "1" est omis si répétition est mis à vrai).
        public bool Loopback { get; set; }
        /// La qualité de l'heuristique de compression. Comme le Gif utilise un format de couleur indexé (256 couleur max),
        /// choisir les bonnes devient crucial. Plus cette valeur est élevé, moins bon sera le résultat.
        public int Quality { get; set; } = 1;
        /// Délais, en milisecondes, entre chaque trame.
        public int Delay { get; set; } = 300;
        /// Redimensionne l'image originale à ces dimensions. Si "Crop" est mis à faux, et si la dimension donnée ne possède pas
        /// le même ratio que l'image originale, une distorsion sera créée.
        public Size? Size { get; set; }
        /// Indique qu'il faut élaguer l'image. Si aucune dimension spécifique n'est fournie, on élague pour obtenir une image carrée.
        /// Si une taille est spécifiée, on utilise l'élaguage pour minimiser l'espace perdu, à partir du centre.
        /// (Voir "CropToSizeMaximized" dans ImageUtil).
        public bool Crop { get; set; } = true;
        /// Référence vers le groupe de sélecteur pour l'image de superposition.
        public IImageSelector OverlaySelector { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}