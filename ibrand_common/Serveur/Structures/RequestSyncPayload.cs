using Newtonsoft.Json;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestSyncPayload
    {
        [JsonProperty("client_machine")]
        [JsonRequired]
        public string ClientMachine { get; set; }

        [JsonProperty("client_type")]
        [JsonRequired]
        public int ClientType => 1;

        [JsonProperty("project_id")]
        [JsonRequired]
        public int ProjectId { get; set; }

        [JsonProperty("questionnaire_id")]
        [JsonRequired]
        public int QuestionnaireId { get; set; }

        [JsonProperty("terminal_id")]
        [JsonRequired]
        public int TerminalId { get; set; }

        [JsonProperty("respondent")]
        [JsonRequired]
        public RequestSyncRespondent Respondent { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}