using System;
using System.Runtime.InteropServices;

namespace NomadLogic.iBrandDesktop.Util
{
    class VlcException : System.Exception
    {
        protected string _err;

        public VlcException()
        {
            var errorPointer = LibVlc.libvlc_errmsg();
            _err = errorPointer == IntPtr.Zero ? "VLC Exception"
                : Marshal.PtrToStringAuto(errorPointer);
        }

        public override string Message => _err;
    }
}