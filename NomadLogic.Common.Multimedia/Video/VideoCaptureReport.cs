using NomadLogic.Common.Services;

namespace NomadLogic.Multimedia.Video
{
    public class VideoCaptureReport : IProgressDeepCopy<VideoCaptureReport>
    {
        public string StatusMessage { get; set; }

        public VideoCaptureReport() {}

        public VideoCaptureReport(string message)
        {
            StatusMessage = message;
        }
        
        public VideoCaptureReport Copy()
        {
            return new VideoCaptureReport
            {
                StatusMessage = StatusMessage
            };
        }
    }
}