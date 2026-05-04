using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Util;
using sony_camera_api;
using sony_camera_api.Api;
using sony_camera_api.Util;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable UnusedVariable
// ReSharper disable NotAccessedField.Local
#pragma warning disable 414

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.G7_Selena
{
    public class RecordVideoScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected readonly PictureBox _panel_preview;

        protected List<Image> _safe_bitmaps = new List<Image>();
        Bitmap _recording_image = null;

        Bitmap _overlay = null;

        protected readonly Rectangle _closeArea;

        private CameraApiClient camera = null;
        private AvContentApiClient avContent = null;
        private SsdpDiscovery discovery = null;

        private CancellationTokenSource _cancelTokenSource = null;

        private BlockingQueue<byte[]> jpegQueue = new BlockingQueue<byte[]>(2);

        private static readonly ImageConverter _imageConverter = new ImageConverter();

        private bool recording = false;
        private bool downloading = false;
        private bool file_downloaded = false;

        ProgressBar _progressBar = null;

        private string videoFileName = null;

        private WebClient webClient = null;
        private AsyncCompletedEventHandler completedEvent = null;
        private DownloadProgressChangedEventHandler progressEvent = null;

        List<Task> _runningPreviewTasklist = new List<Task>();

        protected readonly System.Timers.Timer _count_down_timer;

        protected readonly System.Timers.Timer _record_timer;

        protected int _count_down = 5;

        protected List<Bitmap> _count_bitmaps = new List<Bitmap>();

        protected int _count_down_max = 5;
        

        public RecordVideoScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
            Rectangle cancelZone, Size previewSize, int previewYposition, int previewXposition)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            _closeArea = cancelZone;

            //PictureBox.MouseDown += MouseDownHandler;

            _panel_preview = new PictureBox
            {
                Size = previewSize,
                Location = new Point(previewXposition, previewYposition),
                BackColor = Color.White,
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            //PictureBox.Paint += PaintEvent;

            Controls.Add(_panel_preview);

            _panel_preview.BringToFront();

            _progressBar = new ProgressBar
            {
                Size = new Size(688, 25),
                Location = new Point(previewXposition, previewYposition + previewSize.Height + 10),
                BackColor = Color.White,
                Visible = false,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Step = 1
            };

            Controls.Add(_progressBar);

            _progressBar.BringToFront();

            discovery = new SsdpDiscovery();
            discovery.SonyCameraDeviceDiscovered += OnSonyCameraDiscovered;
            discovery.Timeout += OnTimeout;

            _count_down_timer = new System.Timers.Timer
            {
                Interval = 1000,
                AutoReset = true
            }; //1 sec
            _count_down_timer.Elapsed += OnCountDownTimerEventt;

            _record_timer = new System.Timers.Timer
            {
                Interval = 3000,
                AutoReset = true
            }; //1 sec
            _record_timer.Elapsed += OnRecordTimerEventt;

            for (var i = 1; i <= _count_down_max; i++)
            {
                _count_bitmaps.Add(GetNoneLanguageBitmap(i + ".png"));
            }

            _recording_image = GetNoneLanguageBitmap("recording.png");
            _overlay = GetNoneLanguageBitmap("overlay_preview.png");
        }

        public void OnCountDownTimerEventt(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnCountDownTimerEventDo));
                return;
            }

            OnCountDownTimerEventDo();
        }

        public void OnRecordTimerEventt(object sender, EventArgs e)
        {
            _record_timer.Stop();
            OnStopRecording();
        }

        private void OnCountDownTimerEventDo()
        {
            if (_count_down > 0)
            {
                _count_down--;

                if (_count_down == 1)
                    OnStartRecording();
                Refresh();
            }
            else
            {
                //OnStartRecording();
                _count_down_timer.Stop(); 

            }
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    try
        //    {
        //        if (_count_down > 0)
        //        {
        //            e.Graphics.DrawImageUnscaled(_count_bitmaps[_count_down - 1], new Point(1051,287));
        //        }
        //        else if (_count_down == 0 && recording)
        //        {
        //            // Image recording
        //            e.Graphics.DrawImageUnscaled(_recording_image, new Point(1051, 287));
        //        }
                
        //    }
        //    catch (System.Exception err)
        //    {
        //        log.Error(err, "Error during Paint method.");
        //    }
        //}

        private void OnTimeout(object sender, EventArgs e)
        {
            if (camera != null)
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
            camera = new CameraApiClient(new Uri(endpoints["camera"]));
            avContent = new AvContentApiClient(new Uri(endpoints["avContent"]));

            await camera.StartRecModeAsync();

            StartLivePreview();

            _count_down_timer.Start();
        }


        public override void SetActivated()
        {
            base.SetActivated();

            if (_cancelTokenSource != null)
                _cancelTokenSource.Dispose();

            _cancelTokenSource = new CancellationTokenSource();

            _progressBar.Visible = false;
            _progressBar.Value = 0;

            recording = false;
            file_downloaded = false;
            downloading = false;

            jpegQueue = new BlockingQueue<byte[]>(2);

            Refresh();

            if (camera == null)
            {
                Task.Run(() => discovery.SearchSonyCameraDevices());
            }
            else
            {
                Task.Run(() => StartLivePreview());
                //_count_down_timer.Start();
            }
        }

        private async void StartLivePreview()
        {
            try
            {
                var func = await camera.GetCameraFunctionAsync();

                //TODO tester quand en mode record...
                if (!func.Equals("Remote Shooting"))
                    await camera.SetCameraFunctionAsync("Remote Shooting");

                var liveViewUrl = await camera.StartLiveviewAsync();

                StartPreviewTasks(liveViewUrl);

                _count_down_timer.Start();
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
                StopRecording();
            }
        }

        private void StartPreviewTasks(string liveview_url)
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
                        slicer.Open(liveview_url);

                        log.Debug("StartPreviewTasks");

                        while (_activated && !token.IsCancellationRequested)
                        {
                            var payload = slicer.NextPayload();
                            if (payload == null)
                            { // never occurs
                                log.Error("Liveview Payload is null.");
                                continue;
                            }

                            /*if (jpegQueue.Count == 2)
                            {
                                jpegQueue.Dequeue();
                            }*/
                            jpegQueue.Enqueue(payload.jpegData);
                        }

                        log.Debug("End StartPreviewTasks");
                    }
                    catch (System.Exception e)
                    {
                        log.Error("IOException while fetching: " + e.Message);
                    }
                    finally
                    {
                        if (slicer != null)
                        {
                            slicer.Close();
                        }
                    }
                }, token));

                _runningPreviewTasklist.Add(Task.Run(() =>
                {
                    log.Debug("Update picture");
                    while (_activated && !token.IsCancellationRequested)
                    {
                        byte[] jpegData;
                        if (jpegQueue.TryDequeue(out jpegData))
                        {
                            var bm = (Bitmap)_imageConverter.ConvertFrom(jpegData);
                            UpdatePicture(bm);
                        }

                    }

                    log.Debug("End Update picture");
                }, token));

            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnStartRecording()
        {
            if (downloading || camera == null)
                return;

            if (!recording)
            {
                recording = true;

                /*if (file_downloaded)
                {
                    CancellationToken token = _cancelTokenSource.Token;
                    if (token.IsCancellationRequested)
                        _cancelTokenSource.Dispose();

                    _cancelTokenSource = new CancellationTokenSource();
                    file_downloaded = false;
                }*/
                SuspendTimer();
                Task.Run(() => StartRecording());
                _record_timer.Start();
            }
            Refresh();
        }

        private void OnStopRecording()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnStopRecording));
                return;
            }

            if (recording)
            {
                recording = false;
                downloading = true;

                _progressBar.Visible = true;

                Refresh();

                Task.Run(() => StopRecordingAndDownload());
                Task.Run(() => StopLiveView());
            }
        }

        private void OnDownloadFinished()
        {
            Task.Run(() => SetShootingMode());
            if (_screenConfig.QuestionVideoNo != null && videoFileName != null)
                //SaveAnswerToQuestion(_screenConfig.QuestionVideoNo.Value, 1, videoFileName);
            _count_down = 5;
            Next();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (_activated)
        //    {
        //        base.MouseDownHandler(sender, e);
        //        if (_flagForClosing) return;
        //        if (InArea(e.X, e.Y, _closeArea))
        //        {
        //            if (downloading && webClient != null)
        //            {
        //                webClient.CancelAsync();
        //            }

        //            if (recording)
        //                Task.Run(() => StopRecording());

        //            Task.Run(() => StopLiveView());
        //            FirstScreen();
        //        }
        //    }
        //}

        private async void SetShootingMode()
        {
            try
            {
                if (camera != null)
                {
                    var func = await camera.GetCameraFunctionAsync();

                    if (!func.Equals("Remote Shooting"))
                        await camera.SetCameraFunctionAsync("Remote Shooting");
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private async void StartRecording()
        {
            try
            {
                if (camera != null)
                    await camera.StartMovieRecAsync();
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
                recording = false;

                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        Refresh();
                        MessageBox.Show(this, @"Could not start recording.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }));
                    return;
                }
                Refresh();
                MessageBox.Show(this, @"Could not start recording.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private async void StopRecording()
        {
            try
            {
                if (camera != null)
                {
                    await camera.StopMovieRecAsync();
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
            try
            {
                if (camera != null)
                {
                    await camera.StopMovieRecAsync();

                    var _event = await camera.GetEventAsync(false);

                    while (!_event.CameraStatus.Equals("IDLE"))
                    {
                        Thread.Sleep(300);
                        _event = await camera.GetEventAsync(false);
                    }

                    DownloadVideo();
                }
            }
            catch (System.Exception)
            {
                recording = true;

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
            if (camera != null)
            {
                string camera_status = null;
                try
                {

                    await camera.SetCameraFunctionAsync("Contents Transfer");

                    if (avContent != null)
                    {
                        var uriScheme = new UriScheme();
                        uriScheme.Scheme = "storage";
                        var sourceList = await avContent.GetSourceListAsync(uriScheme);

                        if (sourceList.Count > 0)
                        {
                            var listTarget = new ContentListTarget();
                            listTarget.Uri = sourceList[0].Source;
                            listTarget.Sorting = SortMode.Descending;
                            listTarget.MaxContents = 1;
                            //listTarget.Types = new List<string>().Add("movie_mp4");
                            listTarget.Grouping = "flat";
                            var contents = await avContent.GetContentListAsync(listTarget);
                            if (contents.Count > 0)
                            {
                                var content = contents[0];
                                var url = content.ImageContent.OriginalContents[0].Url;
                                videoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + ".mp4";
                                DownloadFile(url, videoFileName);
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    downloading = false;
                    var test = camera_status;
                    log.Error(ex);
                }

            }
        }

        private void DownloadFile(string urlAddress, string location)
        {
            if (webClient != null)
                return;

            webClient = new WebClient();

            completedEvent = Completed;
            webClient.DownloadFileCompleted += completedEvent;

            progressEvent = ProgressChanged;
            webClient.DownloadProgressChanged += progressEvent;

            // The variable that will be holding the url address (making sure it starts with http://)
            var URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

            try
            {
                // Start downloading the file
                webClient.DownloadFileAsync(URL, location);
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
                    if (_progressBar.Value != e.ProgressPercentage)
                    {
                        _progressBar.Value = e.ProgressPercentage;
                        Refresh();
                    }
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

            webClient.DownloadFileCompleted -= completedEvent;
            webClient.DownloadProgressChanged -= progressEvent;
            webClient.Dispose();

            webClient = null;

            try
            {
                Task.Run(() => camera.SetCameraFunctionAsync("Remote Shooting"));
            }

            catch (System.Exception ex)
            {

                log.Error(ex);
            }

            file_downloaded = true;

            downloading = false;

            Invoke(new Action(() =>
            {
                Refresh();
            }));

            OnDownloadFinished();
        }

        private async void StopLiveView()
        {
            try
            {
                _cancelTokenSource.Cancel();
                jpegQueue.Close();
                Task.WaitAll(_runningPreviewTasklist.ToArray());

                _runningPreviewTasklist.Clear();

                if (camera != null)
                    await camera.StopLiveviewAsync();

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

                /*SetPreviewPicture(_panel_preview, _safe_bitmaps,
                    MakeImageForView(image, _panel_preview.Width, _panel_preview.Height, _overlay, null));*/
                SetPreviewPicture(_panel_preview, _safe_bitmaps, image);
            }
        }


        public override void Recycle()
        {
            base.Recycle();

            recording = false;
            file_downloaded = false;
            downloading = false;

            _panel_preview.Image?.Dispose();
            _panel_preview.Image = null;

            _progressBar.Value = 0;

            videoFileName = null;

            _count_down = 5;
        }
    }
}
