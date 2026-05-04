using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    public class CaptureEventListingScreen : GenericScreen
    {
        public ScreenDefinitionMultimediaEventListing CaptureEventList { get; set; }

        public CaptureEventListingScreen(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig screenConf) : 
            base(backgroundImgEnSrc, backgroundImageFrSrc, screenConf) { }


    }
}