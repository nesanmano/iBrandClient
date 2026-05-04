using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class BasketInfo
    {
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "basket_status"), 
         JsonConverter(typeof(StringEnumConverter))]
        public BasketStatusEnum Status { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}