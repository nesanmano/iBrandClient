using System.Windows.Forms;

namespace NomadLogic.Common.Ui.NomadScreen
{
    public interface IInactiveTimer
    {
        bool OnInactiveTimerEvent(Control screen);
    }
}