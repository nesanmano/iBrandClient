using System;
using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Util.Camera;

namespace NomadLogic.Test.Common.Camera
{
    [PublicAPI]
    public class MockMovieListener : IVideoListener
    {
        public FileInfo MovieFile { get; set; }
        public bool Finished { get; set; }

        public Action OnVideoRequestCompleted { get; set; }

        public Action OnVideoRequestCancelled { get; set; }
        
        public Action OnUnregister { get; set; }
        
        public void OnForcedUnregister()
        {
            OnUnregister?.Invoke();
        }

        public void VideoRequestCompleted(IVideoRequest request)
        {
            if (request.VideoTaken != null)
            {
                MovieFile = request.VideoTaken;
                Finished = true;
            }
            else
            {
                Finished = false;
                MovieFile = null;
            }
            
            OnVideoRequestCompleted?.Invoke();
        }

        public void VideoRequestCancelled(IVideoRequest request)
        {
            OnVideoRequestCancelled?.Invoke();
        }
    }
}