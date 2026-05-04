using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HydroSense
{
    class HydroPreviewAndCutVideoScreen: PreviewScreenAbstract
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

        private readonly Rectangle _barZone;
        private int _dotWhitePos;

        private double _totalFrame;
        private double _showedFrame;
        private double _showedFrameWhenPaused = -1;

        private double _cutTime;
        private double _videoFps;

        private string _firstPictureFile;

        private bool _saveNextFrameAndCutTime;
        private readonly bool _canEdit;

        private bool _showHourGlass;

        private readonly Bitmap _saveActiveImage;

        private readonly int _frontTop;
        private readonly Size _previewSize;
        private string _totalTime;

        public HydroPreviewAndCutVideoScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, Rectangle playZone, Rectangle pauseZone, Rectangle barZone,
            int fromTop, Size previewSize, bool canEditVideo, IList<string> filenames = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc ?? backgroundImgEnSrc, sc, cancelZone, nextZone, backZone, fromTop, previewSize)
        {
            _canEdit = canEditVideo;
            _playArea = playZone;
            _pauseArea = pauseZone;
            _barZone = barZone;
            _dotWhitePos = _barZone.X;

            _dotWhiteImage = GetNoneLanguageBitmap(StringUtil.GetArrayValueWithDefault(filenames, 0, "dot_white.png"));
            _dotBlackImage = GetNoneLanguageBitmap(StringUtil.GetArrayValueWithDefault(filenames, 1, "dot_black.png"));
            _hourGlass = GetNoneLanguageBitmap(StringUtil.GetArrayValueWithDefault(filenames, 2, "hourglass_black.png"));

            _saveActiveImage = GetBitmap(StringUtil.GetArrayValueWithDefault(filenames, 3, "save_enable.png"));

            //PictureBox.Paint += PaintEvent;

            _frontTop = fromTop;
            _previewSize = previewSize;
        }

        private void AddVideoPreviewControl(Size previewSize, int fromTop)
        {
            _videoPlayer = new VideoSourcePlayer()
            {
                Size = previewSize,
                Visible = true,
                Location = new Point((PictureBox.Width - previewSize.Width) / 2, fromTop)
            };

            _videoPlayer.PlayingFinished += OnVideoFinished;

            Controls.Add(_videoPlayer);
            _videoPlayer.BringToFront();
        }

        protected override void AddPreviewControl(Size previewSize, Point location)
        {
            //nothing
        }

        public Point? HourglassPosition { get; set; }

        public Point? DisabledSavePosition { get; set; }

        public Point? CurrentTimePosition { get; set; }

        public Point? TotalTimePosition { get; set; }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_showHourGlass && HourglassPosition != null)
        //    {
        //        if (_hourGlass != null)
        //            e.Graphics.DrawImageUnscaled(_hourGlass, HourglassPosition.Value);
        //    }

        //    if (SaveInactiveDrawPoint != null && _saveActiveImage != null && _cutTime != 0)
        //    {
        //        e.Graphics.DrawImageUnscaled(_saveActiveImage, SaveInactiveDrawPoint.Value);
        //    }

        //    if (_videoPlayer != null && _videoSource != null)
        //    {

        //        if (_videoPlayer.IsRunning && !_videoSource.IsPaused)
        //        {
        //            if (_dotWhiteImage != null)
        //            {
        //                if (_dotWhitePos > 0)
        //                    e.Graphics.DrawImageUnscaled(_dotWhiteImage, _dotWhitePos, _barZone.Y);
        //            }
        //        }
        //    }

        //    if (CurrentTimePosition != null)
        //    {
        //        var time = (_showedFrame / _videoFps).ToString("F2", CultureInfo.InvariantCulture);
        //        e.Graphics.DrawString(time, VideoTimerFont, VideoTimerBrush, CurrentTimePosition.Value);
        //    }

        //    if (TotalTimePosition != null)
        //    {
        //        e.Graphics.DrawString(_totalTime, VideoTimerFont, VideoTimerBrush, TotalTimePosition.Value);
        //    }

        //    if (_showedFrameWhenPaused == -1) return;
        //    if (_dotBlackImage == null) return;

        //    var dotBlackPos = (int)((_showedFrameWhenPaused / _totalFrame * _barZone.Width) + _barZone.X);
        //    e.Graphics.DrawImageUnscaled(_dotBlackImage, dotBlackPos, _barZone.Y);
        //}

        public Font VideoTimerFont { get; set; } = new Font(FontFamily.GenericSansSerif, 15);

        public Brush VideoTimerBrush { get; set; } = new SolidBrush(Color.White);

        /// <summary>
        /// Point pour dessiner l'image d'activation de sauvegarde.
        /// </summary>
        public Point? SaveInactiveDrawPoint { get; set; }

        private void OnVideoFinished(object sender, ReasonToFinishPlaying reason)
        {
            _dotWhitePos = 280;
            _showedFrame = 0;
            _showedFrameWhenPaused = -1;
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

            _dotWhitePos = _barZone.X;
            _totalFrame = 0;
            _showedFrame = 0;
            _showedFrameWhenPaused = -1;
            _firstPictureFile = null;
            _cutTime = 0;
            _showHourGlass = false;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            AddVideoPreviewControl(_previewSize, _frontTop);

            if (_screenConfig.QuestionVideoNo == null) return;

            var fileSource = GetTextAnswerFromQuestion(_screenConfig.QuestionVideoNo.Value);

            try
            {
                if (File.Exists(fileSource))
                {
                    var videoMetadata = FfmpegUtil.GetVideoFileMetadata(null, new FileInfo(fileSource));

                    _videoFps = videoMetadata.Streams[0].FrameRate.DoubleValue;

                    _totalFrame = videoMetadata.Streams[0].FrameCount;

                    _totalTime = videoMetadata.Streams[0].DurationSeconds.ToString("F2", CultureInfo.InvariantCulture);

                    var source = new FileVideoSource(fileSource);
                    if (_videoSource?.Source != null && _videoSource.Source.Equals(fileSource))
                    {
                        _videoPlayer.Start();
                        return;
                    }

                    if (_videoSource != null && _videoSource.IsRunning)
                        _videoSource.Stop();

                    _videoSource = source;
                    _videoSource.VideoSourceError += VideoError;
                    _videoSource.NewFrame += NewVideoFrame;
                    _videoPlayer.VideoSource = _videoSource;


                    Refresh();
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
            catch (System.Exception e)
            {
                throw new System.Exception("Error during metadata reading of file source: " + fileSource, e);
            }
        }

        private void NewVideoFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _showedFrame++;
            ResetTimer();

            //test = showed_frame / total_frame;
            _dotWhitePos = (int)((_showedFrame / _totalFrame * _barZone.Width) + _barZone.X);

            if (_saveNextFrameAndCutTime && _canEdit)
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
                    //SaveAnswerToQuestion(_screenConfig.QuestionFirstPictureNo.Value, 1, _firstPictureFile);
                }
            }

            if (_screenConfig.QuestionCutTimeNo != null)
            {
                //SaveAnswerToQuestion(_screenConfig.QuestionCutTimeNo.Value, 1, _cutTime.ToString(CultureInfo.InvariantCulture));
            }

            if (CutVideoAndSaveToQuestion != null)
            {
                CutVideo(CutVideoAndSaveToQuestion.Value);
            }

            CleanVideoPlayer();

            base.ActionOnNext();
        }

        /// <summary>
        /// Indique si on modifie dès à présent le vidéo et en le sauvegardant comme réponse au questionnaire. Si null, ne fais rien, on ne conserve que l'information
        /// du temps indiqué. Le temps total du vidéo sera (TimeBeforeCut + TimeAfterCut). Si zéro (ou tout près de zéro), on ne fait rien. 
        /// Si le vidéo est trop petit, on prend le maximum de temps de vidéo en respectant les limites du vidéo (sauf si le temps final est de quasi-zéro arès modifications).
        /// NULL par défaut.
        /// </summary>
        public int? CutVideoAndSaveToQuestion { get; set; }

        /// <summary>
        /// Si on coupe le vidéo tout de suite, on indique ici le temps de vidéo après la coupe, en secondes.
        /// 0 par défaut.
        /// </summary>
        public double TimeAfterCut { get; set; }

        /// <summary>
        /// Si on coupe le vidéo tout de suite, on indique ici le temps de vidéo avant la coupe, en secondes.
        /// 0 par défaut.
        /// </summary>
        public double TimeBeforeCut { get; set; }

        /// <summary>
        ///     Indique le nombre de page à revenir en arrière dans le cas que cette valeur est non-nulle.
        ///     Si null, effectue une cancellation du questionnaire.
        /// </summary>
        [PublicAPI]
        public int? CancelBackPage { get; set; } = 1;

        private void CutVideo(int questionNo)
        {
            if ((TimeAfterCut + TimeBeforeCut) < 0.1) return;

            if (_screenConfig.QuestionVideoNo == null) return;

            var videoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionVideoNo.Value);

            if (_screenConfig.QuestionCutTimeNo == null) return;

            var start = double.Parse(GetTextAnswerFromQuestion(_screenConfig.QuestionCutTimeNo.Value)) - TimeBeforeCut;

            var cutVideoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + "_cut.mp4";

            var config = ServicesContainer.GetService<NomadConfiguration>();
            
            var videoMetadata = FfmpegUtil.GetVideoFileMetadata(config.FfprobeLocation, new FileInfo(videoFileName));

            if (start < 0) start = 0.0;
            var endTime = start + TimeAfterCut;
            var totalSec = videoMetadata.Streams[0].DurationSeconds;
            if (totalSec < endTime) endTime = totalSec;

            //Tranformation du video
            FfmpegUtil.CutVideo(config.FfmpegLocation, videoFileName, cutVideoFileName, start, endTime);

            //SaveAnswerToQuestion(questionNo, 1, cutVideoFileName);
        }

        // Methode override pour repondre a la spec
        protected override void ActionOnBack()
        {
            CleanVideoPlayer();
            if (CancelBackPage != null)
            {
                ResetTimer();
                Back(CancelBackPage.Value);
            }

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

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (!_activated) return;

        //    base.MouseDownHandler(sender, e);
        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _playArea))
        //    {
        //        ResetTimer();

        //        if (_videoSource != null && _videoSource.IsPaused)
        //        {
        //            _videoSource.SignalToUnPause();
        //        }
        //        else if (_videoPlayer != null && !_videoPlayer.IsRunning)
        //        {
        //            _showedFrame = 0;
        //            _dotWhitePos = _barZone.X;
        //            _cutTime = 0;
        //            _showedFrameWhenPaused = -1;
        //            Refresh();
        //            _videoPlayer.Start();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _pauseArea))
        //    {
        //        ResetTimer();

        //        if (_videoPlayer == null) return;

        //        _videoSource?.SignalToPause();
        //        _showedFrameWhenPaused = _showedFrame;
        //        _saveNextFrameAndCutTime = _canEdit;
        //        Refresh();
        //    }
        //}
    }
}
