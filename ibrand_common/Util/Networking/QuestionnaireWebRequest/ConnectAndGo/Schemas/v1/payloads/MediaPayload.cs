using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads
{
    public class MediaPayload
    {
        [JsonIgnore]
        public int XtenantId { get; set; }

        [JsonProperty("wallet_id")]
        public int WalletId { get; set; }

        [JsonProperty("source_id")]
        public int SourceId { get; set; }

        [JsonProperty("access_point_id")]
        public int AccessPointId { get; set; }

        [JsonProperty("medias")]
        public List<Media> Medias { get; set; }
    }
}