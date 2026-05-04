using System;

namespace NomadLogic.Common.Ui.NomadScreen
{
    /// <summary>
    /// Si un objet contient des images à initialiser dans un cycle de vie d'écran (soit à la première initialisation, soit à change changement de langue),
    /// elle doit implémenter cette interface.
    /// </summary>
    public interface IImageInitialisable : IDisposable
    {
        /// <summary>
        /// Initialise une image via un répertoire de base et un code de langue.
        /// On vérifie d'abord si le chemin d'une image peut être trouvé de façon absolue, sinon on réessaye avec
        /// une tentative de chemin relatif basée sur le répertoire donnée en paramètre.
        /// </summary>
        /// <param name="baseDir">La répertoire de base, si le chemin absolu n'existe pas.</param>
        /// <param name="languageCode">Le code de langue.</param>
        /// <returns>TRUE si un fichier a été trouvé, et s'il s'est correctement chargé en mémoire.</returns>
        bool InitImage(string baseDir, string languageCode);
    }
}