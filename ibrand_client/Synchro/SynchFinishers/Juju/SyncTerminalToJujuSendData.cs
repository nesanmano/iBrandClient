using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Synchro.SynchFinishers.Juju
{
    [PublicAPI]
    public class SyncTerminalToJujuSendData : SyncTerminalToJujuBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        protected override bool CheckIfRespondentValid(Respondent resp)
        {
            if (!string.IsNullOrWhiteSpace(resp.ExternalUniqueIdentifier)) return true;
            var message = $"Respondent invalid: no EUI, but is required. ({resp.Id})";
            log.Warn(message);
            ParentReporter.SendReport(message);
            return false;
        }

        protected override async Task<bool> ExecuteSend(CancellationToken ct, string authToken, Respondent resp)
        {
            var payload = ServerRestRequestsUtil.PrepareRespondentSendRequestPayload(resp);

            if (log.IsTraceEnabled)
            {
                log.Trace($"Preparing to send respondent to 'send-respondent-data' with following payload: {payload}");
            }

            var sync = await ServerRestRequestsUtil.SendRespondentData(ct, _servConfig, authToken, payload).ConfigureAwait(false);
            if (sync?.Data == null || sync.Result != ServerRestRequestResultType.Success)
            {
                log.Warn("Send respondent data failed with status: " + sync?.Result);
                return false;
            }

            var eui = sync.Data?.TargetRespondent?.UniqueExternalIdentifier;
            if (!string.IsNullOrWhiteSpace(eui)) return true;
            log.Warn("No valid respondent received the data from the server, but value is ok. Must finalize respondent Please check data sent, and maybe qId and tId: " + sync.Data?.ToStringPublicProperties());
            return true;
        }
    }
}
