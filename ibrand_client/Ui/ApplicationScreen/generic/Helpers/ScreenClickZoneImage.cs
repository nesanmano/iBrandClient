using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Encapsule une zone de click avec image fixe, désigné via fichier.
    /// TODO: Ajouter des fonctionalités pour avoir plsuieures images potentielle par code de langue.
    /// </summary>
    public class ScreenClickZoneImage : ScreenClickZone
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Le chemin complet du fichier d'image à charger.
        /// </summary>
        public string ImageFile { get; set; }

        /// <summary>
        /// L'instance actuelle de l'image à afficher.
        /// </summary>
        [JsonIgnore]
        public Bitmap ImageInstance { get; set; }

        /// <summary>
        /// Force l'image à être re-dimensionnée pour être affichée selon la taille de l'élément de pagination (Zone).
        /// </summary>
        public bool ForceResizeOfImage { get; set; }

        public override bool OnLoad(AbstractScreen screen)
        {
            if (ImageInstance != null) return true;

            var filePath = new FileInfo(ImageFile);
            if (!filePath.Exists)
            {
                log.Warn("File does not exist for click zone initialization.");
                return false;
            }

            var bmp = ForceResizeOfImage ? 
                FileUtil.LoadBitmap(filePath.FullName, 300, Zone.Size) : 
                FileUtil.LoadBitmap(filePath.FullName);

            if (bmp == null)
            {
                log.Warn("Could not load image file for click zone initialization.");
                return false;
            }

            ImageInstance = bmp;
            return true;
        }

        public override void Dispose()
        {
            ImageInstance?.Dispose();
        }

        protected override void DoPaint(Graphics g)
        {
            if (IsVisible) ScreenUtil.DrawImage(g, ImageInstance, Zone);
        }
    }
}