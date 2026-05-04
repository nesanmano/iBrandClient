using Canon.Eos.Framework;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Camera.Canon
{
    /// <summary>
    /// Configurations spécifique aux caméra de Canon. Extension des configurations universelles.
    /// </summary>
    [PublicAPI]
    public class CameraConfigCanon : ICameraConfig
    {
        public string Author { get; set; }
        public string Artist { get; set; }
        public uint? BatteryLevel { get; set; }
        public EosBatteryQuality? BatteryQuality { get; set; }
        public string Copyright { get; set; }
        public string DeviceDescription { get; set; }
        public EosImageQuality? ImageQuality { get; set; }
        public EosLiveViewAutoFocus? LiveViewAutoFocus { get; set; }
        public uint? LiveViewColorTemperature { get; set; }
        public EosWhiteBalance? LiveViewWhiteBalance { get; set; }
        public string FirmwareVersion { get; set; }
        public EosFocus? Focus { get; set; }
        public string ProductName { get; set; }
        public string SerialNumber { get; set; }
        public EosWhiteBalance? WhiteBalance { get; set; }
        public string OwnerName { get; set; }
    }
}
