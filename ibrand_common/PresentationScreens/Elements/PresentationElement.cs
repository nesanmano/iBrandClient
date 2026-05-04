using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.PresentationScreens.Elements
{
    [DataContract]
    [PublicAPI]
    public class PresentationElement
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Name { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Clazz { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<string[]> Properties { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public Dictionary<string, PresentationElement> SubElements { get; set; }
    }
}