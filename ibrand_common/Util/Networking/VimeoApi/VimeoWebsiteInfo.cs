namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe d'information JSON pour un site web web d'usager de Vimeo.
    /// </summary>
    public class VimeoWebsiteInfo
    {
        /// <summary>
        /// Nom simple du site web.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descriptif court à propos du site web.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL web pour le site web en question.
        /// </summary>
        public string Link { get; set; }
    }
}