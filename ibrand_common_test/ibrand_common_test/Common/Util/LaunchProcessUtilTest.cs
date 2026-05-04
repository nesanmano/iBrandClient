using System;
using System.IO;
using NomadLogic.Common.Util;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Util
{
    public class LaunchProcessUtilTest : TestBaseNunit
    {
        [Test]
        public void LaunchVlc()
        {
            var videoFile = BaseDir + "swing_slow_crop.mp4";
            Assert.IsTrue(File.Exists(videoFile));

            var outputFile = GetFullOutputDir() + "output.txt";
            EnsureFileNotExists(outputFile);

            const string VLC_LOCATION = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";

            if (!File.Exists(VLC_LOCATION))
            {
                Assert.Ignore("No VLC executable found.");
            }

            var args = $"\"{videoFile}\"";

            Console.Out.WriteLine("Video file: " + videoFile);

            LaunchProcessUtil.LaunchProcess(VLC_LOCATION, args, new FileInfo(outputFile), null, false); // Output vide.
        }
    }
}