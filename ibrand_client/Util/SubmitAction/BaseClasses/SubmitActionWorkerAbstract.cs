using System.Threading;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Classe abstraite qui implémente le minimum pour faire fonctionner un worker de submit action.
    /// </summary>
    /// <typeparam name="T">Le type de SubmitAction associé au Worker.</typeparam>
    public abstract class SubmitActionWorkerAbstract<T> : ISubmitActionWorker where T : ISubmitAction
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// La référence sur le répondant qui est synchronisé.
        /// </summary>
        protected readonly Respondent _respondent;
        /// <summary>
        /// La référence du SubmitAction
        /// </summary>
        protected readonly T _action;
        /// <summary>
        /// L'identifiant numérique du répondant.
        /// </summary>
        protected readonly int _respId;
        /// <summary>
        /// Contient l'information d'exécution pour ce worker, ainsi que les informations pour son successeur, si applicable.
        /// </summary>
        protected SubmitActionWorkerNextStepInfo _stepsInfo;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="action">L'action</param>
        /// <param name="resp">Le répondant</param>
        /// <param name="respId">Son identifiant numérique</param>
        /// <param name="stepInfo">L'information d'exécution de cette étape et de la prochaine. Si null, on considère que cette 
        /// étape s'exécute et qu'il n'y pas de prochaine étape dans les workers.</param>
        protected SubmitActionWorkerAbstract(T action, Respondent resp, int respId, SubmitActionWorkerNextStepInfo stepInfo = null)
        {
            _respondent = resp;
            _action = action;
            _respId = respId;
            _stepsInfo = stepInfo;
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        /// <summary>
        /// La principale méthode de travail. Ne pas surcharger à moins d'un besoin extrêmement précis pour la gestion du prochain Worker.
        /// </summary>
        public virtual void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                if (_action.IsStopped()) return;

                var db = ServicesContainer.GetService<IbrandDatabase>();

                if (_stepsInfo?.CurrentConfig.ExecuteStep ?? true) // Check no-op
                {
                    DoWorkExecute(db, ct);
                }

                if (ct?.IsCancellationRequested ?? false) return;

                // Chaînage
                var nextStepInfo = _stepsInfo?.GetNextWorkerStepInfo();
                if (nextStepInfo != null)
                {
                    // S'il y a une prochaine étape, on instancie la classe.
                    NextWorkerStep = NextWorkerInstance(nextStepInfo);

                    // On chaîne maintenant le backup dans le bassin donnée en paramètre, s'il existe.
                    nextStepInfo.CurrentConfig.Pool?.AddWorker(NextWorkerStep);
                }
                else
                {
                    // Si pas de chaînâge, dernière étape, on ferme le répondant actuel.
                    db.SetRespondentReadyToSync(_respId);
                    _action.NewRespondentEventHandle?.Set();
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex, "Error while executing worker.");
                //_action.UiSynchro?.AddUiMessage($"Error while executing worker: {ex.Message}", 1);
            }
        }

        /// <summary>
        /// Cette méthode, quand appropriée, retourne une instance de la prochaine classe de Worker dans un chaînage de Workers, quand la classe est faite pour un chaînâge.
        /// </summary>
        /// <returns>Une instance de la prochaine classe de ISubmitWorker chaînée. Null par défaut.</returns>
        protected abstract ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo nextSteps);

        /// <summary>
        /// Méthode template à surcharger pour les classe descendantes. Les opérations doivent vérifier s'il est pertinentes qu'elles s'exécutent.
        /// En gros, ne pas faire le travail deux fois si le worker est relancé.
        /// </summary>
        /// <param name="db">Référence sur la base de données.</param>
        /// <param name="ct">Potentiel jeton de cancellation.</param>
        protected abstract void DoWorkExecute([NotNull]IbrandDatabase db, CancellationToken? ct);

        /// <summary>
        /// Obtient l'action qui englobe le travailleur.
        /// </summary>
        /// <returns>Référence vers l'action.</returns>
        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        /// <summary>
        /// L'identifiant Numérique du répondant.
        /// </summary>
        /// <returns>L'ID du répondant.</returns>
        public int GetRespId()
        {
            return _respId;
        }
    }
}