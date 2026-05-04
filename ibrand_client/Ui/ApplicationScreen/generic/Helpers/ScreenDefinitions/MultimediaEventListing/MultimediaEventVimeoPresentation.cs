using System.Drawing;
using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    public class MultimediaEventVimeoPresentation : IMultimediaEventItemPresentationDetail
    {
        public void Dispose()
        {
            // Pas besoin
        }

        public bool Init()
        {
            // Pas besoin
            return true;
        }

        public string BackgroundImageFileName { get; set; }
        public string AssetsDirectory { get; set; }
        public Rectangle QuitButtonPosition { get; set; }
        public string QuitButtonImageFileName { get; set; }
        public Color QuitButtonBackColor { get; set; }
        
        [JsonRequired]
        public Rectangle WebControlZone { get; set; }

        public string LoadingImageFileName { get; set; }
        public int WebControlZoomFactor { get; set; } = 100;
        public int WebControlConnectionTimeOutMili { get; set; } = 5000;
        public string CustomVimeoViewerUrl { get; set; }
        public Rectangle SelectionStatusButtonZone { get; set; }
        public string SelectionActivatedImageFileName { get; set; }
        public string SelectionUnselectedImageFileName { get; set; }
    }
}