using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur.TerminalStructure
{
    public class TerminalChoice
    {
        [JsonProperty("no")]
        public int No { get; set; }

        [JsonProperty("clazz")]
        public string Clazz { get; set; }

        [JsonProperty("texts")]
        public Dictionary<string, string> Texts { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

    }
}
