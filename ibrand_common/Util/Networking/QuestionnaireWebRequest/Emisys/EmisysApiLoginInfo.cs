// ReSharper disable InconsistentNaming

using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    [PublicAPI]
    public class EmisysApiLoginInfo : IJsonConfig
    {
        /// Addresse constante pour l'API d'Emisys.
        public string RootApiUrl { get; set; }

        /// Clé unique liée à l'identifiant courriel pour utiliser le système Emisys.
        public string Apikey { get; set; }

        /// Identifiant courriel de l'application qui fait les demandes vers leur système.
        public string UserEmail { get; set; } = "api@nomadlogic.com";

        /// Version de l'API complète. Inclure le "v". Exemple: "v1"
        public string Version { get; set; } = "v1";

        [JsonIgnore]
        public string DefaultConfigFileName => "emisys_config.json";
    }
}