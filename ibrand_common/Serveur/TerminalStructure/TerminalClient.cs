using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur.TerminalStructure
{
    // TODO C'est quoi exactement cette classe: L'identification pour télécharger un terminal?
    public class TerminalClient
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
