using NLog;

namespace NomadLogic.ShowPicturesOnTV.DisplayControllers
{
    /// <summary>
    /// Controlleur pour afficher des images simples.
    /// </summary>
    class ImageController : DisplayControllerAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override string[] GetFileExtensions()
        {
            return IMAGES_EXTENSIONS;
        }

    }
}
