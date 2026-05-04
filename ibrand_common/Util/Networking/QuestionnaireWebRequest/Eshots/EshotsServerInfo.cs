using Newtonsoft.Json;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    /// <summary>
    /// Information du serveur Eshots. Il ne contient aucune information de Login (user/pass) pour des raisons de sécurité,
    /// et que le système est fait pour renouveler ces informations manuellement et de façon régulière.
    /// </summary>
    public class EshotsServerInfo : IJsonConfig
    {
        /// Addresse constante pour l'API d'Emisys. Se termine par un "slash"
        public string RootApiUrl { get; set; } = @"https://eshots.io/";

        /// Version de l'API complète. Inclure le "v". Exemple: "v1"
        public string Version { get; set; } = "v1";

        [JsonIgnore]
        public string DefaultConfigFileName => "eshots_config.json";
    }
}