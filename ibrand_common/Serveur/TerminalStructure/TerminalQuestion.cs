using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur.TerminalStructure
{
    public class TerminalQuestion
    {
        [JsonProperty("no")]
        public int Num { get; set; }

        [JsonProperty("clazz")]
        public string Clazz { get; set; }

        [JsonProperty("validator")]
        public object Validator { get; set; }

        [JsonProperty("mandatory")]
        public bool Mandatory { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("recyclable")]
        public bool Recyclable { get; set; }

        [JsonProperty("toSave")]
        public bool ToSave { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }

        [JsonProperty("toSync")]
        public bool ToSync { get; set; }

        [JsonProperty("toXML")]
        public bool ToXml { get; set; }

        [JsonProperty("texts")]
        public Dictionary<string,string> Texts { get; set; }

        [JsonProperty("choices")]
        public TerminalChoice[] Choices { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonIgnore]
        public string ConfigLanguage { get; set; }
    }
}
