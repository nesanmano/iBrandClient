using System.Drawing;
using NomadLogic.Multimedia.Gpu.OpenCV.Filters;

namespace NomadLogic.Multimedia.Gpu.OpenCV {

    /// <summary>
    /// Classe pour générer les classe de filtre OpenCv, avec le bon nombre de paramètres.
    /// </summary>
    public static class OpenCvFilterFactory {
        internal static readonly OpenCvFilterNone NoOp = new OpenCvFilterNone();
        internal static readonly OpenCvFilter[] DEFAULT_FILTERS = {
                                 MedianFilter(5),
                                 Erode(new Size(3,3), 1),
                                 BoxFilter(new Size(3,3), true)
                             };

        /// <summary>
        /// Retourne une instance de filtre pour un blur médian. Ça corrige l'alpha en faisant que si un pixel
        /// a trop de voisins opaques, il deviendra plus opaque. Ça donne une "limite" de alpha plus propre, même
        /// si elle sera tout de même très crénelée.
        /// </summary>
        /// <param name="radius">Le nombre de voisins à vérifier. Doit être un nombre impair. Sinon, ajoute un à la valeur</param>
        /// <returns>L'instance du filtre.</returns>
        public static OpenCvFilter MedianFilter(int radius) {
            return new OpenCvFilterMedianBlur(radius % 2 == 1 ? radius : radius + 1);
        }

        /// <summary>
        /// Un filtre de Blur basé sur un nombre de voisins fixe. Moins prononcée que le blur Gaussien.
        /// </summary>
        /// <param name="size">La taille de la fenêtre de vérification.</param>
        /// <param name="normalize">S'il faut normaliser ou non les valeurs.</param>
        /// <returns>L'instance du filtre.</returns>
        public static OpenCvFilter BoxFilter(Size size, bool normalize) {
            return new OpenCvFilterBoxFilter(size, normalize);
        }

        /// <summary>
        /// Ce filtre détecte les limites (edges) et coupe légèrement vers l'intérieur de la masse opaque (comme une érosion d'un flanc de rivière, d'où le nom).
        /// Normalement une itération de un est correct, mais on peut donner une valeur plus élevée, ce qui aura pour effet de calculer l'effet plusieurs fois, sur
        /// le résultat précédent.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static OpenCvFilter Erode(Size size, int iteration) {
            return new OpenCvFilterErode(size, iteration);
        }

        /// <summary>
        /// Retourne une instance de filtre qui ne fait expressément rien.
        /// </summary>
        /// <returns>Un filtre "No-Op"</returns>
        public static OpenCvFilter NoFilter() {
            return NoOp;
        }
    }
}