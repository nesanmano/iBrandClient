using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestGetRespondentDataResult
    {
        [JsonRequired] [JsonProperty("target_respondent")]
        public RequestRespondentDataTarget TargetRespondent { get; set; }

        [JsonProperty("answers_list")]
        public List<RequestRespondentDataAnswer> AnswersFound { get; set; }

        [JsonProperty("dynamic_values_list")]
        public List<RequestRespondentDataDynamicValue> DynamicValuesFound { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class RequestRespondentDataDynamicValue : IComparable<RequestRespondentDataDynamicValue>, IComparable, IEquatable<RequestRespondentDataDynamicValue>
    {
        [JsonRequired] [JsonProperty("date_modified")]
        public DateTime ModificationDate { get; set; }

        [JsonRequired] [JsonProperty("date_created")]
        public DateTime CreationDate { get; set; }

        [JsonRequired] [JsonProperty("data_key")]
        public string ValuesKey { get; set; }

        [JsonRequired] [JsonProperty("data_values")]
        public List<string> Values { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }

        // Les comparaisons et évaluations se font sur tout SAUF les valeurs.
        #region Equality, Compare

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public bool Equals(RequestRespondentDataDynamicValue other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ModificationDate.Equals(other.ModificationDate) 
                   && CreationDate.Equals(other.CreationDate) 
                   && ValuesKey == other.ValuesKey;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ModificationDate.GetHashCode();
                hashCode = (hashCode * 397) ^ CreationDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (ValuesKey != null ? ValuesKey.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(RequestRespondentDataDynamicValue left, RequestRespondentDataDynamicValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RequestRespondentDataDynamicValue left, RequestRespondentDataDynamicValue right)
        {
            return !Equals(left, right);
        }


        public int CompareTo(RequestRespondentDataDynamicValue other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            var modificationDateComparison = ModificationDate.CompareTo(other.ModificationDate);
            if (modificationDateComparison != 0) return modificationDateComparison;
            var creaComp = CreationDate.CompareTo(other.CreationDate);
            if (creaComp != 0) return creaComp;
            return string.Compare(ValuesKey, other.ValuesKey, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is RequestRespondentDataDynamicValue other ? CompareTo(other) 
                : throw new ArgumentException($"Object must be of type {nameof(RequestRespondentDataDynamicValue)}");
        }

        public static bool operator <(RequestRespondentDataDynamicValue left, RequestRespondentDataDynamicValue right)
        {
            return Comparer<RequestRespondentDataDynamicValue>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(RequestRespondentDataDynamicValue left, RequestRespondentDataDynamicValue right)
        {
            return Comparer<RequestRespondentDataDynamicValue>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(RequestRespondentDataDynamicValue left, RequestRespondentDataDynamicValue right)
        {
            return Comparer<RequestRespondentDataDynamicValue>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(RequestRespondentDataDynamicValue left, RequestRespondentDataDynamicValue right)
        {
            return Comparer<RequestRespondentDataDynamicValue>.Default.Compare(left, right) >= 0;
        }
        #endregion

        /// <summary>
        /// Un "ToString" personalisé pour retourner les infos de base de ce header dynamique.
        /// </summary>
        public string ToStringHeader()
        {
            return $"Key={ValuesKey}, Created='{CreationDate:s}', Modified='{ModificationDate:s}'";
        }
    }

    public class RequestRespondentDataAnswer
    {
        /*
            "question_no": 1,
            "choice_no": 1,
            "type": 4,
            "answer_text": "12121"
        */

        [JsonRequired][JsonProperty("question_no")]
        public int QuestionNo { get; set; }

        [JsonRequired][JsonProperty("choice_no")]
        public int ChoiceNo { get; set; }

        // Doit convertir en int, donc pas de string converter ici.
        [JsonRequired][JsonProperty("type")]
        public QuestionTypeEnum QuestionType { get; set; }

        [JsonRequired][JsonProperty("answer_text")]
        public string TextValue { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}