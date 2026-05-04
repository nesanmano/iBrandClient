using System.Collections.Generic;
using System.Drawing;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Classe d'implémentation de base (abstraite), pour tenter de ne pas trop dupliquer de code.
    /// </summary>
    public abstract class  MultimediaEventItemPresentationDetailBase: IMultimediaEventItemPresentationDetail, IThumbnailOverlaySelector
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public string BackgroundImageFileName { get; set; }
        public string AssetsDirectory { get; set; }
        public Rectangle QuitButtonPosition { get; set; }
        public string QuitButtonImageFileName { get; set; }
        public Color QuitButtonBackColor { get; set; } =  Color.White;
        public string ThumbnailDefaultFileName { get; set; }
        public IDictionary<ThumbnailOverlaySelectorState, ThumbnailOverlaySelectorImageInfo> ThumbnailOverlayImagesDefinitions { get; set; }
        
        public Image GetThumbnailOverlayImage(ThumbnailOverlaySelectorState key)
        {
            if (ThumbnailOverlayImagesDefinitions == null || !ThumbnailOverlayImagesDefinitions.TryGetValue(key, out var definition))
            {
                return null;
            }

            return definition.GetImageInstance();
        }
        
        public virtual bool Init()
        {
            foreach (var tid in ThumbnailOverlayImagesDefinitions.Values)
            {
                if (tid.InitImage(AssetsDirectory)) continue;
                log.Error("Could not initialize shared overlay image: {0}", tid.ImageFileName);
                return false;
            }

            // Vérification que l'on a assez d'éléments pour fonctionner.
            if (ThumbnailOverlayImagesDefinitions.Count < 3)
            {
                log.Error("Do not have enough thumbnail image definitions to go on. Enum defs list: {0}", StringUtil.FormatEnumerable(ThumbnailOverlayImagesDefinitions.Keys, false));
                return false;
            }

            return true;
        }

        public virtual void Dispose()
        {
            foreach (var tid in ThumbnailOverlayImagesDefinitions)
            {
                tid.Value?.Dispose();
            }
        }
    }
}