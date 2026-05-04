using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestQuestionnaireDataResponse
    {
        [JsonRequired] [JsonProperty("nb_elements_found")]
        public int AnswersNumber { get; set; }
        
        [JsonRequired] [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("last_element_date")]
        public DateTime LastElementDate { get; set; } = DateTime.MinValue;

        [JsonProperty("elements")]
        public IList<RequestQuestionnaireDataAnswer> Elements { get; set; } = new List<RequestQuestionnaireDataAnswer>();
    }

    public class RequestQuestionnaireDataAnswer
    {
        [JsonRequired] [JsonProperty("id")]
        public int RespondentServerId { get; set; }

        [JsonRequired] [JsonProperty("inserted_local_time")]
        public DateTime CreationDate { get; set; }

        [JsonRequired] [JsonProperty("responses")]
        public IList<Answer> Answers { get; set; }
    }
}