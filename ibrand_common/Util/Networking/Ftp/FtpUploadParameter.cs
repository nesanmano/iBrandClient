using System.Text;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    /// <summary>
    /// Classe de paramètre pour une tâche Ftp d'envoi de fichiers..
    /// </summary>
    public class FtpUploadParameter
    {
        /// <summary>
        /// Le répertoire destination sur le serveur FTP.
        /// </summary>
        internal string[] OutputDirectories
        {
            get;
            set;
        }

        /// <summary>
        /// Le fichier local à téléverser.
        /// </summary>
        internal string[] FilesToUpload
        {
            get;
            set;
        }

        /// <summary>
        /// Le répertoire d'archive sur l'ordinateur local où déplacer le fichier une fois transmis.
        /// Peut être null (aucun déplacement), un lieu absolu (C:\la\bas), ou bien un endroit relatif (.\archive)
        /// </summary>
        internal string ArchiveDirectory
        {
            get;
            set;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var s in FilesToUpload)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append("|");
                }
                sb.Append(StringUtil.GetFileName(s));
            }
            return sb.ToString();
        }
    }
}