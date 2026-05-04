using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class MemberCardInfo : IEquatable<MemberCardInfo>
    {
        /// Represente le resultat de la requête
        [JsonProperty(PropertyName = "result")]
        public bool RequestResult { get; set; }

        /// Represente les valeurs asscociés à la carte
        [JsonProperty(PropertyName = "value")]
        public CardValueInfo CardValue { get; set; }
        
        public bool Equals(MemberCardInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RequestResult == other.RequestResult && Equals(CardValue, other.CardValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MemberCardInfo) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (RequestResult.GetHashCode() * 397) ^ (CardValue != null ? CardValue.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}
