using System.Runtime.InteropServices;

namespace NomadLogic.iBrandDesktop.Util
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MediaStateChanged
    {
        public VlcState new_state;
    }
}