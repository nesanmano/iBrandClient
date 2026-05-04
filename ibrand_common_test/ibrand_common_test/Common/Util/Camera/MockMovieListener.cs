using System;
using System.IO;
using NomadLogic.Common.Util.Camera;

namespace NomadLogic.Test.NomadLogic.iBrand.Common.Util.Camera
{
    internal class MockMovieListener : IVideoListener
    {
        public FileInfo PictureFile { get; set; }
        public bool Finished { get; set; }

        public void OnForcedUnregister()
        {
            throw new NotImplementedException();
        }

        public void VideoRequestCompleted(IVideoRequest request)
        {
            if (request.VideoTaken != null)
            {
                PictureFile = request.VideoTaken;
                Finished = true;
            }
            else
            {
                throw new Exception("Dit not take a picture.");
            }
        }

        public void VideoRequestCancelled(IPictureRequest request)
        {
            throw new NotImplementedException();
        }
    }
}