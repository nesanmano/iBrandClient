using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.PresentationScreens.Elements;

namespace NomadLogic.Common.PresentationScreens
{
    [PublicAPI]
    [DataContract]
    public class PresentationScreenData
    {
        /// <summary>
        /// Nom de la classe à instancier.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ScreenClassName { get; set; }

        /// <summary>
        /// Les images de fond à utiliser.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<PresentationBackground> Backgrounds { get; set; }

        /// <summary>
        /// Les éléments qui composent l'écran
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<PresentationElement> Elements { get; set; }


    }
}