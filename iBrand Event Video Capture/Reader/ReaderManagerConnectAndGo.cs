using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads;
using NomadLogic.Multimedia.Video;

namespace NomadLogic.App.EventVideoCapture.Reader
{
    /// <summary>
    /// Cette variation d'un module de scan RFID ne fait aucun appel externe pour valider les informations lues avec le scan.
    /// Il va tenter de créer des identifiants uniques basé sur les informations qu'il dispose.
    /// Quand on réussi, le code unique du reader devient le WALLET_ID qu'Il faut utiliser.
    /// </summary>
    [PublicAPI]
    public class ReaderManagerConnectAndGo : ReaderManagerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override string DefaultConfigFileName => "reader_config_cng.json";
        
        private WebRequestConnectAndGo _webExecutorCnG;

        protected override bool ExtraInit()
        {
            // Validation de la connexion vers C&G
            _webExecutorCnG = new WebRequestConnectAndGo
            {
                BaseUrl = BaseUrlCnG
            };

            if (!_webExecutorCnG.InitializeExecutor().GetAwaiter().GetResult())
            {
                log.Error("Could not initialize executor for ConnectAndGo.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// URL de base du serveur CnG.
        /// </summary>
        public string BaseUrlCnG { get; set; }

        public int MaxDelay { get; set; } = 3000;

        #region REFACTOR OPC
        public string CnGClientSecret { get; set; }

        public string CnGUsername { get; set; }

        public string CnGPassword { get; set; }

        public int CnGClientId { get; set; }
        
        public long TenantId { get; set; }

        public string GateName { get; set; }

        public int AccessPointId { get; set; }
        #endregion

        // TODO Rendre async
        public override async Task ValidateCardScan(string eCardUid)
        {
            var cts = new CancellationTokenSource(MaxDelay);
            // Fait une recherche via le process scan.
            var cngLogin = await _webExecutorCnG.CnGLogin(cts.Token, CnGClientId, CnGPassword, CnGUsername, CnGClientSecret);
            
            if (cngLogin?.Data == null)
            {
                log.Warn("Could not login to CnG servers.");
                TriggerReport(new VideoCaptureReport("Could not login to Connect AndGo web serveurs. Cannot proceed with RFID verification."), true);
                return;
            }

            var cngToken = cngLogin.Data;

            var pars = new ProcessAccessScanPayload
            {
                UniqueId = eCardUid,
                AccessPointId = AccessPointId,
                Direction = "IN",
                GateName = GateName,
                XtenantId = TenantId
            };

            var processResult = await _webExecutorCnG.ProcessAccessScan(cngToken, pars);
            if (processResult == null)
            {
                log.Warn("Process Scan to ConnectAndGo server failed.");
                TriggerReport(new VideoCaptureReport("Could not process scan to Connect And Go web serveurs. Cannot proceed with RFID verification."), true);
                return;
            }

            // Si l'identifiant est complétement inconnu
            if (processResult.WalletId == 0)
            {
                log.Warn("Could not have access to a valid wallet id with following identifier:{0}. Deleting.", eCardUid);
                TriggerReport(new VideoCaptureReport("No valid Wallet ID found on Connect And Go web serveurs. RFID verification failed."), true);
                return;
            }

            // Retourne le wallet_id associé; pas le RFID qui a été lu.
            RaiseScanDetectedEvent(processResult.WalletId.ToString());
        }


        public override void UpdateWatcherList(IList<ExternalStateWatcher> watcherList)
        {
            // No-op
        }
    }
}