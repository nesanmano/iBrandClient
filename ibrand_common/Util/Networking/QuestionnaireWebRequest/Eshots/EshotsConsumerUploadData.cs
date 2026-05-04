using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    /// <summary>
    /// Structure utilisée pour associer des fichiers en téléversement à un compte.
    /// </summary>
    public class EshotsConsumerUploadData
    {
        [JsonProperty(PropertyName = "uId")]
        public string ConsumerUniqueId { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "eventTokenId")]
        public string EventTokenId { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}