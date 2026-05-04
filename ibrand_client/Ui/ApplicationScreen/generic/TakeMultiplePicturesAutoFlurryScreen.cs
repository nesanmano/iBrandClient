using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Util;

// ReSharper disable InconsistentlySynchronizedField
#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Prend des images en rafales, beaucoup plus rapide qu'avec un décompte mais le sujet ne peut s'adapter à chacune.
    /// </summary>
    public class TakeMultiplePicturesAutoFlurryScreen : TakeMultiplePicturesAutoScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected Rectangle? _takePictureRectangle;
        protected bool _blockInputs;

        private readonly object _lock = new object();
        private Task _fullAutoPictures;
        private CancellationTokenSource _cts;

        /// Milisecondes d'attentes par image à saisir.
        public int TimePerPictureFlurry { get; set; } = 325;
        /// Temps de base pour que la caméra fasses son setting original.
        public int BaseTimeForPictureFlurry { get; set; } = 1000;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="sc">Objet de configuration d'�cran.</param>
        /// <param name="backgroundImgEnSrc">Image de fond anglais.</param>
        /// <param name="backgroundImgFrSrc">Image de fond fran�ais.</param>
        /// <param name="previewZone">Zone pour la pr�visualisation.</param>
        /// <param name="countDownMax">D�compte en seconde entre chaque image � prendre. 5 par d�faut.</param>
        /// <param name="firstCountDown">Valeur du premier d�compte. Qui sera plus long (ou plus court) que les autres. Si null, on utilise la valeur des autres d�comptes.</param>
        public TakeMultiplePicturesAutoFlurryScreen(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc,
            Rectangle? previewZone, int countDownMax = 5, int? firstCountDown = null)
            : base(sc, backgroundImgEnSrc, backgroundImgFrSrc, previewZone, countDownMax, firstCountDown) { }

        /// <inheritdoc/>
        protected override void OnCountdownElapsed()
        {
            _takePictureTimer.Stop();
            if (_fullAutoPictures != null) return;
            log.Trace("Initiating Auto Picture Sequence.");
            _cts = new CancellationTokenSource();
            _fullAutoPictures = Task.Run(() => FullAutoPictureDo(_cts.Token), _cts.Token);
        }

        private void FullAutoPictureDo(CancellationToken ct)
        {
            ShowHidePreview(false);

            if (ct.IsCancellationRequested)
            {
                log.Debug("Sequence ended before it began.");
                _fullAutoPictures = null;
                _cts = null;
                ShowHidePreview(true);
                return;
            }

            if (!CanonCameraManager.GetInstance().PressShutterButton())
            {
                log.Debug("Could not activate shutter press command on camera.");
                StartCountdown();
                _fullAutoPictures = null;
                _cts = null;
                ShowHidePreview(true);
                return;
            }
            log.Debug("Press shutter command sent.");

            var timer = new Stopwatch();

            timer.Start();
            // On lance le shutter pendant une période approximative, car les images arrivent bien moins vite que la caméra est capable
            // d'en prendre.

            var shutterAttente = BaseTimeForPictureFlurry + (TimePerPictureFlurry * NumberOfPictureToTake);

            while (timer.ElapsedMilliseconds < shutterAttente && !ct.IsCancellationRequested)
            {
                Thread.Sleep(100);
                Thread.Yield();
            }
            timer.Stop();
            CanonCameraManager.GetInstance().ReleaseShutterButton();
            log.Trace("Released shutter press.");

            _creatingSequence?.StartSequence();

            int currentNumber;
            timer.Restart();
            timer.Start();
            while (timer.ElapsedMilliseconds < MaxWaitTimeForTransfert && !ct.IsCancellationRequested)
            {
                Thread.Sleep(100);
                Thread.Yield();
                lock (_lock)
                {
                    currentNumber = _picturesTaken.Count;
                    if (log.IsTraceEnabled && currentNumber != _picturesTaken.Count)
                    {
                        log.Trace("New picture detected in loop ({1}/{2}). Time elasped: {0} seconds",
                            (object)timer.Elapsed.TotalSeconds, (object)currentNumber, (object)NumberOfPictureToTake);
                    }
                }
            }

            if (log.IsTraceEnabled)
            {
                lock (_lock)
                {
                    log.Trace("Loop stopped. Final tally: {0}/{1}", _picturesTaken.Count, NumberOfPictureToTake);
                }
            }

            // quoiqu'il arrive, on marque l'instance de capture comme terminée.
            timer.Stop();
            _fullAutoPictures = null;
            _cts = null;

            lock (_lock)
            {
                currentNumber = _picturesTaken.Count;
            }

            if (ct.IsCancellationRequested)
            {
                if (currentNumber < NumberOfPictureToTake)
                {
                    //ShowHidePreview(true);
                    _creatingSequence?.StopSequence();
                    return;
                }

                // Tout est beau, on cancelle.
                LaunchTransformationSequence(_picturesTaken);
                _blockInputs = false;
            }
            else
            {
                log.Debug("Timeout detected. Restarting picture taking.");
                StartCountdown();
                ShowHidePreview(true);
                _creatingSequence?.StopSequence();
                _blockInputs = false;
            }
        }

        /// <summary>
        /// Nombre maximal de milisecondes d'attente pour le transfert d'images.
        /// </summary>
        public long MaxWaitTimeForTransfert { get; set; } = 15000;

        private void ShowHidePreview(bool visibility)
        {
            if (_panelPreview != null)
            {
                FormUtil.SafeCall(this, () =>
                {
                    _panelPreview.Visible = visibility;
                });
            }
        }

        public Rectangle TakePictureRectangle
        {
            set => _takePictureRectangle = value;
        }
        
        /// <inheritdoc/>
        protected override void OnFileSavedOnHostDo(string filePath)
        {
            try
            {
                if (!_activated) return;

                lock (_lock)
                {
                    if (_fullAutoPictures == null)
                    {
                        log.Trace("Full Auto picture asynchronous tak has finished already. Discarding new picture.");
                        return;
                    }

                    var nbrPicTaken = _picturesTaken.Count;

                    if (nbrPicTaken >= NumberOfPictureToTake)
                    {
                        log.Trace("Extra picture taken. Discarded.");
                        return;
                    }

                    _picturesTaken.Add(filePath);
                    nbrPicTaken++;

                    log.Trace("Picture saved. {0}/{1}", nbrPicTaken, NumberOfPictureToTake);

                    if (nbrPicTaken < NumberOfPictureToTake) return;

                    log.Trace("Stop signal Set to task.");
                    _cts?.Cancel(); // Façon normale de stopper la tâche.
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (_blockInputs) return false;
            if (!base.OnMouseDownHandler(x, y)) return false;
            
            if (!InArea(x, y, _takePictureRectangle)) return true;

            _blockInputs = true;
            if (_fullAutoPictures != null) return false;
            log.Trace("Initiating Auto Picture Sequence.");
            _cts = new CancellationTokenSource();
            _fullAutoPictures = Task.Run(() => FullAutoPictureDo(_cts.Token), _cts.Token);
            return false;
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            // Stop la prise d'image, si en cours.
            _cts?.Cancel();
        }

        ~TakeMultiplePicturesAutoFlurryScreen()
        {
            _cts?.Cancel();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            _cts?.Cancel();
        }

        public override void SetActivated()
        {
            base.SetActivated();

            ShowHidePreview(true);
        }
    }
}
