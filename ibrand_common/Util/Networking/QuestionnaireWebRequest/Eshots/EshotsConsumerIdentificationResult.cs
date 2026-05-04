using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsConsumerIdentificationResult
    {
        [JsonIgnore]
        public EshotsComplexRequestResultEnum ExecutionResult { get; set; } = EshotsComplexRequestResultEnum.Unknown;

        /// <summary>
        /// La définition des informations d'un consommateur.
        /// </summary>
        [JsonProperty(PropertyName = "answers")]
        public EshotsConsumerDataInfo ConsumerData { get; set; }
    }
}