namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public enum EshotsComplexRequestResultEnum
    {
        /// <summary>
        /// L'opération est un succès.
        /// </summary>
        Success,
        /// <summary>
        /// Une erreur de programmation est survenue. Vérifier le code.
        /// </summary>
        Error,
        /// <summary>
        /// Problème avec l'envoi via Internet. On peut réessayer plus tard.
        /// </summary>
        InternetProblem,
        /// <summary>
        /// Jour invalide. La sauvegarde ne peut être utilisée.
        /// </summary>
        InvalidDay,
        /// <summary>
        /// Les informations Eshots sont insuffisantes. On doit renouveller les information venant de la campagne.
        /// </summary>
        RequireLogin,
        /// <summary>
        /// L'opération intermédiaire s'est bien déroulée. On peut continuer. (Plus utilisée à l'interne du paquetage).
        /// </summary>
        Continue,
        /// <summary>
        /// Opération générale annulée.
        /// </summary>
        Cancelled,
        /// <summary>
        /// État inconnu. Utilisé surtout quand cette énumération n'est pas utile.
        /// </summary>
        Unknown,
        /// <summary>
        /// La commande n'a pas pu être exécutée sur le serveur de Eshots, même après s'y être rendu.
        /// </summary>
        ServerError
    }
}