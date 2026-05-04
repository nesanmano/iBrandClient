using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas
{
    public interface INameI18N
    {
        // id	int
        [JsonProperty(PropertyName = "id")]
        int Id { get; set; }
        // name	string
        [JsonProperty(PropertyName = "name")]
        string Name { get; set; }
        // name_i18n	string
        [JsonProperty(PropertyName = "name_i18n")]
        string NameI18N { get; set; }
    }
}