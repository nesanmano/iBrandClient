using System;

namespace NomadLogic.iBrandDesktop.Util
{
    class VlcMediaPlayer : IDisposable
    {
        public event EventHandler OnMediaEnded;
        
        internal IntPtr _handle;
        private IntPtr _drawable;
        private bool _playing, _paused;

        internal IntPtr _p_event_manager;

        private EventCallbackDelegate _endedDelegate;

        public VlcMediaPlayer(VlcMedia media)
        {
            _handle = LibVlc.libvlc_media_player_new_from_media(media._handle);
            if (_handle == IntPtr.Zero) throw new VlcException();

            InitalizeEvents();
        }

        public VlcMediaPlayer(VlcInstance instance, IntPtr parent)
        {
            _handle = LibVlc.libvlc_media_player_new(instance._handle);
            if (_handle == IntPtr.Zero) throw new VlcException();

            LibVlc.libvlc_media_player_set_hwnd(_handle, parent);

            InitalizeEvents();
        }

        private void InitalizeEvents()
        {
            //Get our event manager for this class
            _p_event_manager = LibVlc.libvlc_media_player_event_manager(_handle);

            if (_p_event_manager == IntPtr.Zero) throw new VlcException();

            _endedDelegate = MediaPlayerEnded;

            LibVlc.libvlc_event_attach(_p_event_manager, VlcEventType.LibvlcMediaPlayerEndReached, _endedDelegate, IntPtr.Zero);
            //Attach default events 
            //LibVlc.libvlc_event_attach(p_event_manager, VlcEventType.MediaPlayerEndReached, cbStateChanged, IntPtr.Zero);
        }

        public void Dispose()
        {
            LibVlc.libvlc_event_detach(_p_event_manager, VlcEventType.LibvlcMediaPlayerEndReached, _endedDelegate, IntPtr.Zero);
            
            LibVlc.libvlc_media_player_release(_handle);
        }

        public IntPtr Drawable
        {
            get
            {
                return _drawable;
            }
            set
            {
                LibVlc.libvlc_media_player_set_hwnd(_handle, value);
                _drawable = value;
            }
        }

        /*public VlcMedia Media
        {
            Get
            {
                IntPtr media = LibVlc.libvlc_media_player_get_media(Handle);
                if (media == IntPtr.Zero) return null;
                return new VlcMedia(media);
            }
            Set
            {
                LibVlc.libvlc_media_player_set_media(Handle, value.Handle);
            }
        }*/

        public bool IsPlaying => _playing && !_paused;

        public bool IsPaused => _playing && _paused;

        public bool IsStopped => !_playing;

        public void Load(VlcMedia media)
        {
            LibVlc.libvlc_media_player_set_media(_handle, media._handle);
        }

        public void Play()
        {
            var ret = LibVlc.libvlc_media_player_play(_handle);

            if (ret == -1)
                throw new VlcException();

            _playing = true;
            _paused = false;
        }

        private void MediaPlayerEnded(IntPtr userdata)
        {
            if (OnMediaEnded != null)
                OnMediaEnded(this, EventArgs.Empty);

        }

        public void Pause()
        {
            LibVlc.libvlc_media_player_pause(_handle);

            if (_playing)
                _paused ^= true;
        }

        public void Stop()
        {
            LibVlc.libvlc_media_player_stop(_handle);

            _playing = false;
            _paused = false;
        }
    }
}