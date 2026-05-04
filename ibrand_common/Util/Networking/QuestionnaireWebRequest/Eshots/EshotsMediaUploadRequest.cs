using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsMediaUploadRequest
    {
        [JsonProperty(PropertyName = "eventDayId")]
        public int EventDayId { get; set; }

        [JsonProperty(PropertyName = "interactions")]
        public IList<EshotsFileUploadDetail> FileList { get; set; }
    }
}