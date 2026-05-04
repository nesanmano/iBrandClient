using System;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe de marshalling JSON pour contenir l'information d'un album de vidéo du site Vimeo.
    /// </summary>
    public class VimeoAlbumInfo
    {
        /// <summary>
        /// Le nom de l'album
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Lien relatif au API de Vimeo pour interagir avec l'album.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Description sommaire de l'album.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Description sommaire de l'album.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Date de création de l'album.
        /// </summary>
        [JsonProperty(PropertyName = "created_time")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Dernière date de modification de l'album.
        /// </summary>
        [JsonProperty(PropertyName = "modified_time")]
        public DateTime ModifiedTime { get; set; }
        
        /// <summary>
        /// Le type de confidentialité de l'album.
        /// Uniquement utilisé à des fins d'édition de l'album
        /// </summary>
        public VimeoPrivacyInfo Privacy { get; set; }
        
        /// <summary>
        /// Extrait l'identifiant numérique unique de cet album. Si inexistant, sera -1.
        /// </summary>
        public int Id {
            get {
                if (Uri == null) return -1;
                var strVal = Uri.Substring(Uri.LastIndexOf('/') + 1);
                return int.Parse(strVal);
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as VimeoAlbumInfo;
            return Equals(other);
        }

        protected bool Equals(VimeoAlbumInfo other)
        {
            if (other == null) return false;
            return string.Equals(Name, other.Name) && 
                string.Equals(Uri, other.Uri) && 
                string.Equals(Description, other.Description) && 
                string.Equals(Link, other.Link) && 
                CreatedTime.Equals(other.CreatedTime) && 
                ModifiedTime.Equals(other.ModifiedTime) && 
                Equals(Privacy, other.Privacy);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id;
        }
    }
}