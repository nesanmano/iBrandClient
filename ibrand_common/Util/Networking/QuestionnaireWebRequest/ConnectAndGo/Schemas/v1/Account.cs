using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class Account : BaseCreationDate
    {
        /*"account": {
            "id": 1000010,
            "firstname": "Marc-Andre",
            "lastname": "Dube",
            "email": "marc-andre@connectngo.com",
            "image": "https://everest-demo.s3.ca-central-1.amazonaws.com/demopark/accounts/bpAmT85QpogHFtgDiL3ocIiWKZARYzvMDM9uK0CU.png",
            "created_at": 1594674800
        }*/

        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("firstname")]
        public string FirstName { get; set; }
        [JsonProperty("lastname")]
        public string LastName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("image")]
        public string ImageUrl { get; set; }
    }
}