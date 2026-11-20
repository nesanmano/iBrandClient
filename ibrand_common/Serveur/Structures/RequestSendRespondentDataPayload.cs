using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Serveur.Structures
{
    /// <summary>
    /// Cette classe (et son contenu) n'est pas faite pour être directement transformée en JSON, mais plutôt pour créer des éléments multipart à partir d'un seul objet
    /// de configuration (qui lui sera en format JSON), et d'une liste de fichiers à envoyer en même temps.
    /// Une réponse texte qui commence par, très exactement "ref:", est considérée comme une référence vers un fichier binaire inclu avec la requête multipart.
    /// Si c'est le cas, la requête multipart DOIT contenir une référence vers un fichier binaire. Cette référence n'est PAS le nom local du fichier, mais son identifiant de champ.
    /// Par exemple, si on envoie le fichier "c:\user\me\blabla.jpg" avec l'identifiant de champ "fichier_01", alors une des valeurs texte (réponse ou valeur dynamique) doit contenir
    /// la string "ref:fichier_01".
    /// Plusieures réponses textes peuvent référencer vers le MÊME fichier, et n'envoyer qu'un seul fichier dans la requête.
    /// </summary>
    public class RequestSendRespondentDataPayload
    {
        public RequestSendRespondentDataConfig RequestConfig { get; set; }

        public IDictionary<string, FileInfo> RequestFiles { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }
    }

    public class RequestSendRespondentDataConfig
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly string FILE_REFERENCE_PREFIX = "ref:";

        /// <summary>
        /// Cette méthode cherche dans toutes les valeurs textes des réponses et valeurs dynamiques, et tente de trouver les valeurs qui sont
        /// préfixées avec "ref:", et retourne toutes les valeurs distinctes (sans préfixe) trouvées.
        /// Une réponse vide après le préfixe est invalide et sera ignorée.
        /// </summary>
        /// <returns>Un ensemble des références trouvées, sans le préfixe.</returns>
        public ISet<string> GetFileReferences()
        {
            var setResult = new HashSet<string>();

            if (AnswersToSend != null && AnswersToSend.Any())
            {
                foreach (var fich in from a in AnswersToSend 
                    select a.TextValue into str where !string.IsNullOrWhiteSpace(str) && str.StartsWith(FILE_REFERENCE_PREFIX) 
                    select str.Substring(FILE_REFERENCE_PREFIX.Length))
                {
                    if (string.IsNullOrWhiteSpace(fich))
                    {
                        log.Warn("Empty text value for file reference after prefix culling. Skipping");
                    }
                    else
                    {
                        setResult.Add(fich);
                    }
                }
            }

            if (DynamicValuesToSend != null && DynamicValuesToSend.Any())
            {
                foreach (var fich in DynamicValuesToSend.Where(dv => 
                    dv.DataValues != null && dv.DataValues.Any()).SelectMany(dv => 
                    from val in dv.DataValues 
                    where !string.IsNullOrWhiteSpace(val) && val.StartsWith(FILE_REFERENCE_PREFIX) 
                    select val.Substring(FILE_REFERENCE_PREFIX.Length)))
                {
                    if (string.IsNullOrWhiteSpace(fich))
                    {
                        log.Warn("Empty text value for file reference after prefix culling. Skipping");
                    }
                    else
                    {
                        setResult.Add(fich);
                    }
                }
            }

            return setResult;
        }

        [JsonRequired] [JsonProperty("respondent_identification")]
        public RequestSendRespondentDataIdentification SendRespondentIdentification { get; set; }

        [JsonRequired]
        [JsonProperty("unique_external_identifier")]
        public string TextValue { get; set; }

        [JsonProperty("answers")]
        public List<RequestSendRespondentDataAnswer> AnswersToSend { get; set; }

        [JsonProperty("dynamic_values")]
        public List<RequestSendRespondentDataDynamicValue> DynamicValuesToSend { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }
    }

    public class RequestSendRespondentDataAnswer
    {
        /*
            "dont_override": <bool>,
            "question_no": <int>,
            "choice_no": <int>,
            "text_value": <string>,
         */

        [JsonProperty("dont_override")] public bool DontOverrideData { get; set; }

        [JsonRequired] [JsonProperty("question_no")] public int QuestionNumber { get; set; } = -1;

        [JsonProperty("choice_no")] public int ChoiceNumber { get; set; } = 1;

        [JsonRequired] [JsonProperty("text_value")] public string TextValue { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }

    public class RequestSendRespondentDataDynamicValueComparer : IComparer<RequestSendRespondentDataDynamicValue>
    {
        public int Compare(RequestSendRespondentDataDynamicValue x, RequestSendRespondentDataDynamicValue y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var comp = string.CompareOrdinal(x.DataKey, y.DataKey);
            if (comp != 0) return comp;

            comp = DateTimeOffset.Compare(x.DateCreated, y.DateCreated);
            if (comp != 0) return comp;

            return ListUtils.Compare(x.DataValues, y.DataValues);
        }
    }

    public class RequestSendRespondentDataDynamicValue : IEquatable<RequestSendRespondentDataDynamicValue>
    {
        /*
         "date_created": <string>,
         "data_key": <string>,
         "data_values": [<string>],
         */

        [JsonIgnore]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.Now;

        [JsonRequired] [JsonProperty("date_created")] 
        public string DateCreatedStr => DateCreated.ToString(DateUtil.UtcTimezoneFormatString);

        [JsonRequired] [JsonProperty("data_key")] 
        public string DataKey { get; set; }

        [JsonRequired] [JsonProperty("data_values")] 
        public List<string> DataValues { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }

        #region Equality comparers
        public bool Equals(RequestSendRespondentDataDynamicValue other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return DateCreated.Equals(other.DateCreated) 
                   && DataKey == other.DataKey 
                   && ListUtils.EqualToOther(DataValues, other.DataValues);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RequestSendRespondentDataDynamicValue) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DateCreated.GetHashCode();
                hashCode = (hashCode * 397) ^ (DataKey != null ? DataKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DataValues != null ? DataValues.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(RequestSendRespondentDataDynamicValue left, RequestSendRespondentDataDynamicValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RequestSendRespondentDataDynamicValue left, RequestSendRespondentDataDynamicValue right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
    

    public class RequestSendRespondentDataIdentification
    {
        [JsonRequired] [JsonProperty("search_strategy")]
        public RequestSendRespondentDataIdentificationStrategy SearchStrategy { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }
    }

    public class RequestSendRespondentDataIdentificationStrategy
    {
        /*
            "questionnaire_id": <int>,
            "terminal_id": <int>,
            "question_no": <int>,
            "choice_no": <int>,
            "identifier": <string>,
            "searchStrategy":"{'Answer','ExternalIdentifer','Token'}"
        */

        [JsonRequired] [JsonProperty("identifier")]
        public string TextValue { get; set; }

        /// <summary>
        /// Un identifiant externe qui identifie de façon unique un répondant à l'intérieur d'un questionnaire sur le serevur.
        /// </summary>
        [JsonRequired] [JsonProperty("searchStrategy")] [JsonConverter(typeof(StringEnumConverter))]
        public RequestRespondentDataIdentificationStrategyEnum SearchStrategy { get; set; } = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier;
        
        [JsonProperty("question_no")] public int QuestionNumber { get; set; } = -1;

        [JsonProperty("choice_no")] public int ChoiceNumber { get; set; } = 1;

        [JsonProperty("questionnaire_id")] public int ServerIdQuestionnaire { get; set; } = -1;

        [JsonProperty("terminal_id")] public int ServerIdTerminal { get; set; } = -1;

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }
    }
}