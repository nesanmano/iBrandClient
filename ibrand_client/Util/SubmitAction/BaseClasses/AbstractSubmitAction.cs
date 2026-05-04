using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Cette classe contient les commandes de base pour l'exécution et le traitement des fonctions de base pour une action de traitement post-création d'un répondant.
    /// <br/>En gros, chaque sous-classe représente une action à exécuter pour créer ou modifier des valeurs du questionnaire, ou pour créer d'autres fichiers
    /// multimédia à partir du questionnaire, comme la création d'un vidéo ou l'envoi de fichiers sur un serveur.
    /// </summary>
    public abstract class AbstractSubmitAction : ISubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected TerminalInfo _terminalInfo;
        protected internal volatile bool _stopped;
        protected internal readonly Dictionary<string, string> _configData = new Dictionary<string, string>();
        
        /// <summary>
        /// Indique que le traitement d'un répondant a terminé.
        /// </summary>
        public event OnRespondentFinishedDelegate OnRespondentFinish;
        /// <summary>
        /// Indique qu'un répondant à été sauvegardé au préalable d'un traitement.
        /// </summary>
        public event OnSavedDelegate OnRespondentSaved;
        /// <summary>
        /// Indique que la file de traitement de l'action a été complètement stoppée, et que l'instance peut être détruite.
        /// </summary>
        public event OnCompleteStopDelegate OnCompleteStopReached;

        /// <summary>
        /// AbstractConfiguration d'un projet plein écran.
        /// </summary>
        public ScreenConfig ScreenConfiguration
        {
            get;
            set;
        }

        /// <summary>
        /// Permet de notifier le système qu'un nouveau répondant est prêt à être synchronisé.
        /// </summary>
        public AsyncAutoResetEvent NewRespondentEventHandle { get; private set; }

        /// <summary>
        /// L'interface de synchronisation, si utilisée. Peut donc être nul.
        /// </summary>
        /// <returns>L'instance d'objet UI pour l'affichage de l'état de la transformation.</returns>
        public IUiSynchro UiSynchro { get; private set; }

        /// <summary>
        /// La liste des bassins d'exécution liés à l'action post-traitement.
        /// </summary>
        protected readonly IList<SubmitActionWorkerPool> _submitActionWorkerPools = new List<SubmitActionWorkerPool>();

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        protected AbstractSubmitAction()
        {
            InstanciateSubmitActionPools();
        }

        /// <summary>
        /// Cette classe vérifie si toutes les configurations externes critiques sont disponibles.
        /// </summary>
        protected virtual void ValidateConfigData()
        {
            // Rien par défaut.
        }

        /// <summary>
        /// Si l'action en possède, instancier ici tous les SubmitActionWorkerPools que doit utiliser le SubmitAction.
        /// Il est important de ne pas les démarrer! Si possible utilisez la méthode AddSubmitActionWorkerPool.
        /// </summary>
        protected abstract void InstanciateSubmitActionPools();

        /// <summary>
        /// Méthode utilitaire pour ajouter des SubmitActionWorkerPool sans les démarrer.
        /// </summary>
        /// <param name="poolName">Le nom du fil d'exécution.</param>
        /// <param name="maxWorkerThread">Le nombre maximal d'exécution simultanées.</param>
        protected void AddSubmitActionWorkerPool(string poolName, int maxWorkerThread)
        {
            var tmp = new SubmitActionWorkerPool
            {
                MaxWorkerThread = maxWorkerThread,
                PoolName = poolName
            };
            if (_submitActionWorkerPools == null) return;
            _submitActionWorkerPools.Add(tmp);
            log.Trace("Pool added: " + poolName);
        }

        /// <summary>
        /// Lance l'initialisation de l'instance. Ne démarre pas les bassins d'exécution. Pour cela il faut invouqer la méthode
        /// "StartWorkerPools".
        /// </summary>>
        /// <param name="tInfo">L'information de terminal</param>
        /// <param name="newRespHandle"></param>
        public void Init(TerminalInfo tInfo, AsyncAutoResetEvent newRespHandle)
        {
            _terminalInfo = tInfo;
            NewRespondentEventHandle = newRespHandle;

            // Référence: http://stackoverflow.com/questions/485659/can-net-load-and-parse-a-properties-file-equivalent-to-java-properties-class
            var path = DirectoryUtil.GetLocalIbrandTerminalDir(tInfo) + "config.txt";
            
            _configData.Clear();
            ExtractConfigFileData(path);

            ValidateConfigData();

            AdditionalInitOperations();
        }

        private void ExtractConfigFileData(string path)
        {
            if (!File.Exists(path)) return;

            foreach (var row in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                if (row.StartsWith("#")) continue;
                var tab = row.Split('=');
                var rest = string.Concat(tab.Skip(1).ToArray());
                if (!_configData.ContainsKey(tab[0]))
                    _configData.Add(tab[0], rest);
                else
                    _configData[tab[0]] = rest;
            }
        }

        /// <summary>
        /// Démarre les bassins d'exécutions. Si on invoke cette méthode, il faut par la suite invoquer "StopProcessing" pour forcer l'arrêt
        /// des traitements en cours avant de détruire cette instance.
        /// </summary>
        public void StartWorkerPools()
        {
            log.Trace("Starting all worker pools.");
            foreach (var pool in _submitActionWorkerPools)
            {
                SubmitActionWorkerPool.StartPoolInstance(pool);
            }
        }

        /// <summary>
        /// Opération d'initialisation additionnelles à exécuter par les classes enfants.
        /// </summary>
        protected virtual void AdditionalInitOperations()
        {
            // Vide par défaut
        }

        /// <summary>
        /// Quand un répondant a terminé son traitement.
        /// </summary>
        /// <param name="respondentId">L'identifiant du répondant.</param>
        public void OnWorkFinished(int respondentId)
        {
            OnRespondentFinish?.Invoke(respondentId);
        }

        /// <summary>
        /// À invoquer à chaque fois que l'on doit indiquer qu'un répondant à été sauvegardé dans la BD au préalable d'un traitement.
        /// </summary>
        protected void RespondentSaved()
        {
            OnRespondentSaved?.Invoke();
        }

        /// <summary>
        /// Indique quand l'action sait avec certitude qu'aucun traitement n'est présentement en cours et que la boucle
        /// de traitement est terminée.
        /// </summary>
        protected void CompleteStop()
        {
            OnCompleteStopReached?.Invoke();
        }

        /// <summary>
        /// Retourne l'identifiant du questionnaire avec lequel cette instance a été initialisée.
        /// </summary>
        public int QuestionnaireId => _terminalInfo.QuestionnaireId;

        /// <summary>
        /// Retourne l'identifiant numérique du projet avec lequel cette instance a été initialisée.
        /// </summary>
        public int ProjectId => _terminalInfo.ProjectId;

        public int TerminalId => _terminalInfo.TerminalId;

        public TerminalInfo TerminalInfo => _terminalInfo;

        public string GetTerminalDirectory => DirectoryUtil.GetLocalIbrandTerminalDir(_terminalInfo);

        /// <summary>
        /// Lance une commande pour que tous les bassins en exécution soient arrêtés le plus vite possible.
        /// <br/>Cet arrêt n'est pas immédiat: les traitements en cours sont normalement terminés, mais tous ceux en attentes seront enlevés.
        /// </summary>
        public virtual void StopProcessing()
        {
            if (_stopped) return;
            _stopped = true;
            log.Trace("Stopping all worker pools.");
            foreach (var pool in _submitActionWorkerPools)
            {
                pool.Stop();
            }

            new Task(() =>
            {
                do
                {
                    try
                    {
                        Thread.Sleep(500);
                    }
                    catch (System.Exception)
                    {
                        // die nice
                    }
                } while (!IsStopped());

                CompleteStop();
            }).Start();
        }

        /// <summary>
        /// Indique si l'instance est complètement stoppée. Implique que tous les bassins d'exécutions ont terminé leur traitement.
        /// </summary>
        /// <returns>TRUE si et seulement si tous les bassins sont stoppés.</returns>
        public bool IsStopped()
        {
            return _submitActionWorkerPools.Aggregate(_stopped, (current, pool) => current & pool.IsStopFinished());
        }

        /// <summary>
        /// Soumet un nouveau répondant à la file de transformation.
        /// <br/>Un effet de bord de cette méthode est que le répondant sera inséré dans la base de données, mais pas (encore)
        /// flaggé comme prêt à être envoyé vers le serveur.
        /// </summary>
        /// <param name="respondent">Le répondant à soumettre.</param>
        /// <returns>TRUE si le répondant fait partie de la file de transformation. FALSE s'il a été rejeté.</returns>
        public virtual bool SubmitRespondent(Respondent respondent)
        {
            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();
                var respId = db.InsertRespondent(respondent, false);

                var tmpResp = db.GetRespondent(respId);
                tmpResp.Answers = db.GetAnswers(respId);
                tmpResp.DynamicHeaders = db.GetDynamicHeaders(respId);

                var temp = CreateWorker(tmpResp);

                _submitActionWorkerPools[0].AddWorker(temp);

                RespondentSaved();

                return true;
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Could not save respondent and assign a new reference before SubmitAction sequence.");
                return false;
            }
        }

        /// <summary>
        /// Re-créer un worker pour un répondant particulier, et le met dans la queue d'exécution.
        /// <br/>À noter que le répondant doit satisfaire à tous les critères suivants:
        /// - Il doit déjà être sauvegardé dans la BD.
        /// - Il doit être associé au même projet que la Submit Action.
        /// - Il ne doit pas être complété (not ready to synch).
        /// - Il n'est pas déjà dans la queue de transformation.
        /// </summary>
        /// <param name="respondentId">L'identifiant du répondant à ré-essayer.</param>
        /// <returns>TRUE s'il a été remis dans la queue. FALSE s'il ne satisfait pas aux pré-conditions.</returns>
        public virtual bool ReSubmitRespondent(int respondentId)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();

            var respondent = db.GetRespondent(respondentId);
            if (respondent == null) return false;
            if (respondent.IsReadyToSync) return false;
            
            respondent.Answers = db.GetAnswers(respondentId);
            respondent.DynamicHeaders = db.GetDynamicHeaders(respondentId);

            return _submitActionWorkerPools[0].AddWorker(CreateWorker(respondent));
        }

        /// <summary>
        /// Créé un Worker avec les paramètres propre à la classe exécutante.
        /// </summary>
        /// <param name="resp">La référence du répondant</param>
        /// <param name="usePool">Indique s'il faut utiliser les bassins d'exécution (fonctionnement asynchrone), si applicable. TRUE par défaut.
        /// Habituellement, n'est utilisé que dans les cas où il y a plusieurs Workers qui sont chainés ensemble.</param>
        /// <returns>Un Worker fonctionnel.</returns>
        protected abstract ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true);

        /// <summary>
        /// Exécute tous les répondant qui ont été sauvegardés, mais dont le traitement ne s'est pas effectué, et ce, de façon
        /// synchrone. Idéalement, il ne faut pas avoir lancé les bassins avec la commande "StartWorkerPools", car l'exécution est synchrone.
        /// Aussi, la fonction "StopProcessing" ne fonctionnera pas non plus, car on n'utilise pas les fils d'exécutions.
        /// TODO: Enlever cette méthode. Ça devrait être le service de synchro qui identifie les répondants à travailler.
        /// </summary>
        [Obsolete]
        public virtual void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentIdsBySyncStatus(QuestionnaireId);

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var respId in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var respondent = db.GetRespondent(respId);

                respondent.Answers = db.GetAnswers(respId);
                respondent.DynamicHeaders = db.GetDynamicHeaders(respId);

                var temp = CreateWorker(respondent, false);
                var step = 0;
                do
                {
                    UiSynchro?.AddUiMessage($"Doing step {++step} of SubmitAction", 4);
                    temp.DoWork(ct);
                    temp = temp.NextWorkerStep;
                } while (temp != null && ct != null && !ct.Value.IsCancellationRequested);

                UiSynchro?.AddProgressStep(1);
            }
        }
    }
}
