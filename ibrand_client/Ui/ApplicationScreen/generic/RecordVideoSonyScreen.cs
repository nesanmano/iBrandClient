using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using sony_camera_api;
using sony_camera_api.Api;
using sony_camera_api.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cette classe prend un vidéo via une caméra Sony, qui peut fonctionner par Wifi avec un mini-serveur HTTP
    /// à l'interne.
    /// </summary>
    public class RecordVideoSonyScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected readonly PictureBox _panelPreview;

        protected List<Image> _safeBitmaps = new List<Image>();

        protected readonly Rectangle _startArea;
        protected readonly Rectangle _stopArea;
        protected readonly Rectangle? _closeArea;

        private CameraApiClient _camera;
        private AvContentApiClient _avContent;
        private SsdpDiscovery _discovery;

        private CancellationTokenSource _cancelTokenSource;

        private BlockingQueue<byte[]> _jpegQueue = new BlockingQueue<byte[]>(2);

        private static readonly ImageConverter IMAGE_CONVERTER = new ImageConverter();

        private bool _recording;
        private bool _downloading;
        private bool _fileDownloaded;

        private Bitmap _recordInactiveImage;
        private Bitmap _stopInactiveImage;
        private Bitmap _nextImage;

        private readonly ProgressBar _progressBar;

        private string _videoFileName;

        private WebClient _webClient;
        private AsyncCompletedEventHandler _completedEvent;
        private DownloadProgressChangedEventHandler _progressEvent;

        private readonly List<Task> _runningPreviewTasklist = new List<Task>();

        public bool NoLanguageDependency { get; set; }

        public RecordVideoSonyScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
           Rectangle startZone, Rectangle stopZone, Rectangle? cancelZone, Size previewSize, int previewYposition)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            _startArea = startZone;
            _stopArea = stopZone;
            _closeArea = cancelZone;

            _panelPreview = new PictureBox
            {
                Size = previewSize,
                Location = new Point((PictureBox.Width - previewSize.Width) / 2, previewYposition),
                BackColor = Color.White,
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Controls.Add(_panelPreview);

            _panelPreview.BringToFront();

            _progressBar = new ProgressBar
            {
                Size = new Size(previewSize.Width, 25),
                Location = new Point((PictureBox.Width - previewSize.Width) / 2, previewYposition + previewSize.Height + 10),
                BackColor = Color.White,
                Visible = false,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Step = 1
            };

            Controls.Add(_progressBar);

            _progressBar.BringToFront();
        }

        protected override void OnLoad()
        {
            _discovery = new SsdpDiscovery();
            _discovery.SonyCameraDeviceDiscovered += OnSonyCameraDiscovered;
            _discovery.Timeout += OnTimeout;

            var lg = NoLanguageDependency ? null : _activeLg;
            _recordInactiveImage = GetBitmap("start_disable.png", lg);
            _stopInactiveImage = GetBitmap("stop_disable.png", lg);
            _nextImage = GetBitmap("next_enable.png", lg);
        }

        private void OnTimeout(object sender, EventArgs e)
        {
            if (_camera != null)
                return;

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(this, @"Camera is not connected.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }));
                return;
            }

            MessageBox.Show(this, @"Camera is not connected.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private async void OnSonyCameraDiscovered(object sender, SonyCameraDeviceEventArgs e)
        {
            var endpoints = e.SonyCameraDevice.Endpoints; // Dictionary of each service name and endpoint.
            _camera = new CameraApiClient(new Uri(endpoints["camera"]));
            _avContent = new AvContentApiClient(new Uri(endpoints["avContent"]));

            //TODO StopRecording...

            await _camera.StartRecModeAsync();

            StartLivePreview();
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_recording || _downloading)
            {
                ScreenUtil.DrawImage(g, _recordInactiveImage, _startArea);
            }

            if (!_recording)
            {
                ScreenUtil.DrawImage(g, _stopInactiveImage, _stopArea);
            }

            if (_fileDownloaded)
            {
                ScreenUtil.DrawImage(g, _nextImage, _stopArea);
            }
        }

        /// <summary>
        /// DEBUG seulement!!!
        /// Pointe vers un fichier vidéo à utiliser en lieu et place de la caméra Sony, à cause des requis élevés de cette caméra.
        /// Permet de déboguer le projet SANS AVOIR à utiliser la caméra Sony.
        /// </summary>
        public string DebugMovieFile { get; set; }

        public override void SetActivated()
        {
            base.SetActivated();

            _cancelTokenSource?.Dispose();

            _cancelTokenSource = new CancellationTokenSource();

            _progressBar.Visible = false;

            _jpegQueue = new BlockingQueue<byte[]>(2);

            Refresh();

            if (DebugMovieFile != null && File.Exists(DebugMovieFile) && _screenConfig?.QuestionVideoNo != null)
            {
                SaveAnswerToQuestion(new Answer {QuestionNo = _screenConfig.QuestionVideoNo.Value, Response = DebugMovieFile});
                Next();
                return;
            }

            if (_camera == null)
            {
                Task.Run(() =>
                {
                    _discovery.SearchSonyCameraDevices();
                    StartLivePreview();
                });
            }
            else
            {
                Task.Run(StartLivePreview);
            }

        }

        private async void StartLivePreview()
        {
            try {
                var func = await _camera.GetCameraFunctionAsync();

                //TODO tester quand en mode record...
                if (!func.Equals("Remote Shooting"))
                    await _camera.SetCameraFunctionAsync("Remote Shooting");

                var liveViewUrl = await _camera.StartLiveviewAsync();

                StartPreviewTasks(liveViewUrl);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        private void StartPreviewTasks(string liveviewUrl)
        {
            try
            {
                var token = _cancelTokenSource.Token;

                _runningPreviewTasklist.Add(Task.Run(() =>
                {
                    SimpleLiveviewSlicer slicer = null;

                    try
                    {
                        // Create Slicer to open the stream and parse it.
                        slicer = new SimpleLiveviewSlicer();
                        slicer.Open(liveviewUrl);

                        log.Debug("StartPreviewTasks");

                        while (_activated && !token.IsCancellationRequested )
                        {
                            var payload = slicer.NextPayload();
                            if (payload == null)
                            { // never occurs
                                log.Error("Liveview Payload is null.");
                                continue;
                            }
                            _jpegQueue.Enqueue(payload.jpegData);
                        }

                        log.Debug("End StartPreviewTasks");
                    }
                    catch (System.Exception e)
                    {
                        log.Error("IOException while fetching: " + e.Message);
                    }
                    finally
                    {
                        slicer?.Close();
                    }
                }, token));

                _runningPreviewTasklist.Add(Task.Run(() =>
                {
                    log.Debug("Update picture");
                    while (_activated && !token.IsCancellationRequested)
                    {
                        if (!_jpegQueue.TryDequeue(out var jpegData)) continue;
                        var bm = (Bitmap)IMAGE_CONVERTER.ConvertFrom(jpegData);
                        UpdatePicture(bm);
                    }

                    log.Debug("End Update picture");
                }, token));

            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _startArea))
            {
                if (_downloading || _camera == null) return false;

                if (!_recording)
                {
                    _recording = true;

                    if (_fileDownloaded) {
                        var token = _cancelTokenSource.Token;
                        if (token.IsCancellationRequested)
                            _cancelTokenSource.Dispose();

                        _cancelTokenSource = new CancellationTokenSource();
                        _fileDownloaded = false;
                        Task.Run(StartLivePreview, token);
                    }
                    SuspendTimer();
                    Task.Run(StartRecording);
                }
                Refresh();
                return false;
            }

            if (InArea(x, y, _closeArea))
            {
                //TODO tester si ca fonctionne..
                if (_downloading)
                {
                    _webClient?.CancelAsync();
                }

                if (_recording)
                    Task.Run(StopRecording);

                Task.Run(StopLiveView);
                FirstScreen();
                return false;
            }

            if (InArea(x, y, _stopArea))
            {
                if (_downloading) return false;

                if (_recording)
                {
                    _recording = false;
                    _downloading = true;

                    _progressBar.Visible = true;

                    Refresh();

                    Task.Run(StopRecordingAndDownload);
                    Task.Run(StopLiveView);
                }
                else if (_fileDownloaded)//next
                {
                    Task.Run(SetShootingMode);
                    if (_screenConfig.QuestionVideoNo != null && _videoFileName != null)
                        SaveAnswerToQuestion(new Answer {QuestionNo = _screenConfig.QuestionVideoNo.Value, Response = _videoFileName});
                    Next();
                }

                return false;
            }

            return true;
        }

        private async void SetShootingMode()
        {
            if (_camera == null) return;
            try {
                var func = await _camera.GetCameraFunctionAsync();

                if (!func.Equals("Remote Shooting"))
                    await _camera.SetCameraFunctionAsync("Remote Shooting");
            }catch(System.Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private async void StartRecording()
        {
            try {
                if (_camera != null)
                    await _camera.StartMovieRecAsync();
            }catch(System.Exception ex)
            {
                log.Error(ex);
                _recording = false;

                this.SafeCall(() => {
                    Refresh();
                    MessageBox.Show(this, @"Camera detected, but record command failed. Is the camera on the right shooting mode?", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                });
            }
        }

        private async void StopRecording()
        {
            try
            {
                if (_camera != null)
                {
                    await _camera.StopMovieRecAsync();
                }
            }
            catch (System.Exception)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        Refresh();
                        MessageBox.Show(this, @"Could not stop recording.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }));
                    return;
                }
                Refresh();
                MessageBox.Show(this, @"Could not stop recording.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private async void StopRecordingAndDownload()
        {
            try {
                if (_camera != null)
                {
                    await _camera.StopMovieRecAsync();

                    var _event = await _camera.GetEventAsync(false);

                    while (!_event.CameraStatus.Equals("IDLE")) {
                        Thread.Sleep(300);
                        _event = await _camera.GetEventAsync(false);
                    }


                    DownloadVideo();
                }
            }
            catch (System.Exception)
            {
                _recording = true;
                _downloading = false;

                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        Refresh();
                        MessageBox.Show(this, @"Could not stop recording and download file.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }));
                    return;
                }
                Refresh();
                MessageBox.Show(this, @"Could not stop recording and download file.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private async void DownloadVideo()
        {
            if (_camera == null) return;
            try {
                    
                await _camera.SetCameraFunctionAsync("Contents Transfer");

                if (_avContent == null) return;
                var uriScheme = new UriScheme {Scheme = "storage"};
                var sourceList = await _avContent.GetSourceListAsync(uriScheme);

                if (sourceList.Count <= 0) return;

                var listTarget = new ContentListTarget
                {
                    Uri = sourceList[0].Source,
                    Sorting = SortMode.Descending,
                    MaxContents = 10,
                    Grouping = "flat"
                    //,Types = new List<string> { "movie_mp4", "movie_xavcs" } // Provoque un bogue
                };

                var contents = await _avContent.GetContentListAsync(listTarget);
                if (contents.Count <= 0) return;

                var content = contents.First(c => c.ContentKind.StartsWith("movie", StringComparison.InvariantCultureIgnoreCase));
                var url = content.ImageContent.OriginalContents[0].Url;
                _videoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + ".mp4";
                DownloadFile(url, _videoFileName);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        private void DownloadFile(string urlAddress, string location)
        {
            if (_webClient != null)
                return;

            _webClient = new WebClient();

            _completedEvent = Completed;
            _webClient.DownloadFileCompleted += _completedEvent;

            _progressEvent = ProgressChanged;
            _webClient.DownloadProgressChanged += _progressEvent;

            // The variable that will be holding the url address (making sure it starts with http://)
            var url = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

            try
            {
                // Start downloading the file
                _webClient.DownloadFileAsync(url, location);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (_progressBar.Value == e.ProgressPercentage) return;

                    _progressBar.Value = e.ProgressPercentage;
                    Refresh();
                }));
                return;
            }

            _progressBar.Value = e.ProgressPercentage;
            Refresh();
        }

        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                var error = e.Error.ToString();
                log.Error(error);       
            }
            
            _webClient.DownloadFileCompleted -= _completedEvent;
            _webClient.DownloadProgressChanged -= _progressEvent;
            _webClient.Dispose();

            _webClient = null;

            try
            {
                Task.Run(() => _camera.SetCameraFunctionAsync("Remote Shooting"));
            }

            catch (System.Exception ex)
            {

                log.Error(ex);
            }

            _fileDownloaded = true;

            _downloading = false;

            Invoke(new Action(Refresh));
        }

        private async void StopLiveView()
        {
            try {
                _cancelTokenSource?.Cancel();
                _jpegQueue?.Close();
                Task.WaitAll(_runningPreviewTasklist.ToArray());

                _runningPreviewTasklist.Clear();

                if (_camera != null)
                    await _camera.StopLiveviewAsync();

            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
            
        }
        
        protected void UpdatePicture(Bitmap image)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePicture(image)));
            }
            else
            {
                SetPreviewPicture(_panelPreview, _safeBitmaps, image);
            }
        }
        
        public override void Recycle()
        {
            base.Recycle();

            Task.Run(StopLiveView);

            _recording = false;
            _fileDownloaded = false;
            _downloading = false;

            _panelPreview.Image?.Dispose();
            _panelPreview.Image = null;

            _progressBar.Value = 0;

            _videoFileName = null;
        }
    }
}
