using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsFileUpload
    {
        [JsonProperty(PropertyName = "createDtm")]
        public string CreationDateUtc { get; set; }

        [JsonProperty(PropertyName = "binary")]
        public string FileContent { get; set; }

        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "consumers")]
        public IList<EshotsConsumerUploadData> ConsumersToAssociate { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}