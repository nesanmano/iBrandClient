using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DirectX.Capture;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;

//using DirectX.Capture;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class RecordScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly object _lock = new object();

        private readonly Panel _panelVideo;

        private readonly PictureBox _panelThanks;

        private readonly PictureBox _stopButtonGrey;

        //private int time_before_record = 3; //seconds
        private int _timeToRecord = 30; //seconds

        private readonly Timer _timer;

        private string _absolutePathFileName;
        private string _videoFileName;

        private Capture _capture;
        private Filters _filters;

        private readonly List<Bitmap> _beforeRecordCountDownList;
        private readonly List<Bitmap> _recordCountDownList;

        readonly Rectangle _startStopArea;
        readonly Rectangle _doneArea;

        readonly Bitmap _recordingImage;
        readonly Bitmap _stopButton;
        readonly Bitmap _doneEnabledButton;

        readonly Bitmap _question1;
        readonly Bitmap _question2;
        readonly Bitmap _question3;

        int _questionNo = -1;
        bool _isRecording;
        bool _doneEnabled;
        bool _stopEnabled;
        bool _isStoped;

        public RecordScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            IsSaveOnThisPage = true;
            
            _panelVideo = new Panel();
            _panelVideo.Location = new Point(328, 195);
            _panelVideo.Name = "panel_video";
            _panelVideo.Size = new Size(640, 360);

            _panelVideo.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom)
                                  | AnchorStyles.Left)
                                 | AnchorStyles.Right;
            _panelVideo.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(_panelVideo);

            _panelVideo.BringToFront();

            _panelThanks = new PictureBox();
            _panelThanks.Location = new Point(328, 195);
            _panelThanks.Name = "panel_thanks";
            _panelThanks.Size = new Size(640, 360);

            var bitmap = GetNoneLanguageBitmap("thanks.jpg");
            if (bitmap != null) {
                _panelThanks.Image = bitmap;
            }
           
            Controls.Add(_panelThanks);

            _panelThanks.Visible = false;

            _stopButtonGrey = new PictureBox();
            _stopButtonGrey.Enabled = false;
            _stopButtonGrey.Location = new Point(460, 646);
            _stopButtonGrey.Name = "stop_button_grey";
            _stopButtonGrey.Size = new Size(187, 60);

            bitmap = GetNoneLanguageBitmap("stop_button_grey.png");
            if (bitmap != null)
            {
                _stopButtonGrey.Image = bitmap;
            }

            Controls.Add(_stopButtonGrey);

            _stopButtonGrey.Visible = false;

            _timer = new Timer();
            _timer.Interval = 1000; //1 sec
            _timer.Tick += OnTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _beforeRecordCountDownList = new List<Bitmap>(3);
            for (var i = 1; i <= 3; i++)
            {
                _beforeRecordCountDownList.Add(GetNoneLanguageBitmap(i + ".png"));
            }

            _recordCountDownList = new List<Bitmap>(30);
            for (var i = 1; i <= 30; i++)
            {
                _recordCountDownList.Add(GetNoneLanguageBitmap(i + ".png"));
            }

            _startStopArea = new Rectangle(new Point(460, 647), new Size(191, 60));
            _doneArea = new Rectangle(new Point(656, 647), new Size(191, 60));

            _recordingImage = GetNoneLanguageBitmap("recording.png");

            _stopButton = GetNoneLanguageBitmap("stop_button.png");

            _doneEnabledButton = GetNoneLanguageBitmap("done_button.png");

            _question1 = GetNoneLanguageBitmap("question_1.png");

            _question2 = GetNoneLanguageBitmap("question_2.png");

            _question3 = GetNoneLanguageBitmap("question_3.png");

            try
            {
                _filters = new Filters();
            }
            catch (System.Exception)
            {
                _filters = null;
                MessageBox.Show(this, "Web Cam not connected.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _doneArea) && _doneEnabled)
        //    {
        //        Next();
        //    }
        //    if (InArea(e.X, e.Y, _startStopArea))
        //    {
        //        if (!_stopEnabled && !_isStoped)
        //        {
        //            //time_before_record = 3;

        //            _panelThanks.Visible = false;

        //            _panelVideo.BringToFront();

        //            StartRecording();

        //            _timer.Start();

        //            _stopEnabled = true;
        //            _doneEnabled = false;
        //        }
        //        else
        //        {
        //            if (!_isStoped)
        //            {
        //                _timer.Stop();

        //                StopRecording();

        //                InitCaptureDevice();

        //                //time_before_record = 3;

        //                _isRecording = false;
        //                _stopEnabled = false;
        //                _doneEnabled = true;

        //                _timeToRecord = 30;

        //                _stopButtonGrey.Visible = true;
        //                _stopButtonGrey.BringToFront();
        //            }
        //        }
        //        ResetTimer();
        //        Refresh();
        //    }

        //}

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);
            

        //        if (_timeToRecord != 0 && _isRecording)
        //        {
        //            if (_recordingImage != null)
        //                e.Graphics.DrawImageUnscaled(_recordingImage, new Point(473, 565));
                    
        //            var bitmap = _recordCountDownList[_timeToRecord - 1];
        //            if (bitmap != null)
        //                e.Graphics.DrawImageUnscaled(bitmap, new Point(760, 567));
        //        }


        //    if (_doneEnabled)
        //    {
        //        if (_doneEnabledButton != null)
        //            e.Graphics.DrawImageUnscaled(_doneEnabledButton, new Point(656, 647));
        //    }

        //    if (_stopEnabled)
        //    {
        //        if (_stopButton != null)
        //            e.Graphics.DrawImageUnscaled(_stopButton, new Point(461, 647));
        //    }

        //    if (_questionNo == 1)
        //    {
        //        if (_question1 != null)
        //            e.Graphics.DrawImageUnscaled(_question1, new Point(0, 30));
        //    }
        //    else if (_questionNo == 2)
        //    {
        //        if (_question2 != null)
        //            e.Graphics.DrawImageUnscaled(_question2, new Point(0, 30));
        //    }
        //    else if (_questionNo == 3)
        //    {
        //        if (_question3 != null)
        //            e.Graphics.DrawImageUnscaled(_question3, new Point(0, 30));
        //    }

        //}

        public override void Recycle()
        {
            base.Recycle();

            _timer.Stop();

            _absolutePathFileName = null;
            _videoFileName = null;

            _questionNo = -1;

            _timeToRecord = 30;

            _isRecording = false;
            _doneEnabled = false;
            _stopEnabled = false;
            _isStoped = false;

            _stopButtonGrey.Visible = false;
            _panelThanks.Visible = false;

            _panelVideo.BringToFront();
        }

        private string GetAbsolutePathVideoName(string videoName)
        {
            var newVideoName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + _videoFileName;

            return File.Exists(newVideoName) ? null : newVideoName;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            
            if (!InitCaptureDevice())
            {
                MessageBox.Show(this, "Web Cam not connected.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Back();
                return;
            }

            var rnd = new Random();
            _questionNo = rnd.Next(1, 4);

        }

        public void OnTimerEventt(object sender, EventArgs e)
        {
            /*if (time_before_record != 0)
            {
                time_before_record--;
                this.Refresh();
            }
            else
            {*/
                if (_timeToRecord != 0)
                {
                    if (_timeToRecord == 30)
                    {
                        _isRecording = true;

                        Refresh();
                    }

                    _timeToRecord--;
                }
                else
                {
                    _timer.Stop();

                    StopRecording();

                    InitCaptureDevice();

                    //time_before_record = 3;

                    _isRecording = false;
                    _stopEnabled = false;
                    _doneEnabled = true;

                    _timeToRecord = 30;

                    ResetTimer();

                    _stopButtonGrey.Visible = true;
                    _stopButtonGrey.BringToFront();
                }

                Refresh();
           // }
           
        }

        private bool InitCaptureDevice()
        {
            Filter videoDevice = null;
            Filter audioDevice = null;
            try
            {

                if (_filters == null)
                    _filters = new Filters();

                var config = ServicesContainer.GetService<NomadConfiguration>();

                Filter f;
                for (var c = 0; c < _filters.VideoInputDevices.Count; c++)
                {
                    f = _filters.VideoInputDevices[c];
                    if (f.Name.Contains(config.WebCamName))
                    {
                        videoDevice = f;
                        break;
                    }
                }

                for (var c = 0; c < _filters.AudioInputDevices.Count; c++)
                {
                    
                    f = _filters.AudioInputDevices[c];
                    log.Debug("YO :" + f.Name);
                    if (f.Name.Contains(config.AudioInputName))
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
                        //capture.DisposeCapture();
                        _capture = null;
                    }

                    _capture = new Capture(videoDevice, audioDevice/*, false*/);
                    //capture.RecFileMode = DirectX.Capture.Capture.RecFileModeType.Wmv;
                    _capture.FrameSize = new Size(640, 360);

                    do
                    {
                        _videoFileName = GenerateUniqueId.GetId() + ".avi";
                        _absolutePathFileName = GetAbsolutePathVideoName(_videoFileName);
                    } while (_absolutePathFileName == null);

                    _capture.Filename = _absolutePathFileName;

                    _capture.PreviewWindow = _panelVideo;
                }
            }
            catch (System.Exception)
            {
                return false;
            }

            if (videoDevice == null || audioDevice == null)
                return false;

            return true;
        }

        private void StartRecording()
        {
            try
            {
                if (_capture == null)
                    return;

                _capture.Start();

            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message + "\n\n" + ex);
                log.Error(ex.StackTrace);
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
                _isStoped = true;

                if (_videoFileName != null)
                {
                    //ToFlvThread to_flv = new ToFlvThread(_absolute_path_file_name);
                    //new Thread(to_flv.DoWork).Start();
                    
                    //var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

                    //var question = questionnaire.GetQuestion(3);
                    //var choiceCombo = (ChoiceCombo)question.GetChoice(1);
                    //choiceCombo.SetAnswer(_questionNo);

                    //question = questionnaire.GetQuestion(4);
                    //var choiceText = (ChoiceText)question.GetChoice(1);

                    //choiceText.SetAnswer(new Answer(4, 4, 1, _videoFileName));
                }

                _panelThanks.Visible = true;

                _panelThanks.BringToFront();

                _capture.Dispose();
                //capture.DisposeCapture();
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
                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

                    var question = questionnaire.GetQuestion(4);
                    var choiceText = (ChoiceText)question.GetChoice(1);

                    var flvFileName = GetFlvVideoName(_absolutePathFileName);

                    FfmpegUtil.AviToFlv(_absolutePathFileName, flvFileName, config.FfmpegLocation);

                    //choiceText.SetAnswer(new Answer(4, 4, 1, StringUtil.GetFileName(flvFileName)));

                    try
                    {
                        File.Delete(_absolutePathFileName);
                    }
                    catch
                    {
                    }
                }
            }

            private string GetFlvVideoName(string videoName)
            {
                var value = videoName.Substring(0, videoName.LastIndexOf("."));

                return value + ".f4v";
            }


        }

        private string GetJpgFileName(string fileName)
        {
            var value = fileName.Substring(0, fileName.LastIndexOf("."));

            return value + ".jpg";
        }

    }
}
