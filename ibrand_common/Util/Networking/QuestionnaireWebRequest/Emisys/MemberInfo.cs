using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class MemberInfo : IEquatable<MemberInfo>
    {
        ///Represente le id du membre
        [JsonProperty(PropertyName = "CARD_UID")]
        public string CardUid { get; set; }

        /*
        ///Represente le prénom du membre
        [JsonProperty(PropertyName = "FIRST_NAME")]
        public string FirstName { get; set; }

        ///Represente le nom du membre
        [JsonProperty(PropertyName = "LAST_NAME")]
        public string LastName { get; set; }
        */

        [JsonProperty(PropertyName = "EMISYS_NUMBER")]
        public string EmisysNumber { get; set; }

        [JsonProperty(PropertyName = "ID")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "CARD_STATUS")]
        public int CardStatus { get; set; }

        public bool Equals(MemberInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CardUid, other.CardUid) &&
                   string.Equals(EmisysNumber, other.EmisysNumber) &&
                   Id == other.Id &&
                   CardStatus == other.CardStatus;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MemberInfo)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            var hashCode = (EmisysNumber != null ? EmisysNumber.GetHashCode() : 0);
            return hashCode;
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}
