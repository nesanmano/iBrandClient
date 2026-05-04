using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur.TerminalStructure
{
    public class TerminalStructureData
    {
        /// <summary>
        /// Identifiant unique du terminal sur le serveur.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Type énumératif du terminal. Normalement, on ne devrait recevoir dans cette application que des type "Windows".
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Nom du terminal. Pour humains.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// L'identifiant du questionnaire associé (pour confirmations).
        /// </summary>
        [JsonProperty("associatedQuestionnaireId")]
        public int AssociatedQuestionnaireId { get; set; }

        /// <summary>
        /// Liste des langues disponibles pour ce terminal. Normalement, doit être un sous-ensemble des langues
        /// du questionnaire.
        /// </summary>
        [JsonProperty("languages")]
        public string[] Languages { get; set; }

        /// <summary>
        /// Fichiers sur le serveur associés à ce terminal, et requis pour son activation sur client.
        /// </summary>
        [JsonProperty("filesToDownload")]
        public ResourceFileToDownload[] FilesToDownload { get; set; }

    }
}
