using System.Runtime.Serialization;

namespace NomadLogic.Common.Util.HumanEquation.Json
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "lastname")]
        public string Lastname { get; set; }

        [DataMember(Name = "firstname")]
        public string Firstname { get; set; }

        [DataMember(Name = "company")]
        public string Company { get; set; }

        [DataMember(Name = "occupation")]
        public string Occupation { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "lng")]
        public string Lng { get; set; }

        [DataMember(Name = "uuid")]
        public string UUID { get; set; }
    }
}
