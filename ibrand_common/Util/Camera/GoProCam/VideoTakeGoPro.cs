using System;
using System.IO;
using System.Linq;
using System.Threading;
using GoPro.Hero;
using GoPro.Hero.Hero3;
using NLog;

namespace NomadLogic.Common.Util.Camera.GoProCam
{
    public class VideoTakeGoPro
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IVideoRequest _request;
        private bool _finished;
        private volatile bool _stop;

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="cam">La caméra à analyser.</param>
        /// <param name="request">La requête de photo.</param>
        public VideoTakeGoPro(Hero3Camera cam, IVideoRequest request)
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
        public VideoTakeGoPro StartCapture(CancellationToken? token)
        {
            if (_finished) return this;
            _finished = true;

            try
            {
                VideoFile = null;

                // Avant commande de prise photo.
                var browser = Camera.Contents();
                Mode currentMode;
                Camera.Mode(out currentMode);
                if (currentMode != Mode.Video)
                {
                    Camera.Mode(Mode.Video);
                }

                Camera.OpenShutter();

                var t = DateTime.Now;
                var tf = DateTime.Now.AddSeconds(_request.Duration);

                while (t < tf && !(token?.IsCancellationRequested ?? false) && !_stop)
                {
                    Thread.Sleep(100);
                    t = DateTime.Now;
                }

                Camera.CloseShutter();

                t = DateTime.Now;
                tf = DateTime.Now.AddSeconds(5);

                while (t < tf && !(token?.IsCancellationRequested ?? false) && !_stop)
                {
                    Thread.Sleep(100);
                    t = DateTime.Now;
                }

                var video = browser.VideosAsync().Result.LastOrDefault();

                if (!(token?.IsCancellationRequested ?? false))
                {
                    if (video == null)
                    {
                        DeferredException = new Exception("No new video file was created on camera.");
                    }
                    else
                    {
                        var response = video.Download().GetResponseStream();

                        var videoFileName = _request.SaveDirectory + GenerateUniqueId.GetId() + ".mp4";

                        if (!FileUtil.ReadStreamToFile(response, videoFileName, 60 * 1000, token))
                        {
                            DeferredException = new Exception("Video file transfert from camera failed.");
                        }
                        else
                        {
                            VideoFile = new FileInfo(videoFileName);

                            try
                            {
                                Camera.DeleteLastFileOnSdCard();
                            }
                            catch (Exception e)
                            {
                                log.Error(e, "Could not destroy last file on camera.");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                DeferredException = e;
                VideoFile = null;
            }

            return this;
        }

        public void StopCapture()
        {
            _stop = true;
        }

        /// <summary>
        /// Le fichier vidéo généré par la caméra.
        /// </summary>
        public FileInfo VideoFile { get; private set; }

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