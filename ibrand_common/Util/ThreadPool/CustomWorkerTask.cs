using System;
using System.Threading.Tasks;

namespace NomadLogic.Common.Util.ThreadPool
{
    /// <summary>
    /// Tâche asynchrone gardant un oeil sur une tâche personalisé.
    /// <typeparam name="TP">Le type d'objet pour les paramètres de traitement d'un travailleur.</typeparam>
    /// <typeparam name="TR">Le type d'objet pour le retour du traitement. Le résultat quoi.</typeparam>
    /// </summary>
    public class CustomWorkerTask<TP, TR, T> : Task where T : CustomWorkerPool<TP, TR>
    {
        public CustomWorkerTask(Action action)
            : base(action)
        {
            // Rien
        }

        public CustomWorker<TP, TR, T> Worker
        {
            get;
            set;
        }
    }
}