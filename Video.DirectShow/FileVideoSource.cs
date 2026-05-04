// AForge Direct Show Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright ©AForge.NET, 2009-2011
// contacts@aforgenet.com
//

namespace AForge.Video.DirectShow
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading;
    using System.Runtime.InteropServices;

    using Video;
    using Internals;

    /// <summary>
    /// Video source for video files.
    /// </summary>
    /// 
    /// <remarks><para>The video source provides access to video files. DirectShow is used to access video
    /// files.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create video source
    /// FileVideoSource videoSource = new FileVideoSource( fileName );
    /// // set NewFrame event handler
    /// videoSource.NewFrame += new NewFrameEventHandler( video_NewFrame );
    /// // start the video source
    /// videoSource.Start( );
    /// // ...
    /// // signal to stop
    /// videoSource.SignalToStop( );
    /// // ...
    /// 
    /// // New frame event handler, which is invoked on each new available video frame
    /// private void video_NewFrame( object sender, NewFrameEventArgs eventArgs )
    /// {
    ///     // get new frame
    ///     Bitmap bitmap = eventArgs.Frame;
    ///     // process the frame
    /// }
    /// </code>
    /// </remarks>
    /// 
    public class FileVideoSource : IVideoSource
    {
        // video file name
        private string _fileName;
        // received frames count
        private int _framesReceived;
        // recieved byte count
        // prevent freezing
        // reference clock for the graph - when disabled, graph processes frames ASAP

        // TIME_FORMAT_MEDIA_TIME (100-nanosecond units).

        // seeking capability
        // seeking function flags
        //private bool isSetCurrentTime;
        //private bool isGetCurrentTime;
        // seeking function params
        //private long currentSetTime;

        private Thread _thread;
        private ManualResetEvent _stopEvent;
        private ManualResetEvent _pauseEvent;
        private ManualResetEvent _unpauseEvent;

        /// <summary>
        /// New frame event.
        /// </summary>
        /// 
        /// <remarks><para>Notifies clients about new available frame from video source.</para>
        /// 
        /// <para><note>Since video source may have multiple clients, each client is responsible for
        /// making a copy (cloning) of the passed video frame, because the video source disposes its
        /// own original copy after notifying of clients.</note></para>
        /// </remarks>
        /// 
        public event NewFrameEventHandler NewFrame;

        /// <summary>
        /// Video source error event.
        /// </summary>
        /// 
        /// <remarks>This event is used to notify clients about any type of errors occurred in
        /// video source object, for example internal exceptions.</remarks>
        /// 
        public event VideoSourceErrorEventHandler VideoSourceError;

        /// <summary>
        /// Video playing finished event.
        /// </summary>
        /// 
        /// <remarks><para>This event is used to notify clients that the video playing has finished.</para>
        /// </remarks>
        /// 
        public event PlayingFinishedEventHandler PlayingFinished;

        /// <summary>
        /// Video source.
        /// </summary>
        /// 
        /// <remarks>Video source is represented by video file name.</remarks>
        /// 
        public virtual string Source
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// Received frames count.
        /// </summary>
        /// 
        /// <remarks>Number of frames the video source provided from the moment of the last
        /// access to the property.
        /// </remarks>
        /// 
        public int FramesReceived
        {
            get
            {
                var frames = _framesReceived;
                _framesReceived = 0;
                return frames;
            }
        }

        /// <summary>
        /// Received bytes count.
        /// </summary>
        /// 
        /// <remarks>Number of bytes the video source provided from the moment of the last
        /// access to the property.
        /// </remarks>
        /// 
        public long BytesReceived { get; private set; }

        /// <summary>
        /// State of the video source.
        /// </summary>
        /// 
        /// <remarks>Current state of video source object - running or not.</remarks>
        /// 
        public bool IsRunning
        {
            get
            {
                if (_thread == null) return false;
                // check thread status
                if ( _thread.Join( 0 ) == false )
                    return true;

                // the thread is not running, free resources
                Free( );
                return false;
            }
        }

        /// <summary>
        /// Prevent video freezing after screen saver and workstation lock or not.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>The value specifies if the class should prevent video freezing during and
        /// after screen saver or workstation lock. To prevent freezing the <i>DirectShow</i> graph
        /// should not contain <i>Renderer</i> filter, which is added by <i>Render()</i> method
        /// of graph. However, in some cases it may be required to call <i>Render()</i> method of graph, since
        /// it may add some more filters, which may be required for playing video. So, the property is
        /// a trade off - it is possible to prevent video freezing skipping adding renderer filter or
        /// it is possible to keep renderer filter, but video may freeze during screen saver.</para>
        /// 
        /// <para><note>The property may become obsolete in the future when approach to disable freezing
        /// and adding all required filters is found.</note></para>
        /// 
        /// <para><note>The property should be set before calling <see cref="Start"/> method
        /// of the class to have effect.</note></para>
        /// 
        /// <para>Default value of this property is set to <b>false</b>.</para>
        /// 
        /// </remarks>
        /// 
        public bool PreventFreezing { get; set; } = false;

        /// <summary>
        /// Enables/disables reference clock on the graph.
        /// </summary>
        /// 
        /// <remarks><para>Disabling reference clocks causes DirectShow graph to run as fast as
        /// it can process data. When enabled, it will process frames according to presentation
        /// time of a video file.</para>
        /// 
        /// <para><note>The property should be set before calling <see cref="Start"/> method
        /// of the class to have effect.</note></para>
        /// 
        /// <para>Default value of this property is set to <b>true</b>.</para>
        /// </remarks>
        /// 
        public bool ReferenceClockEnabled { get; set; } = true;

        /// <summary>
        /// Used to check if seeking is supported.
        /// </summary>
        public bool IsSeekEnabled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileVideoSource"/> class.
        /// </summary>
        /// 
        public FileVideoSource( ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileVideoSource"/> class.
        /// </summary>
        /// 
        /// <param name="fileName">Video file name.</param>
        /// 
        public FileVideoSource( string fileName )
        {
            _fileName = fileName;
        }

        /// <summary>
        /// Start video source.
        /// </summary>
        /// 
        /// <remarks>Starts video source and return execution to caller. Video source
        /// object creates background thread and notifies about new frames with the
        /// help of <see cref="NewFrame"/> event.</remarks>
        /// 
        public void Start( )
        {
            if (IsRunning) return;
            // check source
            if ( ( _fileName == null ) || ( _fileName == string.Empty ) )
                throw new ArgumentException( "Video source is not specified" );

            _framesReceived = 0;
            BytesReceived = 0;

            // create events
            _stopEvent = new ManualResetEvent( false );
            _pauseEvent = new ManualResetEvent(false);
            _unpauseEvent = new ManualResetEvent(false);

            // create and start new thread
            _thread = new Thread(WorkerThread) {Name = _fileName};
            // mainly for debugging
            _thread.Start( );
        }

        /// <summary>
        /// Signal video source to stop its work.
        /// </summary>
        /// 
        /// <remarks>Signals video source to stop its background thread, stop to
        /// provide new frames and free resources.</remarks>
        /// 
        public void SignalToStop( )
        {
            // stop thread
            if ( _thread != null )
            {
                // signal to stop
                _stopEvent.Set( );

            }
        }


        /// <summary>
        /// Signal video source to pause its work.
        /// </summary>
        /// 
        /// <remarks>Signals video source to pause its background thread, pause to
        /// provide new frames and free resources.</remarks>
        /// 
        public void SignalToPause()
        {
            // stop thread
            if (_thread != null)
            {
                // signal to stop
                _unpauseEvent.Reset();
                _pauseEvent.Set();
                IsPaused = true;
            }
        }

        /// <summary>
        /// Signal video source to unpause its work.
        /// </summary>
        /// 
        /// <remarks>Signals video source to unpause its background thread, pause to
        /// provide new frames and free resources.</remarks>
        /// 
        public void SignalToUnPause()
        {
            // stop thread
            if (_thread != null)
            {
                // signal to stop
                _pauseEvent.Reset();
                _unpauseEvent.Set();
                IsPaused = false;
            }
        }

        /// <summary>
        /// Used to check if is paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Wait for video source has stopped.
        /// </summary>
        /// 
        /// <remarks>Waits for source stopping after it was signalled to stop using
        /// <see cref="SignalToStop"/> method.</remarks>
        /// 
        public void WaitForStop( )
        {
            if ( _thread != null )
            {
                // wait for thread stop
                _thread.Join( );

                Free( );
            }
        }

        /// <summary>
        /// Stop video source.
        /// </summary>
        /// 
        /// <remarks><para>Stops video source aborting its thread.</para>
        /// 
        /// <para><note>Since the method aborts background thread, its usage is highly not preferred
        /// and should be done only if there are no other options. The correct way of stopping camera
        /// is <see cref="SignalToStop">signaling it stop</see> and then
        /// <see cref="WaitForStop">waiting</see> for background thread's completion.</note></para>
        /// </remarks>
        /// 
        public void Stop( )
        {
            if (!IsRunning) return;
            _thread.Abort( );
            WaitForStop( );
        }
        
        /*
        /// <summary>
        /// Sets the current time (seconds) in terms of the total time of the media stream
        /// </summary>
        /// <param name="currentMilliSeconds">The current stream time</param>
        public void SetCurrentTime(long currentMilliSeconds)
        {
            if (thread == null || !isSeekEnabled)
                return;

            currentSetTime = currentMilliSeconds * 10000;
            isSetCurrentTime = true;
        }*/

        /*public void SetCurrentTime(uint currentSeconds)
        {
            if (thread == null || !IsRunning || !isSeekEnabled)
                return;

            currentSetTime = currentSeconds * ONE_SECOND;
            isSetCurrentTime = true;
        }*/

        /// <summary>
        /// Free resource.
        /// </summary>
        /// 
        private void Free( )
        {
            _thread = null;

            // release events
            _stopEvent.Close( );
            _stopEvent = null;
        }

        /// <summary>
        /// Worker thread.
        /// </summary>
        /// 
        private void WorkerThread( )
        {
            var reasonToStop = ReasonToFinishPlaying.StoppedByUser;

            // grabber
            var grabber = new Grabber( this );

            // objects
            object graphObject = null;
            object grabberObject = null;

            // interfaces
            IBaseFilter         sourceBase = null;

            try
            {
                // get type for filter graph
                var type = Type.GetTypeFromCLSID( Clsid.FilterGraph );
                if ( type == null )
                    throw new ApplicationException( "Failed creating filter graph" );

                // create filter graph
                graphObject = Activator.CreateInstance( type );
                var       graph = (IGraphBuilder) graphObject;

                // create source device's object
                graph.AddSourceFilter( _fileName, "source", out sourceBase );
                if ( sourceBase == null )
                    throw new ApplicationException( "Failed creating source filter" );

                // get type for sample grabber
                type = Type.GetTypeFromCLSID( Clsid.SampleGrabber );
                if ( type == null )
                    throw new ApplicationException( "Failed creating sample grabber" );

                // create sample grabber
                grabberObject = Activator.CreateInstance( type );
                var      sampleGrabber = (ISampleGrabber) grabberObject;
                var         grabberBase = (IBaseFilter) grabberObject;

                // add grabber filters to graph
                graph.AddFilter( grabberBase, "grabber" );

                // set media type
                var mediaType = new AMMediaType
                {
                    MajorType = MediaType.Video,
                    SubType = MediaSubType.RGB24
                };
                sampleGrabber.SetMediaType( mediaType );

                // connect pins
                var pinToTry = 0;

                var inPin = Tools.GetInPin( grabberBase, 0 );
                IPin outPin;

                // find output pin acceptable by sample grabber
                while ( true )
                {
                    outPin = Tools.GetOutPin( sourceBase, pinToTry );

                    if ( outPin == null )
                    {
                        Marshal.ReleaseComObject( inPin );
                        throw new ApplicationException( "Did not find acceptable output video pin in the given source" );
                    }

                    if ( graph.Connect( outPin, inPin ) < 0 )
                    {
                        Marshal.ReleaseComObject( outPin );
                        pinToTry++;
                    }
                    else
                    {
                        break;
                    }
                }

                Marshal.ReleaseComObject( outPin );
                Marshal.ReleaseComObject( inPin );

                // get media type
                if ( sampleGrabber.GetConnectedMediaType( mediaType ) == 0 )
                {
                    var vih = (VideoInfoHeader) Marshal.PtrToStructure( mediaType.FormatPtr, typeof( VideoInfoHeader ) );

                    grabber.Width = vih.BmiHeader.Width;
                    grabber.Height = vih.BmiHeader.Height;
                    mediaType.Dispose( );
                }

                // let's do rendering, if we don't need to prevent freezing
                if ( !PreventFreezing )
                {
                    // render pin
                    graph.Render( Tools.GetOutPin( grabberBase, 0 ) );

                    // configure video window
                    var window = (IVideoWindow) graphObject;
                    window.put_AutoShow( false );
                }

                // configure sample grabber
                sampleGrabber.SetBufferSamples( false );
                sampleGrabber.SetOneShot( false );
                sampleGrabber.SetCallback( grabber, 1 );

                // disable clock, if someone requested it
                if ( !ReferenceClockEnabled )
                {
                    var mediaFilter = (IMediaFilter) graphObject;
                    mediaFilter.SetSyncSource( null );
                }

                // get media control
                var       mediaControl = (IMediaControl) graphObject;
                // get media events' interface
                var       mediaEvent = (IMediaEventEx) graphObject;

                // Get media seeking & check seeking capability
                var       mediaSeeking = (IMediaSeeking)graphObject;
                const SeekingCapabilities caps = SeekingCapabilities.CanSeekAbsolute | SeekingCapabilities.CanGetDuration;
                SeekingCapabilities canSeekCap;
                var hr = mediaSeeking.GetCapabilities(out canSeekCap);
                if(hr < 0)
                    throw new ApplicationException("Failed getting seeking capabilities");
                IsSeekEnabled = (canSeekCap & caps) == caps;

                // run
                mediaControl.Run( );

                do
                {
                    // GetCurrentTime
                    /*if (isGetCurrentTime)
                    {
                        mediaSeeking.GetCurrentPosition(out currentGetTime);
                        isGetCurrentTime = false;
                    }
                    // SetCurrentTime
                    if (isSetCurrentTime)
                    {
                        long stop = 0;
                        mediaSeeking.SetPositions(ref currentSetTime, SeekingFlags.AbsolutePositioning, ref stop, SeekingFlags.NoPositioning);
                        isSetCurrentTime = false;
                    }*/

                    if ( mediaEvent != null )
                    {
                        IntPtr p1;
                        IntPtr p2;
                        DsEvCode code;
                        if ( mediaEvent.GetEvent( out code, out p1, out p2, 0 ) >= 0 )
                        {
                            mediaEvent.FreeEventParams( code, p1, p2 );

                            if ( code == DsEvCode.Complete )
                            {
                                reasonToStop = ReasonToFinishPlaying.EndOfStreamReached;
                                break;
                            }
                        }
                    }

                    if (!_pauseEvent.WaitOne(0, false)) continue;
                    mediaControl.Pause();
                    _unpauseEvent.WaitOne();
                    mediaControl.Run();
                }
                while ( !_stopEvent.WaitOne( 100, false ) );

                mediaControl.Stop( );
            }
            catch ( Exception exception )
            {
                // provide information to clients
                VideoSourceError?.Invoke( this, new VideoSourceErrorEventArgs( exception.Message ) );
            }
            finally
            {
                // release all objects

                if ( graphObject != null )
                {
                    Marshal.ReleaseComObject( graphObject );
                }
                if ( sourceBase != null )
                {
                    Marshal.ReleaseComObject( sourceBase );
                }
                if ( grabberObject != null )
                {
                    Marshal.ReleaseComObject( grabberObject );
                }
            }

            PlayingFinished?.Invoke( this, reasonToStop );
        }

        /// <summary>
        /// Notifies client about new frame.
        /// </summary>
        /// 
        /// <param name="image">New frame's image.</param>
        /// 
        protected void OnNewFrame( Bitmap image )
        {
            _framesReceived++;
            //int test = Bitmap.GetPixelFormatSize(image.PixelFormat) >> 3;

            BytesReceived += image.Width * image.Height * ( Image.GetPixelFormatSize( image.PixelFormat ) >> 3 );

            if ( !_stopEvent.WaitOne( 0, false ) )
                NewFrame?.Invoke( this, new NewFrameEventArgs( image ) );
        }

        //
        // Video grabber
        //
        private class Grabber : ISampleGrabberCB
        {
            private readonly FileVideoSource _parent;

            // Width property
            public int Width { private get; set; }

            // Height property
            public int Height { private get; set; }

            // Constructor
            public Grabber( FileVideoSource parent )
            {
                _parent = parent;
            }

            // Callback to receive samples
            public int SampleCB( double sampleTime, IntPtr sample )
            {
                return 0;
            }

            // Callback method that receives a pointer to the sample buffer
            public int BufferCB( double sampleTime, IntPtr buffer, int bufferLen )
            {
                if ( _parent.NewFrame != null )
                {
                    // create new image
                    var image = new Bitmap( Width, Height, PixelFormat.Format24bppRgb );

                    // lock bitmap data
                    var imageData = image.LockBits(
                        new Rectangle( 0, 0, Width, Height ),
                        ImageLockMode.ReadWrite,
                        PixelFormat.Format24bppRgb );

                    // copy image data
                    var srcStride = imageData.Stride;
                    var dstStride = imageData.Stride;

                    unsafe
                    {
                        var dst = (byte*) imageData.Scan0.ToPointer( ) + dstStride * ( Height - 1 );
                        var src = (byte*) buffer.ToPointer( );

                        for ( var y = 0; y < Height; y++ )
                        {
                            Win32.memcpy( dst, src, srcStride );
                            dst -= dstStride;
                            src += srcStride;
                        }
                    }

                    // unlock bitmap data
                    image.UnlockBits( imageData );

                    // notify parent
                    _parent.OnNewFrame( image );

                    // release the image
                    image.Dispose( );
                }

                return 0;
            }
        }
    }
}
