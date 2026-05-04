using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Synchro.Report;
using NomadLogic.iBrandDesktop.Synchro.SynchFinishers;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using static NomadLogic.Common.Services.LongRunningStatus;

namespace NomadLogic.iBrandDesktop.Services
{
    /// <summary>
    /// Implémentation d'un service longue-durée pour la synchronisation.
    /// </summary>
    [PublicAPI]
    public class SynchronizationService : ServiceLongRunningAbstract<SynchronizationReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private DateTime _nextSubmitResend = DateTime.Now;
        private readonly IbrandDatabase _db;
        private UploadProjectReport _lastIncompleteReport;
        private readonly Progress<UploadProjectReport> _internalProgressReport;
        private int _nbrErrors;
        private ulong _totalData;
        private int _totalSyncs;
        private readonly LinkedList<Tuple<DateTime, Tuple<string, string>>> _lastMessages = new LinkedList<Tuple<DateTime, Tuple<string, string>>>();
        private Task _uploadExecution;

        /// <summary>
        /// La liste des submit Actions, associées par projets.
        /// </summary>
        private readonly IDictionary<int, AbstractSubmitAction> _submitActions = new Dictionary<int, AbstractSubmitAction>();
        private readonly ISet<int> _noSubmitActionList = new HashSet<int>();
        private readonly IList<int> _invalidTerminalList = new List<int>();
        
        private ManualResetEventSlim NewRespondentGiven { get; } = new ManualResetEventSlim(false);

        /// <summary>
        /// Temps maximal pour vérifier la BD entre répondants.
        /// </summary>
        public TimeSpan MaxWaitBetweenChecks { get; set; } = TimeSpan.FromSeconds(5);
        
        protected override void OnDispose()
        {
            NewRespondentGiven.Dispose();
            foreach (var asa in _submitActions.Values)
            {
                asa?.StopProcessing();
            }
        }

        /// <summary>
        /// Le nom de l'usager qui envoi les données vers le serveur.
        /// </summary>
        public string CurrentSynchroUserName { get; set; } = "iBrand Client";

        public override string ServiceName { get; protected set; } = "Synchronization service";

        public SynchronizationService(IbrandDatabase db)
        {
            _db = db;

            _internalProgressReport = new Progress<UploadProjectReport>(report =>
            {
                if (report.InvalidateQuestionnaire != null)
                {
                    var tId = report.InvalidateQuestionnaire.TerminalID;
                    _invalidTerminalList.Add(tId);
                    log.Warn("Invalid terminal detected. Will be disabled for the rest if the service lifetime: " + tId);
                }

                ProgressBarData pbd = null;
                var remain = 0;
                if (report.ErrorMessage != null)
                {
                    _lastIncompleteReport = null;
                    _nbrErrors++;
                    SyncServiceReportUpdate(null, report.ErrorMessage);
                    return;
                }
                
                if (report.UploadFinished)
                {
                    _lastIncompleteReport = null;
                    _totalSyncs += report.NbrRespondentsUploaded;
                    _totalData += report.TotalBytesSent;
                }
                else if (report.TotalProjectedBytesSent != 0)
                {
                    _lastIncompleteReport = report;
                    pbd = new ProgressBarData
                    {
                        Val = (int)(report.TotalBytesSent / report.TotalProjectedBytesSent)
                    };
                    remain = report.TotalRespondentToSend - report.NbrRespondentsUploaded;
                }
                else if (report.TotalRespondentToSend > 0)
                {
                    _lastIncompleteReport = report;
                    remain = report.TotalRespondentToSend - report.NbrRespondentsUploaded;
                }

                SyncServiceReportUpdate(null, null, pbd, remain);
            });
        }

        protected override bool DoInit()
        {
            if (_db == null || !_db.Init())
            {
                log.Error("Tried to start synchronization service with invalid database object.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// On appelle cette méthode pour provoquer un événement indiquant qu'un nouveau répondant a été donné.
        /// </summary>
        public void SignalNewRespondent()
        {
            NewRespondentGiven?.Set();
        }

        /// <summary>
        /// On appelle cette méthode pour provoquer un événement indiquant qu'un nouveau répondant a été donné.
        /// Variation pour "events listener" standard.
        /// </summary>
        public void SignalNewRespondent(object sender, EventArgs e)
        {
            SignalNewRespondent();
        }
        
        protected override async Task<bool> DoMainLoop(CancellationToken ct)
        {
            log.Trace("Synchronization service main loop started.");

            while (!ct.IsCancellationRequested)
            {
                NewRespondentGiven.Wait(ct);
                NewRespondentGiven.Reset();

                // Lance une exécution de upload si la dernière exécution est terminée.
                if (_uploadExecution == null)
                {
                    _uploadExecution = Task.Factory.StartNew(() => ExecuteUpload(ct), ct, TaskCreationOptions.None, TaskScheduler.Current);
                }
                
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var qInfoWithSubAct = _db.GetQuestionnaireIdsWithActionsToExecute();
                    if (qInfoWithSubAct.Any())
                    {
                        log.Trace("Unfinished submit actions detected. Launching transformations.");
                        AddSubmitWorkers(qInfoWithSubAct);
                    }
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Error while re-sending unfinished respondents to current Submit Action");
                }

                if (ct.IsCancellationRequested)
                {
                    break;
                }
            }

            log.Debug("Synchronization service main cycle stopped.");
            return true;
        }

        private async Task ExecuteUpload(CancellationToken ct)
        {
            try
            {
                // Liste des projets avec répondants à synchroniser qui ne font pas partie de la liste invalide.
                var terminals = _db.GetTerminalsWithRespondentsToSync().Where(p => !_invalidTerminalList.Contains(p.TerminalId)).ToList();

                if (terminals.Any())
                {
                    OnStatusChanged(Working);

                    var config = ServicesContainer.GetService<NomadConfiguration>();
                    
                    var manager = new SyncFinisherManager
                    {
                        Operator = CurrentSynchroUserName,
                        DeleteSynchroWhenFinished = config.DeletePictureAfterSynchro,
                        ProgressReport = _internalProgressReport,
                        Database = _db,
                        NomadConfig = config
                    };

                    log.Trace("Synchronization service main loop: Respondent to upload detected. Starting send off.");
                    await manager.Go(terminals, ct);

                    SyncServiceReportUpdate("Finished upload sequence.");

                    manager.DeleteSynchro();
                }
            }
            catch (System.Exception ex)
            {
                log.Warn(ex, "Error while uploading data: " + ex.Message);
                _nbrErrors++;
                SyncServiceReportUpdate(null, $"Error during sync: {ex.Message}");
            }
            finally
            {
                _uploadExecution = null;
            }
        }
        
        protected override bool CheckIfIdle()
        {
            return _uploadExecution == null;
        }
        
        private void SyncServiceReportUpdate(string message = null, string errorMessage = null, ProgressBarData currentUpload = null, int remaining = 0)
        {
            var now = DateTime.Now;

            var info = new SynchronizationReport
            {
                ReportTime = now,
                ErrorNumber = _nbrErrors,
                TotalData = _totalData + (_lastIncompleteReport?.TotalBytesSent ?? 0),
                TotalSync = _totalSyncs + (_lastIncompleteReport?.NbrRespondentsUploaded ?? 0),
                ErrorMessage = errorMessage,
                Message = message,
                CurrentUpload = currentUpload,
                RemainToSync = remaining
            };

            // Si le message est vide de toute info intéressante, on skip.
            if (info.IsEmpty())
            {
                return;
            }

            if (errorMessage != null || message != null)
            {
                lock (_lastMessages)
                {
                    _lastMessages.AddLast(new Tuple<DateTime, Tuple<string, string>>(now, new Tuple<string, string>(message, errorMessage)));
                    while (_lastMessages.Count > 30)
                    {
                        _lastMessages.RemoveFirst();
                    }
                }
            }

            TriggerReport(info, true);
        }

        #region Submit actions
        
        private void AddSubmitWorkers(IList<QuestionnaireInfo> questInfos)
        {
            var now = DateTime.Now;

            if (now < _nextSubmitResend)
            {
                return;
            }

            // Initialize les SubmitActions.
            foreach (var qInfo in questInfos)
            {
                if (!_submitActions.ContainsKey(qInfo.Id))
                {
                    var terminal = _db.GetTerminal(qInfo.TerminalID);

                    var loader = new JujuQuestionnaireLoader(terminal) {EnableDatasourceAndEvents = false};
                    var questionnaire = loader.Load(false);
                    var sa = questionnaire.SubmitAction;
                    if (sa == null)
                    {
                        log.Fatal($"Questionnaire could not create SubmitAction but has un-ready respondent: {terminal.ProjectId}/{terminal.QuestionnaireId}/{terminal.TerminalId}");
                        _noSubmitActionList.Add(terminal.QuestionnaireId);
                        continue;
                    }

                    AssignSubmitAction(terminal.QuestionnaireId, sa);
                    
                    sa.Init(terminal, null);
                    sa.StartWorkerPools();
                    log.Debug($"New submit action for project {terminal.ProjectId} and questionnaire {terminal.QuestionnaireId} has been started and initialized.");
                }

                if (!_submitActions.TryGetValue(qInfo.Id, out var submitAction))
                {
                    log.Error($"Could not get SubmnitAction for questionnaire {qInfo.Id}");
                    _noSubmitActionList.Add(qInfo.Id);
                    continue;
                }

                var ids = _db.GetRespondentIdsBySyncStatus(qInfo.Id);

                foreach (var id in ids)
                {
                    log.Trace($"Respondent id={id} was added to be transformed by SubmitAction.");
                    submitAction.ReSubmitRespondent(id);
                }

                log.Debug($"Submit workers re-added for questionnaire {qInfo.Id}.");
            }

            _nextSubmitResend = now.AddMinutes(2);
        }

        public void AssignSubmitAction(int qId,[NotNull] AbstractSubmitAction action)
        {
            if (!_submitActions.ContainsKey(qId))
            {
                _submitActions.Add(qId, action);
            }
        }

        public void RemoveSubmitAction(int qId)
        {
            if (_submitActions.ContainsKey(qId))
            {
                _submitActions[qId]?.StopProcessing();
                _submitActions.Remove(qId);
            }
        }

        public IList<string> GetActiveSubmitActionList()
        {
            var list = new List<string>();
            foreach (var sa in _submitActions.Values)
            {
                var sb = new StringBuilder();
                sb.Append(sa.GetType().Name);
                sb.Append(" (");
                sb.Append(sa.ProjectId);
                sb.Append("/");
                sb.Append(sa.QuestionnaireId);
                sb.Append(")");
            }
            
            return list;
        }

        #endregion
    }
}