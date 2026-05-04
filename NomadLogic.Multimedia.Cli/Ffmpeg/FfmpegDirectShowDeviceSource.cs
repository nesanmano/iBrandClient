namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    public class FfmpegDirectShowDeviceSource
    {
        public FfmpegDirectShowDeviceSourceType DeviceType { get; set; } = FfmpegDirectShowDeviceSourceType.Unknown;
        public string Name { get; set; }
        public string AlternativeName { get; set; } // En ce moment, il n'y a qu'un nom alternatif.

        public FfmpegDirectShowDeviceSourceOptions Options { get; set; }
        
    }
}