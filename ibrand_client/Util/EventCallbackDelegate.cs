using System;
using System.Runtime.InteropServices;

namespace NomadLogic.iBrandDesktop.Util
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void EventCallbackDelegate(IntPtr userdata);
}