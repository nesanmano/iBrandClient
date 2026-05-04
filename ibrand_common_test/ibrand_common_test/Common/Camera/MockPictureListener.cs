using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Util.Camera;

namespace NomadLogic.Test.Common.Camera
{
    [PublicAPI]
    internal class MockPictureListener : IPictureListener
    {
        public FileInfo PictureFile { get; set; }
        public bool Finished { get; set; }

        public void OnForcedUnregister()
        {
            // Die nice
        }

        public void PictureRequestCompleted(IPictureRequest request)
        {
            if (request.PicturesTaken.Any() && request.PicturesTaken[0].Any())
            {
                PictureFile = request.PicturesTaken[0][0];
                Finished = true;
            }
            else
            {
                throw new Exception("Dit not take a picture.");
            }
        }

        public void PictureRequestCancelled(IPictureRequest request)
        {
            // Die nice
        }

        public void PictureTaken(string filePath, string[] cameraId)
        {
            // Die nice
        }
    }
}
