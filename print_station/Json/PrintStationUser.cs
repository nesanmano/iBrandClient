using System.Runtime.Serialization;

namespace NomadLogic.PrintStation.Json
{
    [DataContract]
    public class PrintStationUser
    {
        public PrintStationUser()
        {
        }

        public PrintStationUser(int id, int featureId, int eventId)
        {
            Id = id;
            FeatureId = featureId;
            EventId = eventId;
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "feature_id")]
        public int FeatureId { get; set; }

        [DataMember(Name = "event_id")]
        public int EventId { get; set; }
    }
}