using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Interface pour qu'une instance fournisse une liste de fichier existant sur disque.
    /// </summary>
    [PublicAPI]
    public interface IFileSelector
    {
        /// <summary>
        /// Indique que les fichiers sont tous prêt sur disque. Utile dans les situations où les fichiers peuvent arriver au compte-goutte, dans
        /// des circonstances asynchrone (comme des téléchargement massifs).
        /// </summary>
        bool IsAllFilesReady();

        /// <summary>
        /// Retourne la liste de fichiers présentement disponibles.
        /// </summary>
        List<FileInfo> ObtainFiles();
    }
}