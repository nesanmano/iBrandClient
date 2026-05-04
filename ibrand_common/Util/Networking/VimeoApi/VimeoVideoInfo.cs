using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe JSON pour les résultats d'une requête d'information vidéo avec l'API de Vimeo.
    /// Un vidéo possède deux identifiants: un identifiant court, qui ressemble à un entier, et un identifiant complet
    /// qui est un string composé de l'identifiant court plus un identifiant clé ajouté après un ":". Les deux sont valides.
    /// Une duration de zéro indique que le viédo n'est pas encore prêt pour être joué.
    /// </summary>
    [PublicAPI]
    public class VimeoVideoInfo
    {
        /// <summary>
        /// Le chemin relatif pour trouver le vidéo avec l'API de Vimeo.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// L'identifiant court, constitué seulement d'un nombre.
        /// </summary>
        public string Id => Uri.Substring(Uri.LastIndexOf("/", StringComparison.InvariantCulture) + 1);

        /// <summary>
        /// Lien URL direct pour visionner le vidéo.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Le nom, ou titre, du vidéo.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// La description associée au vidéo.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// La duration, en secondes, du vidéo.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// La langue associée à ce vidéo.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Le temps de création (sur le serveur Vimeo), UTC +0:00.
        /// </summary>
        [JsonProperty(PropertyName = "created_time")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Le temps de la dernière modification (sur le serveur Vimeo), UTC +0:00.
        /// </summary>
        [JsonProperty(PropertyName = "modified_time")]
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// Le temps de disponibilité générale (sur le serveur Vimeo), UTC +0:00.
        /// </summary>
        [JsonProperty(PropertyName = "release_time")]
        public DateTime ReleaseTime { get; set; }

        /// <summary>
        /// Liste des étiquettes de contenu associées au vidéo.
        /// </summary>
        [JsonProperty(PropertyName = "content_rating")]
        public List<string> ContentRatings { get; set; }

        /// <summary>
        /// Le nom de la license de droit associée au vidéo.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// Les configuration de confidentialité du vidéo.
        /// </summary>
        public VimeoVideoPrivacyInfo Privacy { get; set; }

        /// <summary>
        /// Les images associées à l'image.
        /// </summary>
        public VimeoPicturesInfo Pictures { get; set; }

        /// <summary>
        /// Les étiquettes associées à ce vidéo.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Les statistiques associées à un vidéo.
        /// </summary>
        public VimeoVideoStatsInfo Stats { get; set; }

        /// <summary>
        /// Les versions différentes des fichiers vidéos qui existent pour ce vidéo.
        /// Essentiellement, des versions plus grandes/petites que le fichier vidéo original pour fin de visualisation et
        /// téléchargement.
        /// </summary>
        public List<VimeoVideoFileInfo> Files { get; set; }

        /// <summary>
        /// Les versions différentes des options pour télécharger le vidéo.
        /// </summary>
        public List<VimeoVideoDownloadInfo> Downloads { get; set; }

        /// <summary>
        /// Information sur l'application qui a été utilisée pour téléverser le vidéo.
        /// </summary>
        public VimeoAppInfo App { get; set; }

        /// <summary>
        /// Le status actuel du vidéo. Indique les différents états du vidéo.
        /// Normalement, devient "availiable" quand on peut le visionner.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Clé directe pour la ressource Vimeo reliée à cet objet.
        /// </summary>
        [JsonProperty(PropertyName = "resource_key")]
        public string RessourceKey { get; set; }
    }
}