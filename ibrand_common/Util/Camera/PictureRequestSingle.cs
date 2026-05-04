using System;
using System.Collections.Generic;
using System.IO;

namespace NomadLogic.Common.Util.Camera
{
    public class PictureRequestSingle : IPictureRequest
    {
        public int MinCameraNbr { get; } = 1;
        public IPictureListener PictureListener { get; set; }
        public bool UseLiveView { get; set; }
        public ILiveViewListener LiveViewListener { get; set; }
        public bool Cancelled { get; set; }
        public int PictureToTakeNbr { get; set; } = 1;
        public bool ErrorDetected { get; set; }
        public int CameraCountDown { get; set; } = 0;
        public ICountDownEventListener CountDownEventListener { get; set; }
        public DirectoryInfo SaveDirectory { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
        public List<List<FileInfo>> PicturesTaken { get; set; } = new List<List<FileInfo>>();
        public string RequestTag { get; set; }
    }
}
