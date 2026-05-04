using System.Runtime.Serialization;

namespace NomadLogic.Common.Util.HumanEquation.Json
{
    [DataContract]
    public class EventData
    {
        
        [DataMember(Name = "rfid")]
        public string RFID { get; set; }

        [DataMember(Name = "user")]
        public User User { get; set; }
    }
}
