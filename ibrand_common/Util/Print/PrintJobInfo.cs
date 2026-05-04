using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Runtime.Serialization;
using Newtonsoft.Json;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Common.Util.Print
{
    /// <summary>
    /// Classe pouvant re-créer un objet de type PrintJob via des infos minimales.
    /// </summary>
    [DataContract]
    public class PrintJobInfo : IEquatable<PrintJobInfo>, IEqualityComparer<PaperSize>
    {
        /// <summary>
        /// Le type de papier voulu. Si null on prend le papier par défaut.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public PaperSize PaperSize { get; set; }

        /// <summary>
        /// Le nom de l'imprimante. Si null, on prend l'imprimante par défaut.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string PrinterName { get; set; }

        /// <summary>
        /// Les configurations d'impression d'une image.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public PrintJobImageOption DrawOptions { get; set; } = new PrintJobImageOption();
        
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as PrintJobInfo;
            return Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(PrintJobInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(PaperSize, other.PaperSize) && 
                string.Equals(PrinterName, other.PrinterName) &&
                Equals(DrawOptions, other.DrawOptions);
        }

        /// <inheritdoc />
        public bool Equals(PaperSize arg1, PaperSize arg2)
        {
            if (arg1 == null && arg2 == null) return true;
            if (arg1 == null || arg2 == null) return false;
            return arg1.Width == arg2.Width &&
                   arg1.Height == arg2.Height &&
                   arg1.PaperName.Equals(arg2.PaperName) &&
                   arg1.Kind == arg2.Kind &&
                   arg1.RawKind == arg2.RawKind;
        }

        /// <inheritdoc />
        public int GetHashCode(PaperSize obj)
        {
            unchecked
            {
                var hashCode = obj?.GetHashCode() ?? 0;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PaperSize?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (PrinterName?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (DrawOptions?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Surcharge d'opérateur
        /// </summary>
        /// <param name="left">Opérande gauche</param>
        /// <param name="right">Opérane droite</param>
        /// <returns>Égalité</returns>
        public static bool operator ==(PrintJobInfo left, PrintJobInfo right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Surcharge d'opérateur
        /// </summary>
        /// <param name="left">Opérande gauche</param>
        /// <param name="right">Opérane droite</param>
        /// <returns>Non-Égalité</returns>
        public static bool operator !=(PrintJobInfo left, PrintJobInfo right)
        {
            return !Equals(left, right);
        }
    }
}