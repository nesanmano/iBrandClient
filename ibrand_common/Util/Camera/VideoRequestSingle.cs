using System;
using System.IO;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Camera
{
    [PublicAPI]
    public class VideoRequestSingle : IVideoRequest
    {
        public IVideoListener VideoListener { get; set; }
        public bool UseLiveView { get; set; }
        public ILiveViewListener LiveViewListener { get; set; }
        public bool Cancelled { get; set; }
        public bool ErrorDetected { get; set; }
        public double Duration { get; set; }
        public int CountDown { get; set; }
        public ICountDownEventListener CountDownEventListener { get; set; }
        public DirectoryInfo SaveDirectory { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
        public FileInfo VideoTaken { get; set; }
    }
}