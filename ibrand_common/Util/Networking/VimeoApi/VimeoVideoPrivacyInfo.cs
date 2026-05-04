namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe JSON pour la configuration de confidentialité d'un vidéo.
    /// </summary>
    public class VimeoVideoPrivacyInfo
    {
        /// <summary>
        /// Le mode d'affichage à partir du site Vimeo.
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// Le mode d'affichage à partir d'une page HTML externe.
        /// </summary>
        public string Embed { get; set; }

        /// <summary>
        /// Indique s'il est possible de télécharger le vidéo de l'extérieur du site Vimeo.
        /// </summary>
        public bool Download { get; set; }

        /// <summary>
        /// TODO Je ne sais pas à quoi fait référence cette valeur.
        /// </summary>
        public bool Add { get; set; }

        /// <summary>
        /// Indique les droits d'ajout/modification/destruction des commentaires sur le vidéo.
        /// </summary>
        public string Comments { get; set; }
    }
}