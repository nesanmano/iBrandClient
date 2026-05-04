
namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    /// <summary>
    /// Choix de CameraRing avec API Canon pour de multiples images par caméra.
    /// </summary>
    public class ChoiceCameraRingCanonMultiple : ChoiceCameraRing
    {
        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public ChoiceCameraRingCanonMultiple()
        {
            _photo_field = new CameraRingCanonMultipleShotTextBox()
            {
                PictureNumberToBeValid = () => MinPictures,
                CameraNumberToBeValid = () => MinCameras
            };
        }
    }
}
