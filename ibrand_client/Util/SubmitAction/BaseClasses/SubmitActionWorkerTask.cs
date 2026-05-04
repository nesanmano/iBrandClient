using System;
using System.Threading;
using System.Threading.Tasks;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Classe de tâche avec lien vers le Worker qui agit.
    /// </summary>
    internal class SubmitActionWorkerTask : Task
    {
        /// <summary>
        /// Construteur principal.
        /// </summary>
        /// <param name="a">L'action à exécuter.</param>
        /// <param name="worker">L'instance à exécuter.</param>
        /// <param name="ct">Le jeton de cancellation.</param>
        public SubmitActionWorkerTask(Action a, ISubmitActionWorker worker, CancellationToken ct)
            : base(a, ct, TaskCreationOptions.LongRunning)
        {
            Worker = worker;
        }

        /// <summary>
        /// Obtient la référence vers le SubmitActionWorker que l'on surveille.
        /// </summary>
        public ISubmitActionWorker Worker
        {
            get;
        }
    }
}