using System;
using System.Runtime.InteropServices;

namespace NomadLogic.iBrandDesktop.Util
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct VlcCallbackArgs
    {
        [FieldOffset(0)]
        public VlcEventType EventType;

        [FieldOffset(8)]
        public IntPtr Sender;

        [FieldOffset(16)]
        public MediaStateChanged media_state_changed;
    }
}