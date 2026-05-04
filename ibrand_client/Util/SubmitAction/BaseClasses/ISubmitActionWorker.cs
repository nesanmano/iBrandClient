using System.Threading;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Interface de base pour une classe d'exécution d'un SubmitAction.
    /// </summary>
    public interface ISubmitActionWorker
    {
        /// <summary>
        /// Méthode à exécuter sur un fil d'exécution.
        /// </summary>
        void DoWork(CancellationToken? ct);

        /// <summary>
        /// Référence vers le prochain Worker, dans le cas où ils sont chaînés entre eux.
        /// </summary>
        ISubmitActionWorker NextWorkerStep { get; set; }

        /// <summary>
        /// Obtient le AbstractSumbitAction propre à cet exécutant.
        /// </summary>
        /// <returns>La référence du AbstractSubmitAction</returns>
        ISubmitAction GetSubmitAction();

        /// <summary>
        /// Obtient le numéro du répondant traité.
        /// </summary>
        /// <returns>L'identifiant numérique du répondant.</returns>
        int GetRespId();
    }
}
