using System;
using System.IO;
using NomadLogic.Common.Util.Camera;
using NUnit.Framework;

// ReSharper disable LocalizableElement

namespace NomadLogic.Test.Common.Camera
{
    public class TestCameraControllerCanon : TestBaseCanonCamera
    {
        [Test]
        public void TestVideoCapture()
        {
            var ccc = GetViableCanonCameraController();
            if (ccc.NbrCameraReady() == 0) Assert.Ignore("Ignoring test. No camera ready.");
            var outDir = GetFullOutputDir();
            ccc.PictureSaveDirectory = new DirectoryInfo(outDir);
            
            Assert.That(ccc.CurrentState() == CameraControllerStateEnum.Initialized);

            var movTime = new TimeSpan(0, 0, 5);
            var maxTime = new TimeSpan(0, 0, 5);
            
            var mock = TakeSynchMovie(ccc, movTime, maxTime);

            Assert.That(mock.Finished, "Did not complete normally.");
            Assert.That(mock.MovieFile.Exists, "Movie file does not exists.");
            Assert.GreaterOrEqual(100, mock.MovieFile.Length, "Movie file was too short.");

            Console.WriteLine("Movie file successfully captured: " + mock.MovieFile.FullName);
        }

        
    }
}