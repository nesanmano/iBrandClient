using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsEventDayFileUploads
    {
        [JsonProperty(PropertyName = "eventDayId")]
        public int EventDayId { get; set; }

        [JsonProperty(PropertyName = "interactions")]
        public IList<EshotsFileUpload> EventDaysUploads { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}