using System;

namespace NomadLogic.iBrandDesktop.Util
{
    class VlcMedia : IDisposable
    {
        internal IntPtr _handle;

        public VlcMedia(VlcInstance instance, string url)
        {
            _handle = LibVlc.libvlc_media_new_path(instance._handle, url);
            if (_handle == IntPtr.Zero) throw new VlcException();
        }

        internal VlcMedia(IntPtr handle)
        {
            _handle = handle;
        }

        public void Dispose()
        {
            LibVlc.libvlc_media_release(_handle);
        }
    }
}