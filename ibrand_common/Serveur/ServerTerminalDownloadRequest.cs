using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur
{
    /// <summary>
    /// Structure d'information pour télécharger un terminal du serveur principal.
    /// </summary>
    public class ServerTerminalDownloadRequest
    {
        /// <summary>
        /// Usager pour identifier le terminal.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Mot de passe pour le terminal.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Identifiant unique du projet, venant du projet.
        /// </summary>
        [JsonProperty("Pid")]
        public int ProjectId { get; set; }

        /// <summary>
        /// Identifiant unique du questionnaire, venant du server.
        /// </summary>
        [JsonProperty("Qid")]
        public int QuestionnaireId { get; set; }

        /// <summary>
        /// Identifiant unique du terminal, venant du serveur.
        /// </summary>
        [JsonProperty("Tid")]
        public int TerminalId { get; set; }

    }
}
