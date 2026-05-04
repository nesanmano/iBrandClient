using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NLog;
using Timer = System.Timers.Timer;

namespace NomadLogic.Common.Util.Camera.Canon
{
    internal class RequestExecutorVideoCanon : RequestExecutorCanon<IVideoRequest>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public RequestExecutorVideoCanon(CameraControllerCanon cameraControllerCanon, IVideoRequest request, CancellationTokenSource cancellationTokenSource) : 
            base(cameraControllerCanon, request, cancellationTokenSource) {}

        private Timer _countDownTimer;
        private int _countDown;
        
        protected override void DoExecute(CancellationToken token)
        {
            _cams = _ccc.GetCameras(1);
            
            foreach (var camera in _cams)
            {
                camera.Shutdown += CameraOnShutdown;
            }

            if (Request.UseLiveView)
            {
                _ccc.RegisterForLiveView(Request.LiveViewListener);
            }

            // On lance les événements principaux pour la prise de video.
            _countDownTimer = new Timer
            {
                Interval = 1000, //1 sec
                AutoReset = true
            }; 
            _countDownTimer.Elapsed += OnCountDownTimerEvent;
            StartCountDown();

            // Boucle d'attente principale
            while (token.IsCancellationRequested)
            {
                if (_isFinished)
                {
                    // Si la séquence d'événements de la prise de photo est terminée.
                    break;
                }

                try
                {
                    Thread.Sleep(100);
                }
                catch (Exception)
                {
                    // Die nice
                }
            }

            // Dé-enregistrement des caméras et des callbacks le plus vite possible.
            foreach (var camera in _cams)
            {
                camera.Shutdown -= CameraOnShutdown;
            }
            if (Request.UseLiveView)
            {
                _ccc.UnRegisterForLiveView(Request.LiveViewListener);
            }

            // Test des résultats
            if (token.IsCancellationRequested)
            {
                // Dans le cas d'une cancellation, on enregistre ce qui a pu être fait
                Request.Cancelled = true;
            }
            else
            {
                // Vérification des résultats
                if (Request.VideoTaken == null || Request.VideoTaken.Length < 100)
                {
                    Request.ErrorDetected = true;
                }
            }

            _ccc.VideoCaptureFinished(this);
        }

        private void CameraOnShutdown(object sender, EventArgs eventArgs)
        {
            // Si une caméra se désactive pendant la requête, cela l'annule immédiatement.
            _ccc.CancelCurrentVideoRequest(this);
        }

        private void StartCountDown()
        {
            _countDown = Request.CountDown;
            Request.CountDownEventListener?.CountDownTimerEvent(_countDown);
            _countDownTimer.Start();
        }
        
        private void OnCountDownTimerEvent(object sender, ElapsedEventArgs e)
        {
            if (_countDown > 0)
            {
                _countDown--;
                Request.CountDownEventListener?.CountDownTimerEvent(_countDown);
            }
            else
            {
                _countDownTimer.Stop();
                Request.CountDownEventListener?.CamerasActivateEvent();
                
                var takeTokenSource = new CancellationTokenSource();
                var action = new SingleVideoTakeCanon(_cams[0], Request);
                
                var mainTask = Task.Factory.StartNew(() => action.LauchVideoCapture(takeTokenSource.Token),
                    takeTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
                
                // ReSharper disable once CoVariantArrayConversion
                mainTask.Wait((int)(Request.Duration * 1000) + 5000);

                _isFinished = true;
                
                var r = mainTask.Result;
                if (r.DeferredException != null)
                {
                    log.Error(r.DeferredException, "");
                    _ccc.CancelCurrentVideoRequest(this);
                } 
                else if (r.PictureFile.Exists && r.PictureFile.Length > 100)
                {
                    // On a terminé, on notifie le contrôleur.
                    Request.VideoTaken = r.PictureFile;
                }
            }
        }
    }
}