using System.IO;

namespace iBrand_SnapShot.CustomWorker
{
    /// <summary>
    /// Classe de paramètres pour les travailleurs de fichiers.
    /// </summary>
    internal class SnapShotCustomWorkerParameter
    {
        /// <summary>
        /// Le fichier sur lequel appliquer le traitement.
        /// </summary>
        internal FileInfo WorkFile
        {
            get;
            set;
        }

        /// <summary>
        /// Paramètres globaux de l'exécutable.
        /// </summary>
        internal SnapShotSettings Settings
        {
            get;
            set;
        }
    }
}
