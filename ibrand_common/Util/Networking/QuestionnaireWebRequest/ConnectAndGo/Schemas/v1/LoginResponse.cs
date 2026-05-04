using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class LoginResponse
    {
        [JsonRequired]
        [JsonProperty("token_type")]
        public TokenTypeEnum TokenType { get; set; }

        [JsonRequired]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonRequired]
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonRequired]
        [JsonProperty("expires_in")]
        public long ExpirationTime { get; set; }

        public string GetCompleteHeader()
        {
            return $"{TokenType} {AccessToken}";
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenTypeEnum
    {
        Bearer
    }
}