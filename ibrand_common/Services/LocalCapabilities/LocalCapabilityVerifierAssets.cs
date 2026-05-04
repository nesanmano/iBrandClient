using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// Vérification de l'existence des répertoires de fichiers commun à tous les exécutables client de Nomad Logic.
    /// </summary>
    [PublicAPI]
    public class LocalCapabilityVerifierAssets : LocalCapabilityVerifierAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override CapabilityStatusGroupEnum GetCapabilityEnumType()
        {
            return CapabilityStatusGroupEnum.Assets;
        }

        protected override void ExecuteDo(CapabilityStatus capStatus, List<CapabilityStatus> previousTests)
        {
            var assetsDir = new DirectoryInfo(DirectoryUtil.GetLocalNomadLogicAssetsDir());
            
            if (!assetsDir.Exists)
            {
                capStatus.Status = StatusEnum.Missing;
                capStatus.ErrorMessages.Add($"Base directory for assets is missing: '{assetsDir.FullName}'");
            }
            else
            {
                capStatus.Status = StatusEnum.Valid;
            }
        }
    }
}