using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe JSON pour un ticket d'envoi vidéo vers Vimeo en utilisant le mode "streaming".
    /// </summary>
    public class VimeoUploadTicketInfo
    {
        /// <summary>
        /// Le URI relatif du ticket pour l'usager.
        /// ex: /users/12345/tickets/12345
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Le URI relatif du ticket pour l'usager.
        /// ex: /users/12345/tickets/12345
        /// </summary>
        [JsonProperty(PropertyName = "complete_uri")]
        public string CompleteUri { get; set; }

        /// <summary>
        /// L'identifiant unique du ticket.
        /// </summary>
        [JsonProperty(PropertyName = "ticket_id")]
        public string TicketId { get; set; }

        /// <summary>
        /// Le lien URL complet pour l'envoi du viédo.
        /// </summary>
        [JsonProperty(PropertyName = "upload_link_secure")]
        public string UploadLinkSecure { get; set; }
    }
}