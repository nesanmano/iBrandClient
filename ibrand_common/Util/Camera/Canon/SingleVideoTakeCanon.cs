using System;
using System.IO;
using System.Threading;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util.Camera.Canon
{
    /// <summary>
    /// Cetta classe interne englobe les étapes de sauvegarde par événements qui permettent d'aller chercher un
    /// fichier d'image sur une caméra Canon.
    /// </summary>
    internal class SingleVideoTakeCanon
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IVideoRequest _request;
        private CancellationToken _token;
        private volatile bool _finish;

        /// <summary>
        /// Constrcuteur par défaut.
        /// </summary>
        /// <param name="cam">La caméra à analyser.</param>
        /// <param name="request">La requête de photo.</param>
        public SingleVideoTakeCanon([NotNull]EosCamera cam, IVideoRequest request)
        {
            if (cam == null || request == null) throw new NullReferenceException();

            Camera = cam;
            _request = request;
        }

        /// <summary>
        /// Lance la séquence d'événements pour prendre une image avec la caméra.
        /// Ne fait aucune vérification de temps si la caméra ne lance pas ses événements. C'est l'appellant
        /// qui doit annuler l'opération (via le jeton d'annulation) si trop de temps s'est écoulé.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public SingleVideoTakeCanon LauchVideoCapture(CancellationToken token)
        {
            if (!Camera.IsSessionOpen)
            {
                log.Warn("Camera session is not opened for camera {0}", Camera.Author);
                DeferredException = new Exception("Camera session is not opened.");
                return this;
            }

            try
            {
                _token = token;
                Camera.FileCreatedOnCamera += OnFileCreatedOnCamera;
                Camera.FileSavedOnHost += OnFileSavedOnHost;

                Camera.TakePicture();

                while (!token.IsCancellationRequested && !_finish)
                {
                    try
                    {
                        Thread.Sleep(100);
                    }
                    catch (Exception)
                    {
                        // die nice
                    }
                }
            }
            catch (Exception ex)
            {
                DeferredException = ex;
                PictureFile = null;
            }
            finally
            {
                Camera.FileCreatedOnCamera -= OnFileCreatedOnCamera;
                Camera.FileSavedOnHost -= OnFileSavedOnHost;
            }

            return this;
        }

        /// <summary>
        /// Le fichier d'image généré par la caméra.
        /// </summary>
        public FileInfo PictureFile { get; private set; }

        /// <summary>
        /// La caméra sur laquelle on tente de prendre la photo.
        /// </summary>
        public EosCamera Camera { get; }

        /// <summary>
        /// Si une exception s'est produite lors de la prise de photo, on la stocke ici.
        /// Null par défaut.
        /// </summary>
        public Exception DeferredException { get; private set; }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (_token.IsCancellationRequested) return;
            log.Trace("File saved on host: cam={0}, path={1}", Camera.Author, e.FilePath);
            PictureFile = new FileInfo(e.FilePath);
            if (!PictureFile.Exists)
            {
                PictureFile = null;
            }
            _finish = true;
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr diritem, EosCamera camera)
        {
            if (_token.IsCancellationRequested) return;
            log.Trace("File saved on camera. Author={0}", camera.Author);
            camera.SavePicture(diritem, _request.SaveDirectory + GenerateUniqueId.GetId() + ".jpg");
        }
    }
}
