using System;
using System.IO;
using System.Linq;
using System.Threading;
using GoPro.Hero;
using GoPro.Hero.Hero3;
using NLog;

namespace NomadLogic.Common.Util.Camera.GoProCam
{
    /// <summary>
    /// Cetta classe interne englobe les étapes de sauvegarde par événements qui permettent d'aller chercher un
    /// fichier d'image sur une caméra GoPro.
    /// </summary>
    public class SingleCameraTakeGoPro
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IPictureRequest _request;
        private bool _finished;

        /// <summary>
        /// Constrcuteur par défaut.
        /// </summary>
        /// <param name="cam">La caméra à analyser.</param>
        /// <param name="request">La requête de photo.</param>
        public SingleCameraTakeGoPro(Hero3Camera cam, IPictureRequest request)
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
        public SingleCameraTakeGoPro LauchPictureSequence(CancellationToken token)
        {
            if (_finished) return this;
            _finished = true;

            try
            {
                PictureFile = null;

                // Avant commande de prise photo.
                var browser = Camera.Contents();

                Mode currentMode;
                Camera.Mode(out currentMode);
                if (currentMode != Mode.Photo)
                {
                    Camera.Mode(Mode.Photo);
                }

                Camera.OpenShutter();

                var t = DateTime.Now;
                var tf = DateTime.Now.AddSeconds(5);
            
                while (t < tf && !token.IsCancellationRequested)
                {
                    //test = Camera.BacpacStatus().CameraReady;
                    
                        Thread.Sleep(100);
                        t = DateTime.Now;
                }
                
                var image = browser.ImagesAsync().Result.LastOrDefault();

                if (image == null)
                {
                    DeferredException = new Exception("No new image file was created on camera.");
                }
                else
                {
                    var response = image.Download().GetResponseStream();

                    var pictureFileName = _request.SaveDirectory + GenerateUniqueId.GetId() + ".jpg";
                    
                    if (!FileUtil.ReadStreamToFile(response, pictureFileName, 10 * 1000, token))
                    {
                        DeferredException = new Exception("File transfert from camera failed.");
                    }
                    else
                    {
                        PictureFile = new FileInfo(pictureFileName);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                DeferredException = e;
                PictureFile = null;
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
        public Hero3Camera Camera { get; }

        /// <summary>
        /// Si une exception s'est produite lors de la prise de photo, on la stocke ici.
        /// Null par défaut.
        /// </summary>
        public Exception DeferredException { get; private set; }
    }
}
