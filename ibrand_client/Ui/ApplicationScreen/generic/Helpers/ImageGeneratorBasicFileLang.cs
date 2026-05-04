using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Générateur avec un chemins absolu.
    [PublicAPI]
    public class ImageGeneratorBasicFileLang : IImageGenerator
    {
        /// <inheritdoc />
        public Bitmap GenerateImage(string langCode) {

            string path = null;
            FilePath?.TryGetValue(langCode, out path);
            if (path == null) return null;

            var bmp = FileUtil.LoadBitmap(path);
            if (!ImageExtensionDefinition.HasValue) return bmp;

            var tmp = ImageUtil.CreateProceduralExtendedBitmap(bmp, ImageExtensionDefinition.Value);
            bmp.Dispose();
            return tmp;
        }
        
        /// Chemins des fichiers, selon le code de langue.
        public Dictionary<string, string> FilePath { get; set; }

        /// Règle de modification de l'image, si voulu.
        public ProceduralExtendedImageDefinition? ImageExtensionDefinition { get; set; }
    }
}