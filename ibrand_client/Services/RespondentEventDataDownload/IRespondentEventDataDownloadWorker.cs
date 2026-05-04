using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NomadLogic.Common.Serveur;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload
{
    /// <summary>
    /// Interface pour implémenter un travailleur d'un téléchargment.
    /// </summary>
    public interface IRespondentEventDataDownloadWorker
    {
        /// <summary>
        /// Méthode pour traiter un téléchargement. Cette méthode doit être thread-safe: c'est à dire qu'elle doit contenir toutes les informations dans son contexte d'appel, et ne
        /// pas travailler avec des variable d'instance modifiable. En gros, une instance soit être en mesure de faire des appels répétés et simultanés de cette méthode en toute
        /// sécurité, et ce de façon asynchrone (en gros, doit être async).
        /// </summary>
        /// <param name="ct">Le jeton d'arrêt de l'appel.</param>
        /// <param name="currentServerConf">La configuration serveur.</param>
        /// <param name="service">Le service appellant.</param>
        /// <param name="dr">La classe de configuration de base du téléchargement.</param>
        /// <param name="respondentBundleFinalData">Le "contenant" des valeurs finales créées par cette méthode. Son extrant, en somme.</param>
        /// <returns></returns>
        Task DoDownload(CancellationToken ct, 
            [NotNull] ServerConfiguration currentServerConf, 
            [NotNull] RespondentEventDataDownloadService service, 
            [NotNull] RespondentBundleRequest dr, 
            [NotNull] RespondentBundleFinalData respondentBundleFinalData);

        /// <summary>
        /// Méthode qui sert à valider la configuration de l'instance par elle-même.
        /// </summary>
        /// <returns>FALSE si l'instance détecte que sa configuration est invalide pour exécution.</returns>
        bool Validate();
    }
}