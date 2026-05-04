namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    public struct PairOfPoolExecute
    {
        public PairOfPoolExecute(SubmitActionWorkerPool pool, bool executeStep)
        {
            Pool = pool;
            ExecuteStep = executeStep;
        }

        /// <summary>
        /// Le bassin d'exécution de l'étape. Si null, on n'utilise pas les bassins d'exécution (l'exécution est sur un même fil).
        /// </summary>
        public SubmitActionWorkerPool Pool { get; }
        /// <summary>
        /// Indique que l'étape actuelle est nécessaire. Si faux, il s'agit d'un no-op, et on passe au prochain (si existant).
        /// </summary>
        public bool ExecuteStep { get; }
    }
}