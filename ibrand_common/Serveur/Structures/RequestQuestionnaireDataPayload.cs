using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestQuestionnaireDataPayload
    {
        [JsonRequired] [JsonProperty("questionnaire_id")]
        public int QuestionnaireId { get; set; }

        [JsonProperty("terminal_id")]
        public int? TerminalId { get; set; }

        [JsonProperty("sort_by_descending_date")]
        public bool ReverseOrderByDate { get; set; }

        [JsonRequired] [JsonProperty("search_params")]
        public SearchParametersQuestionnaireData SearchParameters { get; set; }
    }

    public class SearchParametersQuestionnaireData
    {
        [JsonIgnore]
        public DateTimeOffset MinimalDate { get; set; } = DateTimeOffset.MinValue;

        [JsonRequired] [JsonProperty("min_date")] 
        public string MinimalDateStr => MinimalDate.ToString(DateUtil.UtcTimezoneFormatString);

        [JsonIgnore]
        public DateTimeOffset? MaximumDate { get; set; }

        [JsonProperty("max_date")] 
        public string MaximumDateStr => MaximumDate?.ToString(DateUtil.UtcTimezoneFormatString);

        [JsonProperty("max_element")] 
        public int? MaximumResponse { get; set; }

        [JsonRequired] [JsonProperty("question_def_list")] 
        public IList<string> QuestionDefinitions { get; set; } = new List<string>();
    }
}