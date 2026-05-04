using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestSyncRespondent
    {
        [JsonProperty("local_id")]
        [JsonRequired]
        public int RespondentId { get; set; }

        [JsonProperty("language")]
        [JsonRequired]
        public string Language { get; set; }

        [JsonProperty("local_date_created")]
        [JsonRequired]
        public string CreationDateStr => CreationDate.ToString(DateUtil.UtcTimezoneFormatString);

        [JsonIgnore]
        public DateTime CreationDate { get; set; } = DateTime.MinValue;

        [JsonProperty("answers")]
        [JsonRequired]
        public IList<Answer> Answers { get; set; }
    }
}