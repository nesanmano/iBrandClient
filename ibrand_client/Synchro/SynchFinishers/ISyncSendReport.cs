using NomadLogic.Common.Services.Configuration;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro.Report;

namespace NomadLogic.iBrandDesktop.Synchro.SynchFinishers
{
    public interface ISyncSendReport
    {
        NomadConfiguration NomadConfig { get; }
        
        string Operator { get; }
        IbrandDatabase Database { get; set; }

        void ReportSetNbrRespondentToTreat(int nbrResp);

        /// <summary>
        /// Envoie un simple rapport texte.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        void ReportMessage(string message, bool error = false);

        /// <summary>
        /// Envoie un rapport.
        /// </summary>
        void Report(UploadProjectReport report);

        void SendErrorReport(string errorMessage);

        void SendReport(string message = null, string errorMessage = null, bool finished = false, QuestionnaireInfo invalidateQuestionnaire = null);
        
        void ReportSetTotalRespToSync(int respondentListCount);
        
        void ReportRespondentTreated();
        void ReportAddDataSent(ulong dataSent);
    }
}