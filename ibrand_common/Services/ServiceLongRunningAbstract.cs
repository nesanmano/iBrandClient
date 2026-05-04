using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Util;
using static NomadLogic.Common.Services.LongRunningStatus;

namespace NomadLogic.Common.Services
{
    /// <summary>
    /// Implémentation de base pour un service de longue durée asynchrone.
    /// </summary>
    public abstract class ServiceLongRunningAbstract<T> : IService, IAsyncLongRunning, IReportObserver<T> where T : IProgressDeepCopy<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Task _mainLoop;
        private CancellationTokenSource _cts;
        private bool _initCompleted;
        private bool _stopCalled;
        private LongRunningStatus _lastState;
        private readonly List<IProgress<T>> _reportObservers = new List<IProgress<T>>();
        private T _lastReport;

        private LimitedConcurrencyLevelTaskScheduler _lcts;
        private TaskFactory _factory;

        /// <summary>
        /// On peut s'abonner à cet événement pour savoir exactement quand le Status de ce service change.
        /// Le seul état qui ne peut être intercepté est l'état de nouvellement créé.
        /// </summary>
        public EventHandler<LongRunningStatus> StatusChanged;
        
        /// <summary>
        /// À invoquer à chaque fois que l'état change de façon claire.
        /// Permet aux classes d'implémentation d'agir sur un événement.
        /// Si le nouvel état est le même que le dernier, l'événement ne sera pas lancé (pour éviter des spams).
        /// </summary>
        /// <param name="status">Le nouveau status.</param>
        protected void OnStatusChanged(LongRunningStatus status)
        {
            if (status == _lastState) return;
            _lastState = status;
            StatusChanged?.Invoke(this, status);
        }

        /// <summary>
        /// Niveau maximal de concurence voulu sur le ThreadPool.
        /// </summary>
        private int MaxConcurrency { get; set; } = 5;

        /// <summary>
        /// Stoppe et détruit l'instance.
        /// Cette méthode va provoquer l'appel de "StopLongRunning" et va bloquer de façon synchrone jusqu'à la fin de l'arrêt si le service
        /// n'a pas été arrêter au préalable. Ce pourquoi il est recommandé d'appeler "StopLongRunning" avant de façon asynchrone pour s'assurer
        /// d'une exécution correcte.
        /// Quand Tout est arrêté, on appelle le template method pour que les enfants puisse détruire des informations de leur cru.
        /// </summary>
        public void Dispose()
        {
            if (_stopCalled) return;
            _stopCalled = true;
            _factory = null;
            _lcts = null;
            if (_mainLoop == null) return;
            _cts.Cancel();
            _mainLoop?.ContinueWith(p =>
            {
                OnStatusChanged(Stopped);
                log.Warn("Main loop stopped in Dispose for service '{0}'", ServiceName);
                _cts = null;
                _mainLoop = null;
                OnDispose();
            });
        }

        protected virtual void OnDispose()
        {
            // Rien par défaut;
        }

        /// <summary>
        /// Indique le nom du service. Est lié à la classe exécutante.
        /// </summary>
        public abstract string ServiceName { get; protected set; }

        /// <summary>
        /// Cette implémentation s'assure qu'il n'y a qu'une seule initialisation de fait par instance. Retourne false si l'initialisation à échouée, ou si
        /// la classe ne peut plus être initialisée (I.E. Un appel pour stopper à été lancé).
        /// </summary>
        /// <returns>Retourne vrai si l'exécution: 1) s'est bien effectué. 2) À déjà été effectuée avec succès (no-op). 3) Ou est en train d'être exécutée. (no-op)</returns>
        public bool Init()
        {
            if (_stopCalled) return false;
            if (_initCompleted || _mainLoop != null) return true;

            if (!DoInit()) return false;
            OnStatusChanged(Idle);
            _initCompleted = true;
            _lcts = new LimitedConcurrencyLevelTaskScheduler(MaxConcurrency);
            _factory = new TaskFactory(_lcts);

            return _initCompleted;
        }

        /// <summary>
        /// Implémentation directe de l'initialisation.
        /// Quand cette méthode est appellée, l'exécution du service n'est pas en cours.
        /// Basé sur le concept de Template Method.
        /// </summary>
        /// <returns>Indique si l'initialisation à fonctionnée.</returns>
        protected abstract bool DoInit();

        public Task<bool> StartLongRunning()
        {
            log.Debug("Start Long running is called");
            if (_stopCalled) return Task.FromResult(false);
            if (!_initCompleted) return Task.FromResult(false);
            
            // Si déjà partit on ne fait rien de plus.
            if (_mainLoop != null && !_mainLoop.IsCompleted) return Task.FromResult(false);

            _cts = new CancellationTokenSource();
            void A() => DoMainLoop(_cts.Token);
            _mainLoop = _factory.StartNew(A, _cts.Token);
            OnStatusChanged(Status);
            return Task.FromResult(true);
        }

        public bool IsStartedAndRunning()
        {
            if (_mainLoop == null) return false;
            if (_mainLoop.IsCompleted) return false;

            switch (Status)
            {
                case ServiceCreated:
                case Stopped:
                case Paused:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Boucle principale de la classe d'implémentation. Doit vérifier le plus souvent possible
        /// les conditions de sortie données via le jeton de cancellation. Cette boucle doit rester en vie tout le long
        /// à moins que la mette en pause.
        /// Si jamais une boucle venait à être stoppée, On peut repartir le processus immédiatement avec un appel avec "StartLongRunning".
        /// </summary>
        /// <param name="ct">Le jeton de cancellation.</param>
        /// <returns>True si la boucle à été lancé avec succès.</returns>
        protected abstract Task<bool> DoMainLoop(CancellationToken ct);

        public async Task<bool> Pause()
        {
            if (_mainLoop == null) return true;
            var tmpRef = _mainLoop;
            _mainLoop = null;
            _cts.Cancel();
            _cts = null;
            await tmpRef.ContinueWith(p =>
            {
                OnStatusChanged(Paused);
                log.Debug("Service '{0}' is paused.", ServiceName);
            });
            return true;
        }

        public LongRunningStatus Status
        {
            get
            {
                if (_stopCalled && _cts == null && _mainLoop == null) return Stopped;
                if (_mainLoop == null && _initCompleted) return Paused;
                // Working or Idle
                if (_mainLoop != null)
                {
                    return CheckIfIdle() ? Idle : Working;
                }
                return ServiceCreated;
            }
        }

        /// <summary>
        /// Méthode interne qui détermine si la boucle principale est en attente où si elle est présentement en train
        /// de travailler. Dépend de l'implémentation.
        /// </summary>
        /// <returns>TRUE si et seulement si la boucle principale est active et qu'elle est en attente de quelque chose à faire.</returns>
        protected abstract bool CheckIfIdle();

        public async Task StopLongRunning()
        {
            if (_stopCalled) return;
            _stopCalled = true;
            if (_mainLoop == null) return;
            _cts.Cancel();
            await _mainLoop.ContinueWith(p =>
            {
                OnStatusChanged(Stopped);
                log.Debug("Main loop stopped for service '{0}'", ServiceName);
                _cts = null;
                _mainLoop = null;
            });
        }

        public void AddObserver(IProgress<T> observer)
        {
            lock (_reportObservers)
            {
                _reportObservers.Add(observer);
            }
        }

        public void RemoveObserver(IProgress<T> observer)
        {
            lock (_reportObservers)
            {
                _reportObservers.Remove(observer);
            }
        }

        public void TriggerReport(T report, bool dontCopy = false)
        {
            if (report == null) return;

            lock (_reportObservers)
            {
                var copy = dontCopy ? report : report.Copy();
                if (_reportObservers.Any())
                {
                    foreach (var observer in _reportObservers)
                    {
                        observer.Report(copy);
                    }
                }

                _lastReport = copy;
            }
        }

        public void SendLastReport()
        {
            TriggerReport(_lastReport);
        }
    }
}