using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.UI;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Respondent
{
    /// <summary>
    /// Un groupe dynamique (ou réponse dynamique) est une liste de string sans forme propre, groupée par une date de création et identifiant commun.
    /// Un répondant peut accumuler autant de groupe dynamiques qu'il veut, et chaque groupe peut ne pas être mutuellement exclusif entre eux, même si leur
    /// valeurs de groupe sont strictement identiques. À cause de cela, le concept d'égalité sur cette classe est pratiquement inutile.
    /// Un groupe dynamique peut par exemple, contenir une liste de courriels, contenir un groupe de fichiers d'images pris au même instant, etc...
    /// Habituellement, ce format de donnée sert dans un contexte où le répondant est réutilisé au niveau du serveur, pour ajouter des groupes de données
    /// au même répondant sur une période supérieur à un seul traitement de questionnaire, ou sur de multiples terminaux.
    /// </summary>
    [PublicAPI]
    public class DynamicHeader
    {
        public DynamicHeader() {}

        /// <summary>
        /// Constructeur par copie profonde.
        /// </summary>
        /// <param name="dh">Le groupe original.</param>
        public DynamicHeader(DynamicHeader dh)
        {
            DataKey = dh.DataKey;
            DateCreated = dh.DateCreated;
            DataValues = new List<DataValue>();
            foreach (var dv in dh.DataValues)
            {
                DataValues.Add(new DataValue(dv));
            }
        }

        /// <summary>
        /// Le tag de données du groupe. Ne doit pas être vide ou nul pour être valide.
        /// </summary>
        [JsonRequired]
        public string DataKey { get; set; }
        
        /// <summary>
        /// Date de création.
        /// </summary>
        public DateTime DateCreated { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Version string de la date. Doit être en format ISO-8601.
        /// </summary>
        [JsonRequired]
        public string DateCreatedStr
        {
            get => DateCreated.ToString(DateUtil.Iso8601FormatString);
            set
            {
                if (DateTime.TryParseExact(value, DateUtil.Iso8601FormatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var res))
                {
                    DateCreated = res;
                }
                else
                {
                    throw new ArgumentException("string value must must ISO-8601 in invariant culture compatible.");
                }
            }
        }

        /// <summary>
        /// Liste des valeurs. Ne possède pas de forme précise, peut contenir des valeurs nulles, et être vide.
        /// </summary>
        [JsonRequired] 
        public List<DataValue> DataValues { get; set; } = new List<DataValue>();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("DataKey=");
            sb.Append(DataKey);
            sb.Append(", DateCreated=");
            sb.Append(DateCreatedStr);
            sb.Append(", Values=[");
            foreach (var dv in DataValues)
            {
                sb.Append('\'');
                sb.Append(dv);
                sb.Append("\',");
            }
            // On enlève le dernier ','
            var lastIndex = sb.Length - 1;
            sb.Remove(lastIndex, 1);
            sb.Append("]");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Normalement, une valeur d'un groupe dynamique n'est qu'un string, mais il est pratique de lui attacher des méta-données pour le formater.
    /// Cette classe prend soin de faire en sorte que quand on "marshall" cette classe, on que l'on la compare, ce soit comme une string.
    /// Le constructeur incomplet, va tenter de déterminer quel est la nature de la valeur, et de les adapter (en gros, il va retirer les préfixes s'il en détecte à la valeur pure).
    /// Utile quand on lit des valeurs venant du serveur. Alternativement, on peut lancer la méthode "InferDataType" (ce que fait le constructeur). Cette étape peut s'avérer
    /// nécessaire si la classe a été construite de façon réflexive (I.E. via une marshalisation d'un JSON, par exemple).
    /// À noter!: Le "toString" de cette classe ne retourne pas la valeur pure! Il va modifier cette valeur selon le type de valeur.
    /// </summary>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    [PublicAPI]
    public class DataValue : IEqualityComparer<DataValue>, IEquatable<DataValue>, IEquatable<string>, IComparable<DataValue>, IComparable<string>
    {
        public const string VIMEO_PREFIX = "vimeo:";
        public const string SERVER_FILE_PREFIX = "file:";

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public DataValue() { }

        /// <summary>
        /// Constructeur complet. Ne provoque PAS de vérification et conversion car on donne le type en paramètre.
        /// Similaire à une construction via "Properties setters"
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="valueType"></param>
        public DataValue(string raw, DataValueType valueType)
        {
            ValueRaw = raw;
            TypeMetadata = valueType;
        }

        /// <summary>
        /// Ce constructeur est spécial par rapport au autres. Il va tenter de 
        /// </summary>
        /// <param name="raw"></param>
        public DataValue(string raw)
        {
            ValueRaw = raw;
            InferDataType(true);
        }

        /// <summary>
        /// Constructeur par copie.
        /// </summary>
        /// <param name="original">Valeur dynamique originale.</param>
        public DataValue(DataValue original)
        {
            ValueRaw = original.ValueRaw;
            TypeMetadata = original.TypeMetadata;
        }

        /// <summary>
        /// Appeller cette méthode lance une vérification qui va tenter de déterminer le type de données contenue, principalement en analysant les premiers caractères
        /// pour trouver préfixes.
        /// Par défaut, si un type de métadonné existe déjà, on saute cette étape. On peut forcer l'instance à modifier une métadonnée existante.
        /// </summary>
        public void InferDataType(bool overrideExistingType = false)
        {
            if (!overrideExistingType && TypeMetadata != null) return;

            if (ValueRaw == null)
            {
                TypeMetadata = null;
                return;
            }

            if (ValueRaw.StartsWith(VIMEO_PREFIX))
            {
                TypeMetadata = DataValueType.VimeoIdPrefix;
                ValueRaw = ValueRaw.Substring(VIMEO_PREFIX.Length - 1);
                return;
            }

            if (ValueRaw.StartsWith(SERVER_FILE_PREFIX))
            {
                TypeMetadata = DataValueType.FileToken;
                ValueRaw = ValueRaw.Substring(SERVER_FILE_PREFIX.Length - 1);
                return;
            }

            if (File.Exists(ValueRaw))
            {
                TypeMetadata = DataValueType.FileLocal;
                return;
            }

            // Type par défaut
            TypeMetadata = DataValueType.RawText;
        }

        public string ValueRaw { get; set; }

        public DataValueType? TypeMetadata { get; set; }

        public override string ToString()
        {
            switch (TypeMetadata)
            {
                case DataValueType.VimeoIdPrefix:
                    return "vimeo:" + ValueRaw;
                case DataValueType.FileToken:
                    return "file:" + ValueRaw;
                default:
                    // Inclu le cas de "null"
                    return ValueRaw;
            }
        }

        #region Comparers, equality, operators

        public bool Equals(DataValue x, DataValue y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.ValueRaw == y.ValueRaw;
        }

        public int GetHashCode(DataValue obj)
        {
            return (obj.ValueRaw != null ? obj.ValueRaw.GetHashCode() : 0);
        }

        public bool Equals(DataValue other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ValueRaw == other.ValueRaw;
        }

        public int CompareTo(DataValue other)
        {
            return string.Compare(ValueRaw, other?.ValueRaw, StringComparison.InvariantCultureIgnoreCase);
        }

        public int CompareTo(string other)
        {
            return string.Compare(ValueRaw, other, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DataValue) obj);
        }

        public bool Equals(string obj)
        {
            return !(obj is null) && string.Equals(obj, ValueRaw);
        }

        public override int GetHashCode()
        {
            return (ValueRaw != null ? ValueRaw.GetHashCode() : 0);
        }

        public static bool operator ==(DataValue left, DataValue right)
        {
            switch (left)
            {
                case null when right is null:
                    return true;
                case null:
                    return false;
                default:
                    return left.Equals(right);
            }
        }

        public static bool operator !=(DataValue left, DataValue right)
        {
            return !(left == right);
        }

        #endregion
    }

    /// <summary>
    /// Énumération des possibles représentations de la valeur "crue".
    /// </summary>
    [PublicAPI]
    public enum DataValueType
    {
        /// <summary>
        /// Le texte cru doit être interprété comme tel.
        /// </summary>
        RawText,
        /// <summary>
        /// Indique que la valeur contient un chemin vers un fichier local. Lors de l'envoi vers le serveur, peut être manipulé.
        /// </summary>
        FileLocal,
        /// <summary>
        /// Indique que le champ est un identifiant vimeo. Sera préfixé par "vimeo:" lorqu'envoyé vers le serveur.
        /// </summary>
        VimeoIdPrefix,
        /// <summary>
        /// Indique que la valeur est détecté comme référence vers un fichier distant sur le serveur.
        /// </summary>
        FileToken
    }
}