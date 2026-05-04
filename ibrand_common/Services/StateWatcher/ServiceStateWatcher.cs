using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util.Win32;
using Control = System.Windows.Forms.Control;

namespace NomadLogic.Common.Services.StateWatcher
{
    [PublicAPI]
    public class ServiceStateWatcher : ServiceLongRunningAbstract<ExternalStateReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override string ServiceName { get; protected set; } = "External component State watcher";
        private readonly List<ExternalStateWatcher> _watchers = new List<ExternalStateWatcher>();
        private bool _initDone;
        private volatile bool _working;

        public void AddStateWatchers(IList<ExternalStateWatcher> watchers)
        {
            if (_initDone)
            {
                log.Info("Cannot add watchers after initialization finishes.");
                return;
            }

            _watchers.AddRange(watchers);
        }

        public void AddStateWatchers(ExternalStateWatcher watcher)
        {
            if (_initDone)
            {
                log.Info("Cannot add watchers after initialization finishes.");
                return;
            }

            _watchers.Add(watcher);
        }

        protected override bool DoInit()
        {
            if (_initDone) return true; // Pas de double initialisation.

            _initDone = true; //  Bloque ajout sur les surveillants.
            if (_watchers.All(w => w.Init())) return true;
            log.Error("Could not initialize watchers. Initialisation of service failed.");
            _initDone = false;
            return false;
        }

        protected override async Task<bool> DoMainLoop(CancellationToken ct)
        {
            if (!_initDone) return false;

            while (!ct.IsCancellationRequested)
            {
                var checkTAsks = _watchers.Where(w => w.CheckNeeded()).Select(w => w.LaunchCheck()).ToList();

                if (checkTAsks.Any())
                {
                    _working = true;
                    await Task.WhenAll(checkTAsks);
                    
                    ReportCheckResults();
                    _working = false;
                }
                else
                {
                    log.Trace("No watcher needs to execute.");
                }

                await Task.Delay(200, ct);
            }

            return true;
        }

        protected override bool CheckIfIdle()
        {
            return _working;
        }

        private void ReportCheckResults()
        {
            var report = new ExternalStateReport(_watchers);
            TriggerReport(report, true);
        }
    }

    public class ExternalStateReport : IProgressDeepCopy<ExternalStateReport>
    {
        public ExternalStateReport(IEnumerable<ExternalStateWatcher> watchers)
        {
            GetWatcherStatusList = watchers.Select(w => (IWatcherStatus) w).ToList();
        }

        public ExternalStateReport(IEnumerable<IWatcherStatus> listToCopy)
        {
            GetWatcherStatusList = listToCopy.ToList();
        }

        public IReadOnlyList<IWatcherStatus> GetWatcherStatusList { get; }
        
        public ExternalStateReport Copy()
        {
            return new ExternalStateReport(GetWatcherStatusList);
        }
    }

    public abstract class ExternalStateWatcher : IInitialisable, IWatcherStatus
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected ServiceStateWatcherControl WinFormControl { get; set; }

        public List<ExternalStateWatcher> DependentStateWatchers { get; } = new List<ExternalStateWatcher>();

        public WatcherStatusEnum Status { get; protected set; }

        public IReadOnlyList<IWatcherStatus> SubWatchers
        {
            get
            {
                return _internalSublist?.Select(dsw => (IWatcherStatus) dsw).ToList().AsReadOnly();
            }
        }

        public bool IsChild => Parent != null;

        public IWatcherStatus Parent { get; protected set; }

        public ServiceStateWatcherControl GetWinFormStatusControl(Control parent)
        {
            WinFormControl = CreateWinFormStatusControl(parent.Width);
            WinFormControl.Parent = parent;
            return WinFormControl;
        }

        public abstract string GetDescription();

        public virtual string GetStatusDescription()
        {
            switch (Status)
            {
                case WatcherStatusEnum.NotInitialized:
                    return "Not yet initialized";
                case WatcherStatusEnum.LastCheckSuccessfull:
                    return "Last check passed";
                case WatcherStatusEnum.LastCheckFailed:
                    return "Last check failed";
                case WatcherStatusEnum.ErrorDetected:
                    return "Error";
                case WatcherStatusEnum.Skipped:
                    return "Skipped";
                case WatcherStatusEnum.ImpossibleToCheck:
                    return "Impossible to run check.";
                default:
                    return "UNKNOWN STATE";
            }
        }

        protected abstract ServiceStateWatcherControl CreateWinFormStatusControl(int width);

        protected DateTime NextCheck = DateTime.MinValue;
        private IReadOnlyList<ExternalStateWatcher> _internalSublist;

        public TimeSpan DelayBetweenChecks { get; set; } = TimeSpan.FromSeconds(1);

        public void Dispose()
        {
            var tmp = WinFormControl;
            WinFormControl = null;
            tmp?.Dispose();
            DisposeDo();
        }

        public abstract void DisposeDo();

        public bool Init()
        {
            if (_internalSublist != null) return true;

            _internalSublist = DependentStateWatchers;
            var res = InitDo();
            
            if (_internalSublist?.Any() ?? false)
            {
                foreach (var sw in _internalSublist)
                {
                    sw.Parent = this;
                    res &= sw.Init();
                }
            }

            if (!res)
            {
                _internalSublist = null;
            }

            return res;
        }

        /// <summary>
        /// Initialisation pour cette instance. Les enfants sont gérer par la classe de base.
        /// </summary>
        protected abstract bool InitDo();

        public bool CheckNeeded()
        {
            var now = DateTime.Now;
            return now > NextCheck;
        }

        /// <summary>
        /// Lance la vérification ainsi que sur les surveillant dépendents.
        /// </summary>
        /// <returns>Une référence vers cette instance, une fois la mise à jour terminée.</returns>
        public async Task<IWatcherStatus> LaunchCheck()
        {
            if (!CheckNeeded()) return this;

            var oldStatus = Status;

            if (Parent != null && Parent.Status != WatcherStatusEnum.LastCheckSuccessfull)
            {
                Status = WatcherStatusEnum.Skipped;
            }
            else
            {
                Status = await LaunchCheckDo();
            }

            var checkTAsks = DependentStateWatchers.Where(w => w.CheckNeeded()).Select(w => w.LaunchCheck()).ToList();

            if (checkTAsks.Any())
            {
                await Task.WhenAll(checkTAsks);
            }

            if (WinFormControl != null && oldStatus != Status)
            {
                WinFormControl.SafeCall(() => WinFormControl.UpdateWatcherState(this));
                log.Trace($"{GetDescription()}: Winform updated.");
            }

            return this;
        }
        
        /// <summary>
        /// Patron du "template method". Le retour du résultat est la seule chose que les implémenteur de cette classe doivent exécuter.
        /// Ne pas oublier de vérifier l'état du père, au besoin.
        /// </summary>
        protected abstract Task<WatcherStatusEnum> LaunchCheckDo();
    }

    public interface IWatcherStatus : IDisposable
    {
        /// <summary>
        /// L'état actuel général de ce surveillant.
        /// </summary>
        WatcherStatusEnum Status { get; }

        /// <summary>
        /// Liste des surveillants dépendants de cette instance.
        /// </summary>
        IReadOnlyList<IWatcherStatus> SubWatchers { get; }

        /// <summary>
        /// Indique si ce surveillant est l'enfant d'un autre.
        /// </summary>
        bool IsChild { get; }

        IWatcherStatus Parent { get; }

        /// <summary>
        /// Si on demande un contrôle Winforms, l'instance du surveillant va garder en mémoire
        /// ces contrôle et se mettre à jour automatiquement quand une changement d'état va survenir.
        /// Une fois cette méthode appellée, on va réutiliser la même instance de contrôle jusqu'à la destruction de cette classe.
        /// </summary>
        /// <returns>Une référence vers le contrôle géré par le surveillant.</returns>
        ServiceStateWatcherControl GetWinFormStatusControl(Control parent);

        /// <summary>
        /// Description rapide du surveillant.
        /// </summary>
        string GetDescription();

        /// <summary>
        /// Obtient une description lisible, selon l'implémentation, de son état.
        /// </summary>
        string GetStatusDescription();
    }

    [PublicAPI]
    public enum WatcherStatusEnum
    {
        /// <summary>
        /// État de départ.
        /// </summary>
        NotInitialized,
        /// <summary>
        /// Dernière vérification était positive.
        /// </summary>
        LastCheckSuccessfull,
        /// <summary>
        /// Derière vérification était négative.
        /// </summary>
        LastCheckFailed,
        /// <summary>
        /// Une erreur a empêcher la vérification de se conclure. On peut réessayer.
        /// </summary>
        ErrorDetected,
        /// <summary>
        /// Il a été inutile de faire la vérification car la vérification parent était négative.
        /// </summary>
        Skipped,
        /// <summary>
        /// Un événement ou un état empêche toutes les vérifications subséquentes. Cet état ne peut être enlevé une fois établi.
        /// </summary>
        ImpossibleToCheck

    }

    public class ServiceStateWatcherControl : Control
    {
        private readonly Label _labelName;
        private readonly Label _labelState;

        public ServiceStateWatcherControl([NotNull] IWatcherStatus watcher, int availiableWidth)
        {
            var totalLenght = availiableWidth - 10;
            var nameLength = (int) ( (totalLenght - 5) * 0.4);
            var stateLenght = (totalLenght - 5) - nameLength;

            _labelName = new Label
            {
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Black,
                Size = new Size(nameLength, 30),
                BorderStyle = BorderStyle.FixedSingle

            };
            _labelState = new Label
            {
                Font = new Font(FontFamily.GenericMonospace, 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.Black,
                Size = new Size(stateLenght, 30),
                BorderStyle = BorderStyle.Fixed3D
            };

            Height = Math.Max(_labelName.Height, _labelState.Height);
            Width = totalLenght;
            Controls.Add(_labelName);
            _labelName.Location = new Point(0,0);
            Controls.Add(_labelState);
            _labelState.Location = new Point(nameLength + 5, 0);

            UpdateWatcherState(watcher);
        }

        public void UpdateWatcherState([NotNull] IWatcherStatus watcher)
        {
            _labelName.Text = (watcher.IsChild ? "╠ " : "") + watcher.GetDescription();
            _labelState.Text = watcher.GetStatusDescription();
        }
    }
}