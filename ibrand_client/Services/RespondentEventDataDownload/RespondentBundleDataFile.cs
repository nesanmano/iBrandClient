using System.IO;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload
{
    public class RespondentBundleDataFile
    {
        /// <summary>
        /// Valeur jeton venant du serveur.
        /// </summary>
        public string OriginalDataValue { get; set; }

        /// <summary>
        /// L'index de cet item dans la séquence de valeur dynamiques. (IE: le "Xième élément" dans la liste)
        /// </summary>
        public int OriginalDataIndex { get; set; }

        /// <summary>
        /// Indique si la valeur du jeton est un lien valide vers un fichier téléchargeable.
        /// </summary>
        /// <returns></returns>
        public bool IsDownloadableFile()
        {
            return OriginalDataValue?.StartsWith("file:") ?? false;
        }

        /// <summary>
        /// Référence fichier finale de la valeur une fois présente sur le système. Peut ne pas encore exister.
        /// </summary>
        public FileInfo LocalFile { get; set; }

        /// <summary>
        /// Indique si le fichier est complètement téléchargé, et n'est pas sujet à un traitement du service de téléchargement.
        /// </summary>
        public bool Downloaded { get; set; }

        /// <summary>
        /// Indique si le fichier est présent dans le projet terminal; en gros, s'il a été acheté ou "processé" de façon officielle.
        /// </summary>
        public bool Processed { get; set; }
        
        public DirectoryInfo LocalDirectory { get; set; }
        
        /// <summary>
        /// Indique qu'une erreur s'est glissée et que cette référence de fichier est invalide.
        /// </summary>
        public bool HasError { get; set; }
    }
}