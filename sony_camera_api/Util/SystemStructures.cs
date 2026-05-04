using Newtonsoft.Json;

namespace sony_camera_api.Util
{
    public class TimeOffset
    {
        [JsonProperty("dateTime")]
        public string DateTime { set; get; }

        [JsonProperty("timeZoneOffsetMinute")]
        public int TimeZoneOffsetMinute { set; get; }

        [JsonProperty("dstOffsetMinute")]
        public int DstOffsetMinute { set; get; }
    }
}
