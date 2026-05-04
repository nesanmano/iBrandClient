using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia.Cli.Ffmpeg;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Classe pour visualiser et éditer le vidéo pour n'en conserver qu'une partie.
    /// http://www.aforgenet.com/framework/docs/html/bd272e5f-344c-56a5-f74f-1ac3062a9554.htm
    /// </summary>
    public class PreviewAndCutVideoScreen : PreviewScreenAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private FileVideoSource _videoSource;
        private VideoSourcePlayer _videoPlayer;

        private Bitmap _dotWhiteImage;
        private Bitmap _dotBlackImage;
        private Bitmap _hourGlass;

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
        private readonly bool _saveThumbnail;

        private bool _showHourGlass;
        
        private Bitmap _saveActiveImage;

        private readonly int _frontTop;
        private readonly Size _previewSize;
        private string _totalTime;
        private Rectangle? _hourGlassRect;
        private Rectangle? _saveInactiveRect;

        public IList<string> ImageFileNames { get; set; }

        /// <summary>
        /// Initialize l'écran pour visualiser et indiquer à quel moment on doit "couper" un vidéo. Ce temps enregistré comme question
        /// est habituellement utilisé lors d'une composition vidéo lors des étapes Post-Submit.
        /// </summary>
        /// <param name="backgroundImgEnSrc">Fond d'écran en anglais.</param>
        /// <param name="backgroundImgFrSrc">Fond d'écran en français (facultatif).</param>
        /// <param name="sc">Paramètres globaux d'écrans.</param>
        /// <param name="cancelZone">Zone pour canceller le répondant.</param>
        /// <param name="nextZone">Zone pour passer au prochain écran.</param>
        /// <param name="backZone">Zone pour revenir à l'écran précédent.</param>
        /// <param name="playZone">Zone pour commencer la visualisation du vidéo.</param>
        /// <param name="pauseZone">Zone pour stopper le vidéo et enregistrer le temps voulu.</param>
        /// <param name="barZone">Barre de défilement pour indiquer la duration du vidéo et où la visualisation est rendue..</param>
        /// <param name="fromTop">Espace qui sépare la visualisation du vidéo et le haut de l'écran.</param>
        /// <param name="previewSize">Dimension de la visualisation du vidéo.</param>
        /// <param name="saveThumbnail">Indique si on doit sauvegarder un thumbnail en même temps que l'on indique le temps d'arrêt.</param>
        public PreviewAndCutVideoScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, Rectangle playZone, Rectangle pauseZone, Rectangle barZone,
            int fromTop, Size previewSize, bool saveThumbnail)
            : base(backgroundImgEnSrc, backgroundImgFrSrc ?? backgroundImgEnSrc, sc, cancelZone, nextZone, backZone, fromTop, previewSize)
        {
            _saveThumbnail = saveThumbnail;
            _playArea = playZone;
            _pauseArea = pauseZone;
            _barZone = barZone;
            _dotWhitePos = _barZone.X;
            _frontTop = fromTop;
            _previewSize = previewSize;
        }

        protected override void OnLoad()
        {
            var tmp = ImageFileNames ?? new List<string>();

            _dotWhiteImage = GetNoneLanguageBitmap(tmp.GetArrayValueWithDefault(0, "dot_white.png"));
            _dotBlackImage = GetNoneLanguageBitmap(tmp.GetArrayValueWithDefault(1, "dot_black.png"));
            _hourGlass = GetNoneLanguageBitmap(tmp.GetArrayValueWithDefault(2, "hourglass_black.png"));

            var lg = NoLanguageDependency ? null : _activeLg;
            _saveActiveImage = GetBitmap(tmp.GetArrayValueWithDefault(3, "save_enable.png"), lg);
        }
        
        public bool NoLanguageDependency { get; set; }

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

        protected override void AddPreviewControl(Size previewSize, Point location) { 
            //nothing
        }

        public Point? HourglassPosition { get; set; }

        public Point? DisabledSavePosition { get; set; }

        public Point? CurrentTimePosition { get; set; }

        public Point? TotalTimePosition { get; set; }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_showHourGlass && HourglassPosition != null)
            {
                if (_hourGlass != null)
                {
                    //e.Graphics.DrawImageUnscaled(_hourGlass, HourglassPosition.Value);
                    if (_hourGlassRect == null)
                    {
                        _hourGlassRect = new Rectangle(HourglassPosition.Value, _hourGlass.Size);
                    }
                    ScreenUtil.DrawImage(g, _hourGlass, _hourGlassRect.Value);
                }
            }

            if (SaveInactiveDrawPoint != null && _saveActiveImage != null && _cutTime != 0)
            {
                //e.Graphics.DrawImageUnscaled(_saveActiveImage, SaveInactiveDrawPoint.Value);
                if (_saveInactiveRect == null)
                {
                    _saveInactiveRect = new Rectangle(SaveInactiveDrawPoint.Value, _saveActiveImage.Size);
                }
                ScreenUtil.DrawImage(g, _saveActiveImage, _saveInactiveRect.Value);
            }

            if (_videoPlayer != null && _videoSource != null)
            {

                if (_videoPlayer.IsRunning && !_videoSource.IsPaused)
                {
                    if (_dotWhiteImage != null)
                    {
                        if (_dotWhitePos > 0)
                            //e.Graphics.DrawImageUnscaled(_dotWhiteImage, _dotWhitePos, _barZone.Y);
                            // TODO Trouver moyen de ne PAS faire un rectangle à chaque fois.
                            ScreenUtil.DrawImage(g, _dotWhiteImage, new Rectangle(_dotWhitePos, _barZone.Y, _dotWhiteImage.Width, _dotWhiteImage.Height));
                    }
                }
            }

            if (CurrentTimePosition != null)
            {
                var time = (_showedFrame / _videoFps).ToString("F2", CultureInfo.InvariantCulture);
                g.DrawString(time, VideoTimerFont, VideoTimerBrush, CurrentTimePosition.Value);
            }

            if (TotalTimePosition != null)
            {
                g.DrawString(_totalTime, VideoTimerFont, VideoTimerBrush, TotalTimePosition.Value);
            }

            if (_showedFrameWhenPaused == -1) return;
            if (_dotBlackImage == null) return;

            var dotBlackPos = _totalFrame == 0 ? _barZone.X : (int)((_showedFrameWhenPaused / _totalFrame * _barZone.Width) + _barZone.X);
            ScreenUtil.DrawImage(g, _dotBlackImage, new Rectangle(dotBlackPos, _barZone.Y, _dotBlackImage.Width, _dotBlackImage.Height));
        }

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
                    var config = ServicesContainer.GetService<NomadConfiguration>();
                    var videoMetadata = FfmpegUtil.GetVideoFileMetadata(config.FfprobeLocation, new FileInfo(fileSource), DebugVideoProbeFile == null ? null : new FileInfo(DebugVideoProbeFile));

                    var vidStream = videoMetadata.GetMainVideoStreamData();

                    if (vidStream == null || vidStream.FrameRate == null)
                    {
                        log.Error("Cannot parse metadata file for video: {0}", fileSource);
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show(Resources.iBrand.Copyright, Resources.iBrand.NoVideoSource);
                        }));
                        return;
                    }

                    _videoFps = vidStream.FrameRate.DoubleValue;

                    _totalFrame = vidStream.FrameCount;

                    _totalTime = vidStream.DurationSeconds.ToString("F2", CultureInfo.InvariantCulture);

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
                    _videoPlayer.Start();

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
            _dotWhitePos = _totalFrame == 0 ? _barZone.X : (int) ((_showedFrame / _totalFrame * _barZone.Width) + _barZone.X);

            if (_saveNextFrameAndCutTime)
            {
                if (_saveThumbnail)
                {
                    if (_firstPictureFile != null && File.Exists(_firstPictureFile))
                        File.Delete(_firstPictureFile);

                    _firstPictureFile = GetTerminalDirectoryDataFile(GenerateUniqueId.GetId() + ".jpg",
                                            DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                            DirectoryUtil.ProjectDataDirectoriesModEnum.none);

                    eventArgs.Frame.Save(95, _firstPictureFile);
                }

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
                    SaveAnswerToQuestion(new Answer { QuestionNo = _screenConfig.QuestionFirstPictureNo.Value, Response = _firstPictureFile});
                }
            }

            if (_screenConfig.QuestionCutTimeNo != null)
            {
                SaveAnswerToQuestion(new Answer {QuestionNo = _screenConfig.QuestionCutTimeNo.Value, Response = _cutTime.ToString(CultureInfo.InvariantCulture)});
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
        /// Pour deboguer le resultat dune analyse de video.
        /// </summary>
        public string DebugVideoProbeFile { get; set; }

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

            if (videoMetadata == null)
            {
                throw new System.Exception("Cannot find video metadata. Bad video file.");
            }
            
            if (start < 0) start = 0.0;
            var endTime = start + TimeAfterCut;
            var totalSec = videoMetadata.GetMainVideoStreamData().DurationSeconds;
            if (totalSec < endTime) endTime = totalSec;

            //Tranformation du video
            FfmpegUtil.CutVideo(config.FfmpegLocation, videoFileName, cutVideoFileName, start, endTime);

            SaveAnswerToQuestion(new Answer {QuestionNo = questionNo, Response = cutVideoFileName});
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

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _playArea))
            {
                ResetTimer();

                if (_videoSource != null && _videoSource.IsPaused)
                {
                    _videoSource.SignalToUnPause();
                }
                else if (_videoPlayer != null && !_videoPlayer.IsRunning)
                {
                    _showedFrame = 0;
                    _dotWhitePos = _barZone.X;
                    _cutTime = 0;
                    _showedFrameWhenPaused = -1;
                    Refresh();
                    _videoPlayer.Start();
                }

                return false;
            }

            if (InArea(x, y, _pauseArea))
            {
                ResetTimer();

                if (_videoPlayer == null) return false;

                _videoSource?.SignalToPause();
                _showedFrameWhenPaused = _showedFrame;
                _saveNextFrameAndCutTime = true;
                Refresh();
                return false;
            }

            return true;
        }
    }
}
