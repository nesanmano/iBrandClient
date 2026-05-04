using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro.Report;
using NomadLogic.iBrandDesktop.Synchro.SynchFinishers.Juju;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Synchro.SynchFinishers
{
    public class SyncFinisherManager : ISyncSendReport
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public IProgress<UploadProjectReport> ProgressReport { get; set; }
        protected bool _canDeleteSynchro;
        
        protected int _totalRespToSync;
        protected int _nbrRespondentUploaded;
        protected ulong _totalByteSent;

        public IbrandDatabase Database { get; set; }

        public string Operator { get; set; }

        public bool DeleteSynchroWhenFinished { get; set; }

        public NomadConfiguration NomadConfig { get; set; }

        private readonly IDictionary<int, Questionnaire> _questionnaireTable = new Dictionary<int, Questionnaire>();

        public void ReportAddDataSent(ulong dataSent)
        {
            _totalByteSent += dataSent;
        }

        /// <summary>
        /// Envoie un simple rapport texte.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        public void ReportMessage(string message, bool error = false)
        {
            if (ProgressReport == null) return;
            var report = Activator.CreateInstance<UploadProjectReport>();
            if (!error) report.Message = message;
            else report.ErrorMessage = message;
            ProgressReport.Report(report);
        }

        /// <summary>
        /// Envoie un rapport.
        /// </summary>
        public void Report(UploadProjectReport report)
        {
            if (ProgressReport == null || report == null) return;
            ProgressReport.Report(report);
        }
        
        public void SendErrorReport(string errorMessage)
        {
            SendReport(null, errorMessage, true);
        }

        public void SendReport(string message = null, string errorMessage = null, bool finished = false, QuestionnaireInfo invalidateQuestionnaire = null)
        {
            Report(new UploadProjectReport
            {
                InvalidateQuestionnaire = invalidateQuestionnaire,
                Message = message,
                ErrorMessage = errorMessage,
                TotalRespondentToSend = _totalRespToSync,
                NbrRespondentsUploaded = _nbrRespondentUploaded,
                TotalBytesSent = _totalByteSent,
                UploadFinished = finished
            });
        }
        
        /// <summary>
        /// Dit à la classe de détruire les synchro terminées SI l'option de destruction de synchro (DeleteSynchroWhenFinished) à été mis à vrai.
        /// </summary>
        public void DeleteSynchro()
        {
            log.Trace("BeginDataUpload: Deletion of synchro verification.");
            if (!_canDeleteSynchro) return;
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var projects = db.GetProjects();

            var conf = ServicesContainer.GetService<NomadConfiguration>();
            foreach (var synchronization in projects.Select(project => db.GetSynchro(conf.PcName, Operator, Convert.ToString(project.Id))))
            {
                db.DeleteSynchro(synchronization.SynchroId);
            }
        }

        public void ReportSetNbrRespondentToTreat(int nbrResp)
        {
            _nbrRespondentUploaded = nbrResp;
        }

        public void ReportSetTotalRespToSync(int respondentListCount)
        {
            _totalRespToSync = respondentListCount;
        }

        public void ReportRespondentTreated()
        {
            _nbrRespondentUploaded++;
        }

        /// <summary>
        /// Lance l'opération de finalisation.
        /// </summary>
        /// <param name="terminals">Les terminaux à finaliser.</param>
        /// <param name="ct">Le jeton de cancellation.</param>
        /// <returns>La tâche future.</returns>
        public async Task Go(IList<TerminalInfo> terminals, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            foreach (var ti in terminals)
            {
                // Analyse du projet et questionnaire

                var projIdStr = ti.ProjectId.ToString();

                var termSynchro = Database.GetSynchro(this.NomadConfig.PcName, Operator, projIdStr);

                var project = Database.GetProject(projIdStr);

                var respList = termSynchro.RespondentsToSync;
                ReportSetTotalRespToSync(respList.Count);

                log.Trace("SyncTerminal: Synchronization for termuinal={0}/{2}/{3} (respondents to upload = {1})).",
                    ti.ProjectId, respList.Count, ti.QuestionnaireId, ti.TerminalId);

                if (respList.Count <= 0)
                {
                    log.Trace("SyncTerminal: No respondent for project {0}. Skipping.", project.Id);
                    continue;
                }

                Questionnaire q = null;
                foreach (var qInfo in project.QuestionnaireInfos)
                {
                    if (_questionnaireTable.ContainsKey(qInfo.Id))
                    {
                        q = _questionnaireTable[qInfo.Id];
                        continue;
                    }

                    var questionnaireLoader = new JujuQuestionnaireLoader(ti) { EnableDatasourceAndEvents = false };
                    try
                    {
                        q = questionnaireLoader.Load(true);
                        _questionnaireTable.Add(qInfo.Id, q);
                    }
                    catch (System.Exception)
                    {
                        SendReport(null, "Impossible to load questionnaire file. Skipping this questionnaire for the rest of the application lifetime: " + qInfo.Id, false, qInfo);
                    }
                }

                if (q == null) continue;

                // Création de la classe de finalisation.
                var clazz = q.SyncFinisherClassName;

                if (string.IsNullOrEmpty(clazz))
                {
                    log.Debug("No finisher class given by terminal info. Using local finisher instead.");
                    clazz = typeof(SyncFinisherLocal).AssemblyQualifiedName;
                }

                // 1- Créer la classe d'exécution selon la configuration du terminal
                var sync = ClassUtil.InstanciateClassFromName<SyncFinisherBase>(clazz, "NomadLogic.iBrandDesktop.Synchro.SynchFinishers");

                if (sync == null)
                {
                    log.Error("Could not instanciate finisher class for terminal '{2}'({0}): {1}", 
                        clazz, ti.TerminalId, ti.TerminalName);
                    continue;
                }

                // Configuration/setup de la classe d'exécution.
                sync.ParentReporter = this;
                sync.CurrentQuestionnaire = q;
                sync.CurrentTerminal = ti;
                sync.CurrentSynchronization = termSynchro;
                sync.ListRespondentId = respList;

                await sync.Execute(ct);

            }
        }
    }
}