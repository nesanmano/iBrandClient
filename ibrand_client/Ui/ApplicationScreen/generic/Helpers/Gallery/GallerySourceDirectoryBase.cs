using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MCollections;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Util;
using Timer = System.Timers.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery
{
    /// <summary>
    /// Classe de base pour une source de gallerie qui utilise un répertoire spécifique de travail comme source complète ou partielle
    /// de fichier d'image à présenter. Comment les fichiers sont obtenus et gérés dépend de l'implémentation.
    /// </summary>
    public abstract class GallerySourceDirectoryBase : IGallerySource<ComparableFileInfo>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected readonly List<IElementMonitor<ComparableFileInfo>> _monitors = new List<IElementMonitor<ComparableFileInfo>>();
        protected IndexedSet<ComparableFileInfo> _fileSet = new IndexedSet<ComparableFileInfo>();
        protected bool _initialized;
        protected volatile bool _disposing;
        private Timer _timer;
        private Task _updateEventTask;
        private CancellationTokenSource _cts;
        
        protected bool IsInitialized;

        protected bool IsDisposing => _disposing;

        public TimeSpan UpdateEventTime = TimeSpan.FromMilliseconds(5000);
        public ISaveToQuestionnaireFormatter<ComparableFileInfo> SaveFormatter { get; set; }
        public DirectoryInfo WorkDirectory { get; set; }

        public void Dispose()
        {
            _disposing = true;
            if (_timer == null) return;
            _timer.Elapsed -= TimerOnElapsed;
            _timer.Stop();

            _cts?.Cancel();
            _cts = null;
            _updateEventTask = null;

            lock (_fileSet)
            {
                _fileSet.Clear();
            }

            ClearAllElementMonitors();

            ExtraDispose();
        }

        /// <summary>
        /// Étapes supplémentaires pour la destruction de l'instance.
        /// </summary>
        protected virtual void ExtraDispose()
        {
            // Rien par défaut.
        }

        public bool Init()
        {
            if (_disposing || _timer != null) return false;

            _timer = new Timer
            {
                AutoReset = true,
                Interval = UpdateEventTime.TotalMilliseconds
            };

            if (!ExtraInitialization())
            {
                return false;
            }

            return WorkDirectory?.Exists ?? false;
        }

        /// <summary>
        /// Initialisation supplémentaires pour classes enfants.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ExtraInitialization()
        {
            return true;
        }

        public bool StartUpdates()
        {
            if (_disposing || _timer == null) return false;

            if (_timer.Enabled) return true;

            if (!WorkDirectory.Exists)
            {
                _initialized = false;
                return false;
            }
            
            _timer.Start();
            _timer.Elapsed += TimerOnElapsed;

            // On lance immédiatement un "event", de façon asynchrone.
            //https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
            TimerOnElapsed(null, null);

            return true;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_updateEventTask != null)
            {
                log.Trace("Update task still alive. Skipping event event.");
                return;
            }

            _cts = new CancellationTokenSource(); 
            _updateEventTask = Task.Factory.StartNew(LaunchEventUpdate, _cts.Token, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private async Task LaunchEventUpdate()
        {
            try
            {
                await UpdateEventDo();
            }
            catch (System.Exception e)
            {
                log.Warn(e, @"Last gallerie update event raised an exception.");
            }
            finally
            {
                _updateEventTask = null;
                _cts = null;
            }
        }

        /// <summary>
        /// Méthode principale pour lancer le travail.
        /// </summary>
        protected abstract Task UpdateEventDo();

        public bool StopUpdates()
        {
            if (_disposing || _timer == null) return false;

            _timer.Stop();
            _cts?.Cancel();
            return true;
        }

        public bool AddElementMonitor(IElementMonitor<ComparableFileInfo> monitor)
        {
            if (_disposing || _timer == null) return false;

            lock (_fileSet)
            {
                if (_monitors.Contains(monitor)) return false;
                _monitors.Add(monitor);

                if (_initialized) monitor.UpdateElements(_fileSet);
            }
            return true;
        }

        public bool RemoveElementMonitor(IElementMonitor<ComparableFileInfo> monitor)
        {
            if (_disposing || _timer == null) return false;

            lock (_fileSet)
            {
                return _monitors.Remove(monitor);
            }
        }

        public void ClearAllElementMonitors()
        {
            lock (_fileSet)
            {
                _monitors.Clear();
            }
        }
    }
}