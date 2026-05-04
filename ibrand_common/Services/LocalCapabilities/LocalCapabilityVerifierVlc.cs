using System.Collections.Generic;
using System.IO;
using NLog;
using NomadLogic.Common.Services.Configuration;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    public class LocalCapabilityVerifierVlc : LocalCapabilityVerifierAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override CapabilityStatusGroupEnum GetCapabilityEnumType()
        {
            return CapabilityStatusGroupEnum.VlcLibs;
        }

        protected override void ExecuteDo(CapabilityStatus capStatus, List<CapabilityStatus> previousTests)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();
            var vlcLibsDir = new DirectoryInfo(config.VlcLibsLocation);
            
            if (!vlcLibsDir.Exists)
            {
                capStatus.Status = StatusEnum.Missing;
                capStatus.ErrorMessages.Add($"Base directory for vlc librairies is missing: '{vlcLibsDir.FullName}'");
            }
            else
            {
                if (vlcLibsDir.GetDirectories().Length != 3)
                {
                    capStatus.Status = StatusEnum.Missing;
                    capStatus.ErrorMessages.Add($"Vlc librairies are missing some sub-directories: '{vlcLibsDir.FullName}'");
                }
                else
                {
                    capStatus.Status = StatusEnum.Valid;
                }
            }
        }
    
    }
}