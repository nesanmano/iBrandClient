using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    [PublicAPI]
    public class Event : BaseCreationDate
    {
        public const string DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK";

        /*
         *  "id": 1000001,
            "event_template": OrderedMap {<Template>},
            "start_datetime": "2020-09-30T17:37:39.000000Z",
            "end_datetime": "2023-07-13T18:00:00.000000Z",
            "created_at": 1594590066*/
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("event_template")]
        public List<EventTemplate> Templates { get; set; }
        [JsonProperty("start_datetime")]
        public string StartDateString { get; set; }
        [JsonProperty("end_datetime")]
        public string EndDateString { get; set; }
        [JsonIgnore]
        public DateTime StartDate
        {
            get => DateTime.ParseExact(StartDateString, DATE_FORMAT, CultureInfo.InvariantCulture);

            set => StartDateString = value.ToString(DATE_FORMAT);
        }
        [JsonIgnore]
        public DateTime EndDate
        {
            get => DateTime.ParseExact(EndDateString, DATE_FORMAT, CultureInfo.InvariantCulture);

            set => EndDateString = value.ToString(DATE_FORMAT);
        }
    }
}