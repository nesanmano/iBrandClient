using System;
using System.Threading;

namespace NomadLogic.iBrandDesktop.Util
{
    [Obsolete]
    public class SyncEvents
    {
        public SyncEvents()
        {
            NewItemEvent = new AutoResetEvent(false);
        }

        public EventWaitHandle NewItemEvent { get; }
    }
}
