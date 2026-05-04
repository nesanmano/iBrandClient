using System.Runtime.Serialization;

namespace NomadLogic.Common.Util.HumanEquation.Json
{
    [DataContract]
    public class EventScanResponse : Response 
    {
        public EventScanResponse() : base()
        {

        }

        [DataMember(Name = "data")]
        public Data ReturnData { get; set; }

        [DataContract]
        public class Data : ResponseData
        {

            public Data()
            {
            }

            [DataMember(Name = "eventName")]
            public string EventName { get; set; }

            [DataMember(Name = "createdAt")]
            public string CreatedAt { get; set; }

            [DataMember(Name = "eventData")]
            public EventData EventData { get; set; }
        }
    }
}
