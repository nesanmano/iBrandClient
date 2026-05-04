using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas
{
    public interface ICreationDate
    {
        [JsonProperty(PropertyName = "created_at")]
        long CreationDateInt { get; set; }
    }
}