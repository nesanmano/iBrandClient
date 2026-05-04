using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class ProductInfo
    {
        [JsonProperty(PropertyName = "quantity")]
        public float Quantity { get; set; }

        [JsonProperty(PropertyName = "product_id")]
        public int ProductId { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}