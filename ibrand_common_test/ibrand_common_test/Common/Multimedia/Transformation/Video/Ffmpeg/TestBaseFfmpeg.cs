using System.IO;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.Video.Ffmpeg
{
    public class TestBaseFfmpeg : TestBaseNunit
    {
        protected string ExecLocationFfmpeg { get; set; }
        protected string ExecLocationFfprobe { get; set; }

        public override void InitConfigurationAndNlog()
        {
            base.InitConfigurationAndNlog();

            var config = new NomadConfiguration();
            var appDir = DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Test);

            var ffmpegConfig = new ConfigTestFfmpeg
            {
                FfmpegExecLocation = config.FfmpegLocation,
                FfprobeExecLocation = config.FfprobeLocation,
            };

            // Ajoute les informations de FFMPEG
            var ffmpegConfigFile = new FileInfo(appDir + "testConfigFfmpeg.json");
            if (!ffmpegConfigFile.Exists)
            {
                Assert.IsTrue(JsonUtil.WriteJsonFile(ffmpegConfigFile, ffmpegConfig));
                ffmpegConfigFile.Refresh();
            }

            var sers = JsonUtil.ReadJsonFile<ConfigTestFfmpeg>(ffmpegConfigFile);
            Assert.IsNotNull(sers);

            Assert.IsTrue(File.Exists(sers.FfmpegExecLocation), 
                "Cannot launch FFMPEG test suite without a valid FFMPEG executable location: " + sers.FfmpegExecLocation);
            Assert.IsTrue(File.Exists(sers.FfprobeExecLocation), 
                "Cannot launch FFMPEG test suite without a valid FFPROBE executable location." + sers.FfprobeExecLocation);

            config.FfmpegLocation = sers.FfmpegExecLocation;
            config.FfprobeLocation = sers.FfprobeExecLocation;
            ExecLocationFfmpeg = sers.FfmpegExecLocation;
            ExecLocationFfprobe = sers.FfprobeExecLocation;
        }
    }

    public class ConfigTestFfmpeg
    {
        public string FfmpegExecLocation { get; set; }
        public string FfprobeExecLocation { get; set; }
    }
}
