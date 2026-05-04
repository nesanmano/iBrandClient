using System;

namespace NomadLogic.iBrandDesktop.Synchro
{
    [Obsolete]
    public interface IUiSynchro
    {
        void AddUiMessage(string message, int priority);
        void SetProgressBarMinMax(int min, int max);
        void AddProgressStep(int value);
    }
}
