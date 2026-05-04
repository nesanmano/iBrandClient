using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Cette classe englobe toutes les propriétés d'un jeton de fichier dans notre système d'exécution de commandes CLI.
    /// Comme un traitement de CLI est souvent une composite d'opérations, il faut spécifier comment on cartographie
    /// les résultats intermédiares, ansi que leur type de sauvegarde
    /// </summary>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class FileTokenInfo : IEquatable<FileTokenInfo>, IComparable<FileTokenInfo>
    {
        /// <summary>
        /// Le rôle du fichier dans la séquence.
        /// </summary>
        public FileTokenType TokenType { get; set; }
        /// <summary>
        /// Son identifiant unique. À noter que les identifiants sont uniques dans leur même groupe de type.
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// Optionnel. Le degré de quality pour l'écriture disque. Invoque le paramère de qualité du fichier ("-quality" dans ImageMagick).
        /// La valeur varie selon le type. Utilise 92 pour les JPG, et 90 pour les PNG. MPC ne requiert pas de qualité.
        /// </summary>
        public int? Compression { get; set; }

        /// <summary>
        /// Le format de fichier du jeton avec lequel on veut faire la transformation. Par défaut, non-spécifié (on va lui en donner un par défaut selon le type).
        /// </summary>
        public FileTokenFormat Format { get; set; } = FileTokenFormat.UNSPECIFIED;

        /// <summary>
        /// Constructeur vide par défaut.
        /// </summary>
        public FileTokenInfo() { }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }

        /// <summary>
        /// Constructeur avec interprétation d'un jeton complet. Exemple: {TMP_OUT:1:JPG:60}
        /// Voir StringUtil pour plus d'information.
        /// </summary>
        /// <param name="token">Le jeton à analyser.</param>
        /// <param name="onlyValue">Optionel. Indique que les paranthèse ne sont pas présentes.</param>
        public FileTokenInfo(string token, bool onlyValue = false)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException();

            var tokenValue =
                onlyValue
                    ? token
                    : token.Substring(1,
                        token.Length - 2); // On enlève les paranthèses avant et après. "{VALUE}" => "VALUE"
            var vals = tokenValue.Split(':'); // Split avec le caractère ':'.
            // On sait qu'il y a minimum 2 paramètres (à cause du REGEX), et deux paramètres optionnels (Format et Compression).
            Enum.TryParse(vals[0], false, out FileTokenType tokenType);
            TokenType = tokenType;
            Tag = vals[1];
            if (vals.Length >= 3)
            {
                Enum.TryParse(vals[2], false, out FileTokenFormat formatType);
                Format = formatType;
            }
            if (vals.Length < 4) return;
            int.TryParse(vals[3], out var compValue);
            Compression = compValue;
        }

        /// <summary>
        /// Convertit le jeton en paramètres de commandes ImageMagick
        /// </summary>
        /// <param name="filesDic">Dictionnaire de référence des fichiers externes.</param>
        /// <param name="sb">Le constructeur de chaîne de caractère.</param>
        public void ToCommandParameter(Dictionary<string, FileInfo> filesDic, StringBuilder sb)
        {
            // Si on a une compression, ajouter le paramètre ici.
            if (Compression.HasValue && Format == FileTokenFormat.JPG)
            {
                sb.Append("-quality ");
                sb.Append(Compression.Value);
                sb.Append(' ');
            }
            else if (Format == FileTokenFormat.PNG)
            {
                sb.Append("PNG32:");
            }

            sb.Append('"');
            sb.Append(filesDic[Tag].FullName);
            sb.Append('"');
        }

        public void ToCommandParameter(Dictionary<string, string> filesDic, StringBuilder sb, bool omitQuotes = false)
        {
            if (!omitQuotes) sb.Append('"');
            sb.Append(filesDic[Tag]);
            if (!omitQuotes) sb.Append('"');
        }

        public void ToText(StringBuilder sb)
        {
            sb.Append(Tag);
        }

        /// <summary>
        /// Créer un nom de fichier aléatoire avec la bonne extension de fichier basé sur le type de fichier.
        /// </summary>
        /// <param name="forceName">Au lieu d'un nom aléatoire, on utilise cette valeur.</param>
        /// <returns>Un nom de fichier.</returns>
        public string CreateFileName(string forceName = null)
        {
            var ext = ".png";
            switch (Format)
            {
                case FileTokenFormat.JPG:
                    ext = ".jpg";
                    break;
                case FileTokenFormat.MPC:
                    ext = ".mpc";
                    break;
                case FileTokenFormat.PNG:
                    ext = ".png";
                    break;
                case FileTokenFormat.UNSPECIFIED:
                    ext = TokenType == FileTokenType.OUT ? ".jpg" : ".mpc";
                    break;
            }
            return !string.IsNullOrWhiteSpace(forceName) ? forceName + ext : GenerateUniqueId.GetId() + ext;
        }

        #region Equality members
        public bool Equals(FileTokenInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TokenType == other.TokenType && 
                string.Equals(Tag, other.Tag, StringComparison.InvariantCulture) && 
                Compression == other.Compression && 
                Format == other.Format;
        }

        public int CompareTo(FileTokenInfo other)
        {
            if (other == null) return 1;
            if (TokenType != other.TokenType) return TokenType.CompareTo(other.TokenType);
            var strComp = string.Compare(Tag, other.Tag, StringComparison.Ordinal);
            if (strComp != 0) return strComp;
            if (Compression.HasValue && !other.Compression.HasValue) return 1;
            if (!Compression.HasValue && other.Compression.HasValue) return -1;
            if (Compression.HasValue && other.Compression.HasValue)
            {
                if (Compression.Value != other.Compression.Value)
                {
                    return Compression.Value - other.Compression.Value;
                }
            }
            return Format.CompareTo(other.Format);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(FileTokenInfo)) return false;
            return Equals((FileTokenInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) TokenType;
                hashCode = (hashCode * 397) ^ (Tag != null ? StringComparer.InvariantCulture.GetHashCode(Tag) : 0);
                hashCode = (hashCode * 397) ^ Compression.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Format;
                return hashCode;
            }
        }

        public static bool operator ==(FileTokenInfo left, FileTokenInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileTokenInfo left, FileTokenInfo right)
        {
            return !Equals(left, right);
        }
        #endregion
    }

    /// <summary>
    /// Types de fichiers. Cela indique comment ils sont gérés à l'interne. Normalement, un fichier temporaire est
    /// détruit à la fin du traitement.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum FileTokenType
    {
        /// <summary>
        /// Fichier temporaire à créer.
        /// </summary>
        TMP_OUT,
        /// <summary>
        /// Fichier temporaire à utiliser comme paramètre de lecture.
        /// </summary>
        TMP_IN,
        /// <summary>
        /// Fichier résultants voulus. À conserver pour le reste du traitement par iBrand.
        /// </summary>
        OUT,
        /// <summary>
        /// Chaîne de caractère représentant un fichier intrant.
        /// </summary>
        IN,
        /// <summary>
        /// Chaîne de charactère représentant une valeur intrant indéfinie.
        /// </summary>
        IN_VAL
    }

    /// <summary>
    /// Formats de fichier disponibles pour jetons
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum FileTokenFormat
    {
        /// <summary>
        /// Format JPEG. sRGB
        /// </summary>
        JPG,
        /// <summary>
        /// Format PNG. sRGBA
        /// </summary>
        PNG,
        /// <summary>
        /// Format natif de Imagemagick. Est en fait un groupe de deux fichiers. Est utilisé uniquement pour des opérations temporaires,
        /// car illisible par tous les autres logiciels de traitement d'image.
        /// </summary>
        MPC,
        /// <summary>
        /// Inconnu. On va interpréter un format passe-partout selon le type de jeton.
        /// </summary>
        UNSPECIFIED
    }
}