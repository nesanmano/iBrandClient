using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NomadLogic.Common.Services.StateWatcher;

namespace NomadLogic.App.EventVideoCapture.Reader
{
    /// <summary>
    /// Cette variation d'un module de scan RFID ne fait aucun appel externe pour valider les informations lues avec le scan.
    /// Il va tenter de créer des identifiants uniques basé sur les informations qu'il dispose.
    /// </summary>
    public class ReaderManagerStandAlone : ReaderManagerBase
    {
        public override string DefaultConfigFileName => "reader_config_standalone.json";
        
        public override Task ValidateCardScan(string eCardUid)
        {
            var uniqueB64Id = "nomad_" + AssignUniqueIdToCode(eCardUid);
            RaiseScanDetectedEvent(uniqueB64Id);
            return Task.CompletedTask;
        }

        protected override string AssignUniqueIdToCode(string code)
        {
            // Par défaut, on prend le code et la date
            var today = DateTime.Today.ToString("yyyyMMdd");
            return $"{code}_{today}";
        }

        public override void UpdateWatcherList(IList<ExternalStateWatcher> watcherList)
        {
            // No-op
        }
    }
}