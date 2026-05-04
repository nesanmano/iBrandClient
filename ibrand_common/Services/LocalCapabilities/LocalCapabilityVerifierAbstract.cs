using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    public abstract class LocalCapabilityVerifierAbstract : ILocalCapabilityVerifier
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <inheritdoc />
        public void VerifyCapability(CapabilitiesReport masterReport)
        {
            var tmp = Execute(masterReport.Groups);
            if (tmp != null) masterReport.Groups.Add(tmp);
        }

        // Template Method
        private CapabilityStatus Execute(List<CapabilityStatus> previousTests)
        {
            var result = new CapabilityStatus
            {
                Name = GetCapabilityEnumType()
            };
            
            try
            {
                ExecuteDo(result, previousTests); // Exécution de la portion propre à la classe d'implémentation.
            }
            catch (System.Exception e)
            {
                const string MESSAGE = "Error during asset check. Check logs for details.";
                log.Warn(e, MESSAGE);
                result.Status = StatusEnum.Invalid;
                result.ErrorMessages.Add(MESSAGE);
            }

            return result;
        }

        protected static bool CheckPreviousTests(List<CapabilityStatus> previousTests, params CapabilityStatusGroupEnum[] enumToCheck)
        {
            var capsToCheck = previousTests?.Where(g => enumToCheck.Contains(g.Name) && g.Status == StatusEnum.Valid).ToList();
            return capsToCheck != null && capsToCheck.Count == enumToCheck.Length;
        }
        
        /// Retourne le type énumératif du test.
        protected abstract CapabilityStatusGroupEnum GetCapabilityEnumType();

        /// <summary>
        /// Exécution commune de la vérification de capacité.
        /// </summary>
        /// <returns></returns>
        protected abstract void ExecuteDo(CapabilityStatus result, [CanBeNull] List<CapabilityStatus> previousTests);
    }
}