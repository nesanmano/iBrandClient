using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// Options pour créer un vidéo qui affiche les images avec une vitesse précise.
    /// Utilise FFMPEG, et souvent les vidéos vont avoir un "frame rate" bien inférieur à celui que l'on voit partout
    /// sur Internet.
    /// L'algorithme qui sélectionne les images n'affichera jamais une même trame deux fois de suite dans le vidéo, pour ne pas
    /// créer de disparités dans les boucles d'itérations.
    /// Les vidéos sont créé avec le codec H264 dans un contenant MP4.
    [PublicAPI]
    public class ImageListTransformOptionsVideo
    {
        /// Numéro de la question où sauvegarder la réponse. Si cette valeur est nulle (elle l'est par défaut),
        /// cela implique que la transformation voulue n'aura pas lieu et que l'invocation soit considérée comme "no-op".
        public int? SaveToQuestion { get; set; }
        /// Taille que doit avoir le vidéo final. Le ratio des pixels est sauvegardé en élaguant l'extra et en centrant l'image au mieux.
        public Size? NewSize { get; set; }
        /// Contrairement à un GIF, un vidéo ne roule pas à l'infini sur la plupart des visionneurs courant. Cette variable indique
        /// combiens de fois on va boucler à travers un itération complète des images (incluant ou non l'effet boomerang).
        public int Iterations { get; set; } = 1;
        /// Tout comme un GIF, indique si l'on doit ajouter les trames intermédiares pour revenir vers la première trame.
        /// Quel que soit le nombre d'itérations, la toute première trame sera aussi la toute dernière du vidéo.
        public bool Loopback { get; set; }
        /// Nombre de fichiers à insérer par seconde dans le vidéo. Si la séquence compte 90 images, si on compte 10 images par secondes,
        /// la vidéo finale durera 9 secondes. À ne pas confondre avec la vitesse de trame / taux de rafraîchissement (framerate ou  FPS).
        public double FilesPerSecond { get; set; } = 3D;
        /// Taux de rafraîchissement à l'écran pour le vidéo. Comme il ne s'agit d'un vidéo normal, on peut utiliser un taux de rafraîchissement
        /// beaucoup plus bas que la normale (29.4, 30, ou même 60 dans certains sites).
        /// Valeur recommandée au moins deux fois celle du nombre de fichier par seconde. On peut plafonner à 20 ou 30, à moins que l'on veut vraiment donner
        /// un traitement spécial au vidéo.
        public uint FrameRate { get; set; } = 15;
        /// Référence vers le groupe de sélecteur pour l'image de superposition.
        public IImageSelector OverlaySelector { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}