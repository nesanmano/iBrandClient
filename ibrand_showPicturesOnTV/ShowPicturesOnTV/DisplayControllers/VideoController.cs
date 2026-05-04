namespace NomadLogic.ShowPicturesOnTV.DisplayControllers
{
    /// <summary>
    /// Contrôleur pour afficher des vidéos, un à la fois, à l'écran. Utilise VLC comme contenant à vidéo.
    /// </summary>
    class VideoController : DisplayControllerAbstract
    {
        protected override string[] GetFileExtensions()
        {
            return MOVIES_EXTENSIONS;
        }
    }
}
