using System;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe JSON pour une option de téléchargement d'un vidéo Vimeo.
    /// </summary>
    public class VimeoVideoDownloadInfo : VimeoVideoFileInfo
    {
        /// <summary>
        /// Date à laquelle le lien de téléchargement "Link" devient inutile.
        /// </summary>
        public DateTime Expires { get; set; }
    }

}