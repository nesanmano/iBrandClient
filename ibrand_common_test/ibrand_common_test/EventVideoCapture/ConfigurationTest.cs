using System;
using System.Collections.Generic;
using System.IO;
using NomadLogic.App.EventVideoCapture.Capture;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Video;
using NUnit.Framework;

namespace NomadLogic.Test.EventVideoCapture
{
    public class ConfigurationTest : TestBaseNunit
    {
        [Test]
        public void ConfigurationFileGenerator()
        {
            var outDir = GetFullOutputDir();

            var capFfScript = new CaptureFfmpegScript
            {
                DelayAfterTrigger = TimeSpan.Zero,
                CaptureConfiguration = new VideoCaptureConfiguration<VideoCaptureFfmpeg>
                {
                    CreatePreviewVideo = false,
                    SourcesConfigurations = new List<VideoCaptureFfmpeg>
                    {
                        new VideoCaptureFfmpeg
                        {
                            CaptureDelay = TimeSpan.FromSeconds(10),
                            ScriptFileName = "ffmpeg_cam_capture_01.txt",
                            VideoName = "01.mp4",
                            SourceName = "Axis Source"
                        },
                        new VideoCaptureFfmpeg
                        {
                            CaptureDelay = TimeSpan.FromSeconds(5),
                            ScriptFileName = "ffmpeg_cam_capture_03.txt",
                            VideoName = "03.mp4",
                            SourceName = "W12X Source"
                        },
                        new VideoCaptureFfmpeg
                        {
                            CaptureDelay = TimeSpan.FromSeconds(0),
                            ScriptFileName = "ffmpeg_cam_capture_02.txt",
                            VideoName = "02.mp4",
                            SourceName = "WAX Source"
                        }
                    }
                }
            };

            CaptureVideoBase vidBaseConfig = capFfScript; // On cache l'implémentation exacte.

            var outFileSource = new FileInfo(outDir + "capture_script_ffmpeg.json");
            EnsureFileNotExists(outFileSource);

            Assert.IsTrue(JsonUtil.WriteJsonFile(outFileSource, vidBaseConfig));
            outFileSource.Refresh();

            var fromFile = JsonUtil.ReadJsonFile<CaptureVideoBase>(outFileSource);
            Assert.IsNotNull(fromFile);

        }
    }
}