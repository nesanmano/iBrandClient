using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Cette classe est une structure de données pour définir un Font pour C# et pour être facilement sérialisé en JSON.
    /// </summary>
    public class FontDefinitionInfo : IEquatable<FontDefinitionInfo>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public Color Color { get; set; } = Color.Black;
        public string FamilyName { get; set; } = FontFamily.GenericSerif.Name;
        [JsonConverter(typeof(StringEnumConverter))]
        public GraphicsUnit GraphicsUnit { get; set; } = GraphicsUnit.Pixel;
        public int Size { get; set; } = 20;
        public string Style { get; set; } = FontStyle.Regular.ToString();
        public string FontFile { get; set; }

        public FontStyle ParseStyleValueToEnum()
        {
            if (!Enum.TryParse(Style, out FontStyle style))
            {
                log.Warn("Could not parse Font style value to a valid FontStyle Enum value ({0}), using default value as Regular.", Style ?? "null");
            }

            return style;
        }

        #region Equality members

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }

        public bool Equals(FontDefinitionInfo other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Color.Equals(other.Color) 
                   && string.Equals(FamilyName, other.FamilyName) 
                   && GraphicsUnit == other.GraphicsUnit 
                   && Size == other.Size 
                   && string.Equals(Style, other.Style) 
                   && string.Equals(FontFile, other.FontFile);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((FontDefinitionInfo) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Color.GetHashCode();
                hashCode = (hashCode * 397) ^ (FamilyName != null ? FamilyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) GraphicsUnit;
                hashCode = (hashCode * 397) ^ Size;
                hashCode = (hashCode * 397) ^ (Style != null ? Style.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FontFile != null ? FontFile.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(FontDefinitionInfo left, FontDefinitionInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FontDefinitionInfo left, FontDefinitionInfo right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
