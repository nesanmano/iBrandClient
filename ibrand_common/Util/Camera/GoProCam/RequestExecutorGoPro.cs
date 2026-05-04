using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using GoPro.Hero.Hero3;
using NLog;
using Timer = System.Timers.Timer;

namespace NomadLogic.Common.Util.Camera.GoProCam
{
    /// <summary>
    /// Classe qui gère le timing et les prises de photos pour le contrôleur GoPro.
    /// </summary>
    class RequestExecutorGoPro : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private CameraControllerGoProSingle _controller;
        private Hero3Camera[] _cams;
        private readonly CancellationTokenSource _cts;
        private Task _runningTask;
        private readonly List<List<FileInfo>> _picturesTaken = new List<List<FileInfo>>();
        private bool _isFinished;
        private Timer _countDownTimer;
        private int _countDown;

        public RequestExecutorGoPro(CameraControllerGoProSingle controller, IPictureRequest request, CancellationTokenSource cts)
        {
            _controller = controller;
            Request = request;
            _cts = cts;
        }
        
        /// <summary>
        /// La requête de prise de photo.
        /// </summary>
        public IPictureRequest Request { get; private set; }

        public void StartRequest()
        {
            if (_controller == null) return;
            _runningTask = Task.Factory.StartNew(() => Execute(_cts.Token), _cts.Token);
        }

        private void Execute(CancellationToken token)
        {
            _cams = _controller.GetCameras(Request.MinCameraNbr);
            /*
            foreach (var camera in _cams)
            {
                camera.Shutdown += OnCameraShutDown; // Si une caméra se désactive pendant la requête, cela l'annule immédiatement.
            }
            */

            if (Request.UseLiveView)
            {
                _controller.RegisterForLiveView(Request.LiveViewListener);
            }

            // On lance les événements principaux pour la prise de photo.
            _countDownTimer = new Timer
            {
                Interval = 1000,
                AutoReset = true
            }; //1 sec
            _countDownTimer.Elapsed += OnCountDownTimerEvent;
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

            // Dé-enregistrement des caméras et des callbacks le plus vite possible.
            /*
            foreach (var camera in _cams)
            {
                camera.Shutdown -= OnCameraShutDown;
            }
            */
            if (Request.UseLiveView)
            {
                _controller.UnRegisterForLiveView(Request.LiveViewListener);
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
                if (CheckCurrentPictureTaken() < Request.PictureToTakeNbr)
                    Request.ErrorDetected = true;
            }

            _controller.TakePicturesFinished(this);
        }

        private int CheckCurrentPictureTaken()
        {
            var fullSetTaken = -1;
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var fileList in _picturesTaken)
            {
                if (fullSetTaken < fileList.Count && fileList.Count != 0)
                    fullSetTaken = fileList.Count;
            }
            return fullSetTaken;
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

                var camCount = _cams.Length;

                var tasks = new Task<SingleCameraTakeGoPro>[camCount];
                var takeTokenSource = new CancellationTokenSource();
                

                for (var i = 0; i < camCount; i++)
                {
                    var cam = _cams[i];
                    var action = new SingleCameraTakeGoPro(cam, Request);
                    tasks[i] = Task.Factory.StartNew(() => action.LauchPictureSequence(takeTokenSource.Token),
                        takeTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
                }

                // ReSharper disable once CoVariantArrayConversion
                Task.WaitAll(tasks);

                var takeWithException = new List<SingleCameraTakeGoPro>();
                var takeWithoutFilesAndException = new List<SingleCameraTakeGoPro>();
                foreach (var res in tasks.Select(t => t.Result))
                {
                    if (res.DeferredException != null) takeWithException.Add(res);
                    else if (res.PictureFile == null) takeWithoutFilesAndException.Add(res);
                }

                // Aucune exception ou fichier manquant... On ne logue rien.
                if (takeWithoutFilesAndException.Any() || takeWithException.Any())
                {

                    var builder = new StringBuilder();
                    builder.Append(Resources.common.ErrorCameraTakePictureCanon);
                    foreach (var take in takeWithException)
                    {
                        builder.Append('\n');
                        builder.Append(Resources.common.Camera);
                        builder.Append(' ');
                        builder.Append(take.Camera.BacpacInformation().Id);
                        builder.Append(": ");
                        builder.Append(take.DeferredException.Message);
                    }
                    foreach (var take in takeWithoutFilesAndException)
                    {
                        builder.Append('\n');
                        builder.Append(Resources.common.Camera);
                        builder.Append(' ');
                        builder.Append(take.Camera.BacpacInformation().Id);
                        builder.Append(": ");
                        builder.Append(Resources.common.ErrorCameraTakePictureNoFile);
                    }

                    var messageToSend = builder.ToString();
                    log.Error(messageToSend);

                    // On notifie l'usager qu'il y a eu un problème.
                    var sentToUser = _controller.ShowErrorMessageBox(messageToSend, diagResult =>
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
                            _controller.CancelCurrentPictureRequest(this);
                        }
                    });

                    if (sentToUser) return;
                    // On ne peut pas avoir du feedback de l'usager, et donc il faut prendre une décision: on annule.
                    log.Warn("No form availiable. Cancelling picture request.");
                    _controller.CancelCurrentPictureRequest(this);
                }
                else
                {
                    // On continue. On place les images dans la requête.
                    _picturesTaken.Clear();
                    var resultFiles = tasks.Select(t => t.Result.PictureFile).ToArray();
                    for (var i = 0; i < camCount; i++)
                    {
                        var subList = new List<FileInfo> {resultFiles[i]};
                        _picturesTaken.Insert(i, subList);
                    }

                    // Si on a pas fini, on recommence
                    if (CheckCurrentPictureTaken() < Request.PictureToTakeNbr)
                    {
                        StartCountDown();
                    }
                    else
                    {
                        // On a terminé, on notifie le contrôleur.
                        Request.PicturesTaken = _picturesTaken;
                        _isFinished = true;
                    }
                }
            }
        }

        private void StartCountDown()
        {
            _countDown = Request.CameraCountDown;
            Request.CountDownEventListener?.CountDownTimerEvent(_countDown);
            _countDownTimer.Start();
        }

        /// <summary>
        /// Annule la requête asynchrone d'image.
        /// Cette méthode est synchrone et s'assure que la tâche est arrêtée.
        /// </summary>
        public void CancelRequest()
        {
            if (_controller == null || _cts == null || _runningTask == null) return;

            _cts.Cancel();

            _runningTask.Wait(1000);
            _runningTask = null;
            log.Trace("Picture request cancelled.");
        }

        /*
        private void OnCameraShutDown(object sender, EventArgs e)
        {
            // Si une caméra se débranche, on annule toute l'opération.
            _controller.CancelCurrentPictureRequest(this);
        }
        */

        public void Dispose()
        {
            if (_runningTask != null)
            {
                CancelRequest();
            }

            _controller = null;
            _cams = null;
            Request = null;
        }
    }
}