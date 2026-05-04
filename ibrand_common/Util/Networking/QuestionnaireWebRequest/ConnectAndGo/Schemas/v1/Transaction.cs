using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class Transaction : BaseCreationDate
    {
        /*
         *  "id": 1036651, 
            "transaction_id": "SAC-36599",
            "created_at": 1601497326
         */
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }
    }
}