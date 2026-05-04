using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    public class VimeoAccountInfo
    {
        /// <summary>
        /// Uri relatif du compte authentifié à partir de la racine de l'API de Vimeo. Example: "/users/11563808"
        /// </summary>
        public string Uri { get; set; }
        
        /// <summary>
        /// Nom de l'usager Vimeo
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Lien public vers le compte de l'usager sur Vimeo.com
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Lieu où habite l'usager.
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Description de l'usager par lui-même.
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Date de création de l'usager.
        /// </summary>
        [JsonProperty(PropertyName = "created_time")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Type de compte.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Images du portrait de l'usager vimeo.
        /// </summary>
        public VimeoPicturesInfo Pictures { get; set; }

        /// <summary>
        /// Information sur les sites web associé au compte.
        /// </summary>
        public List<VimeoWebsiteInfo> Websites { get; set; }

        /// <summary>
        /// Information d'espace pour les vidéos. Représente un maximum de transfert dans une période donnée (semaines).
        /// </summary>
        [JsonProperty(PropertyName = "upload_quota")]
        public VimeoSpaceInfo UploadQuota { get; set; }

        /// <summary>
        /// Identifiant unique de la ressource.
        /// </summary>
        [JsonProperty(PropertyName = "resource_key")]
        public string ResourceKey { get; set; }
    }
}