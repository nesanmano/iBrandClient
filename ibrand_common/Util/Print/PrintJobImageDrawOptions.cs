using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Print
{
    /// <summary>
    /// Options de placement et de rendu de l'image sur la surface d'impression.
    /// On prend en compte que l'on a qu'une seule image à imprimer.
    /// </summary>
    [DataContract]
    public class PrintJobImageOption : IEquatable<PrintJobImageOption>
    {
        /// <summary>
        /// Indique si on doit maximiser la surface d'impression avec l'image. Si l'image à la même proportion
        /// que la surface d'impression, cela va prendre tout l'espace. Sinon, il y aura des zones vides.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Scaled { get; set; } = true;

        /// <summary>
        /// Alignement vertical. Par défaut, centré.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        [JsonConverter(typeof(StringEnumConverter))]
        public PrintJobImageVerticalAlignment VerticalAlignment { get; set; } =
            PrintJobImageVerticalAlignment.Center;

        /// <summary>
        /// Alignement horizontal. Par défaut, centré.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        [JsonConverter(typeof(StringEnumConverter))]
        public PrintJobImageHorizontalAlignment HorizontalAlignment { get; set; } =
            PrintJobImageHorizontalAlignment.Center;

        /// <summary>
        /// Utilise la résolution de la page au lieu de celle de l'image. Vrai par défaut. Ne sert à rien
        /// si on utilise l'option de "Scaled" à TRUE.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool UsePageResolution { get; set; } = true;

        /// <summary>
        /// Ignore les marges d'impression et maximise la surface d'impression. Il peut y avoir une perte selon l'imprimante.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IgnoreMargins { get; set; } = true;

        /// <summary>
        /// Indique si on affiche l'image selon son orientation naturelle sur la surface. Vrai par défaut. N'est pas pris en compte
        /// si l'option de détection d'orientation automatique est TRUE.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool LandScape { get; set; }

        /// <summary>
        /// Indique que l'on va analyser la largeur et hauteur de la surface d'impression (PaperSize) et celle de l'image à afficher
        /// pour trouver l'orientation (landscape ou portrait) qui va laisser un minimum d'espace vide.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool DetectOrientation { get; set; } = true;

        /// <summary>
        /// Si on utilise le scaling, indique si on doit maximiser la surface d'impression au prix d'un élagage de l'Image à imprimer.
        /// En gros, si cette option est vrai, on priorise la surface d'impression quite à sacrifier une partie non-représentative
        /// de l'image à imprimer. Seul un axe sera sacrifié.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool CropMaximizedScale { get; set; }

        /// <summary>
        /// Créer une copie profonde de l'instance.
        /// </summary>
        /// <returns>Une référence différente mais identique.</returns>
        public PrintJobImageOption Clone()
        {
            return new PrintJobImageOption
            {
                LandScape = LandScape,
                IgnoreMargins = IgnoreMargins,
                CropMaximizedScale = CropMaximizedScale,
                HorizontalAlignment = HorizontalAlignment,
                Scaled = Scaled,
                UsePageResolution = UsePageResolution,
                VerticalAlignment = VerticalAlignment,
                DetectOrientation = DetectOrientation
            };
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as PrintJobImageOption;
            return Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(PrintJobImageOption other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Scaled == other.Scaled && 
                VerticalAlignment == other.VerticalAlignment && 
                HorizontalAlignment == other.HorizontalAlignment && 
                UsePageResolution == other.UsePageResolution && 
                IgnoreMargins == other.IgnoreMargins && 
                LandScape == other.LandScape && 
                DetectOrientation == other.DetectOrientation && 
                CropMaximizedScale == other.CropMaximizedScale;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Scaled.GetHashCode();
                hashCode = (hashCode*397) ^ (int) VerticalAlignment;
                hashCode = (hashCode*397) ^ (int) HorizontalAlignment;
                hashCode = (hashCode*397) ^ UsePageResolution.GetHashCode();
                hashCode = (hashCode*397) ^ IgnoreMargins.GetHashCode();
                hashCode = (hashCode*397) ^ LandScape.GetHashCode();
                hashCode = (hashCode*397) ^ DetectOrientation.GetHashCode();
                hashCode = (hashCode*397) ^ CropMaximizedScale.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PrintJobImageOption left, PrintJobImageOption right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PrintJobImageOption left, PrintJobImageOption right)
        {
            return !Equals(left, right);
        }
    }

    /// <summary>
    /// Alignement vertical
    /// </summary>
    public enum PrintJobImageVerticalAlignment
    {
        /// <summary>
        /// À partir du haut
        /// </summary>
        Top,
        /// <summary>
        /// Parfaitement centré
        /// </summary>
        Center,
        /// <summary>
        /// À partir du bas
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Alignement horizontal
    /// </summary>
    public enum PrintJobImageHorizontalAlignment
    {
        /// <summary>
        /// À partir de la gauche
        /// </summary>
        Left,
        /// <summary>
        /// À partir du centre
        /// </summary>
        Center,
        /// <summary>
        /// À partir de la droite
        /// </summary>
        Right
    }
}
