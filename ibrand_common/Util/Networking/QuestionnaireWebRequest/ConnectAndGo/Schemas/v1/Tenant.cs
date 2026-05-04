using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class Tenant : BaseNameI18N
    {
        /*
         * OrderedMap { "id": 1, "short_name": "cng", "name": "Connect&GO", "name_i18n": "" }
         */
        
        // short_name	string
        [JsonProperty(PropertyName = "short_name")]
        public string ShortName { get; set; }
    }
}