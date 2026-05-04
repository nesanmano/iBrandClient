using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoPro.Hero;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Camera.GoProCam;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;
using Timer = System.Timers.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cette classe prend un vidéo via une caméra Sony, qui peut fonctionner par Wifi avec un mini-serveur HTTP
    /// à l'interne.
    /// </summary>
    [PublicAPI]
    public class RecordVideoGoProScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        protected readonly Rectangle _startArea;
        protected readonly Rectangle _stopArea;
        protected readonly Rectangle? _closeArea;

        private readonly CameraControllerGoProSingle _ccgp;
        private CancellationTokenSource _cts;
        
        private bool _recording;
        private bool _downloading;
        private bool _fileDownloaded;
        private string _videoFileName;

        private Bitmap _recordInactiveImage;
        private Bitmap _stopInactiveImage;
        private Bitmap _nextImage;
        
        private volatile bool _stop;
        private readonly Timer _timerKeepAlive;
        private readonly object _lock = new object();
        private bool _cameraWorking;
        private Task _recordingTask;

        /// Séquence d'image de l'enregistrement.
        private AnimatedImageSequence _recordSequence;

        /// <summary>
        /// Générateur d'images pour le message d'enregistrement.
        /// </summary>
        [NotNull]
        public AnimatedImageSequence RecordImageSequence
        {
            get => _recordSequence;
            set
            {
                if (_recordSequence != null)
                {
                    _recordSequence.StopSequence();
                    _recordSequence.UpdateDraw -= InvokeRefresh;
                    _recordSequence.Dispose();
                    _recordSequence = null;
                }

                _recordSequence = value;
                _recordSequence.UpdateDraw += InvokeRefresh;
            }
        }
        
        /// Séquence d'image de l'enregistrement.
        private AnimatedImageSequence _transferSequence;

        /// <summary>
        /// Générateur d'images pour le décompte de transfert.
        /// </summary>
        [NotNull]
        public AnimatedImageSequence TransferImageSequence
        {
            get => _transferSequence;
            set
            {
                if (_transferSequence != null)
                {
                    _transferSequence.StopSequence();
                    _transferSequence.UpdateDraw -= InvokeRefresh;
                    _transferSequence.Dispose();
                    _transferSequence = null;
                }

                _transferSequence = value;
                _transferSequence.UpdateDraw += InvokeRefresh;
            }
        }

        public RecordVideoGoProScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
           Rectangle startZone, Rectangle stopZone, Rectangle? cancelZone)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            _startArea = startZone;
            _stopArea = stopZone;
            _closeArea = cancelZone;
            
            _ccgp = CameraControllerGoProSingle.GetInstance();
            _ccgp.PictureSaveDirectory = new DirectoryInfo(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos));
            _ccgp.Init();

            _timerKeepAlive = new Timer
            {
                AutoReset = true,
                Interval = 30 * 1000
            };
            _timerKeepAlive.Elapsed += (sender, args) =>
            {
                lock (_lock)
                {
                    if (!_cameraWorking && _recordingTask == null)
                    {
                        try
                        {
                            var test = Mode.Settings;
                            _ccgp?.GetCamera()?.Mode(out test);
                            log.Trace($"Keep alive issued: {0}", test.ToString());
                        }
                        catch (System.Exception e)
                        {
                            log.Error(e, "Error during keep alive");
                        }
                    }
                    else
                    {
                        log.Trace("Keep alive test skipped because camera in use.");
                    }
                }
            };
            _timerKeepAlive.Start();

            RecordImageSequence = new AnimatedImageSequence
            {
                Loop = true,
                DrawLocation = new Point((PictureBox.Width - 800) / 2, (PictureBox.Height - 100) / 2),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorFromText
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            BitmapSize = new Size(800, 100),
                            Color = Color.White,
                            Center = false,
                            FontFamilly = FontFamily.GenericSerif.ToString(),
                            FontSize = 50
                        },
                        TextsLang = new Dictionary<string, IList<string>>
                        {
                            {"en", new List<string>{"Recording.", "Recording..", "Recording..." } },
                            {"fr", new List<string>{"Enregistrement.", "Enregistrement..", "Enregistrement..." } }
                        }
                    }
                }
            };

            TransferImageSequence = new AnimatedImageSequence
            {
                Loop = true,
                DrawLocation = new Point((PictureBox.Width - 800) / 2, (PictureBox.Height - 100) / 2),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorFromText
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            BitmapSize = new Size(800, 100),
                            Color = Color.White,
                            Center = false,
                            FontFamilly = FontFamily.GenericSerif.ToString(),
                            FontSize = 50
                        },
                        TextsLang = new Dictionary<string, IList<string>>
                        {
                            {"en", new List<string>{"Downloading file from camera.", "Downloading file from camera..", "Downloading file from camera..." } },
                            {"fr", new List<string>{"Transfert du fichier en cours.", "Transfert du fichier en cours..", "Transfert du fichier en cours..." } }
                        }
                    }
                }
            };

            log.Trace("Constructor finished.");
        }

        /// <inheritdoc />
        protected override void OnLoad()
        {
            _recordInactiveImage = GetNoneLanguageBitmap("start_disable.png");
            _stopInactiveImage = GetNoneLanguageBitmap("stop_disable.png");
            _nextImage = GetNoneLanguageBitmap("next_enable.png");
            InitBitmap();
            log.Trace("OnLoad finished.");
        }

        protected void InitBitmap()
        {
            InitGenerators();
        }

        private void InitGenerators()
        {
            RecordImageSequence.ClearCurrentImages();
            RecordImageSequence.CreateImages(_activeLg);

            TransferImageSequence.ClearCurrentImages();
            TransferImageSequence.CreateImages(_activeLg);

            log.Trace("Generators initialized with lg={0}", _activeLg);
        }

        /// <inheritdoc />
        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);

            if (!change) return false;

            InitGenerators();

            return true;
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            RecordImageSequence.Paint(g);
            TransferImageSequence.Paint(g);

            if (_recording || _downloading)
            {
                if (_recordInactiveImage != null)
                {
                    DrawImage(g, _recordInactiveImage, _startArea);
                }
            }

            if (!_recording)
            {
                if (_stopInactiveImage != null)
                {
                    DrawImage(g, _stopInactiveImage, _stopArea);
                }
            }

            if (_fileDownloaded)
            {
                if (_nextImage != null)
                {
                    DrawImage(g, _nextImage, _stopArea);
                }
            }

        }

        public override void SetActivated()
        {
            base.SetActivated();

            _cts?.Dispose();
            _cts = null;

            _recording = false;
            _fileDownloaded = false;
            _downloading = false;
            _videoFileName = null;

            Refresh();

            log.Trace("Screen activated.");

            if (_ccgp.Init()) return;
            MsgBox.Show(this, "Could not connect to GoPro Camera.");
            Back();
        }
        
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _startArea))
            {
                if (_downloading || _ccgp == null) return false;

                if (!_recording)
                {
                    _recording = true;
                    _fileDownloaded = false;
                    SuspendTimer();
                    StartRecording();
                }
                Refresh();
                return false;
            }

            if (InArea(x, y, _closeArea))
            {
                if (_downloading)
                {
                    _cts?.Cancel();
                    log.Trace("Cancellation issued.");
                } else if (_recording)
                {
                    _cts?.Cancel();
                    log.Trace("Cancellation issued.");
                }
                FirstScreen();
                return false;
            }

            if (InArea(x, y, _stopArea))
            {
                if (_downloading) return false;

                if (_recording)
                {
                    _recording = false;
                    
                    Refresh();

                    if (_recordingTask != null)
                    {
                        log.Trace("Stop issued.");
                        _stop = true;
                    }
                    else
                    {
                        _recordingTask = null;
                        _stop = false;
                        _downloading = false;
                        _recording = false;
                        log.Debug("Illegal state: tried to stop recording while no recording task exists. Resetting status.");
                    }
                }
                else if (_fileDownloaded)
                {
                    log.Trace("Next clicked.");
                    Next();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Duration maximale du vidéo en secondes.
        /// </summary>
        public int VideoDuration { get; set; }

        private void StartRecording()
        {
            if (_ccgp == null) return;
            _cts = new CancellationTokenSource();
            _recordingTask = Task.Run(() =>
            {
                try
                {
                    lock (_lock) _cameraWorking = true;

                    RecordImageSequence.StartSequence();
                    var camera = _ccgp.GetCamera();
                    camera.Mode(out var currentMode);
                    if (currentMode != Mode.Video)
                    {
                        camera.Mode(Mode.Video);
                        log.Trace("Switched to correct mode: video.");
                    }

                    camera.OpenShutter();
                    log.Trace("OpenShutter command.");

                    var t = DateTime.Now;
                    var tf = DateTime.Now.AddSeconds(VideoDuration);

                    while (t < tf && !_cts.IsCancellationRequested && !_stop)
                    {
                        Thread.Sleep(100);
                        t = DateTime.Now;
                    }

                    camera.CloseShutter();
                    log.Trace("CloseShutter command.");

                    t = DateTime.Now;
                    tf = DateTime.Now.AddSeconds(2);

                    while (t < tf && !_cts.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                        t = DateTime.Now;
                    }

                    lock (_lock) _cameraWorking = false;
                    RecordImageSequence.StopSequence();

                    if (!_cts.IsCancellationRequested) DownloadVideo();
                }
                catch (System.Exception e)
                {
                    FormUtil.SafeCall(this, () =>
                    {
                        _recordingTask = null;
                        lock (_lock) _cameraWorking = false;
                        RecordImageSequence.StopSequence();
                        Refresh();
                        log.Error(e);
                        MessageBox.Show(this, @"Could not start recording.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    });
                }
            });
        }
        
        private void DownloadVideo()
        {
            if (_cts?.IsCancellationRequested == true) return;
            _downloading = true;
            TransferImageSequence.StartSequence();
            try
            {
                lock (_lock) _cameraWorking = true;
                var camera = _ccgp.GetCamera();

                var browser = camera.Contents();
                log.Trace("Browser command.");

                _videoFileName = GetTerminalDirectory(DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                     DirectoryUtil.ProjectDataDirectoriesModEnum.none) + GenerateUniqueId.GetId() + ".mp4";

                var video = browser.VideosAsync().Result.LastOrDefault();
                log.Trace("Last video file identified on camera: {0}", video?.Name);

                if (_cts?.IsCancellationRequested ?? false) return;

                if (video == null)
                {
                    log.Error("Video was not downloaded.");
                    FormUtil.SafeCall(this, () =>
                    {
                        MsgBox.Show(this, "Video could not be fetched from GoPro Camera.");
                    });
                    return;
                }

                log.Trace("Begin file download.");
                var response = video.Download().GetResponseStream();
                var testDownload = FileUtil.ReadStreamToFile(response, _videoFileName, 60 * 1000, _cts?.Token);
                log.Trace("End file download.");

                // On ne fait rien de plus en cas de cancellation.
                if (!(!_cts?.IsCancellationRequested ?? false)) return;

                if (!testDownload || !File.Exists(_videoFileName) || new FileInfo(_videoFileName).Length < 100)
                {
                    _videoFileName = null;
                    log.Error("Video was not downloaded.");
                    FormUtil.SafeCall(this, () =>
                    {
                        MsgBox.Show(this, "Video could not be fetched from GoPro Camera.");
                    });
                    return;
                }

                if (_screenConfig.QuestionVideoNo != null)
                {
                    SaveAnswerToQuestion(new Answer{QuestionNo = _screenConfig.QuestionVideoNo.Value, Response = _videoFileName});
                    log.Trace("Question saved: {0}", _videoFileName);
                }
                else
                {
                    log.Warn("File not saved to questionnaire, as there is no video question specified.");
                }

                _fileDownloaded = true;

                try
                {
                    log.Trace("Last File deletion issued.");
                    camera.DeleteLastFileOnSdCard();
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Could not destroy last file on camera.");
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
            }
            finally
            {
                _recordingTask = null;
                lock (_lock) _cameraWorking = false;
                TransferImageSequence.StopSequence();
                _downloading = false;
                Invoke(new Action(Refresh));
            }
        }
        
        public override void Recycle()
        {
            base.Recycle();

            _recording = false;
            _fileDownloaded = false;
            _downloading = false;
            _videoFileName = null;

            // au cas où.
            RecordImageSequence.StopSequence();
            TransferImageSequence.StopSequence();
        }

        public override void Dispose()
        {
            base.Dispose();
            _timerKeepAlive.Stop();

            RecordImageSequence.Dispose();
            TransferImageSequence.Dispose();
        }
    }
}
