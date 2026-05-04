using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using NomadLogic.iBrandDesktop.Util.SubmitAction;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction
{
    public class TestStampede2016SubmitAction : TestBaseNunit
    {
        
        [Test]
        public void CreateStampede2016Video()
        {
            var timer = new Stopwatch();
            timer.Start();
            const string baseDir = @"C:\Users\François Coupal\Documents\projets\2016\MAC-2016-02 Calgary Stampede\test\";

            const string videoFileName = baseDir + "Vid_stampede2.mp4";
            Assert.IsTrue(File.Exists(videoFileName));
            
            const string videoIntro = baseDir + "intro.jpg";
            Assert.IsTrue(File.Exists(videoIntro));
            const string bkgVideo = baseDir + "background_1.mp4";
            Assert.IsTrue(File.Exists(bkgVideo));
            const string soundtrack = baseDir + "sound.aac";
            Assert.IsTrue(File.Exists(soundtrack));
            const string interFile = baseDir + "middle.jpg";
            Assert.IsTrue(File.Exists(interFile));
            const string overlayFile = baseDir + "overlay.png";
            Assert.IsTrue(File.Exists(overlayFile));
            const string output = baseDir + @"result\final_1080.mp4";
            EnsureFileNotExists(output);
            
            const string videoIntro2 = baseDir + "intro-outro_720.jpg";
            Assert.IsTrue(File.Exists(videoIntro2));
            const string bkgVideo2 = baseDir + "bkg_720.mp4";
            Assert.IsTrue(File.Exists(bkgVideo2));
            const string interFile2 = baseDir + "middle_720.jpg";
            Assert.IsTrue(File.Exists(interFile2));
            const string overlayFile2 = baseDir + "overlay_720.png";
            Assert.IsTrue(File.Exists(overlayFile2));
            const string output2 = baseDir + @"result\final_720.mp4";
            EnsureFileNotExists(output2);

            const float cutTime = 6.0f;
            const float sectionTime = 4.0f;
            const float interTime = 2.0f;
            const float frame = 30f;
            const float fadeTime = 0.5f;
            const float secondSectionTime = 3.0f;
            const float slowFactor = 2.5f;

            StampedeCalgary2016VideoSubmitAction.CreateStampede2016Video("", output, videoFileName, videoIntro, bkgVideo, soundtrack,
                cutTime, overlayFile, sectionTime, interTime, fadeTime, secondSectionTime, slowFactor,
                frame, FfmpegChromaKeyInfo.NewInstanceChromaKeyGreen(), interFile, new Size(1920, 1080));

            Assert.IsTrue(File.Exists(output));
            timer.Stop();
            Console.WriteLine(@"Time 1920x1080: " + timer.Elapsed.TotalSeconds);

            timer.Restart();
            StampedeCalgary2016VideoSubmitAction.CreateStampede2016Video("", output2, videoFileName, videoIntro2, bkgVideo2, soundtrack,
                cutTime, overlayFile2, sectionTime, interTime, fadeTime, secondSectionTime, slowFactor,
                frame, FfmpegChromaKeyInfo.NewInstanceChromaKeyGreen(), interFile2, new Size(1280, 720));

            Assert.IsTrue(File.Exists(output));
            timer.Stop();
            Console.WriteLine(@"Time 1280x720: " + timer.Elapsed.TotalSeconds);
        }
    }
}