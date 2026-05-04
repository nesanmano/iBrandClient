using System;

namespace NomadLogic.iBrandDesktop.Util
{
    class VlcInstance : IDisposable
    {
        internal IntPtr _handle;

        //internal IntPtr p_event_manager;

        //private VlcCallback cbStateChanged;

        //internal EventCallbackDelegate _endedDelegated;

        //private EventCallbackDelegate callback;

        //internal IntPtr myEventCallback;

        //public event EventHandler<StateChangedEventArgs> StateChanged;

        public VlcInstance(string[] args)
        {
            _handle = LibVlc.libvlc_new(args.Length, args);
            if (_handle == IntPtr.Zero) throw new VlcException();

            //p_exception = new libvlc_exception_t();
            //p_exception.Initalize(); 

            

            //InitalizeEvents();
        }

        public void Dispose()
        {
            LibVlc.libvlc_release(_handle);
        }

        /* public void InitalizeEvents()
        {
            //Get our event manager for this class
            p_event_manager = LibVlc.libvlc_media_player_event_manager(Handle);
            //_EventManager = new VlcEventManager(p_event_manager);

            if (p_event_manager == IntPtr.Zero) throw new VlcException();

            //Initalize our callback methods
            //cbStateChanged = new VlcCallback(OnStateChanged);
            callback = MediaPlayerPaused;
            myEventCallback = Marshal.GetFunctionPointerForDelegate(callback);
            GC.KeepAlive(callback);

            LibVlc.libvlc_event_attach(p_event_manager, VlcEventType.libvlc_MediaPlayerEndReached, IntPtr.Zero, IntPtr.Zero);
            string test = LibVlc.libvlc_event_type_name(VlcEventType.libvlc_MediaPlayerEndReached);
            //Attach default events 
            //LibVlc.libvlc_event_attach(p_event_manager, VlcEventType.MediaPlayerEndReached, cbStateChanged, IntPtr.Zero);

            //LibVlc.libvlc_event_attach(p_event_manager, VlcEventType.MediaPlayerEncounteredError, cbStateChanged, IntPtr.Zero);
        }*/

        
        



        /*private void OnStateChanged(ref VlcCallbackArgs Args, IntPtr UserData)
        {
            if (this.StateChanged != null)
            {
                switch (Args.EventType)
                {
                    case VlcEventType.MediaPlayerNothingSpecial:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.NothingSpecial));
                        break;
                    case VlcEventType.MediaPlayerOpening:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Opening));
                        break;
                    case VlcEventType.MediaPlayerBuffering:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Buffering));
                        break;
                    case VlcEventType.MediaPlayerPlaying:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Playing));
                        break;
                    case VlcEventType.MediaPlayerPaused:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Paused));
                        break;
                    case VlcEventType.MediaPlayerStopped:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Stopped));
                        break;
                    case VlcEventType.MediaPlayerForward:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Forward));
                        break;
                    case VlcEventType.MediaPlayerBackward:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Backward));
                        break;
                    case VlcEventType.MediaPlayerEndReached:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Ended));
                        break;
                    case VlcEventType.MediaPlayerEncounteredError:
                        StateChanged(this, new StateChangedEventArgs(Args, UserData, VlcState.Error));
                        break;
                }
            }
        }*/
    }
}