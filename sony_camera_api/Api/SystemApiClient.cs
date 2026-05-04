using System;
using System.Threading.Tasks;
using sony_camera_api.Internal;
using sony_camera_api.Util;

namespace sony_camera_api.Api
{
    public class SystemApiClient : ApiClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint">Endpoint URL of system service.</param>
        public SystemApiClient(Uri endpoint)
            : base(endpoint)
        {
        }

        public async Task SetCurrentTimeAsync(DateTimeOffset UtcTime, int OffsetInMinute)
        {
            var req = new TimeOffset
            {
                DateTime = UtcTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                TimeZoneOffsetMinute = OffsetInMinute,
                DstOffsetMinute = 0
            };
            await NoValue(RequestGenerator.Serialize("setCurrentTime", ApiVersion.V1_0, req)).ConfigureAwait(false); ;
        }
    }
}
