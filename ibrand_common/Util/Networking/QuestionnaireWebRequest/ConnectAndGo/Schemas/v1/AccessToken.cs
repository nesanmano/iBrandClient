using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class AccessToken
    {
        /*
         * OrderedMap { "token_type": "Bearer", "expires_in": 1296000, "access_token": "<access_token>", "refresh_token": "<refresh_token>" }
         */

        // token_type	string
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; } 
        // expires_in	int
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; } 
        // access_token	string
        [JsonProperty(PropertyName = "access_token")]
        public string AccessTokenData { get; set; } 
        // refresh_token	string
        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshTokenData { get; set; } 
    }
}