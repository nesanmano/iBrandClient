using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    public interface IScreenElement : IDisposable
    {
        int GetHeight(int availableWidth, Graphics g);
        int AddToUserControl(UserControl screenPanel, int availableWidth, int y, Graphics g);
    }
}
