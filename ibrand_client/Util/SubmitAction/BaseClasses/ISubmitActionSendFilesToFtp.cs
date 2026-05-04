namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Interface pour les méthodes requise d'une SubmitAction pour l'envoi vers un site FTP.
    /// </summary>
    public interface ISubmitActionSendFilesToFtp : ISubmitAction
    {
        /// <summary>
        /// usager pour l'authentification. Sensible à la case.
        /// </summary>
        string FtpUser { get; }

        /// <summary>
        /// Mot de passe pour l'authentification. Sensible à la case.
        /// </summary>
        string FtpPassword { get; }

        /// <summary>
        /// Adresse IP du serveur FTP,
        /// </summary>
        string FtpAdress { get; }

        /// <summary>
        /// Port FTP
        /// </summary>
        int FtpPort { get; }

        /// <summary>
        /// Définition du répertoire où placer les éléments.
        /// Si vide ou null, ce sera la répertoire racine de l'usager FTP.
        /// Utilise des jetons pour déterminer certains éléments dynamiques. Voir la méthode "StringUtil.CreateSubDirectoryFromTokenizedPattern"
        /// </summary>
        string FtpDir { get; }

        /// <summary>
        /// Liste des questions qui contiennent les fichiers à envoyer sur le FTP.
        /// </summary>
        int[] FtpQuestions { get; }

        /// <summary>
        /// Indique si les fichiers sont tous obligatoire ou non. Si cette option est fausse, et que au moins une des questions ne possède pas de fichier valide,
        /// toute l'opération est considérée un échec, aucun fichier ne sera envoyé.
        /// </summary>
        bool FtpOptionalFiles { get; }

        /// <summary>
        /// Indique s'il faut détruire les fichiers une fois transférés vers le FTP.
        /// </summary>
        bool FtpDestroyFiles { get; }

        /// <summary>
        /// Indique qu'il faut quand même finaliser le répondant, même s'il y a une erreur au niveau des fichiers inexistant.
        /// Une erreur de communication IP ne provoquera pas ce comportement: puisque l'on peut réessayer plus tard. Ne concerne que des fichiers
        /// inexistants.
        /// </summary>
        bool FtpSynchRespondentWithMissingFiles { get; }

        /// <summary>
        /// Saute complétement la phase d'édition vidéo. La question contient déjà un vidéo valide.
        /// </summary>
        bool NoVideoEdit { get; }

        /// <summary>
        /// Ne fait pas la tentative de transfert du vidéo vers le Repository de Nomad Logic.
        /// Implique habituellement que l'on a pas besoin d'une copie supplémentaire.
        /// </summary>
        bool NoNomadFtp { get; }
    }
}