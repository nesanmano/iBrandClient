using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsFileUploadsRequest
    {
        [JsonProperty(PropertyName = "media")]
        public IList<EshotsEventDayFileUploads> EventDaysUploads { get; set; }
    }
}