using System.Windows.Forms;

namespace NomadLogic.Common.Ui.NomadScreen
{
    /// Implique qu'une instance peut faire un "mini-Dispose"; implique un nettoyage de valeurs transitives, mais pas une destruction
    /// de référence critique à son utilisation. Est normalement appellé plusieurs fois dans un cycle de vie avant d'être définitivement détruite.
    public interface IRecyclable
    {
        void Recycle(Control parent);
    }
}