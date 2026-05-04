using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    /// <summary>
    /// Indique qu'une instance doit avoir un contrôle parent pour bien fonctionner.
    /// </summary>
    public interface INeedWinformParentControl
    {
        Control WinformParentControl { get; set; }
    }
}