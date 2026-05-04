using System;
using System.Runtime.InteropServices;

namespace NomadLogic.iBrandDesktop.Util
{
    static class LibVlc
    {
        #region core
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray,
          ArraySubType = UnmanagedType.LPStr)] string[] argv);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_release(IntPtr instance);
        #endregion

        #region media
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_path(IntPtr pInstance,
          [MarshalAs(UnmanagedType.LPStr)] string pszMrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_release(IntPtr pMetaDesc);
        #endregion

        #region media player
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_release(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_hwnd(IntPtr player, IntPtr drawable);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_get_media(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_media(IntPtr player, IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_play(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_pause(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_stop(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr libvlc_media_player_new(IntPtr pInstance);

        #endregion

        #region exception
        [DllImport("libvlc",CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_clearerr();

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_errmsg();
        #endregion

        #region Event Manager Methods

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr libvlc_media_player_event_manager(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_event_attach(IntPtr pEventManager, VlcEventType iEventType, EventCallbackDelegate fCallback, IntPtr pUserData);
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_event_detach(IntPtr pEventManager, VlcEventType iEventType, EventCallbackDelegate fCallback, IntPtr pUserData);
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern string libvlc_event_type_name(VlcEventType eventType);
        #endregion
    }

    /*[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void VlcCallback(ref VlcCallbackArgs Event, IntPtr UserData);*/


    /*public class StateChangedEventArgs : VlcEventArgs
    {
        private VlcState _NewState;
        internal StateChangedEventArgs(VlcCallbackArgs Args, IntPtr UserData)
            : base(Args, UserData)
        {
            _NewState = base.Args.media_state_changed.new_state;
        }
        internal StateChangedEventArgs(VlcCallbackArgs Args, IntPtr UserData, VlcState NewState)
            : base(Args, UserData)
        {
            _NewState = NewState;
        }

        public VlcState NewState
        {
            Get { return _NewState; }
        }
    }*/
}