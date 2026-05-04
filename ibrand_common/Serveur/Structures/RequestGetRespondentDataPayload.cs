using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestGetRespondentDataPayload
    {
        [JsonRequired] [JsonProperty("respondent_identification")]
        public RespondentIdentificationStrategy RespondentIdentification { get; set; }
        
        /// <summary>
        /// La liste des description des filtres doit avoir la forme "qX(cY)", tel que décrit par la classe <see cref="QuestionIdentifier"/>.
        /// </summary>
        [JsonProperty("answers_filter")]
        public List<string> QuestionDescriptionsFilter { get; set; }

        [JsonProperty("dynamic_values_filter")]
        public RespondentDataDynamicValueRequestFilter DynamicValuesFilter { get; set; }
    }

    #region Nested classes

    // Sérieux, ce niveau est inutile....
    public class RespondentIdentificationStrategy
    {
        [JsonProperty("search_strategy")]
        [JsonRequired]
        public RespondentIdentificationStrategyDefinition Strategy { get; set; }
    }

    public class RespondentIdentificationStrategyDefinition
    {
        
        [JsonProperty("searchStrategy")]
        public RequestRespondentDataIdentificationStrategyEnum StrategyType { get; set; }

        //"questionnaire_id": <int>,
        [JsonProperty("questionnaire_id")]
        public int QuestionnaireId { get; set; }

        //"terminal_id": <int>,
        [JsonProperty("terminal_id")]
        public int TerminalId { get; set; }

        //"identifier": <string>
        [JsonProperty("identifier")]
        public string ExternalUniqueIdentifier { get; set; }

        [JsonProperty("fail_on_empty")]
        public bool FailOnEmpty { get; set; } 

        //"question_no": <int>,
        [JsonProperty("question_no")]
        public int QuestionNo { get; set; }

        //"choice_no": <int>,
        [JsonProperty("choice_no")]
        public int ChoiceNo { get; set; }
    }
    
    public class RespondentDataDynamicValueRequestFilter
    {
        [JsonIgnore]
        public DateTimeOffset? MinimalDate { get; set; } = DateTimeOffset.MinValue;

        [JsonProperty("min_date")] 
        public string MinimalDateStr => MinimalDate?.ToString(DateUtil.UtcTimezoneFormatString);

        [JsonIgnore]
        public DateTimeOffset? MaximumDate { get; set; }

        [JsonProperty("max_date")] 
        public string MaximumDateStr => MaximumDate?.ToString(DateUtil.UtcTimezoneFormatString);

        [JsonRequired] [JsonProperty("keys_filter")] 
        public List<string> DynamicKeysList { get; set; }
    }

    #endregion
}