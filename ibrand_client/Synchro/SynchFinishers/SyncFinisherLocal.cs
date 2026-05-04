using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;

namespace NomadLogic.iBrandDesktop.Synchro.SynchFinishers
{
    public class SyncFinisherLocal : SyncFinisherBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override Task Execute(CancellationToken ct)
        {
            var acti = ServicesContainer.GetService<ActivityLoggerService>();
            var db = ParentReporter.Database;
            foreach (var repId in ListRespondentId)
            {
                ParentReporter.ReportRespondentTreated();
                db.SetRespondentSynchronized(repId, CurrentSynchronization.No);
                if (CurrentQuestionnaire.AutoCleanAnswers) db.DeleteAllAnswers(repId);
                log.Trace("Finished local sequence for respondent {0} in terminal {1}/{2}/{3}. Finalized.",
                    repId, CurrentTerminal.ProjectId, CurrentTerminal.QuestionnaireId, CurrentTerminal.TerminalId);
                acti.LogActivity(this.CurrentTerminal.TerminalId, ActivityLoggerActionType.RespondentFinalized, repId.ToString());
            }

            return Task.CompletedTask;
        }
    }
}