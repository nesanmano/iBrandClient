using Canon.Eos.Framework.Eventing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Iris
{
    public class TakePictureScreenProfil : TakePictureScreenFace
    {
        public TakePictureScreenProfil(string backgroundImgEnSrc, ScreenConfig sc, int count, string explanationPicture)
            : base(backgroundImgEnSrc, sc, count, explanationPicture)
        {
            // Rien de plus
        }
        
        protected override void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (_activated)
                SaveFileFromCameraToQuestion(e, 6);
        }
    }
}
