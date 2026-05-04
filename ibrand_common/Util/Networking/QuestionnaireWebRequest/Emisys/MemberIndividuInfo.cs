using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class MemberIndividuInfo : IEquatable<MemberIndividuInfo>
    {
        ///Represente le id du membre
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        ///Represente le prénom du membre
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        ///Represente le nom du membre
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        ///Represente le nom du membre
        [JsonProperty(PropertyName = "aliasName")]
        public string AliasName { get; set; }

        ///Represente le nom d'utilisateur du membre
        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        ///Represente l'email du membre
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        ///Represente le date d'anniversaire du membre
        [JsonProperty(PropertyName = "birthday")]
        public string Birthday { get; set; }

        ///Represente le numéro de téléphone du membre
        [JsonProperty(PropertyName = "phonenumber")]
        public string Mobile { get; set; }

        ///Represente le mobile_device_token du membre
        [JsonProperty(PropertyName = "mobileDeviceToken")]
        public string MobileDeviceToken { get; set; }

        ///Represente le timestamp où le membre s'est conncté la dernière fois
        [JsonProperty(PropertyName = "mobileDeviceLastConnect")]
        public string MobileDeviceLastConnect { get; set; }

        ///Represente le système d'exploitation sur le device
        [JsonProperty(PropertyName = "mobileDeviceOS")]
        public string MobileDeviceOs { get; set; }

        ///Represente la langue par défaut
        [JsonProperty(PropertyName = "defaultLanguage")]
        public string DefaultLanguage { get; set; }

        ///Represente l'attribut local du membre
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

       ///Represente l'attribut artist_picture_thumb du membre
        [JsonProperty(PropertyName = "artistPictureThumb")]
        public string ArtistPictureThumb { get; set; }

        ///Represente l'attribut artist_picture_full du membre
        [JsonProperty(PropertyName = "artistPictureFull")]
        public string ArtistPictureFull { get; set; }

        ///Represente l'attribut artist_picture_lineup du membre
        [JsonProperty(PropertyName = "artistPictureLineup")]
        public string ArtistPictureLineup { get; set; }

        public bool Equals(MemberIndividuInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id) &&
                   string.Equals(FirstName, other.FirstName) &&
                   string.Equals(LastName, other.LastName) &&
                   string.Equals(AliasName, other.AliasName) &&
                   string.Equals(UserName, other.UserName) &&
                   string.Equals(Birthday, other.Birthday) &&
                   string.Equals(Mobile, other.Mobile) &&
                   string.Equals(MobileDeviceToken, other.MobileDeviceToken) &&
                   string.Equals(MobileDeviceLastConnect, other.MobileDeviceLastConnect) &&
                   string.Equals(MobileDeviceOs, other.MobileDeviceOs) &&
                   string.Equals(DefaultLanguage, other.DefaultLanguage) &&
                   string.Equals(Locale, other.Locale) &&
                   string.Equals(ArtistPictureThumb, other.ArtistPictureThumb) &&
                   string.Equals(ArtistPictureFull, other.ArtistPictureFull) &&
                   string.Equals(ArtistPictureLineup, other.ArtistPictureLineup);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MemberIndividuInfo)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FirstName != null ? LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LastName != null ? LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AliasName != null ? AliasName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Birthday != null ? Birthday.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Mobile != null ? Mobile.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MobileDeviceToken != null ? MobileDeviceToken.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MobileDeviceLastConnect != null ? MobileDeviceLastConnect.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MobileDeviceOs != null ? MobileDeviceOs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DefaultLanguage != null ? DefaultLanguage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Locale != null ? Locale.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ArtistPictureThumb != null ? ArtistPictureThumb.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ArtistPictureFull != null ? ArtistPictureFull.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ArtistPictureLineup != null ? ArtistPictureLineup.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}
