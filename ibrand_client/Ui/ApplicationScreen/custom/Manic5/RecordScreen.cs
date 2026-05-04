using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class RecordScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly PictureBox _panelPreview;

        private int _timeBeforeRecord = 3; //seconds
        private int _timeToRecord = 30; //seconds

        private readonly Timer _timer;

        private string _videoFileName;
        
        private List<Bitmap> _beforeRecordCountDownList;
        private readonly List<Bitmap> _recordCountDownList;

        readonly Rectangle _startStopArea;
        readonly Rectangle _doneArea;

        readonly Bitmap _recordingImage;
        readonly Bitmap _transfertFile;
        readonly Bitmap _startDisabledButton;
        readonly Bitmap _startEnabledButton;
        readonly Bitmap _doneEnabledButton;
        readonly Bitmap _doneDisabledButton;

        bool _isRecording;
        bool _doneEnabled;
        bool _isStoped;
        bool _canSave;
        bool _startEnabled = true;

        private readonly Point _startStopLocation = new Point((1920/2) - 435, 900);
        private readonly Point _doneLocation = new Point((1920 / 2) - 65, 900);

        public RecordScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _panelPreview = new PictureBox
            {
                Location = new Point((1920 - 600)/2, 100),
                Size = new Size(600, 600),
                BackColor = Color.White
            };

            Controls.Add(_panelPreview);

            _panelPreview.BringToFront();

            _timer = new Timer {Interval = 1000};
            _timer.Tick += OnTimerEventRecord;

            //PictureBox.Paint += PaintEvent;

            _beforeRecordCountDownList = new List<Bitmap>(3);

            _recordCountDownList = new List<Bitmap>(30);
            for (var i = 1; i <= 30; i++)
            {
                _recordCountDownList.Add(GetNoneLanguageBitmap(i + ".png"));
            }

            _beforeRecordCountDownList = _recordCountDownList.GetRange(0, 3);

            _startStopArea = new Rectangle(new Point(600, 912), new Size(350, 98));
            _doneArea = new Rectangle(new Point(970, 912), new Size(350, 98));

            _recordingImage = GetNoneLanguageBitmap("enregistrement.png");
            
            _startDisabledButton = GetNoneLanguageBitmap("begin_off.png");
            _startEnabledButton = GetNoneLanguageBitmap("begin_on.png");
            _doneEnabledButton = GetNoneLanguageBitmap("stop_on.png");
            _doneDisabledButton = GetNoneLanguageBitmap("stop_off.png");
            _transfertFile = GetNoneLanguageBitmap("transfer.png");

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed && _activated)
            {
                UpdatePicture(e.GetImage());
            }
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (IsDisposed || !_activated) return;
            try
            {
                if (!_canSave) return;

                _videoFileName = GetTerminalDirectory(
                    DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                    DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + ".mp4";

                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                FfmpegUtil.CompressVideo(configuration.FfmpegLocation, e.FilePath, _videoFileName, 24);

                //SaveAnswerToQuestion(3, 1, _videoFileName);

                FinishToDownloadFile();
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                try
                {
                    File.Delete(e.FilePath);
                }
                catch (System.Exception exception)
                {
                    log.Error(exception, "Die Nice. Could not destroy original video file.");
                }
            }
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (IsDisposed || !_activated) return;
            log.Debug("New file on camera detected.");
            if (!_canSave) return;
            var dir = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);

            var fileName = dir + GenerateUniqueId.GetId() + ".MOV";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            try
            {
                camera.SavePicture(dirItem, fileName);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }
        
        private void UpdatePicture(Image image)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePicture(image)));
            }
            else
            {
                var finalBitmap = image.Resize(_panelPreview.Width, _panelPreview.Height);
                finalBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                _panelPreview.Image = finalBitmap;
            }
        }
        
        private void FinishToDownloadFile()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(FinishToDownloadFile));
            }
            else
            {
                Next();
            }
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (_startEnabled && !_isStoped && InArea(e.X, e.Y, _startStopArea))
        //    {
        //        CanonCameraManager.GetInstance().StartRecord();
        //        _timer.Start();
        //        _startEnabled = false;
        //        _doneEnabled = true;
        //        Refresh();
        //    }
        //    else if (_doneEnabled && !_isStoped && InArea(e.X, e.Y, _doneArea))
        //    {
        //        _timer.Stop();
        //        _doneEnabled = false;
        //        _canSave = true;
        //        StopRecording();
        //        Refresh();
        //    }
        //}

        private void StopRecording()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                CanonCameraManager.GetInstance().StopRecord();

                _isRecording = false;
                _isStoped = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + @"\n\n" + ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);
        //    var numberPos = new Point((1920 - 88) / 2, 750);
        //    if (_timeBeforeRecord != 0)
        //    {
        //        var bitmap = _recordCountDownList[_timeBeforeRecord - 1];
        //        if (bitmap != null)
        //            e.Graphics.DrawImageUnscaled(bitmap, numberPos);
        //    }

        //    if (_timeToRecord != 0 && _isRecording)
        //    {
        //        if (_recordingImage != null)
        //            e.Graphics.DrawImageUnscaled(_recordingImage, new Point((1920 - 500) / 2, 800));

        //        var bitmap = _recordCountDownList[_timeToRecord - 1];
        //        if (bitmap != null)
        //            e.Graphics.DrawImageUnscaled(bitmap, numberPos);
        //    }

        //    if (_isStoped)
        //    {
        //        if (_transfertFile != null)
        //            e.Graphics.DrawImageUnscaled(_transfertFile, new Point((1920 - 1000) / 2, 800));
        //    }

        //    if (_doneEnabled)
        //    {
        //        if (_doneEnabledButton != null)
        //            e.Graphics.DrawImageUnscaled(_doneEnabledButton, _doneLocation);
        //    }
        //    else
        //    {
        //        if (_doneDisabledButton != null)
        //            e.Graphics.DrawImageUnscaled(_doneDisabledButton, _doneLocation);
        //    }

        //    if (_startEnabled)
        //    {
        //        if (_startEnabledButton != null)
        //            e.Graphics.DrawImageUnscaled(_startEnabledButton, _startStopLocation);
        //    }
        //    else
        //    {
        //        if (_startDisabledButton != null)
        //            e.Graphics.DrawImageUnscaled(_startDisabledButton, _startStopLocation);
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();

            _timer.Stop();

            _videoFileName = null;
            
            _timeBeforeRecord = 3;
            _timeToRecord = 30;

            _isRecording = false;
            _doneEnabled = false;
            _isStoped = false;
            _startEnabled = true;
            _canSave = false;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            
            _panelPreview.BringToFront();
            
            if (_panelPreview.Image != null)
            {
                _panelPreview.Image = null;
            }
        }

        private void OnTimerEventRecord(object sender, EventArgs e)
        {
            if (_timeBeforeRecord != 0)
            {
                _timeBeforeRecord--;

                if (_timeBeforeRecord == 0)
                    _doneEnabled = true;
                Refresh();
            }
            else
            {
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

                    _canSave = true;
                    
                    StopRecording();

                    _isRecording = false;
                    _doneEnabled = false;
                }

                Refresh();
            }

        }

        // On termine en sauvant ce qui a été fait si on timeout sur cette page.
        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            if (_activated)
            {
                End();
            }
        }
    }
}
