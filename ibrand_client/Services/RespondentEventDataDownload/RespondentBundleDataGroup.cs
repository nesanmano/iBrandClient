using System.Collections.Concurrent;
using System.IO;
using NomadLogic.Common.Serveur.Structures;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload
{
    /// <summary>
    /// Cette classe représente les valeurs venant du serveur ainsi que leur représentation local sur le système de fichier, ainsi que leur état.
    /// On garde, en gros:
    /// * Les information de l'en-tête de la valeur dynamique associée.
    /// * Une liste d'information sur chaque élément de cette valeur dynamique, comme:
    /// -- Le jeton de téléchargement (la valeur venant du serveur)
    /// -- Le nom du fichier local associé à cette valeur.
    /// -- Si cet élément existe dans le projet B (en gros, s'il a été acheté ou "processé").
    /// Cette classe est généralement associée à une autre classe décorative, qui explique comment interpréter les fichiers, comme
    /// par exemple, si l'un est une image thumbnail. Le service de téléchargement ne s'occupe que de télécharger les fichiers requis.
    /// </summary>
    public class RespondentBundleDataGroup
    {
        /// <summary>
        /// Valeur originale, telle que reçue du serveur, concernant ce groupe de fichiers.
        /// </summary>
        public RequestRespondentDataDynamicValue OriginalValueHeader { get; set; }

        /// <summary>
        /// Une liste complémentaire des informations de valeurs de l'en-tête, relative au traitement
        /// de téléchargements et fichiers locaux des informations du serveur.
        /// </summary>
        public ConcurrentDictionary<string, RespondentBundleDataFile> Items { get; set; } = new ConcurrentDictionary<string, RespondentBundleDataFile>();

        /// <summary>
        /// Le répertoire où est stocké ce fichier et les autres en association avec l'événement.
        /// Si ça parait redondant, il faut savoir que l'on ne connait pas le nom de fichier à l'avance, mais le répertoire, oui.
        /// </summary>
        public DirectoryInfo LocalDirectory { get; set; }
    }
}