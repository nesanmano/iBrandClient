using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NomadLogic.Common.Services.LocalCapabilities;

namespace NomadLogic.Multimedia.Gpu.OpenGL
{
    /// <summary>
    /// Liste des fonctionnalités OpenGL détectées.
    /// </summary>
    [DataContract]
    // ReSharper disable once InconsistentNaming
    public class OpenGLSubCapabilities : SubCapabilityStatus
    {
        /// <summary>
        /// Nom de manufacturier de la carte graphique.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Vendor { get; set; }

        /// <summary>
        /// Version de OpenGL détectée.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Version { get; set; }

        /// <summary>
        /// La liste des extensions détectées.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<string> Extensions { get; set; } = new List<string>();
    }
}