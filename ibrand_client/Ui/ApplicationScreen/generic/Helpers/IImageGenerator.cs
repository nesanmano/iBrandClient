using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers {
    /// Interface d'un générateur d'une image unique.
    public interface IImageGenerator {
        /// <summary>
        /// Génère l'image selon le code de langue actuel.
        /// </summary>
        /// <param name="langCode">Le code de langue à utiliser.</param>
        /// <returns>Une référence vers le bitmap créé.</returns>
        /// TODO Peut-être fusionner avec IImageInitialisable?
        Bitmap GenerateImage(string langCode);
    }
}