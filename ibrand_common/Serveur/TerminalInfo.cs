using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur
{
    /// <summary>
    /// Information complète sur un terminal, sans références externes. Contient les identifiants importants (Projet-Questionnaire-Terminal) pour toutes
    /// les tâches de recherches, ainsi que le fichier de chargement principal, au besoin.
    /// </summary>
    public class TerminalInfo
    {
        [JsonProperty( "projectId")]
        public int ProjectId { get; set; }

        [JsonProperty("questionnaireId")]
        public int QuestionnaireId { get; set; }

        /// <summary>
        /// L'identifiant unique du terminal à sauvegarder.
        /// </summary>
        [JsonProperty("terminalId")]
        public int TerminalId { get; set; }

        /// <summary>
        /// Le nom du projet, pour le marketing.
        /// </summary>
        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        /// <summary>
        /// Le nom du questionnaire du terminal, côté serveur.
        /// </summary>
        [JsonProperty("questionnaireName")]
        public string QuestionnaireName { get; set; }

        /// <summary>
        /// Le nom du questionnaire du terminal, côté serveur.
        /// </summary>
        [JsonProperty("terminalName")]
        public string TerminalName { get; set; }

        /// <summary>
        /// Le fichier principal pour charger le client avec ce terminal.
        /// </summary>
        [JsonProperty("main_file")]
        public string LoaderFile { get; set; }
    }
}