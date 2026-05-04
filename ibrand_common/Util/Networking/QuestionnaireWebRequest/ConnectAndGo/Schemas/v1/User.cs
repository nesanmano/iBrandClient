using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class User
    {
        /*
         * OrderedMap { "id": 2, "email": "arthur@connectngo.com", "firstname": "Arthur", "lastname": "Evenard", "company": "Connect&GO" }
         */

        // id	int
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        // firstname	string
        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; set; }
        // lastname	string
        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; set; }
        // email	string
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        // company	string
        [JsonProperty(PropertyName = "company")]
        public string Company { get; set; }
    }
}