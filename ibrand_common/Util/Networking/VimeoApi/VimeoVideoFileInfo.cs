using System;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe JSON pour décrire une version d'un fichier multimédia d'un vidéo. Un vidéo peut posséder plusieures
    /// variations de fichiers.
    /// Une description de fichier qui contient une largeur/hauteur égale à un implique que ce fichier est en traitement
    /// ou/et ne peut pas être utilisé.
    /// </summary>
    public class VimeoVideoFileInfo
    {
        /// <summary>
        /// Le type de qualité du vidéo.
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// Le type de fichier. Essentiellement, indique s'il s'agit du fichier original ("source") ou un fichier
        /// dérivé.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Largeur en pixels du vidéo.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// La hauteur du vidéo.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Vitesse de trame pour le vidéo.
        /// </summary>
        public double Fps { get; set; }

        /// <summary>
        /// Taille en octets du fichier vidéo sur disque.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Hash MD5 du fichier vidéo.
        /// </summary>
        public string Md5 { get; set; }

        /// <summary>
        /// Le temps de création (sur le serveur Vimeo), UTC +0:00.
        /// </summary>
        [JsonProperty(PropertyName = "created_time")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// URL pour visualiser le fichier vidéo.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// URL sécurisé (HTTPS) pour visualiser le fichier vidéo.
        /// </summary>
        [JsonProperty(PropertyName = "link_secure")]
        public string LinkSecure { get; set; }
    }
}