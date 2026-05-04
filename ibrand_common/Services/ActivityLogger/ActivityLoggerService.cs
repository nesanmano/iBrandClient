using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Services.ActivityLogger
{
    public class ActivityLoggerService : ServiceLongRunningAbstract<ActivityLoggerReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override string ServiceName { get; protected set; } = "Activity Logger";
        
        private ManualResetEventSlim NewLogEntryGiven { get; } = new ManualResetEventSlim(false);

        private readonly BlockingQueue<Activity> _mainQueue = new BlockingQueue<Activity>(1000);

        private volatile Activity _currentActivity = null;

        private ActivityLoggerDatabase _database;
        private int _nbrActivityAdded;

        public string ActivityDatabaseFilePath { get; }

        public ActivityLoggerService(string filePath)
        {
            ActivityDatabaseFilePath = filePath;
        }

        protected override bool DoInit()
        {
            _database = new ActivityLoggerDatabase(ActivityDatabaseFilePath);
            _nbrActivityAdded = 0;
            return _database.Init();
        }

        protected override async Task<bool> DoMainLoop(CancellationToken ct)
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Service has not been initialized.");
            }

            log.Trace("Synchronization service main loop started.");

            while (!ct.IsCancellationRequested)
            {
                NewLogEntryGiven.Wait(ct);
                
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // Add entries to DB
                while (_mainQueue.Count > 0)
                {
                    _mainQueue.TryDequeue(out var act);
                    _database.AddActivityReport(act);
                    _nbrActivityAdded++;
                    
                    TriggerReport(new ActivityLoggerReport
                    {
                        NewLogEntriesSinceStart = _nbrActivityAdded
                    }, true);
                }

                NewLogEntryGiven.Reset();

                if (ct.IsCancellationRequested)
                {
                    break;
                }
            }

            log.Debug("Synchronization service main cycle stopped.");
            return true;
        }

        protected override bool CheckIfIdle()
        {
            return _mainQueue.Count == 0 && _currentActivity == null;
        }

        public void LogActivity(Activity act)
        {
            if (act == null) return;
            _mainQueue.Enqueue(act);
            NewLogEntryGiven.Set();
        }

        public void LogActivity(int terminalId, ActivityLoggerActionType acti, string details = null)
        {
            LogActivity(new Activity
            {
                ActionType = acti,
                Details = details,
                TerminalId = terminalId
            });
        }

        public IList<ActivityReportItem> GetActivityReport(int terminalId, DateTime? minDate = null, DateTime? maxDate = null, ActivityLoggerActionType? actionType = null)
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Service has not been initialized.");
            }

            return _database.GetActivityReport(terminalId, actionType, minDate, maxDate);
        }
    }
}