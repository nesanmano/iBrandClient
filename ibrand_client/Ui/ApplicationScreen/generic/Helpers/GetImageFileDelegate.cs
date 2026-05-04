using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Signature d'une méthode qui retourne une instance de Bitmap à partir d'un nom de fichier relatif.
    /// </summary>
    /// <param name="fileName">Le nom de fichier relatif.</param>
    /// <returns>Une référence vers un Bitmap initialisé à partir du fichier.</returns>
    public delegate Bitmap GetImageFileDelegate(string fileName);
}