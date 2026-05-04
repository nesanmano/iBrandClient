using System.IO;
using NomadLogic.iBrandDesktop.Util.SubmitAction;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction
{
    public class TestHarleyDavidsonSubmitAction : TestBaseNunit
    {
        
        [Test]
        public void CreateHarleyDavidsonVideo()
        {
            const string baseDir = @"C:\Users\François Coupal\Documents\projets\2016\BON-2016-02 Harley V2\video\";

            const string videoFileName = baseDir + "resp.mp4";
            Assert.IsTrue(File.Exists(videoFileName));
            const string videoOutro = baseDir + "outro.mp4";
            Assert.IsTrue(File.Exists(videoOutro));
            const string soundtrack = baseDir + "sound.aac";
            Assert.IsTrue(File.Exists(soundtrack));
            const string overlayWhite = baseDir + "overlay_logoblanc.png";
            Assert.IsTrue(File.Exists(overlayWhite));
            const string overlay = baseDir + "overlay.png";
            Assert.IsTrue(File.Exists(overlay));
            
            const string output = baseDir + @"result\final_test.mp4";
            EnsureFileNotExists(output);

            HarleyDavidsonSubmitAction.CreateHarleyVideoVideo("", output, 
                videoFileName, videoOutro, soundtrack, overlayWhite, overlay,
                1.5, 1, 10, 7, 1, 9, 1);

            Assert.IsTrue(File.Exists(output));
        }
    }
}