using System.Windows.Forms;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    public interface ILocalCapabilityVerifierForm : ILocalCapabilityVerifier
    {
        /// Forme parent, donannt accès au fil d'exécution UI.
        Form ParentForm { get; set; }
    }
}