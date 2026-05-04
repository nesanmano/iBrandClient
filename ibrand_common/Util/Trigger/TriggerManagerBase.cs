using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Trigger
{
    [PublicAPI]
    [JsonConverter(typeof(TriggerManagerConverter))]
    public abstract class TriggerManagerBase : IDisposable, IExternalFileConfig, IJsonConfig, IInitialisable, IWatcherListUpdate, IReportObserver<TriggerManagerReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private DateTime? _minTime;
        private readonly object _lock = new object();
        private readonly List<IProgress<TriggerManagerReport>> _observerList = new List<IProgress<TriggerManagerReport>>();
        private TriggerManagerReport _lastMessage;

        /// <summary>
        /// Détruit toutes les ressources systèmes de cette instance.
        /// Si un suivi du périphérique est en cours, il sera stoppé au préalable.
        /// </summary>
        public virtual void Dispose()
        {
            lock (_lock)
            {
                _observerList.Clear();
            }
        }

        /// <summary>
        /// Indique si l'instance est présentement en train de surveiller le déclencheur.
        /// </summary>
        [JsonIgnore]
        public abstract bool IsRunning { get; protected set; }

        /// <summary>
        /// Indique si le déclencheur est prêt à être utilisé.
        /// </summary>
        [JsonIgnore]
        public abstract bool IsInitialized { get; protected set; }

        /// <summary>
        /// Initialise le lien entre le matériel du déclencheur et l'instance.
        /// </summary>
        /// <returns>TRUE si le périphérique est connecté et prêt à être utilisé.</returns>
        public abstract bool Init();

        /// <summary>
        /// Démarre un fil d'exécution qui vérifie périodiquement si le déclenchement est survenu.
        /// <br/>Ne démarre que si la méthode init() retourne TRUE.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stoppe la vérification, mais ne détruit pas le gestionnaire.
        /// <br/>Ne fait rien si la vérification n'est pas en cours.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Délégué pour la méthode de "callback" lorsque un déclenchement arrive.
        /// </summary>
        public delegate void TriggerEventHandler();

        /// <summary>
        /// Événement appellé lorsque le déclencheur est activé.
        /// </summary>
        public event TriggerEventHandler Triggered;

        /// <summary>
        /// La période de délais entre deux "trigger", pour éviter de surcharger le système de messagerie.
        /// Par défaut, 500 milisecondes.
        /// Format: Days.Hours:Minutes:Seconds:Milliseconds
        /// </summary>
        [JsonConverter(typeof(TimespanConverter))]
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public TimeSpan TriggerDeactivationTime { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Lève le event via invocation.
        /// </summary>
        protected void InvokeTrigger()
        {
            lock (_lock)
            {
                var n = DateTime.Now;
                var update = false;
                if (_minTime == null)
                {
                    Triggered?.Invoke();
                    update = true;
                }
                else if (_minTime <= n)
                {
                    Triggered?.Invoke();
                    update = true;
                }

                if (!update) return;

                _minTime = n + TriggerDeactivationTime;
                
                if (!log.IsTraceEnabled) return;
                TriggerReport(new TriggerManagerReport {StatusMessage = $"Triggered: next possible trigger in {TriggerDeactivationTime.ToStringHourMinSec(true, true)}"});
            }
        }

        protected DateTime? GetNextTimeForTrigger()
        {
            return _minTime;
        }

        public string ConfigFileName { get; set; }

        public abstract string DefaultConfigFileName { get; }

        public virtual void UpdateWatcherList(IList<ExternalStateWatcher> watcherList)
        {
            // Rien par défaut
        }

        public void RemoveObserver(IProgress<TriggerManagerReport> observer)
        {
            lock (_observerList)
            {
                _observerList.Remove(observer);
            }
        }

        public void AddObserver(IProgress<TriggerManagerReport> observer)
        {
            lock (_observerList)
            {
                _observerList.Add(observer);
            }
        }

        public void TriggerReport(TriggerManagerReport report, bool dontCopy = false)
        {
            lock (_observerList)
            {
                _lastMessage = dontCopy ? report : report.Copy();

                foreach (var progress in _observerList)
                {
                    progress.Report(report);
                }
            }
        }

        public void SendLastReport()
        {
            TriggerReport(_lastMessage);
        }
    }

    public class TriggerManagerReport : IProgressDeepCopy<TriggerManagerReport>
    {
        public string StatusMessage { get; set; }

        public TriggerManagerReport Copy()
        {
            return new TriggerManagerReport
            {
                StatusMessage = StatusMessage
            };
        }
    }
}