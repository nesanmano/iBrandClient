namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    /// <summary>
    /// Choix de CameraRing avec API de Canon.
    /// </summary>
    public class ChoiceCameraRingCanon : ChoiceCameraRing
    {
        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public ChoiceCameraRingCanon()
        {
            _photo_field = new CameraRingCanonTextBox
            {
                PictureNumberToBeValid = () => MinPictures,
                CameraNumberToBeValid = () => MinCameras
            };
        }
    }
}
