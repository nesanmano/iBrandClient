using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// Classe pour stocker les résultat des test de capacités.
    /// </summary>
    [DataContract]
    public class CapabilitiesReport
    {
        /// <summary>
        /// Liste des groupes
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<CapabilityStatus> Groups { get; set; } = new List<CapabilityStatus>();
    }
}