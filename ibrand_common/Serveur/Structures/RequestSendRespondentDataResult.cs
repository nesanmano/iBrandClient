using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestSendRespondentDataResult
    {
        [JsonRequired] [JsonProperty("target_respondent")]
        public RequestRespondentDataTarget TargetRespondent { get; set; }

        [JsonProperty("answers_result")]
        public RequestSendRespondentResultDataInfo AnswersResults { get; set; }

        [JsonProperty("dynamic_values_result")]
        public RequestSendRespondentResultDataInfo DynamicValuesResults { get; set; }
    }
    
    public class RequestSendRespondentResultDataInfo
    {
        [JsonRequired] [JsonProperty("added")]
        public int Added { get; set; }
        
        [JsonRequired] [JsonProperty("replaced")]
        public int Replaced { get; set; }

        [JsonRequired] [JsonProperty("ignored")]
        public int Ignored { get; set; }

        [JsonProperty("errors")]
        public List<string> ErrorMessages { get; set; }
    }
}