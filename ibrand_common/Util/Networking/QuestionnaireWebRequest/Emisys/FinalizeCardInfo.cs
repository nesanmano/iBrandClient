using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class FinalizeCardInfo : IEquatable<FinalizeCardInfo>
    {

        /// Represente le resultat de la requête
        [JsonProperty(PropertyName = "result")]
        public int Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "nl_firstname")]
        public string FirstnameNl { get; set; }

        [JsonProperty(PropertyName = "nl_lastname")]
        public string LastnameNl { get; set; }

        [JsonProperty(PropertyName = "nl_zipcode")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "nl_sms")]
        public string Sms { get; set; }

        [JsonProperty(PropertyName = "nl_legal_age")]
        public bool IsLegalAge { get; set; }

        [JsonProperty(PropertyName = "nl_contest_rules")]
        public bool IsAgreeContestRules { get; set; }

        [JsonProperty(PropertyName = "nl_td_client")]
        public bool IsTdClient { get; set; }

        [JsonProperty(PropertyName = "nl_lifestyle")]
        public int LifeStyleCode { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "orignalBalance")]
        public double OrignalBalance { get; set; }

        [JsonProperty(PropertyName = "currentBalance")]
        public double CurrentBalance { get; set; }

        [JsonProperty(PropertyName = "numBasket")]
        public int NumBasket { get; set; }

        [JsonProperty(PropertyName = "distinctPos")]
        public int DistinctPos { get; set; }

        public bool Equals(FinalizeCardInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Result == other.Result && Equals(Username, other.Username) && Equals(Email,other.Email) && Equals(Language,other.Language) && OrignalBalance.Equals(other.OrignalBalance) && CurrentBalance.Equals(other.CurrentBalance) && NumBasket == other.NumBasket && DistinctPos == other.DistinctPos;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FinalizeCardInfo)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (Result.GetHashCode() * 397) ^ (Username != null ? Username.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}
