using System.Drawing;
using JetBrains.Annotations;

namespace NomadLogic.Common.Ui.NomadScreen
{
    /// <summary>
    /// Interface qui définie les méthodes pour dessiner sur une Surface GDI+.
    /// </summary>
    public interface IPaint
    {
        void Paint([NotNull] Graphics g, bool debug = false);
    }
}