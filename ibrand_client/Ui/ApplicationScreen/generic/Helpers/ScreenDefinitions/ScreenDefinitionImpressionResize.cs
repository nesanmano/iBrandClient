using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Définition d'impression avec option de redimensionnement au besoin.
    /// On tente de préserver l'orientation, et on ne sauvegarde pas le fichier d'impression modifié, si créé.
    /// </summary>
    [PublicAPI]
    public class ScreenDefinitionImpressionResize: ScreenDefinitionImpressionBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// La taille finale voulue du fichier à imprimer.
        /// Si le ratio est exactement pareil, on utilisera le fichier original.
        /// Sinon, il y aura élaguage pour préserver le format des pixels (I.E. Il n'y aura pas de "stretching").
        /// Dans la situation où l'orientation est différente, on va garder l'orientation originale en inversant les dimensions.
        /// </summary>
        public Size DesiredSize { get; set; }
        
        protected override bool PrepareExecution()
        {
            _fileToPrint = null;
            var originalPrintFile = new FileInfo(DataModelSource.GetActivationTextValue(_curResp));

            if (!originalPrintFile.Exists)
            {
                log.Warn("File does not exists: " + originalPrintFile);
                return false;
            }

            _fileToPrint = ImageMagickCliUtil.ResizeAndCopyImageFile(originalPrintFile, DesiredSize)?.FullName;

            return _fileToPrint != null;
        }
    }
}