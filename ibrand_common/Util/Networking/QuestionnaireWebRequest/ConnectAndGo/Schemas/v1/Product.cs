using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class Product : BaseNameI18N
    {
        /*"product": {
            "id": 1000016,
            "name": "48 pouces et plus",
            "image": "https://everest-demo.s3.ca-central-1.amazonaws.com/demopark/products/E6KxsYSkN5foMXzNdvsGfpaCB7yWI6zzdKWPTSI2.jpeg",
            "price": "$299.00",
            "amount": 299,
            "min_quantity": null,
            "max_quantity": null,
            "increment": null,
            "time_based": false,
            "event_group_id": null,
            "event_group_name": null
        }*/

        [JsonProperty("image")]
        public string ImageUrl { get; set; }
        [JsonProperty("price")]
        public string Price { get; set; }
        [JsonProperty("amount")]
        public double Amount { get; set; }
        [JsonProperty("min_quantity")]
        public string QuantityMin { get; set; }
        [JsonProperty("max_quantity")]
        public string QuantityMax { get; set; }
        [JsonProperty("increment")]
        public string Increment { get; set; }
        [JsonProperty("time_based")]
        public bool TimeBased { get; set; }
        [JsonProperty("event_group_id")]
        public string EventGroupId { get; set; }
        [JsonProperty("event_group_name")]
        public string EventGroupName { get; set; }
    }
}