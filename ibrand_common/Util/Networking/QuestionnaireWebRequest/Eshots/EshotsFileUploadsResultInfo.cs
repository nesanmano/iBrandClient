using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsFileUploadsResultInfo : EshotsResult
    {
        [JsonProperty(PropertyName = "media")]
        public IList<EshotsFileUploadsResult> UploadResults { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}