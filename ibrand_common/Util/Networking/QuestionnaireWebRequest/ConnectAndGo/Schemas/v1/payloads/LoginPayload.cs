using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads
{
    public class LoginPayload
    {
        /*
             "grant_type": "password",
             "client_id": 5,
             "client_secret": "...",
             "username": "...",
             "password": "...",
             "scope": "*"
         */

        [JsonRequired]
        [JsonProperty("grant_type")]
        public GrantTypeEnum GrantType { get; set; } = GrantTypeEnum.password;

        [JsonRequired]
        [JsonProperty("client_id")]
        public int ClientId { get; set; }

        [JsonRequired]
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonRequired]
        [JsonProperty("scope")]
        public string Scope { get; set; } = "*";

        [JsonRequired]
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonRequired]
        [JsonProperty("password")]
        public string Password { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GrantTypeEnum
    {
        password
    }
}