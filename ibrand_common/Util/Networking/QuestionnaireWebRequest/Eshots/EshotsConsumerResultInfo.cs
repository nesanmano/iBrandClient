using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsConsumerResultInfo : EshotsResult
    {
        [JsonProperty(PropertyName = "consumer")]
        public EshotsConsumerIdentificationResult ConsumerIdentificationResult { get; set; }
    }
}