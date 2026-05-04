using System.Threading;

namespace NomadLogic.iBrandDesktop
{
    public class NewPrintEvent
    {
        private static NewPrintEvent _printEvent;

        private NewPrintEvent()
        {
            NewItemEvent = new AutoResetEvent(false);
        }

        public EventWaitHandle NewItemEvent { get; }

        public static NewPrintEvent GetInstance()
        {
            return _printEvent ?? (_printEvent = new NewPrintEvent());
        }
    }
}
