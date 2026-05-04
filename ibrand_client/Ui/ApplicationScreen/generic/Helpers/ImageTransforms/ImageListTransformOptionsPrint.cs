using System;
using System.Drawing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// Options pour créer un image d'impression. Les images sont disposées en rangées à partir du point en haut à gauche,
    /// selon un assemblage en carré.
    [PublicAPI]
    public class ImageListTransformOptionsPrint : IComparable<ImageListTransformOptionsPrint>, IEquatable<ImageListTransformOptionsPrint>
    {
        /// Numéro de la question où sauvegarder la réponse. Si cette valeur est nulle (elle l'est par défaut),
        /// cela implique que la transformation voulue n'aura pas lieu et que l'invocation soit considérée comme "no-op".
        public int? SaveToQuestion { get; set; }
        /// Indique un poucentage de la largeur de l'image qui sera pris pour "cadrer" les images à placer. On utilise
        /// un pourcentage (valeur entre 0.0 et 1.0) car cela permet à des valeurs fixes d'être utilisée quelques soit les tailles en pixels
        /// des images utilisées.
        public float? OffsetPerc { get; set; }
        /// Ajustement, en pixels, du point de départ de la séquence. Une valeur négative déplace vers la gauche.
        public int OffsetExtraTop { get; set; }
        /// Ajustement, en pixels, du point de départ de la séquence. Une valeur négative déplace vers le haut.
        public int OffsetExtraSide { get; set; }
        /// Indique, si vrai, si on doit dessiner l'image du cadre par dessus (i.e. après) les images de la séquence.
        /// La taille du canevas sera rigoureusement égale à celle du cadre. Assume que l'image possède une transparence.
        public bool FrameOver { get; set; }
        /// Uniquement utile dans le cas d'un cadre superposé, indique quelle est la couleur de fond à mettre sur le canevas, avant les images.
        /// L'ordre de dessein devient donc: couleur => séquence d'images => cadre
        public Color? BackgroundColor { get; set; }
        /// Référence vers le groupe de sélecteur pour l'image du frame.
        [JsonConverter(typeof(ImageSelectorJsonCreationConverter))]
        public IImageSelector PrintFrameSelector { get; set; }
        /// Qualité de sauvegarde des fichiers, ce qui baisse leur taille en octets. On peut baisser à 80 environ si on veut sauver sur l'espace disque.
        public int JpegSaveQuality { get; set; } = 90;
        /// Indique une rotation et/ou une iversion d'axe à affectuer les les images à placer.
        [JsonConverter(typeof(StringEnumConverter))]
        public RotateFlipType? RotateFlip { get; set; }
        /// <summary>
        /// Indique, si vrai, qu'il faut utiliser uniquement la première image de la liste.
        /// </summary>
        public bool UseOnlyFirstPicture { get; set; }
        /// <summary>
        /// Si vrai, indique qu'il faut utiliser la première image, sur toute la surface du frame.
        /// À préséance sur les autres configurations. Utilise le "BackgroundColor" et "FrameOver". Dessine à partir du point (0,0)
        /// </summary>
        public bool UseFullFramePicture { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        public int CompareTo(ImageListTransformOptionsPrint other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var saveToQuestionComparison = Nullable.Compare(SaveToQuestion, other.SaveToQuestion);
            if (saveToQuestionComparison != 0) return saveToQuestionComparison;
            var offsetPercComparison = Nullable.Compare(OffsetPerc, other.OffsetPerc);
            if (offsetPercComparison != 0) return offsetPercComparison;
            var offsetExtraTopComparison = OffsetExtraTop.CompareTo(other.OffsetExtraTop);
            if (offsetExtraTopComparison != 0) return offsetExtraTopComparison;
            var offsetExtraSideComparison = OffsetExtraSide.CompareTo(other.OffsetExtraSide);
            if (offsetExtraSideComparison != 0) return offsetExtraSideComparison;
            var frameOverComparison = FrameOver.CompareTo(other.FrameOver);
            if (frameOverComparison != 0) return frameOverComparison;
            var jpegSaveQualityComparison = JpegSaveQuality.CompareTo(other.JpegSaveQuality);
            if (jpegSaveQualityComparison != 0) return jpegSaveQualityComparison;
            var rotateFlipComparison = Nullable.Compare(RotateFlip, other.RotateFlip);
            if (rotateFlipComparison != 0) return rotateFlipComparison;
            var useOnlyFirstPictureComparison = UseOnlyFirstPicture.CompareTo(other.UseOnlyFirstPicture);
            if (useOnlyFirstPictureComparison != 0) return useOnlyFirstPictureComparison;
            return UseFullFramePicture.CompareTo(other.UseFullFramePicture);
        }

        public bool Equals(ImageListTransformOptionsPrint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SaveToQuestion == other.SaveToQuestion &&
                   OffsetPerc.Equals(other.OffsetPerc) &&
                   OffsetExtraTop == other.OffsetExtraTop &&
                   OffsetExtraSide == other.OffsetExtraSide &&
                   FrameOver == other.FrameOver &&
                   BackgroundColor.Equals(other.BackgroundColor) &&
                   Equals(PrintFrameSelector, other.PrintFrameSelector) &&
                   JpegSaveQuality == other.JpegSaveQuality &&
                   RotateFlip == other.RotateFlip &&
                   UseOnlyFirstPicture == other.UseOnlyFirstPicture &&
                   UseFullFramePicture == other.UseFullFramePicture;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ImageListTransformOptionsPrint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SaveToQuestion.GetHashCode();
                hashCode = (hashCode * 397) ^ OffsetPerc.GetHashCode();
                hashCode = (hashCode * 397) ^ OffsetExtraTop;
                hashCode = (hashCode * 397) ^ OffsetExtraSide;
                hashCode = (hashCode * 397) ^ FrameOver.GetHashCode();
                hashCode = (hashCode * 397) ^ BackgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ (PrintFrameSelector != null ? PrintFrameSelector.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ JpegSaveQuality;
                hashCode = (hashCode * 397) ^ RotateFlip.GetHashCode();
                hashCode = (hashCode * 397) ^ UseOnlyFirstPicture.GetHashCode();
                hashCode = (hashCode * 397) ^ UseFullFramePicture.GetHashCode();
                return hashCode;
            }
        }
    }
}