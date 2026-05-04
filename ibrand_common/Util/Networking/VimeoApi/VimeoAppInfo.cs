namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe JSON pour les information d'une application Vimeo, qui servent à téléverser les vidéos.
    /// </summary>
    public class VimeoAppInfo
    {

        /// <summary>
        /// Le nom de l'application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Chemin relatif par rapport à l'API Vimeo pour trouver des informations sur cette application.
        /// </summary>
        public string Uri { get; set; }
    }
}