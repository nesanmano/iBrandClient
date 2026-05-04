using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using NLog;
using Timer = System.Timers.Timer;

namespace NomadLogic.Common.Util.Camera.Canon
{
    /// <summary>
    /// Classe qui englobe le concept de timer entre les séquences de prise de photo, et qui délègue les prises de photos multiples sur des tâches
    /// asynchrones. Marche en tandem avec le contrôleur de caméra Canon, car hautement basé sur l'API de Canon.
    /// </summary>
    internal class RequestExecutorPicturesCanon : RequestExecutorCanon<IPictureRequest>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Timer _countDownTimer;
        private int _countDown;
        
        public RequestExecutorPicturesCanon(CameraControllerCanon cameraControllerCanon, IPictureRequest request, 
            CancellationTokenSource cancellationTokenSource) : base(cameraControllerCanon, request, cancellationTokenSource) { }

        private void CameraShutDown(object sender, EventArgs eventArgs)
        {
            // Si une caméra se débranche, on annule toute l'opération.
            log.Trace("CameraShutdown called");
            _ccc.CancelCurrentPictureRequest(this);
        }
        
        protected override void DoExecute(CancellationToken token)
        {
            log.Trace("Begin Execute");
            _cams = _ccc.GetCameras(Request.MinCameraNbr);
            
            foreach (var camera in _cams)
            {
                camera.Shutdown += CameraShutDown; // Si une caméra se désactive pendant la requête, cela l'annule immédiatement.
            }

            if (Request.UseLiveView)
            {
                _ccc.RegisterForLiveView(Request.LiveViewListener);
            }

            // On lance les événements principaux pour la prise de photo.
            _countDownTimer = new Timer
            {
                Interval = 1000,
                AutoReset = true
            }; //1 sec
            _countDownTimer.Elapsed += OnCountDownTimerEvent;
            log.Trace("Countdown Started.");
            StartCountDown();

            // Boucle d'attente principale
            while (!token.IsCancellationRequested)
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

            log.Trace("Wait loop over. Desactivating.");
            // Dé-enregistrement des caméras et des callbacks le plus vite possible.
            foreach (var camera in _cams)
            {
                camera.Shutdown -= CameraShutDown;
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
                Request.ErrorDetected =
                    Request.PicturesTaken == null ||
                    Request.PicturesTaken.Count != Request.MinCameraNbr ||
                    Request.PicturesTaken.Max(l => l.Count) != Request.PictureToTakeNbr;
            }

            log.Trace("End Execute");
            _ccc.TakePicturesFinished(this);
        }
        
        private void OnCountDownTimerEvent(object sender, ElapsedEventArgs e)
        {
            if (_countDown > 0)
            {
                log.Trace("Countdown: {0}", _countDown);
                _countDown--;
                log.Trace("Countdown tick :{0}", _countDown);
                Request.CountDownEventListener?.CountDownTimerEvent(_countDown);
            }
            else
            {
                log.Trace("Countdown elapsed");
                _countDownTimer.Stop();
                Request.CountDownEventListener?.CamerasActivateEvent();

                var nbrCam = System.Math.Min(Request.MinCameraNbr, _cams.Length);

                Request.PicturesTaken = new List<List<FileInfo>>(nbrCam);

                var tasks = new Task<SinglePictureTakeCanon>[nbrCam];
                var takeTokenSource = new CancellationTokenSource();
                for (var i = 0; i < nbrCam; i++)
                {
                    Request.PicturesTaken.Add(new List<FileInfo>(Request.PictureToTakeNbr));
                    var cam = _cams[i];
                    SinglePictureTakeCanon TakePic()
                    {
                        var exe = new SinglePictureTakeCanon(cam, Request);
                        return exe.LauchPictureSequence(takeTokenSource.Token);
                    }

                    tasks[i] = new Task<SinglePictureTakeCanon>(TakePic);
                    tasks[i].Start();
                }
                log.Trace("Take picture Tasks sent: {0}", _cams.Length);

                // ReSharper disable once CoVariantArrayConversion
                Task.WhenAll(tasks);
                log.Trace("All cameras finished.");

                var takeWithException = new List<SinglePictureTakeCanon>();
                var takeWithoutFilesAndException = new List<SinglePictureTakeCanon>();
                for (var i = 0; i < tasks.Length; i++)
                {
                    var res = tasks[i].Result;
                    if (res.DeferredException != null) takeWithException.Add(res);
                    else if (res.PictureFile == null) takeWithoutFilesAndException.Add(res);
                    else Request.PicturesTaken[i].Add(res.PictureFile);
                }
                log.Trace("Task results analysed.");

                // Aucune exception ou fichier manquant... On ne logue rien.
                if (takeWithoutFilesAndException.Any() || takeWithException.Any())
                {
                    log.Trace("Error detected.");
                    var builder = new StringBuilder();
                    builder.Append("Error during Canon take picture sequence.");
                    foreach (var take in takeWithException)
                    {
                        builder.Append('\n');
                        builder.Append("Camera");
                        builder.Append(' ');
                        builder.Append(take.Camera.Author);
                        builder.Append(": ");
                        builder.Append(take.DeferredException.Message);
                    }
                    foreach (var take in takeWithoutFilesAndException)
                    {
                        builder.Append('\n');
                        builder.Append("Camera");
                        builder.Append(' ');
                        builder.Append(take.Camera.Author);
                        builder.Append(": ");
                        builder.Append("No picture taken");
                    }

                    var messageToSend = builder.ToString();
                    log.Error(messageToSend);

                    // On notifie l'usager qu'il y a eu un problème.
                    var sentToUser = _ccc.ShowErrorMessageBox(messageToSend, diagResult =>
                    {
                        if (diagResult == DialogResult.Retry)
                        {
                            // On détruit les images prises et rejetées.
                            foreach (var picFile in tasks.Select(t => t.Result.PictureFile))
                            {
                                try
                                {
                                    if (picFile != null && picFile.Exists) picFile.Delete();
                                }
                                catch (Exception)
                                {
                                    // die nice
                                }
                            }

                            StartCountDown(); // On recommence, sans prendre en considération les images prises jusqu'à maintenant.
                        }
                        else
                        {
                            // On annule tout.
                            _ccc.CancelCurrentPictureRequest(this);
                        }
                    });

                    if (sentToUser) return;
                    // On ne peut pas avoir du feedback de l'usager, et donc il faut prendre une décision: on annule.
                    log.Warn("No form availiable. Cancelling picture request.");
                    _ccc.CancelCurrentPictureRequest(this);
                }
                else
                {
                    // Si on a pas fini, on recommence
                    var remain = Request.PictureToTakeNbr - Request.PicturesTaken.Max(l => l.Count);
                    if (remain > 0)
                    {
                        log.Trace("Picture remaining to take :{0}", remain);
                        StartCountDown();
                    }
                    else
                    {
                        // On a terminé, on notifie le contrôleur.
                        _isFinished = true;
                    }
                }
            }
        }

        private void StartCountDown()
        {
            log.Trace("Start countdown sequence.");
            _countDown = Request.CameraCountDown;
            Request.CountDownEventListener?.CountDownTimerEvent(_countDown);
            if (Request.CameraCountDown > 0)
            {
                _countDownTimer.Start();
            }
            else
            {
                // Immédiatement appeler la méthode.
                OnCountDownTimerEvent(null, null);
            }
        }
    }
}