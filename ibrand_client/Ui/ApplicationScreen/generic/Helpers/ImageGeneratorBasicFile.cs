using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Générateur avec un chemins absolu, sans code de langue
    [PublicAPI]
    public class ImageGeneratorBasicFile : IImageGenerator
    {
        /// <inheritdoc />
        public Bitmap GenerateImage(string langCode)
        {
            var bmp = FileUtil.LoadBitmap(FilePath);
            if (!ImageExtensionDefinition.HasValue) return bmp;

            var tmp = ImageUtil.CreateProceduralExtendedBitmap(bmp, ImageExtensionDefinition.Value);
            bmp.Dispose();
            return tmp;
        }
        
        /// Chemin du fichier
        public string FilePath { get; set; }

        /// Règle de modification de l'image, si voulu.
        public ProceduralExtendedImageDefinition? ImageExtensionDefinition { get; set; }
    }
}