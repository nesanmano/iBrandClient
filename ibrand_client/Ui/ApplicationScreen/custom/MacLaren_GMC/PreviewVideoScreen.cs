using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.Multimedia.Cli.Ffmpeg;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.MacLaren_GMC
{
    /// <summary>
    /// Classe de presentation video.
    /// http://www.aforgenet.com/framework/docs/html/bd272e5f-344c-56a5-f74f-1ac3062a9554.htm
    /// </summary>
    public class PreviewVideoScreen : PreviewScreenAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private FileVideoSource _videoSource;
        private VideoSourcePlayer _videoPlayer;

        private readonly Bitmap _dotWhiteImage;
        private readonly Bitmap _dotBlackImage;
        //private readonly Bitmap _hourGlass;

        protected readonly Rectangle _playArea;
        protected readonly Rectangle _pauseArea;

        private static readonly int STARTING_DOT_POSITION = 264;
        private static readonly int DOT_POSITION_Y = 570;
        private int _dotWhitePos = STARTING_DOT_POSITION;

        private double _totalFrame;
        private double _showedFrame;
        private double _showedFrameWhenPaused = -1;

        private double _cutTime;
        private double _videoFps;

        private bool _saveNextFrameAndCutTime;

        private bool _isRunning;

        //private bool _showHourGlass;

        private readonly Bitmap _keeperButtonEnable;
        private readonly Bitmap _cutButtonDisable;
        private readonly Bitmap _playButtonDisable;

        private readonly int _previewY;
        private readonly int _previewX;
        private readonly Size _previewSize;

        public PreviewVideoScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, Rectangle playZone, Rectangle pauseZone, Size previewSize, int previewYposition, int previewXposition)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, cancelZone, nextZone, backZone, previewYposition,  previewSize)
        {

            _playArea = playZone;
            _pauseArea = pauseZone;

            _dotWhiteImage = GetNoneLanguageBitmap("dot.png");
            _dotBlackImage = GetNoneLanguageBitmap("dot.png");

            _keeperButtonEnable = GetNoneLanguageBitmap("keeper_enable.png");

            _cutButtonDisable = GetNoneLanguageBitmap("cut_disable.png");

            _playButtonDisable = GetNoneLanguageBitmap("play_disable.png");

            //PictureBox.Paint += PaintEvent;

            _previewY = previewYposition;
            _previewX = previewXposition;
            _previewSize = previewSize;
        }

        private void AddVideoPreviewControl(Size previewSize)
        {
            _videoPlayer = new VideoSourcePlayer()
            {
                Size = previewSize,
                Visible = true,
                Location = new Point(_previewX, _previewY)
            };

            _videoPlayer.PlayingFinished += OnVideoFinished;

            Controls.Add(_videoPlayer);
            _videoPlayer.BringToFront();
        }

        protected override void AddPreviewControl(Size previewSize, Point location)
        {
            //nothing
        }

        /*
        public new void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (_videoPlayer != null && _videoSource != null)
            {

                if (_isRunning)
                {
                    if (_dotWhiteImage != null)
                    {
                        if (_dotWhitePos > 0)
                            e.Graphics.DrawImageUnscaled(_dotWhiteImage, _dotWhitePos, DOT_POSITION_Y);
                    }

                    e.Graphics.DrawImageUnscaled(_playButtonDisable, 38, 216);
                }
                else
                {
                    e.Graphics.DrawImageUnscaled(_cutButtonDisable, 38, 383);
                }
            }

            if (_cutTime != 0)
            {
                e.Graphics.DrawImageUnscaled(_keeperButtonEnable, 1043, 364);
            }

            if (_showedFrameWhenPaused != -1)
            {
                if (_dotBlackImage == null) return;
                var dotBlackPos = (int)((_showedFrameWhenPaused / _totalFrame * 751) + STARTING_DOT_POSITION);
                e.Graphics.DrawImageUnscaled(_dotBlackImage, dotBlackPos, DOT_POSITION_Y);
            }
        }
        */

        private void OnVideoFinished(object sender, ReasonToFinishPlaying reason)
        {
            _dotWhitePos = STARTING_DOT_POSITION;
            _showedFrame = 0;
            //_showedFrameWhenPaused = -1;
            _isRunning = false;
            if (InvokeRequired)
            {
                Invoke(new Action(Refresh));
                return;
            }

            Refresh();
        }
        
        private void CleanVideoPlayer()
        {
            if (_videoSource != null)
            {
                if (_videoSource.IsPaused)
                    _videoSource.SignalToUnPause();

                _videoSource.SignalToStop();
                _videoSource.VideoSourceError -= VideoError;
                _videoSource.NewFrame -= NewVideoFrame;
                _videoSource = null;
            }

            if (_videoPlayer != null)
            {
                Controls.Remove(_videoPlayer);
                _videoPlayer.PlayingFinished -= OnVideoFinished;
                _videoPlayer.Dispose();
                _videoPlayer = null;
            }

            _dotWhitePos = STARTING_DOT_POSITION;
            _totalFrame = 0;
            _showedFrame = 0;
            _showedFrameWhenPaused = -1;
            _cutTime = 0;
            _isRunning = false;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            AddVideoPreviewControl(_previewSize);

            if (_screenConfig.QuestionVideoNo == null) return;
            var fileSource = GetTextAnswerFromQuestion(_screenConfig.QuestionVideoNo.Value, 1);

            if (File.Exists(fileSource))
            {
                var conf = ServicesContainer.GetService<NomadConfiguration>();

                try {
                    var videoMetadata = FfmpegUtil.GetVideoFileMetadata(conf.FfprobeLocation, new FileInfo(fileSource));

                    throw new System.Exception("Refactor to new FFMPEG specs.");
                    
                    _videoFps = 0d;

                    _totalFrame = 0d;

                    var source = new FileVideoSource(fileSource);
                    if (_videoSource?.Source != null && _videoSource.Source.Equals(fileSource))
                    {
                        _isRunning = true;
                        _videoPlayer.Start();
                        return;
                    }

                    if (_videoSource != null && _videoSource.IsRunning)
                        _videoSource.Stop();

                    _videoSource = source;
                    _videoSource.VideoSourceError += VideoError;
                    _videoSource.NewFrame += NewVideoFrame;
                    _videoPlayer.VideoSource = _videoSource;
                }catch(System.Exception ex)
                {
                    log.Error(ex, "No video file found");
                    Back();
                }
            }
            else
            {
                log.Error("No video file found: {0}", fileSource);
                Invoke(new Action(() =>
                {
                    MessageBox.Show(Resources.iBrand.Copyright, Resources.iBrand.NoVideoSource);
                }));
            }
        }

        private void NewVideoFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _showedFrame++;
            
            _dotWhitePos = (int)((_showedFrame / _totalFrame * 751) + STARTING_DOT_POSITION);

            if (_saveNextFrameAndCutTime)
            {
                _cutTime = _showedFrame / _videoFps;

                _saveNextFrameAndCutTime = false;
            }

            if (InvokeRequired)
            {
                Invoke(new Action(Refresh));
                return;
            }

            Refresh();
        }

        private void VideoError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            log.Error("Error during video parsing: {0}", eventArgs.Description);

            if (eventArgs.Description.Contains("output video pin"))
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(Resources.iBrand.Copyright, @"Install a codec pack to display videos.");
                }));
            }
        }

        protected override void ActionOnNext()
        {
            if (_cutTime == 0) return;
            var totalTime = _totalFrame / _videoFps;
            if (( totalTime - _cutTime < 2) || _cutTime < 2)
            {
                MessageBox.Show(this, @"Cut Time Should Be at least 2 sec after video started And 2 sec from the end of the video.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (_videoSource != null && _videoSource.IsPaused)
            {
                _videoSource.SignalToUnPause();
            }
            _videoPlayer?.SignalToStop();

               
            if (_screenConfig.QuestionCutTimeNo != null)
            {
                //SaveAnswerToQuestion(_screenConfig.QuestionCutTimeNo.Value, 1, _cutTime.ToString(CultureInfo.InvariantCulture));
            }

            //CutVideo();

            CleanVideoPlayer();

            base.ActionOnNext();
        }
        
        protected override void ActionOnBack()
        {
            CleanVideoPlayer();

            base.ActionOnBack();
        }

        protected override void ActionOnClose()
        {
            CleanVideoPlayer();
            base.ActionOnClose();
        }

        protected override void PrepareToClose()
        {
            //Il faut absolument stopper le video si on ferme l'application a partir de la page.
            CleanVideoPlayer();
            Recycle();
        }
        /*
        protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (_activated)
            {
                base.MouseDownHandler(sender, e);

                ResetTimer();

                if (_flagForClosing) return;

                if (InArea(e.X, e.Y, _playArea))
                {
                    if (_videoSource != null && _videoSource.IsPaused)
                    {
                        _videoSource.SignalToUnPause();
                        _isRunning = true;
                        Refresh();
                    }
                    else if (_videoPlayer != null && !_videoPlayer.IsRunning)
                    {
                        _showedFrame = 0;
                        _dotWhitePos = STARTING_DOT_POSITION;
                        _cutTime = 0;
                        _showedFrameWhenPaused = -1;
                        _isRunning = true;
                        Refresh();
                        _videoPlayer.Start();

                    }
                }
                else if (InArea(e.X, e.Y, _pauseArea))
                {
                    if (_videoPlayer != null)
                    {
                        _isRunning = false;
                        _videoSource.SignalToPause();
                        _showedFrameWhenPaused = _showedFrame;
                        _saveNextFrameAndCutTime = true;
                        Refresh();
                    }
                }
              
            }
        }
        */
    }
}
