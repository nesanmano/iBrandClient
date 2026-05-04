using System.Runtime.Serialization;

namespace NomadLogic.PrintStation.Json
{
    [DataContract]
    public class UiFile
    {
        public UiFile()
        {
        }

        public UiFile(int id, int featureId, string type, string src)
        {
            Id = id;
            FeatureId = featureId;
            Type = type;
            Src = src;
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "feature_id")]
        public int FeatureId { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "src")]
        public string Src { get; set; }
    }
}