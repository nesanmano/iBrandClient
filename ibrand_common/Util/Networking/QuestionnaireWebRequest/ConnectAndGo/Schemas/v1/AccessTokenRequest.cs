using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    [PublicAPI]
    public class AccessTokenRequest
    {
        /*
         OrderedMap { "grant_type": "<password> or <uid>", "client_id": 1, "client_secret": "xxxxxxx", "username": "arthur@connectngo.com", "password": "xxxxxx", "uid": "xxxxxx", "scope": "*" }
         */

        // grant_type	string
        [JsonProperty(PropertyName = "grant_type")]
        public GrantTypeEnum GrantType { get; set; } 
        // client_id	int
        [JsonProperty(PropertyName = "client_id")]
        public int ClientId { get; set; }
        // client_secret	string
        [JsonProperty(PropertyName = "client_secret")]
        public string ClientSecret { get; set; }
        // username	string
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }
        // password	string
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
        // uid	string
        [JsonProperty(PropertyName = "uid")]
        public string Uid { get; set; }
        // scope	string
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }


    }
}