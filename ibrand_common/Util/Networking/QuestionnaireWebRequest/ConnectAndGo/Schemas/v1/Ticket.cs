using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class Ticket
    {
        /*"ticket": {
        "id": 327,
        "ticket_type": {
            "id": 1000002,
            "name": "Fulltime membership",
            "name_i18n": "{}",
            "validity": {
                "id": 1000002,
                "name": "season admission",
                "name_i18n": "{}",
                "created_at": 1588714152
            },
            "status": "ACTIVE",
            "issuing": "WALLET",
            "created_at": 1588714415
        },
        "event": null,
        "transaction": {
            "id": 1000276,
            "transaction_id": "DEM-238",
            "created_at": 1611144507
        },
        "product": {
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
        },
        "created_at": 1611144508
    } */
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("ticket_type")]
        public TicketType TicketTypes { get; set; }
        [JsonProperty("event")]
        public string Event { get; set; }
        [JsonProperty("transaction")]
        public Transaction Transaction { get; set; }
        [JsonProperty("product")]
        public Product Product { get; set; }
    }
}