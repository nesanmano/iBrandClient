using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsCampaign
    {
        [JsonProperty(PropertyName = "campaign")]
        public string CampaignName { get; set; }

        [JsonProperty(PropertyName = "clientLicenseId")]
        public int ClientLicenseId { get; set; }

        [JsonProperty(PropertyName = "events")]
        public IList<EshotsEvent> Events { get; set; }

        public override string ToString()
        {
            return $"{CampaignName} - ({ClientLicenseId})";
        }
    }
}