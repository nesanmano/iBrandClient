using System.Runtime.Serialization;

namespace NomadLogic.PrintStation.Json
{
    /// <summary>
    /// Représente un Post venant du serveur Hastag principal. Contient la plupart des informations utiles à un Printstation, incluant où chercher les images,
    /// mais pas les images elle-mêmes.
    /// </summary>
    [DataContract]
    public class Post
    {
        /// Format de date stocké dans la BD sur le serveur et localement.
        public const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Constructeur vide.
        /// </summary>
        public Post() { /* Vide */ }

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="id">Identifiant local Printstation unique du Post.</param>
        /// <param name="elementId">Identifiant unique venant de l'application sociale.</param>
        /// <param name="imageUrl">URL de l'image principal du post.</param>
        /// <param name="userImageUrl">URL de l'image de l'usager de l'application sociale.</param>
        /// <param name="socialAppUserName">Nom de l'usager de l'application sociale.</param>
        /// <param name="creationDate">Date de création</param>
        /// <param name="imageDownloaded">Indique si on a téléchargée l'image principale ou non.</param>
        /// <param name="userImageDownloaded">Indique si on a téléchargé l'image de l'usager du service social.</param>
        /// <param name="updateId">Identifiant de la dernière mise à jour.</param>
        /// <param name="statusId">Valeur énumérative du status du Post sur le serveur.</param>
        /// <param name="serverId">Valeur de clé primaire sur le serveur HashTag lié à ce Post.</param>
        /// <param name="repondantNoFromServer">Le numéro de répondant associé au serveur.</param>
        public Post(long id, string elementId, string imageUrl, string userImageUrl, string socialAppUserName,
            string creationDate, bool imageDownloaded, bool userImageDownloaded, int updateId, int statusId,
            int serverId, int repondantNoFromServer)
        {
            Id = id;
            ElementId = elementId;
            PictureUrl = imageUrl;
            UserPictureUrl = userImageUrl;
            SocialAppUsername = socialAppUserName;
            CreationDate = creationDate;
            PictureDownloaded = imageDownloaded;
            UserPictureDownloaded = userImageDownloaded;
            UpdateId = updateId;
            StatusId = statusId;
            ServerId = serverId;
            RepondantNoFromServer = repondantNoFromServer;
        }

        /// <summary>
        /// Identifiant numérique qui associe le Post à un répondant sur le serveur principal. Uniquement utilisé si la source d'images est
        /// un projet iBrand. La valeur -1 est utilisée si il n'y a pas de lien entre ce Post et un répondant.
        /// </summary>
        [DataMember(Name = "ibrand_respondent_no")]
        public int RepondantNoFromServer { get; set; } = -1;

        /// <summary>
        /// Identifiant numérique unique (clé primaire) du Post sur le serveur principal.
        /// </summary>
        [DataMember(Name = "id")]
        public int ServerId { get; set; }

        /// <summary>
        /// Identifiant unique venant du service social d'où viens le Post d'origine, on peut déduire sa provenance d'après son format, mais ce n'est pas
        /// recommandé.
        /// </summary>
        [DataMember(Name = "element_id")]
        public string ElementId { get; set; }

        /// <summary>
        /// L'identifiant du "feed" Hastag du serveur principal qui sert à obtenir des Posts venant des services sociaux. Un feed est lié à un événement en particulier.
        /// </summary>
        [DataMember(Name = "feed_id")]
        public string FeedId { get; set; }

        /// <summary>
        /// Chemin web pour aller chercher l'image sur le web.
        /// </summary>
        [DataMember(Name = "picture_url")]
        public string PictureUrl { get; set; }

        /// <summary>
        /// Chemin web pour chercher l'image de présentation d'un usager du service social duquel on a obtenu le Post.
        /// </summary>
        [DataMember(Name = "user_picture_url")]
        public string UserPictureUrl { get; set; }

        /// <summary>
        /// Le nom de l'usager du service social duquel est tiré le Post.
        /// </summary>
        [DataMember(Name = "user_name")]
        public string SocialAppUsername { get; set; }

        /// <summary>
        /// Le nom de l'application sociale (Twitter, Instagram, Facebook, iBrand, Eshots, ...) d'où à été obtenu le Post.
        /// </summary>
        [DataMember(Name = "service_name")]
        public string SocialAppServiceName { get; set; }

        /// <summary>
        /// Date associée au Post sur le serveur. Représente la date que le Post à été créé sur le serveur.
        /// Format: "yyyy-MM-dd hh:mm:ss"
        /// </summary>
        [DataMember(Name = "date")]
        public string CreationDate { get; set; }

        /// <summary>
        /// Énumération d'état du Post sur le serveur (0,1,2)
        /// TODO: Décrire ces valeurs.
        /// </summary>
        [DataMember(Name = "status_id")]
        public int StatusId { get; set; }

        /// <summary>
        /// Identifiant numérique indiquant le dernier changement qu'on a fait sur le Post au niveau du serveur (comme création, annulation, destruction).
        /// Permet de savoir si on a modifié le Post sur le serveur, et si on doit mettre à jour le Post local.
        /// </summary>
        [DataMember(Name = "update_id")]
        public int UpdateId { get; set; }

        /// <summary>
        /// Identifiant numérique lié à l'événement Hashtag qui contient ce Post.
        /// </summary>
        [DataMember(Name = "event_id")]
        public int EventId { get; set; }

        /// <summary>
        /// Indique si on a téléchargé l'image de ce Post à partir du Net.
        /// </summary>
        public bool PictureDownloaded { get; set; }
        
        /// <summary>
        /// Indique si on a téléchargé l'image de l'usager du service social à partir du Net.
        /// </summary>
        public bool UserPictureDownloaded { get; set; }

        /// <summary>
        /// Identifiant unique LOCAL pour le Printstation.
        /// </summary>
        public long Id { get; set; }
    }
}