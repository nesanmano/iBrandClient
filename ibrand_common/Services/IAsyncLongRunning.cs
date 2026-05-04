using System.Threading.Tasks;

namespace NomadLogic.Common.Services
{
    /// <summary>
    /// Cette interface défini les différents appels et états généraux pour un service de longue durée.
    /// En gros, on peut partir le service, l'arrêter, le mettre en pause (et pouvoir le repartir).
    /// </summary>
    public interface IAsyncLongRunning
    {
        /// <summary>
        /// Démarre ou re-démarre le service. Assume une initialisation correcte.
        /// </summary>
        /// <returns>TRUE si l'état à changé. FALSE sinon. Ce qui représente un no-op. Dans un cas grave,
        /// il est préférable de lancer une exception.</returns>
        Task<bool> StartLongRunning();

        /// <summary>
        /// Arrête momentanément le service. Peut ne pas se faire immédiatement selon la nature de l'exécution du service, d'où
        /// le fait que cette méthode retourne un Task. Quand cette tâche est stoppée, la pause est effective.
        /// </summary>
        /// <returns>True si l'état à changé. False représente un no-op.</returns>
        Task<bool> Pause();

        /// <summary>
        /// Retourne l'état actuel du service. N'est pas bloqueur et peut ne pas représenter l'état subséquent.
        /// </summary>
        LongRunningStatus Status { get; }

        /// <summary>
        /// Stoppe définitivement le service. Une fois stoppé, on ne peut le réactiver.
        /// </summary>
        /// <returns>La tâche d'arrêt. Une fois cette tâche terminé le service est complétement stoppé.</returns>
        Task StopLongRunning();
    }

    /// <summary>
    /// Liste des états possibles d'un service longue durée.
    /// Quand l'instance est nouvellement créée, L'état est "Created".
    /// Une fois partit, le service longue durée est soit "Idle" ou "Working".
    /// On peut à ce moment le mettre en "Paused", un état où le service ne fait rien mais garde ses
    /// ressources actives. On peut repartir le service en utilisant la méthode de départ.
    /// Si on stoppe le service, il sera mis en état "Stopped", et une fois terminé il n'est plus possible
    /// de le réactiver.
    /// </summary>
    public enum LongRunningStatus
    {
        ServiceCreated,
        Idle,
        Working,
        Paused,
        Stopped
    }
}