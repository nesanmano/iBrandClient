using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class CardValueInfo : IEquatable<CardValueInfo>
    {
        ///Represente le Id de la carte
        [JsonProperty(PropertyName = "ID")]
        public string CardId { get; set; }

        ///Represente le numéro de la carte
        [JsonProperty(PropertyName = "CARD_NUMBER")]
        public string CardNumber { get; set; }

         ///Represente le numéro d'emisys      
        [JsonProperty(PropertyName = "EMISYS_NUMBER")]
        public string EmisysNumber { get; set; }

         ///Represente le numéro du membre
        [JsonProperty(PropertyName = "MEMBER_ID")]
        public string MemberId { get; set; }

        ///Represente le uid de la carte
        [JsonProperty(PropertyName = "CARD_UID")]
        public string CardUid { get; set; }

        /// Represente le statut de la carte
        [JsonProperty(PropertyName = "CARD_STATUS")]
        public string CardStatus { get; set; }

        /* // N'est plus valide 2019-10-25
        /// Represente le member
        [JsonProperty(PropertyName = "MEMBER")]
        public MemberInfo Member { get; set; }
        */

        #region Equality methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() !=GetType()) return false;
            return Equals((CardValueInfo)obj);
        }

        public bool Equals(CardValueInfo other)
        {
           
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CardId, other.CardId) &&
                   string.Equals(CardNumber, other.CardNumber) &&
                   string.Equals(EmisysNumber, other.EmisysNumber) &&
                   string.Equals(MemberId, other.MemberId) &&
                   string.Equals(CardUid, other.CardUid) &&
                   string.Equals(CardStatus, other.CardStatus)
                //   && Equals(Member, other.Member)
                ;

        }

        public int CompareTo(CardValueInfo other)
        {
            if (other == null) return 1;
            if (CardId == null && other.CardId == null) return 0;
            return string.Compare(CardId, other.CardId, StringComparison.Ordinal);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CardId != null ? CardId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CardNumber != null ? CardNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EmisysNumber != null ? EmisysNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MemberId != null ? MemberId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CardUid != null ? CardUid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CardStatus != null ? CardStatus.GetHashCode() : 0);
                //hashCode = (hashCode * 397) ^ (Member != null ? Member.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}
