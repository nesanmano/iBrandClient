using System.Collections.Generic;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// Interface pour une classe capable de recevoir une liste de vérification de capacités système, et de les vérifier.
    /// </summary>
    public interface ILocalCapabilityTester
    {
        /// <summary>
        /// Liste des capacités à vérifier.
        /// </summary>
        IList<ILocalCapabilityVerifier> CapabilityVerifiers { get; set; }

        /// <summary>
        /// Lance la vérification des capacités système. Doit être capable, idéalement, de vérifier des capacité avec des fonctions de formes.
        /// </summary>
        /// <returns></returns>
        void VerifyCapabilities();
    }
}