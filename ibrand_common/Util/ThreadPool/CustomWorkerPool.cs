using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util.ThreadPool
{
    /// <summary>
    /// Classe qui gère une queue de traitements via des instance de travail. Ces instances ne sont pas disponibles en tant que telles, mais sont
    /// automatiquement générées via la méthode AddCustomWorker().
    /// <p/>Il est très important qu'une fois l'instance créée, on appelle la méthode statique StartPoolInstance() pour lancer son exécution
    /// sur un fil séparé. À moins que l'on ne veuille utiliser l'instance de façon synchrone, auquel cas il faut appeller ses méthodes
    /// séparéments.
    /// <typeparam name="TP">Le type d'objet pour les paramètres de traitement d'un travailleur.</typeparam>
    /// <typeparam name="TR">Le type d'objet pour le retour du traitement. Le résultat quoi.</typeparam>
    /// </summary>
    [PublicAPI]
    public class CustomWorkerPool<TP, TR>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public delegate void CustomWorkerFinishedHandler(bool success, TR result);
        public event CustomWorkerFinishedHandler WorkerFinished;

        private volatile bool _running; // Sémaphore d'arrêt principale.
        private readonly Queue<CustomWorker<TP, TR, CustomWorkerPool<TP, TR>>> _queue = new Queue<CustomWorker<TP, TR, CustomWorkerPool<TP, TR>>>();
        private readonly LinkedList<CustomWorkerTask<TP, TR, CustomWorkerPool<TP, TR>>> _listeActive = new LinkedList<CustomWorkerTask<TP, TR, CustomWorkerPool<TP, TR>>>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private volatile bool _paused;
        private volatile int _nbrActive;

        /// <summary>
        /// Constructeur de base. Met le nombre de travailleur maximal à 1;
        /// </summary>
        public CustomWorkerPool()
        {
            MaxWorkers = 1;
        }

        /// <summary>
        /// Démarre avec les bons paramètres une instance de cette classe sur un fil d'exécution séparé.
        /// L'instance ne doit pas être nulle et ne pas avoir avoir été démarrée au préalable.
        /// </summary>
        /// <param name="instance">L'instance à démarrer.</param>
        /// <param name="threadName">Nom pour le fil d'exécution, utile pour le débogage.</param>
        public static void StartPoolInstance(CustomWorkerPool<TP, TR> instance, string threadName)
        {
            if (instance == null || instance.IsRunning()) return;
            var t = new Thread(instance.Run)
            {
                IsBackground = true,
                Name = threadName,
                Priority = ThreadPriority.Lowest
            };
            // Important, sinon l'application ne ferme pas.

            t.Start();
        }

        /// <summary>
        /// Provoque un arrêt du traitement asynchrone de cette instance.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _cts.Cancel();
            StateUpdate?.Invoke();
            if (!ActiveLog) return;
            lock (_queue)
            {
                log.Debug("(Pool) stop requested.");
            }
        }

        /// <summary>
        /// Met l'opération en pause. Une pause n'empêche aucun traitement en cours, mais empêche d'en démarrer un nouveau.
        /// Ne touche pas à la file existante.
        /// </summary>
        public void Pause()
        {
            _paused = true;
            StateUpdate?.Invoke();
        }

        /// <summary>
        /// Enlève le drapeau de pause, si présent.
        /// </summary>
        public void UnPause()
        {
            _paused = false;
            StateUpdate?.Invoke();
        }

        /// <summary>
        /// Indique si on est en pause. Une pause indique que la file n'est pas traitée.
        /// </summary>
        public bool IsPaused()
        {
            return _paused;
        }

        /// <summary>
        /// Indique s'il faut journaliser les événements de cette instance. Faux par défaut.
        /// </summary>
        public bool ActiveLog
        {
            get;
            set;
        }

        public bool IsRunning()
        {
            return _running;
        }

        /// <summary>
        /// Obtient un "Type" valide pour cette instance de bassin d'exécution basé sur un nom de classe, avec ou sans candidat de "namespace".
        /// Permet de tester si une classe potentielle est valide au lieu de s'en rendre compte plus tard.
        /// </summary>
        /// <param name="className">Le nom de la classe à tester.</param>
        /// <param name="candidateNamespace">En cas d'erreur si on utilise le nom tel tel, remplace le namespace par celui-ci.</param>
        /// <returns>NULL si le type n'est pas valide pour cette instance, un Type valide sinon.</returns>
        public Type ClassTypeCheckFromName(string className, string candidateNamespace = null)
        {
            var res = ClassUtil.InstanciateClassFromName<CustomWorker<TP, TR, CustomWorkerPool<TP, TR>>>(className, candidateNamespace);
            return res?.GetType();
        }

        /// <summary>
        /// Ajoute un CustomWorker à la queue.
        /// La demande sera encapsulée dans un travailleur et mise en file pour être traitée.
        /// <p/>Ne fait rien si l'instance ne roule pas, vérifiable avec IsRunning. Ne fait rien si on ne fournit pas
        /// un objet de paramètre correct au travailleur (non-nul).
        /// </summary>
        /// <param name="clazz">Le type de classe à instancier par réflection.</param>
        /// <param name="parameter">L'objet de paramètre à donner à la nouvelle instance.</param>
        /// <param name="workerId">L'identifiant numérique du travailleur.</param>
        /// <param name="logger">La référence de l'instance qui doit journaliser les messages.</param>
        /// <param name="extraInit">Une référence à une méthode d'initialisation d'extra, pour les cas difficile et pour contourner les difficultés d'initialisation de classe spécifique avec de la réflexion.</param>
        /// <returns>NULL si la méthode à fonctionnée, sinon une référence vers une Exception</returns>
        public Exception AddCustomWorker(int workerId, Type clazz, TP parameter, IEventLogger logger, 
            Func<CustomWorker<TP, TR, CustomWorkerPool<TP, TR>>, bool> extraInit = null)
        {
            Exception exception = null;

            if (clazz == null) return null;
            if (!IsRunning()) {
                log.Trace("Worker not added because pool is not running.");
                return null;
            }
            
            try
            {
                var tmp = (CustomWorker<TP, TR, CustomWorkerPool<TP, TR>>)Activator.CreateInstance(clazz);
                tmp.Id = workerId;
                tmp.Pool = this;
                tmp.Parameter = parameter;
                tmp.LoggerReference = logger;
                
                if (extraInit != null)
                {
                    var r = extraInit.Invoke(tmp);
                    if (!r)
                    {
                        throw new Exception("Could not execute extra initialisation step for worker instance.");
                    }
                }

                tmp.DoPreparationWork();

                lock (_queue)
                {
                    _queue.Enqueue(tmp);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Could not instanciate custom worker.");
                exception = e;
            }
            
            return exception;
        }

        public Exception AddWorker(CustomWorker<TP, TR, CustomWorkerPool<TP, TR>> oldWorker)
        {
            Exception exception = null;

            if (oldWorker == null) return null;

            if (!IsRunning()) {
                log.Trace("Worker not added because pool is not running.");
                return null;
            }
            
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(oldWorker);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Could not instanciate custom worker.");
                exception = e;
            }
            
            return exception;
        }

        /// <summary>
        /// Retourne la taille de la queue de traitement, incluant les instances en cours.
        /// </summary>
        /// <returns>Le nombre de travailleurs qui n'ont pas encore terminés.</returns>
        public int GetQueueSize()
        {
            return _nbrActive;
        }

        /// <summary>
        /// Cette méthode ASSUME qu'elle est lancée sur un fil d'exécution séparé.
        /// </summary>
        protected virtual void Run()
        {
            _running = true;

            // Boucle principale
            while (_running)
            {
                if (!_paused)
                {

                    lock (_queue)
                    {
                        // On vérifie l'état de complétion des tâches en cours.
                        var node = _listeActive.First;

                        while (node != null)
                        {
                            var tmp = node.Value;

                            if (tmp.IsCompleted)
                            {
                                var current = node;
                                node = node.Next;
                                _listeActive.Remove(current);
                                OnWorkerFinished(tmp.Worker.Success, tmp.Worker.WorkerResult);
                            }
                            else
                            {
                                node = node.Next;
                            }
                        }

                        _nbrActive = _queue.Count + _listeActive.Count;

                        if (_queue.Count > 0 && _listeActive.Count < MaxWorkers)
                        {
                            var check = _queue.Peek();
                            if (!IsDelayed(check))
                            {
                                var worker = _queue.Dequeue();

                                PreTaskExecution(worker);

                                var tache = new CustomWorkerTask<TP, TR, CustomWorkerPool<TP, TR>>(() => worker.DoWork(_cts.Token))
                                {
                                    Worker = worker
                                };
                                _listeActive.AddLast(tache);
                                if (ActiveLog)
                                {
                                    var sb = new StringBuilder();
                                    sb.Append("(Pool) New worker launched. Queue size is ");
                                    sb.Append(_queue.Count);
                                    sb.Append(", Worker size is ");
                                    sb.Append(_listeActive.Count);
                                    sb.Append(" {ID=");
                                    sb.Append(worker.Id);
                                    sb.Append(", Parameters=");
                                    sb.Append(ToStringParameter(worker.Parameter));
                                    sb.Append("}");

                                    log.Debug(sb.ToString());
                                }

                                tache.Start();
                                StateUpdate?.Invoke();
                            }
                            else
                            {
                                log.Debug("Worker queue delayed by current worker check.");
                            }
                        }
                    }
                }

                Thread.Sleep(500);
            }

            // On tente de stoppper toute les travailleurs restants.
            _queue.Clear();

            foreach (var tache in _listeActive)
            {
                try
                {
                    tache.Dispose();
                } catch(Exception) { /* Rien */ }
            }
        }

        private bool IsDelayed(CustomWorker<TP, TR, CustomWorkerPool<TP, TR>> check)
        {
            var now = DateTime.Now;
            return check.TimeDelay > now;
        }

        /// <summary>
        /// À exécuter avant de transférer le travailleur dans la liste asynchrone.
        /// <br/>Rien par défaut, il faut surcharger pour y ajouter des opérations.
        /// </summary>
        /// <param name="worker">Le travailleur qui va ensuite être mis dans la liste.</param>
        protected virtual void PreTaskExecution(CustomWorker<TP, TR, CustomWorkerPool<TP, TR>> worker)
        {
            // Rien par défaut.
        }

        /// <summary>
        /// Méthode qui décrit de façon succinte un objet du type du paramètre.
        /// Par défaut, ne fait qu'un ToString(), mais peut être surchargée au besoin.
        /// </summary>
        /// <param name="tp">Le paramètre à décrire.</param>
        /// <returns></returns>
        public virtual string ToStringParameter(TP tp)
        {
            return tp?.ToString();
        }

        /// <summary>
        /// Indique qu'un traitement a terminé.
        /// </summary>
        /// <param name="success">Avec succès ou non.</param>
        /// <param name="result">Référence (possible) vers le résultat du traitement.</param>
        internal void OnWorkerFinished(bool success, TR result)
        {
            WorkerFinished?.Invoke(success, result);
            StateUpdate?.Invoke();
        }

        /// <summary>
        /// Retourne le nombre de travailleur présentement en train d'envoyer des fichiers.
        /// </summary>
        /// <returns>Le nombre de travailleur actuels.</returns>
        public int GetCurrentWorkerCount()
        {
            return _listeActive.Count;
        }

        /// <summary>
        /// Nombre maximal de travailleurs simultané.
        /// </summary>
        public int MaxWorkers
        {
            get;
            set;
        }

        /// <summary>
        /// Événement sporadique qui avertit les écoutant que l'état interne du bassin à changé. Est lancé à chaque fois qu'un travailleur commence ou arrête,
        /// ainsi qu'a chaque état forcé par un appellant externe.
        /// </summary>
        public event Action StateUpdate;
    }
}
