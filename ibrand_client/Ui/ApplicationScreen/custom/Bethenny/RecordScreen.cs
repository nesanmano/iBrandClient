using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DirectX.Capture;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using Timer = System.Windows.Forms.Timer;
/*using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder;*/

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class RecordScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly object _lock = new object();
        
        private readonly Panel _panelVideo;

        private int _timeBeforeRecord = 3; //seconds
        private int _timeToRecord = 30; //seconds

        private readonly Timer _timer;

        private string _absolutePathFileName;
        private string _videoFileName;

        /*EncoderDevice video = null;
        EncoderDevice audio = null;

        LiveJob _job = null;

        private LiveDeviceSource _deviceSource;*/

        private Capture _capture;
        private Filters _filters;

        private readonly List<Bitmap> _beforeRecordCountDownList;
        private readonly List<Bitmap> _recordCountDownList;

        readonly Rectangle _stopArea;

        readonly PictureBox _getReadyPictureBox;
        readonly PictureBox _recordingPictureBox;
        readonly PictureBox _blankPictureBox;

        bool _isRecording;

        public RecordScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;
            
            _panelVideo = new Panel();
            _panelVideo.Location = new Point(311, 24);
            _panelVideo.Name = "panel_video";
            _panelVideo.Size = new Size(1280, 720);

            _panelVideo.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom)
                                  | AnchorStyles.Left)
                                 | AnchorStyles.Right;
            _panelVideo.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(_panelVideo);

            _panelVideo.BringToFront();

            _timer = new Timer();
            _timer.Interval = 1000; //1 sec
            _timer.Tick += OnTimerEventt;

            PictureBox.Paint += PaintEvent;

            _beforeRecordCountDownList = new List<Bitmap>(3);
            for (var i = 1; i <= 3; i++)
            {
               _beforeRecordCountDownList.Add(GetNoneLanguageBitmap(i+".png"));
            }

            _recordCountDownList = new List<Bitmap>(30);
            for (var i = 1; i <= 30; i++)
            {
                _recordCountDownList.Add(GetNoneLanguageBitmap(i + ".png"));
            }

            _stopArea = new Rectangle(new Point(1611, 612), new Size(235, 150));

            var getReadyImage = GetNoneLanguageBitmap("get_ready.png");
            _getReadyPictureBox = new PictureBox();
            _getReadyPictureBox.Location = new Point(800, 762);
            _getReadyPictureBox.Size = new Size(346, 63);
            _getReadyPictureBox.Visible = true;

            _getReadyPictureBox.Image = getReadyImage;

            Controls.Add(_getReadyPictureBox);

            _getReadyPictureBox.BringToFront();

            var recordingImage = GetNoneLanguageBitmap("recording.png");
            _recordingPictureBox = new PictureBox();
            _recordingPictureBox.Location = new Point(800, 762);
            _recordingPictureBox.Size = new Size(364, 57);
            _recordingPictureBox.Visible = false;

            _recordingPictureBox.Image = recordingImage;

            Controls.Add(_recordingPictureBox);

            _recordingPictureBox.BringToFront();

            var blankImage = GetNoneLanguageBitmap("blank.png");
            _blankPictureBox = new PictureBox();
            _blankPictureBox.Location = new Point(1612, 615);
            _blankPictureBox.Size = new Size(234, 141);
            _blankPictureBox.Visible = true;

            _blankPictureBox.Image = blankImage;

            Controls.Add(_blankPictureBox);

            _blankPictureBox.BringToFront();

            //InitCaptureDevice();
            try
            {
                _filters = new Filters();
            }
            catch (System.Exception)
            {
                _filters = null;
                MessageBox.Show(this, "Web Cam or Micro not connected.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {

            if (InArea(e.X, e.Y, _stopArea) && _timeBeforeRecord == 0)
            {
                _timer.Stop();
                
                StopRecording();
                
                Next();

                _recordingPictureBox.Visible = false;
                _getReadyPictureBox.Visible = true;
                _blankPictureBox.Visible = true;
                
                //InitCaptureDevice();
            }

        }

        public new void PaintEvent(object sender, PaintEventArgs e)
        {
            if (_timeBeforeRecord != 0)
            {               
                var bitmap = _beforeRecordCountDownList[_timeBeforeRecord-1];
                e.Graphics.DrawImageUnscaled(bitmap, new Point(852, 824));
            }
            else
            {
                if (_timeToRecord != 0 && _isRecording)
                {
                    var bitmap = _recordCountDownList[_timeToRecord - 1];
                    e.Graphics.DrawImageUnscaled(bitmap, new Point(852, 824));
                }
            }
        }
       
        public override void Recycle()
        {
            base.Recycle();

            _timer.Stop();
            
            _absolutePathFileName = null;
            _videoFileName = null;
            _blankPictureBox.Visible = true;
            _recordingPictureBox.Visible = false;
            _getReadyPictureBox.Visible = true;
            _isRecording = false;
        }

        private string GetAbsolutePathVideoName(string videoName)
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            var newVideoName = //configuration.VideosDir + 
                _videoFileName;

            if (File.Exists(newVideoName))
            {
                return null;
            }

            return newVideoName;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            /*if (_absolute_path_file_name != null)//re-record
            {
                File.Delete(_absolute_path_file_name);
            }*/

            if (!InitCaptureDevice())
            {
                MessageBox.Show(this, "Web Cam or Micro not connected.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Back();
                return;
            }

            _recordingPictureBox.Visible = false;
            _getReadyPictureBox.Visible = true;
            _blankPictureBox.Visible = true;

            _timeBeforeRecord = 3;
            _timeToRecord = 30;
            StartRecording();

            _timer.Start();
        }

        /*void StopJob()
        {
            // Has the Job already been created ?
            if (_job != null)
            {
               
                _job.StopEncoding();

                // Remove the Device Source and destroy the job
                _job.RemoveDeviceSource(_deviceSource);

                // Destroy the device source
                _deviceSource.PreviewWindow = null;
                _deviceSource = null;
            }
        }

        private void StartPreview()
        {
            
            StopJob();

            if (video == null)
            {
                return;
            }

            // Starts new job for preview window
            _job = new LiveJob();

            // Checks for a/v devices
            if (video != null && audio != null)
            {
                // Create a new device source. We use the first audio and video devices on the system
                _deviceSource = _job.AddDeviceSource(video, audio);

              
                // No
                // Setup the video resolution and frame rate of the video device
                // NOTE: Of course, the resolution and frame rate you specify must be supported by the device!
                // NOTE2: May be not all video devices support this call, and so it just doesn't work, as if you don't call it (no error is raised)
                // NOTE3: As a workaround, if the .PickBestVideoFormat method doesn't work, you could force the resolution in the 
                //        following instructions (called few lines belows): 'panelVideoPreview.Size=' and '_job.OutputFormat.VideoProfile.Size=' 
                //        to be the one you choosed (640, 480).
                _deviceSource.PickBestVideoFormat(new Size(1280, 720), 30);


                // Get the properties of the device video
                SourceProperties sp = _deviceSource.SourcePropertiesSnapshot();

                // Resize the preview panel to match the video device resolution Set
                panel_video.Size = new Size(sp.Size.Width, sp.Size.Height);

                // Setup the output video resolution file as the preview
                _job.OutputFormat.VideoProfile.Size = new Size(sp.Size.Width, sp.Size.Height);


                // Sets preview window to winform panel hosted by xaml window
                _deviceSource.PreviewWindow = new PreviewWindow(new HandleRef(panel_video, panel_video.Handle));

                // Make this source the active one
                _job.ActivateSource(_deviceSource);

            }
            else
            {
                // Gives error message as no audio and/or video devices found
                MessageBox.Show("No Video/Audio capture devices have been found.", "Warning");
            }
        }*/

        public void OnTimerEventt(object sender, EventArgs e)
        {
            if (_timeBeforeRecord != 0)
            {
                _timeBeforeRecord--;
                Refresh();
            }
            else
            {
                if (_timeToRecord != 0)
                {
                    if (_timeToRecord == 30)
                    {
                        //StartRecording();

                        _blankPictureBox.Visible = false;
                        _getReadyPictureBox.Visible = false;
                        _getReadyPictureBox.Invalidate();
                        _recordingPictureBox.Visible = true;
                        _recordingPictureBox.Invalidate();
                        
                        Refresh();

                        _isRecording = true;
                    }

                    _timeToRecord--;

                    Refresh();

                }
                else
                {
                    _timer.Stop();
                    StopRecording();
                    Next();
                    //InitCaptureDevice();
                }
            }
        }

        /*private void InitCaptureDevice()
        {
            AbstractConfiguration configuration = ServicesContainer.GetService<NomadConfiguration>();
            // Get the selected video device            
            foreach (EncoderDevice edv in EncoderDevices.FindDevices(EncoderDeviceType.Video))
            {
                if (edv.Name.Contains(configuration.WebCamName))
                {
                    video = edv;
                    break;
                }
            }

            // Get the selected audio device            
            foreach (EncoderDevice eda in EncoderDevices.FindDevices(EncoderDeviceType.Audio))
            {
                if (eda.Name.Contains(configuration.AudioInputName))
                {
                    audio = eda;
                    break;
                }
            }
        }

        private void StartRecording()
        {
            try
            {
                // Sets up publishing format for file archival type
                FileArchivePublishFormat fileOut = new FileArchivePublishFormat();

                // Sets file path and name
                _video_file_name = GenerateUniqueID.GetID() + ".wmv";
                _absolute_path_file_name = GetAbsolutePathVideoName(_video_file_name);

                fileOut.OutputFileName = _absolute_path_file_name;//String.Format(@"C:\Users\Nomad Logic\Desktop\EE4WebCam_update2\EE4WebCam\bin\Debug\WebCam{0:yyyyMMdd_hhmmss}.wmv", DateTime.Now);

                // Adds the format to the job. You can add additional formats as well such as
                // Publishing streams or broadcasting from a port
                _job.PublishFormats.Add(fileOut);

                // Start encoding
                _job.StartEncoding();

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.ToString());
            }
        }

        private void StopRecording()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                _job.StopEncoding();

                if (_video_file_name != null)
                {

                    Questionnaire questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

                    Question.Question question = questionnaire.GetQuestion(17);
                    ChoiceText choice_text = (ChoiceText)question.GetChoice(1);

                    choice_text.SetAnswer(new Answer(7, 17, 1, _video_file_name));
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }*/

        private bool InitCaptureDevice()
        {
            Filter f = null;
            Filter videoDevice = null;
            Filter audioDevice = null;
            try
            {

                if (_filters == null)
                    _filters = new Filters();

                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                for (var c = 0; c < _filters.VideoInputDevices.Count; c++)
                {
                    f = _filters.VideoInputDevices[c];
                    if (f.Name.Contains(configuration.WebCamName))
                    {
                        videoDevice = f;
                        break;
                    }
                }

                for (var c = 0; c < _filters.AudioInputDevices.Count; c++)
                {
                    f = _filters.AudioInputDevices[c];
                    if (f.Name.Contains(configuration.AudioInputName))
                    {
                        audioDevice = f;
                        break;
                    }
                }

                // Create capture object
                if ((videoDevice != null) || (audioDevice != null))
                {
                    if (_capture != null)
                    {
                        _capture.Stop();
                        _capture.Dispose();
                        _capture = null;
                    }

                    _capture = new Capture(videoDevice, audioDevice/*, false*/);
                    _capture.FrameSize = new Size(1280, 720);
                    _capture.PreviewWindow = _panelVideo;
                }
            }
            catch (System.Exception)
            {
                return false;
            }

            if (videoDevice == null/* || audioDevice == null*/)
                return false;

            return true;
        }

        private void StartRecording()
        {
            try
            {
                if (_capture == null)
                    return;

                do
                {
                    _videoFileName = GenerateUniqueId.GetId() + ".avi";
                    _absolutePathFileName = GetAbsolutePathVideoName(_videoFileName);
                } while (_absolutePathFileName == null);

                _capture.Filename = _absolutePathFileName;

                _capture.Start();
                
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message + "\n\n" + ex);
                MessageBox.Show(ex.Message + "\n\n" + ex);
            }
        }

        private void StopRecording()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (_capture == null)
                    return;

                _capture.Stop();

                _isRecording = false;

                if (_videoFileName != null)
                {
                    var toFlv = new ToFlvThread(_absolutePathFileName);
                    new Thread(toFlv.DoWork).Start();
                    
                    var configuration = ServicesContainer.GetService<NomadConfiguration>();

                    var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

                    var thumbnailFileName = GetJpgFileName(_videoFileName);

                    new FfmpegThumbnailsGenerator().GenerateThumbnail(_absolutePathFileName, //configuration.PicturesDir + 
                        thumbnailFileName, 2);

                    var question = questionnaire.GetQuestion(18);
                    var choiceText = (ChoiceText)question.GetChoice(1);

                    //choiceText.SetAnswer(new Answer(11, 18, 1, thumbnailFileName));
                }

                _capture.Dispose();
                _capture = null;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private class ToFlvThread
        {
            readonly string _absolutePathFileName;

            public ToFlvThread(string absolutePathFileName)
            {
                _absolutePathFileName = absolutePathFileName;

            }

            public void DoWork()
            {

                lock (_lock)
                {
                    var configuration = ServicesContainer.GetService<NomadConfiguration>();

                    var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

                    var question = questionnaire.GetQuestion(17);
                    var choiceText = (ChoiceText)question.GetChoice(1);

                    var flvFileName = GetFlvVideoName(_absolutePathFileName);

                    FfmpegUtil.AviToFlv(_absolutePathFileName, flvFileName, configuration.FfmpegLocation);

                    //choiceText.SetAnswer(new Answer(7, 17, 1, StringUtil.GetFileName(flvFileName)));

                    try
                    {
                        File.Delete(_absolutePathFileName);
                    }
                    catch
                    {
                        // die noce
                    }
                }
            }

            private static string GetFlvVideoName(string videoName)
            {
                var value = videoName.Substring(0, videoName.LastIndexOf(".", StringComparison.Ordinal));

                return value + ".f4v";
            }

           
        }

        private static string GetJpgFileName(string fileName)
        {
            var value = fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal));

            return value + ".jpg";
        }
        
    }
}
