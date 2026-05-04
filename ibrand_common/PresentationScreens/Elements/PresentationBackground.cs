using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.PresentationScreens.Elements
{
    [DataContract]
    [PublicAPI]
    public class PresentationBackground : IPresentationLanguageDependent
    {
        #region Implementation of IPresentationLanguageDependent

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string LanguageCode { get; set; }
        
        #endregion

        /// <summary>
        /// Le fichier de l'image de fond.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ImageFile { get; set; }
    }
}
