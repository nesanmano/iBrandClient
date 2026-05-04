using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;

namespace NomadLogic.iBrandDesktop.Synchro.SynchFinishers
{
    public abstract class SyncFinisherBase
    {
        public TerminalInfo CurrentTerminal { get; set; }

        public Questionnaire CurrentQuestionnaire { get; set; }

        public List<int> ListRespondentId { get; set; }

        public Synchronization CurrentSynchronization { get; set; }

        public abstract Task Execute(CancellationToken ct);
        
        public ISyncSendReport ParentReporter { get; set; }
    }
}