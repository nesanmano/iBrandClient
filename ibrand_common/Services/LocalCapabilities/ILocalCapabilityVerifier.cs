using JetBrains.Annotations;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// Interface de base pour les classes de vérification de capacité système.
    [PublicAPI]
    public interface ILocalCapabilityVerifier
    {
        /// <summary>
        /// Exécute la vérification système demandé, et met comme effet de bord le rapport généré comme fils du rapport
        /// maître en paramètre.
        /// </summary>
        /// <param name="masterReport">Le rapport maître.</param>
        void VerifyCapability([NotNull] CapabilitiesReport masterReport);
    }
}