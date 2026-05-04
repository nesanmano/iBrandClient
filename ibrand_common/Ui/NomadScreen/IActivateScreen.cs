using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Ui.NomadScreen
{
    /// <summary>
    /// Interface pour les éléments d'écrans qui réagissent sur la phase d'activation du cycle d'un écran.
    /// </summary>
    public interface IActivateScreen
    {
        void Activate([NotNull] Control parent, IRespondent respondent);
    }
}