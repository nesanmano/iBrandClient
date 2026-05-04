using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// Options pour la création d'une image destinée à un mur social, de façon à "regrouper" les
    /// images d'une même série ensemble, c'est-à-dire dans la même image. Une composition en somme.
    [PublicAPI]
    public class ImageListTransformOptionsSocialWall
    {
        public bool SendToDisplayOnTvDirectory { get; set; }

        /// Numéro de la question où sauvegarder la réponse. Si cette valeur est nulle (elle l'est par défaut),
        /// cela implique que la transformation voulue n'aura pas lieu et que l'invocation soit considérée comme "no-op".
        public int? SaveToQuestion { get; set; }
        /// Taille du canevas, ou de l'image finale.
        public Size FinalSize { get; set; } = new Size(990, 990);
        /// Taille individuelle de chaque image à placer.
        public Size SubImageSize { get; set; } = new Size(480, 480);
        /// Nombre de pixels entre le côté gauche du cadre et entre chaque image, en largeur comme en hauteur.
        public int Padding { get; set; } = 10;
        /// Résolution (DPI) des images à traiter. Convertit le DPI avant chaque traitement. N'a aucun effet sur les images originales.
        public int ImageResolution { get; set; } = 72;
        /// Qualité de sauvegarde des fichiers, ce qui baisse leur taille en octets. On peut baisser à 80 environ si on veut sauver sur l'espace disque.
        public int JpegSaveQuality { get; set; } = 90;
        /// Référence vers le groupe de sélecteur pour l'image du frame.
        public IImageSelector PrintFrameSelector { get; set; }
        /// Indique, si on utilsie un cadre, s'il faut le dessiner par-dessus ou en-dessous des images superposées.
        public bool FrameOverPicture { get; set; }
        /// Si on dessine un cadre, et si ce cadre est dessiné par dessus les images, on peut au préalable remplir le canevas avec cette couleur.
        public Color? BackgroundColor { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}