using Emgu.CV;
using JetBrains.Annotations;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Enveloppe de base pour les fonction d'un filtre de Matrice OpenCv.
    /// </summary>
    [PublicAPI]
    public abstract class OpenCvFilter
    {
        /// <summary>
        /// Indique si on peut exécuter ce filtre sur une matrice avec un seul canal.
        /// </summary>
        public bool CanExecuteOnSingleChannel => MinChannels == 1;

        /// Indique le nombre de canaux minimum que doivent contenir les matrices pour que l'exécution se fasse. Certains filtres OpenCv nécéssitent
        /// plus qu'une matrice sur lesquelles s'exécuter.
        public abstract uint MinChannels { get; }

        /// <summary>
        /// Exécute le filter OpenCv sur la matrice entrante, et met le résultat dans la matrice sortante.
        /// </summary>
        /// <param name="inMat">Matrice entrante.</param>
        /// <param name="outMat">Matrice qui reçoit le résultat.</param>
        /// <returns>TRUE s'il y a eu une exécution. FALSE si rien ne s'est passé et que la matrice de résultat n'a pas été modifiée.</returns>
        public abstract bool Execute(Mat inMat, Mat outMat);
    }
}