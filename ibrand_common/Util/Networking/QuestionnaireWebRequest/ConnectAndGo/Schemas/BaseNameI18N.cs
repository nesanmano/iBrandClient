using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas
{
    public abstract class BaseNameI18N : INameI18N
    {
        // id	int
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        // name	string
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        // name_i18n	string
        [JsonProperty(PropertyName = "name_i18n")]
        public string NameI18N { get; set; }
    }
}