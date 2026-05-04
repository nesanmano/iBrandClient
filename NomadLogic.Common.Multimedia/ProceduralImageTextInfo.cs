using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Util;

namespace NomadLogic.Multimedia
{
    /// <summary>
    /// Classe qui contient les informations nécessaires pour générer automatiquement des images à partir de texte, plutôt que des images.
    /// </summary>
    public class ProceduralImageTextInfo : IEquatable<ProceduralImageTextInfo>
    {
        /// <summary>
        /// La couleur du texte. Noir par défaut.
        /// </summary>
        public Color Color { get; set; } = Color.Black;

        /// <summary>
        /// La couleur de fond. Transparent par défaut.
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.Transparent;

        /// <summary>
        /// La taille du bitmap final.
        /// </summary>
        public Size BitmapSize { get; set; } = new Size(50, 120);

        /// <summary>
        /// La famille de font pour la génération. Monospace par défaut.
        /// </summary>
        public string FontFamilly { get; set; } = FontFamily.GenericMonospace.Name;

        /// <summary>
        /// La taille du font pour la génération.
        /// </summary>
        public int FontSize { get; set; } = 40;

        /// <summary>
        /// Le style du Font pour la génération. Par défaut, Regular
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;

        /// <summary>
        /// La type d'unité graphique pour la génération. Pixel par défaut.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GraphicsUnit FontGraphicUnit { get; set; } = GraphicsUnit.Pixel;

        /// <summary>
        /// Indique qu'il faut aligner le texte dans le centre dans les deux axes. Vrai par défaut. Si faux, le texte sera aligné
        /// en haut à gauche.
        /// </summary>
        public bool Center { get; set; } = true;

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }

        public bool Equals(ProceduralImageTextInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Color.Equals(other.Color) && 
                BackgroundColor.Equals(other.BackgroundColor) && 
                BitmapSize.Equals(other.BitmapSize) && 
                Equals(FontFamilly, other.FontFamilly) && 
                FontSize == other.FontSize &&
                FontStyle == other.FontStyle && 
                FontGraphicUnit == other.FontGraphicUnit && 
                Center == other.Center;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProceduralImageTextInfo) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Color.GetHashCode();
                hashCode = (hashCode * 397) ^ BackgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ BitmapSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (FontFamilly != null ? FontFamilly.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FontSize;
                hashCode = (hashCode * 397) ^ (int) FontStyle;
                hashCode = (hashCode * 397) ^ (int) FontGraphicUnit;
                hashCode = (hashCode * 397) ^ Center.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ProceduralImageTextInfo left, ProceduralImageTextInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProceduralImageTextInfo left, ProceduralImageTextInfo right)
        {
            return !Equals(left, right);
        }
    }
}