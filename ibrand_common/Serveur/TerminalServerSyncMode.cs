namespace NomadLogic.Common.Serveur
{
    /// <summary>
    /// La liste des mode d'envoi vers le serveur. Certains se recoupent, mais la plupart sont mutuellement exclusifs.
    /// </summary>
    public enum TerminalServerSyncMode
    {
        /// <summary>
        /// Utilise la synchronisation via fichiers pour créer un nouveau répondant à chaque finalisation de questionnaire.
        /// N'envoie pas de réponses dynamiques.
        /// Ceci est une méthode "legacy".
        /// </summary>
        CreateAnswersOnly,
        /// <summary>
        /// Créer un nouveau répondant complet à chaque finalisation de questionnaire.
        /// </summary>
        Create,
        /// <summary>
        /// Consolide les informations d'un répondant via un identifiant externe unique, et un end-point précis.
        /// </summary>
        Consolidate
    }
}