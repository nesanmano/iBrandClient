using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads
{
    public class ProcessAccessScanPayload
    {
        [JsonProperty("x-tenant-id")]
        public long XtenantId { get; set; }
        [JsonProperty("gate_name")]
        public string GateName { get; set; }
        [JsonProperty("uid")]
        public string UniqueId { get; set; }
        [JsonProperty("access_point_id")]
        public int AccessPointId { get; set; }
        [JsonProperty("direction")]
        public string Direction { get; set; }
    }
}