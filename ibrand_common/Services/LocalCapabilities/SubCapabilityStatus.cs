using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// État d'une fonctionnalité précise d'un outil particulier.
    /// </summary>
    [DataContract]
    public class SubCapabilityStatus
    {
        /// <summary>
        /// Nom de la fonction testée.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string FunctionName { get; set; }

        /// <summary>
        /// Indique si de façon totale la fonctionnalité est disponible.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusEnum Status { get; set; } = StatusEnum.Pending;
    }
}