using System;
using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur
{
    public class TerminalProjectInfo 
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("startDate")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("questionnaires")]
        public TerminalIbrandDefinition[] Questionnaires { get; set; }
        
        [JsonProperty("selected")]
        public bool Selected { get; set; }


    }
}
