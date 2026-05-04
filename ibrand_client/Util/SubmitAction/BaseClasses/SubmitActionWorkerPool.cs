using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Classe qui exécute les classe exécutantes d'un SubmitAction une à la fois. Agit comme un gestionnaire de fils d'exécution, avec un principe de file.
    /// </summary>
    public class SubmitActionWorkerPool
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Queue<ISubmitActionWorker> _queue = new Queue<ISubmitActionWorker>();
        private volatile bool _running; // Sémaphore d'arrêt principale.
        private readonly List<SubmitActionWorkerTask> _listeWorker = new List<SubmitActionWorkerTask>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _stopFinished;
        private int _maxWorker = 1;

        /// <summary>
        /// Démarre avec les bons paramètres une instance de cette classe sur un fil d'exécution séparé.
        /// L'instance ne doit pas être nulle et ne pas avoir avoir été démarrée au préalable.
        /// </summary>
        /// <param name="instance">L'instance à démarrer.</param>
        public static void StartPoolInstance(SubmitActionWorkerPool instance)
        {
            if (instance == null || instance.IsRunning()) return;

            var t = new Thread(instance.Run)
            {
                IsBackground = true,
                Name = "SubmitAction Worker Pool" + instance.PoolName,
                Priority = ThreadPriority.Lowest
            };
            // Important, sinon l'application ne ferme pas.
            // Nom pour le thread, utile pour le débogage
            t.Start();
        }

        /// <summary>
        /// Un identifiant, optionnel, pour le bassin. Question de l'identifier correctement.
        /// Ce nom sera visible dans le nom de Thread et dans les logs.
        /// </summary>
        public string PoolName
        {
            get;
            set;
        }

        /// <summary>
        /// Nombre maximal de travilleurs. 1 par défaut. Valeurs sous un ignorées.
        /// </summary>
        public int MaxWorkerThread
        {
            get => _maxWorker;

            set
            {
                if (value > 0)
                {
                    _maxWorker = value;
                }
            }
        }

        /// <summary>
        /// Indique que le processus de fermeture s'est bien terminé.
        /// </summary>
        /// <returns>True une fois que le processus d'arrêt à été complétement terminé et que l'instance est prête à se faire détruire.</returns>
        public bool IsStopFinished()
        {
            return _stopFinished;
        }

        /// <summary>
        /// Ajoute un travailleur de type ISubmitActionWorker à la file.
        /// Attention, n'ajoute pas de travailleur si le pool n'est pas en train de fonctionner (IsRunning() == false), ou
        /// si le répondant de ce dernier est déjà dans la queue de transformation (son répondant doit être unique dans la queue);
        /// Le travailleur ainsi ajouté sera mis à la fin de la file.
        /// </summary>
        /// <param name="worker">Le travailleur à ajouter.</param>
        /// <returns>True, si le travailleur à été ajouté à la file. False s'il a été rejeté.</returns>
        public bool AddWorker(ISubmitActionWorker worker)
        {
            if (worker == null) return false;

            if (IsRunning())
            {
                lock (_queue)
                {
                    if (_queue.Any(actionWorker => actionWorker.GetRespId() == worker.GetRespId()) ||
                        _listeWorker.Any(actionWorker => actionWorker.Worker.GetRespId() == worker.GetRespId()))
                    {
                        log.Trace("Pool already contains this respondent id. Adding is rejected. ID={0}", worker.GetRespId());
                        return false;
                    }
                    _queue.Enqueue(worker);
                    log.Trace("Worker Added");
                    return true;
                }
            }
            log.Trace("Worker not added because pool is not running.");
            return false;
        }

        /// <summary>
        /// Retourne la taille de la queue de traitement, incluant l'instance de travailleur en cours.
        /// </summary>
        /// <returns>Le nombre de travailleur qui n'ont pas encore terminés.</returns>
        public int GetQueueSize()
        {
            int nbr;

            lock (_queue)
            {
                nbr = _queue.Count + _listeWorker.Count;
            }

            return nbr;
        }

        /// <summary>
        /// Demande à l'instance de stopper ses activités. IsRunning() Retournera faux immédiatement, mais il faut attendre que
        /// IsStopFinished() retourne vrai pour que tout soit définitivement arrêté.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _cts.Cancel();
            log.Debug("(Pool {0}) stop requested.", PoolName);
        }

        /// <summary>
        /// Retourne vrai si l'instance de pool est active. Par contre, "vrai" ne veut dire que l'instance est en boucle. Il est
        /// possible qu'elle soit à "faux" et que la méthode principale Run() soit en train d'attendre que le travail commencé avant
        /// le demande d'arrêt soit terminé.
        /// Pour savoir si tout est terminé, voir la méthode IsStopFinished().
        /// </summary>
        /// <returns>Vrai si l'instance est présentement en train de rouler en boucle.</returns>
        public bool IsRunning()
        {
            return _running;
        }

        /// <summary>
        /// Cette méthode ASSUME qu'elle est lancée sur un fil d'exécution séparé.
        /// Si un processus d'arrêt à été complété (IsStopFinished retourne vrai), cette méthode ne fera rien.
        /// </summary>
        public void Run()
        {
            if (_running) return;

            _stopFinished = false;
            _running = true;

            // Boucle principale
            while (_running)
            {
                lock (_queue)
                {
                    PruneFinishedWorkers();

                    if (_listeWorker.Count < MaxWorkerThread)
                    {
                        if (_queue.Count > 0)
                        {
                            var worker = _queue.Dequeue();
                            var tache = new SubmitActionWorkerTask(() => { worker.DoWork(_cts.Token); }, worker, _cts.Token);
                            _listeWorker.Add(tache);

                            log.Debug("(Pool {0}) New worker launched (ID={1}). Queue size is currently {2}. Worker count is currently {3}",
                                PoolName, worker.GetRespId(), _queue.Count, _listeWorker.Count);

                            tache.Start();
                        }
                    }
                }

                Thread.Sleep(500);
            }

            /* Quand on sort de cette boucle, il faut vérifier les cas suivants
                    * - Les tâches courantes sont-elles complétées?
                    * - Si non, on attend qu'elles le soient.
                    * - Entretemps, on vide la queue en mentionnant qu'ils ne font plus partie d'une queue à l'action (OnWorkFinished).
                    * - Une fois toutes les tâches courantes terminées, on sort de cette méthode.
                    */

            lock (_queue)
            {
                log.Debug("(Pool {0}) Stop initiated. Queue size is {1}. Worker count is {2}",
                    PoolName, _queue.Count, _listeWorker.Count);

                // On détruit la reste de la queue en mentionnant que le travail est terminé.
                var sb = new StringBuilder(_queue.Count * 7);
                var nbr = 0;
                var first = true;
                while (_queue.Count > 0)
                {
                    nbr++;
                    var temp = _queue.Dequeue();
                    FinishWorker(temp);

                    if (!log.IsDebugEnabled) continue;

                    sb.Append(temp.GetRespId());
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(nbr % 20 == 0 ? ", " : "\n");
                    }
                }

                log.Debug("(Pool {0}) Workers skipped due to stop process :\n{1}", PoolName, sb);
            }

            // On attend maintenant que les tâches en cours terminent.
            lock (_queue)
            {
                while (_listeWorker.Count > 0)
                {
                    PruneFinishedWorkers();
                    if (_listeWorker.Count <= 0) continue;

                    log.Debug("(Pool {0}) Waiting for current workers to finish. Worker count={1}",
                        PoolName, _listeWorker.Count);
                    Thread.Sleep(500);
                }
            }

            _stopFinished = true;
        }

        private void PruneFinishedWorkers()
        {
            lock (_queue)
            {
                for (var i = _listeWorker.Count - 1; i > -1; i--)
                {
                    if (_listeWorker[i] == null || !_listeWorker[i].IsCompleted) continue;

                    FinishWorker(_listeWorker[i].Worker);

                    log.Debug("(Pool {0}) Worker finished for respondent {1}",
                        PoolName, _listeWorker[i].Worker.GetRespId());

                    _listeWorker.RemoveAt(i);
                }
            }
        }

        private static void FinishWorker(ISubmitActionWorker worker)
        {
            var action = worker.GetSubmitAction();

            if (action == null) return;
            action.OnWorkFinished(worker.GetRespId());
            action.NewRespondentEventHandle?.Set();
        }

        ~SubmitActionWorkerPool()
        {
            log.Debug("(Pool {0}) Destructor called.", PoolName);
            Stop();
        }
    }
}
