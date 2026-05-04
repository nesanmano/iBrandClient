using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe d'information JSON sur les images provenant du site web Vimeo.
    /// </summary>
    public class VimeoPicturesInfo
    {
        /// <summary>
        /// Lien relatif de l'image par rapport à l'API Vimeo utilisé.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Indique si l'image est active ou non. Aucune idée de ce que non implique.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Le type d'image, selon Vimeo.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Liste des dimensions possibles avec cette image.
        /// </summary>
        public List<VimeoPictureSizeInfo> Sizes { get; set; }

        /// <summary>
        /// Identifiant unique de la ressource.
        /// </summary>
        [JsonProperty(PropertyName = "resource_key")]
        public string ResourceKey { get; set; }
    }
}