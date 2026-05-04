using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Définition d'impression avec option de redimensionnement au besoin.
    /// On tente de préserver l'orientation, et on ne sauvegarde pas le fichier d'impression modifié, si créé.
    /// </summary>
    [PublicAPI]
    public class ScreenDefinitionImpressionPutOnCanvas: ScreenDefinitionImpressionBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Nom de fichier relatif pour l'image de fond.
        /// </summary>
        public string CanvasImageFileName { get; set; }

        /// <summary>
        /// Cette variable indique non seulement la position mais aussi la taille de l'image à superposer.
        /// Si le ratio de cette "fenêtre" n'est pas le même que l'image originale, on fera une "maximisation" de cet esâce avec un élagage.
        /// </summary>
        public Rectangle ImagePosition { get; set; }

        /// <summary>
        /// Répertoire de base pour trouver les fichiers. Donné en temps réel habituellement.
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo BaseDirectory { get; set; }
        
        protected override bool PrepareExecution()
        {
            _fileToPrint = null;
            var imageFile = DataModelSource.GetActivationTextValue(_curResp);

            if (!File.Exists(imageFile))
            {
                log.Warn("File does not exists: " + imageFile);
                return false;
            }

            var canvasFile = new FileInfo(Path.Combine(BaseDirectory.FullName, CanvasImageFileName));
            if (!canvasFile.Exists)
            {
                log.Error("Image file for canvas does not exists: {0}", canvasFile);
                return false;
            }

            if (ImagePosition.Equals(Rectangle.Empty))
            {
                log.Error("No draw rectangle defined: {0}", ImagePosition);
                return false;
            }

            var tmpFile = DirectoryUtil.CreateLocalTemporaryFile();

            using (var canvas = FileUtil.LoadBitmap(canvasFile.FullName))
            {
                using (var image = FileUtil.LoadBitmap(imageFile))
                {
                    using (var g = Graphics.FromImage(canvas))
                    {
                        using (var bmp = image.Frame(ImagePosition.Width, ImagePosition.Height))
                        {
                            g.DrawImageUnscaled(bmp, ImagePosition);

                            if (canvas.Save(ImageFormat.Png, 9, tmpFile))
                            {
                                _fileToPrint = tmpFile;
                                return true;
                            }

                            log.Error("Image file for print does not exists: {0}", tmpFile);
                            return false;
                        }
                    }
                }
            }
        }
    }
}