using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// <summary>
    /// Structure pour configurer toutes les étapes possibles de transformations sur des listes d'images. Chaque groupe d'options peut être nul,
    /// auquel cas on prend la configuration par défaut de chacun.
    /// </summary>
    // TODO Éventuellement mettre toutes les transformations dans une liste, pour forcer leur ordre d'exécution, et pouvoir gérer des cas multiples du même type.
    [PublicAPI]
    public class ImageListTransformOptions : IJsonConfig
    {
        /// Options pour la transformation de la première image, ou image de présentation.
        public ImageListTransformOptionsFirstPicture FirstPictureOptions { get; set; }
        /// Options pour la transformation de la première image, ou image de présentation.
        public ImageListTransformOptionsDisplayOnTv DisplayOnTvPictureOptions { get; set; }
        /// Options pour la transformation de l'image du mur social, qui est une variation de l'image d'impression.
        public ImageListTransformOptionsSocialWall SocialWallPictureOptions { get; set; }
        /// Options pour la transformation d'un fichier ZIP qui regroupe les images.
        public ImageListTransformOptionsBundle BundleOptions { get; set; }
        /// Options pour la transformation en vidéo d'un groupe d'image.
        public ImageListTransformOptionsVideo VideoOptions { get; set; }
        /// Options pour la transformation en GIF animé d'un groupe d'images.
        public ImageListTransformOptionsGif GifOptions { get; set; }
        /// Options pour la transformation d'une image d'impression.
        public ImageListTransformOptionsPrint PrintOptions { get; set; }
        /// Options pour la transformation d'une image avec chroma keying
        [Obsolete]
        public ImageListTransformOptionsFirstPictureChromaKey FirstPictureChromaKeyOptions { get; set; }
        // Options pour la transformation d'une image avec clé chromatique et détection de visage.
        public ImageListTransformOptionsChroma ChromaOptions { get; set; }
        // Options pour la transformation d'une image avec clé chromatique et détection de visage.
        public IList<ImageListTransformOptionsImageMagick> ImageMagickOptions { get; set; }

        public string DefaultConfigFileName => "ImageListTransformOptions.config";

        /// Options de créations d'images génériques
        /// TODO À implémenter correctement
        //public ImageListGenericTransforms GenericImageTransforms { get; set; }

        public ImageListTransformOptionsBaseBackground BaseBackgroundOptions { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this, 1);
        }
    }
}