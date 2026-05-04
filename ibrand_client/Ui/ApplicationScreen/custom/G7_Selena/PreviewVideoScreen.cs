using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.G7_Selena
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
        private readonly Bitmap _hourGlass;

        protected readonly Rectangle _playArea;
        protected readonly Rectangle _pauseArea;
        protected readonly Rectangle _optInArea;

        private static readonly int STARTING_DOT_POSITION = 303;
        private int _dotWhitePos = STARTING_DOT_POSITION;

        private double _totalFrame;
        private double _showedFrame;
        private double _showedFrameWhenPaused = -1;

        private double _cutTime;
        private double _videoFps;

        private string _firstPictureFile;

        private bool _saveNextFrameAndCutTime;

        private bool _isRunning = false;

        private bool _showHourGlass;

        private readonly Bitmap _keeperButtonEnable;
        private readonly Bitmap _cutButtonDisable;
        private readonly Bitmap _playButtonDisable;
        private readonly Bitmap _check;
        private readonly Bitmap _overlay;

        private readonly int _previewY;
        private readonly int _previewX;
        private readonly Size _previewSize;

        private bool _opt_in_checked = false;

        public PreviewVideoScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, Rectangle playZone, Rectangle pauseZone, Size previewSize, int previewYposition, int previewXposition)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, cancelZone, nextZone, backZone, previewYposition,  previewSize)
        {

            _playArea = playZone;
            _pauseArea = pauseZone;

            _optInArea = new Rectangle(26, 709, 80, 80);

            _dotWhiteImage = GetNoneLanguageBitmap("dot_cut.png");
            _dotBlackImage = GetNoneLanguageBitmap("dot_cut.png");
            _hourGlass = GetNoneLanguageBitmap("hourglass_black.png");
            _overlay = GetNoneLanguageBitmap("overlay.png");

            _keeperButtonEnable = GetNoneLanguageBitmap("keeper_button_enable.png");

            _cutButtonDisable = GetNoneLanguageBitmap("cut_button_disable.png");

            _playButtonDisable = GetNoneLanguageBitmap("play_button_disable.png");

            _check = GetNoneLanguageBitmap("check.png");

            PictureBox.Paint += PaintEvent;

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

        public new void PaintEvent(object sender, PaintEventArgs e)
        {
            //base.PaintEvent(sender, e);

            

            if (_opt_in_checked)
            {
                e.Graphics.DrawImageUnscaled(_check, 41, 714);
            }

            if (_videoPlayer != null && _videoSource != null)
            {

                if (_isRunning)
                {
                    if (_dotWhiteImage != null)
                    {
                        if (_dotWhitePos > 0)
                            e.Graphics.DrawImageUnscaled(_dotWhiteImage, _dotWhitePos, 590);
                    }

                    e.Graphics.DrawImageUnscaled(_playButtonDisable, 456, 632);
                }
                else
                {
                    e.Graphics.DrawImageUnscaled(_cutButtonDisable, 652, 632);
                }
            }

            if (_cutTime != 0)
            {
                e.Graphics.DrawImageUnscaled(_keeperButtonEnable, 1018, 410);
            }

            if (_showedFrameWhenPaused != -1)
            {
                if (_dotBlackImage != null)
                {
                    var dotBlackPos = (int)((_showedFrameWhenPaused / _totalFrame * 688) + STARTING_DOT_POSITION);
                    e.Graphics.DrawImageUnscaled(_dotBlackImage, dotBlackPos, 590);
                }
            }

            if (_showHourGlass)
            {
                if (_hourGlass != null)
                    e.Graphics.DrawImageUnscaled(_hourGlass, 1101, 445);
            }

        }

        private void OnVideoFinished(object sender, ReasonToFinishPlaying reason)
        {
            _dotWhitePos = STARTING_DOT_POSITION;
            _showedFrame = 0;
            //_showedFrameWhenPaused = -1;
            _isRunning = false;
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    Refresh();
                }));
                return;
            }

            Refresh();
        }

        public override void Recycle()
        {
            base.Recycle();

            _opt_in_checked = false;
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
            _firstPictureFile = null;
            _cutTime = 0;
            _isRunning = false;
            _showHourGlass = false;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            AddVideoPreviewControl(_previewSize);
            
            if (_screenConfig.QuestionVideoNo != null)
            {
                var fileSource = new FileInfo(GetTextAnswerFromQuestion(_screenConfig.QuestionVideoNo.Value));

                if (fileSource.Exists)
                {
                    try {
                        var videoMetadata = FfmpegUtil.GetVideoFileMetadata(null, fileSource);
                        var streamData = videoMetadata.GetMainVideoStreamData();
                        
                        _videoFps = streamData.FrameRate.DoubleValue;

                        _totalFrame = streamData.FrameCount;

                        var source = new FileVideoSource(fileSource.FullName);
                        if (_videoSource?.Source != null && _videoSource.Source.Equals(fileSource.FullName))
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
                        log.Error("No video file found", ex);
                        /*Invoke(new Action(() =>
                        {
                            MessageBox.Show(Resources.iBrand.Copyright, Resources.iBrand.NoVideoSource);
                        }));*/
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
        }

        private void NewVideoFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _showedFrame++;

            //test = showed_frame / total_frame;
            _dotWhitePos = (int)((_showedFrame / _totalFrame * 688) + STARTING_DOT_POSITION);

            if (_saveNextFrameAndCutTime)
            {
                if (_firstPictureFile != null && File.Exists(_firstPictureFile))
                    File.Delete(_firstPictureFile);

                _firstPictureFile = GetTerminalDirectory(
                                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                        DirectoryUtil.ProjectDataDirectoriesModEnum.none)
                                    + GenerateUniqueId.GetId() + ".jpg";

                eventArgs.Frame.Save(95, _firstPictureFile);

                _cutTime = _showedFrame / _videoFps;

                _saveNextFrameAndCutTime = false;
            }

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    Refresh();
                }));
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
            if (_cutTime != 0)
            {
                if (!_opt_in_checked)
                {
                    new MessageDialog().ShowMessage("You must check opt-in to continue.");
                    return;
                }

                _showHourGlass = true;
                Refresh();

                if (_videoSource != null && _videoSource.IsPaused)
                {
                    _videoSource.SignalToUnPause();
                }
                _videoPlayer?.SignalToStop();

                if (_screenConfig.QuestionFirstPictureNo != null)
                {
                    if (_firstPictureFile != null)
                    {
                        var firstPictureFile = GetTerminalDirectory(
                                                   DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                   DirectoryUtil.ProjectDataDirectoriesModEnum.none)
                                               + GenerateUniqueId.GetId() + ".jpg";
                        var temp = FileUtil.LoadBitmap(_firstPictureFile);
                        temp.AddOverlay(_overlay);
                        temp.Save(95, firstPictureFile);
                        
                        //SaveAnswerToQuestion(_screenConfig.QuestionFirstPictureNo.Value, 1, firstPictureFile);
                        temp.Dispose();
                    }
                    else
                        return;//TODO show message...
                }

                if (_screenConfig.QuestionCutTimeNo != null)
                {
                    //SaveAnswerToQuestion(_screenConfig.QuestionCutTimeNo.Value, 1, _cutTime.ToString());
                }

                //CutVideo();

                CleanVideoPlayer();

                base.ActionOnNext();
            }
        }

        /*private void CutVideo()
        {
            var videoFileName = GetTextAnswerFromQuestion(_screen_config.QuestionVideoNo.Value, 1);

            var start = double.Parse(GetTextAnswerFromQuestion(3, 1));

            var cutVideoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + "_cut.mp4";

            VideoMetadata videoMetadata = FfmpegUtil.GetVideoFileMetadata(videoFileName);

            double endTime = start + 2;
            double totalSec = videoMetadata.Duration.TotalMilliseconds / (double)1000;
            if (totalSec < endTime)
                endTime = start + (totalSec - start);

            //Tranformation du video
            var config = ServicesContainer.GetService<NomadConfiguration>();
            FfmpegUtil.CutVideo(config.FfmpegLocation, videoFileName, cutVideoFileName, start, endTime);

            SaveAnswerToQuestion(5, 1, cutVideoFileName);
        }*/

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
            Recycle();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (_activated)
        //    {
        //        base.MouseDownHandler(sender, e);

        //        ResetTimer();

        //        if (_flagForClosing) return;

        //        if (InArea(e.X, e.Y, _playArea))
        //        {
        //            if (_videoSource != null && _videoSource.IsPaused)
        //            {
        //                _videoSource.SignalToUnPause();
        //                _isRunning = true;
        //                Refresh();
        //            }
        //            else if (_videoPlayer != null && !_videoPlayer.IsRunning)
        //            {
        //                _showedFrame = 0;
        //                _dotWhitePos = STARTING_DOT_POSITION;
        //                _cutTime = 0;
        //                _showedFrameWhenPaused = -1;
        //                _isRunning = true;
        //                Refresh();
        //                _videoPlayer.Start();

        //            }
        //        }
        //        else if (InArea(e.X, e.Y, _pauseArea))
        //        {
        //            if (_videoPlayer != null)
        //            {
        //                _isRunning = false;
        //                _videoSource.SignalToPause();
        //                _showedFrameWhenPaused = _showedFrame;
        //                _saveNextFrameAndCutTime = true;
        //            }
        //        }
        //        else if (InArea(e.X, e.Y, _optInArea))
        //        {
        //            if (_opt_in_checked)
        //                _opt_in_checked = false;
        //            else
        //                _opt_in_checked = true;
        //            Refresh();
        //        }
        //    }
        //}
    }
}
