using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using NLog;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.T1DFC
{
    /// <summary>
    /// Classe de presentation video.
    /// http://www.aforgenet.com/framework/docs/html/bd272e5f-344c-56a5-f74f-1ac3062a9554.htm
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class PreviewVideoScreenT1DFC : PreviewScreenAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private FileVideoSource _videoSource;
        private VideoSourcePlayer _videoPlayer;

        protected readonly Rectangle _reeditArea;
        protected readonly Rectangle _saveArea;

        private readonly int _frontTop;
        private readonly Size _previewSize;

        public PreviewVideoScreenT1DFC(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle nextZone, Rectangle backZone, int fromTop, Size previewSize)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, cancelZone, nextZone, backZone, fromTop, previewSize)
        {

            _reeditArea = backZone;
            _saveArea = nextZone;

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
            _videoPlayer.Click += OnVideoClicked;
            Controls.Add(_videoPlayer);
            _videoPlayer.BringToFront();
        }

        protected override void AddPreviewControl(Size previewSize, Point fromTop)
        {
            //nothing
        }

        private void OnVideoClicked(object sender, EventArgs e)
        {
            if (_videoPlayer.IsRunning)
            {
                _videoPlayer.SignalToStop();
            }
            else if (_videoPlayer.VideoSource != null)
            {
                _videoPlayer.Start();
            }
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //}

        private void OnVideoFinished(object sender, ReasonToFinishPlaying reason)
        {
            /*var t = new Task(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        if (_videoPlayer != null)
                            _videoPlayer.Start();
                    }));
                    return;
                }
                if (_videoPlayer != null)
                    _videoPlayer.Start();

            });
            t.Start();*/
        }

        public override void Recycle()
        {
            base.Recycle();
        }

        private void CleanVideoPlayer()
        {
            if (_videoSource != null)
            {
                if (_videoSource.IsPaused)
                    _videoSource.SignalToUnPause();

                _videoSource.SignalToStop();
                _videoSource.VideoSourceError -= VideoError;
                _videoSource = null;
            }

            if (_videoPlayer != null)
            {
                Controls.Remove(_videoPlayer);
                _videoPlayer.PlayingFinished -= OnVideoFinished;
                _videoPlayer.Dispose();
                _videoPlayer = null;
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();

            AddVideoPreviewControl(_previewSize, _frontTop);

            if (_screenConfig.QuestionVideoNo != null)
            {
                //var fileSource = CutVideo();
                var fileSource = GetTextAnswerFromQuestion(5, 1);

                if (File.Exists(fileSource))
                {
                    var source = new FileVideoSource(fileSource);
                    if (_videoSource != null && _videoSource.Source != null && _videoSource.Source.Equals(fileSource))
                    {
                        _videoPlayer.Start();
                        return;
                    }

                    if (_videoSource != null && _videoSource.IsRunning)
                        _videoSource.Stop();

                    _videoSource = source;
                    _videoSource.VideoSourceError += VideoError;
                    _videoPlayer.VideoSource = _videoSource;
                    _videoPlayer.Start();
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
            if (_videoSource != null && _videoSource.IsPaused)
            {
                _videoSource.SignalToUnPause();
            }
            _videoPlayer?.SignalToStop();
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
        //        if (_flagForClosing) return;
        //    }
        //}
    }
}
