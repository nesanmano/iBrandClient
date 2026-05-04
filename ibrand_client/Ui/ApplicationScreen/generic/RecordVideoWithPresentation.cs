using System;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util.Win32;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Video.Vlc;
using Vlc.DotNet.Core;
using Vlc.DotNet.Forms;
#pragma warning disable 612

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cette variation de présentation présente un vidéo simultanément à une saisie vidéo, d'une durée égale (en théorie).
    /// Quand on cesse de saisir un vidéo, ou si le vidéo de présentation stoppe, on stoppe tout et on tente de passer à l'écran suivant.
    /// </summary>
    public class RecordVideoWithPresentation : RecordVideoScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private VlcControl _vlcControl;

        /// Doit être assigné avant le chargement de l'écran, sinon lance une exception.
        [PublicAPI, CanBeNull]
        public Rectangle? VideoZone { get; set; }

        [PublicAPI, CanBeNull]
        public FileInfo ExternalVideoFile { get; set; }

        /// Indique si on va automatiquement au prochain écran une fois le vidéo terminé.
        /// Sinon, le vidéo recommence automatiquement.
        [PublicAPI]
        public bool AutomaticNextOnVideoFinish { get; set; } = true;

        public RecordVideoWithPresentation(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc, 
            int beforeCountdown, Rectangle? cancelZone, bool compressFile)
            : base(sc, backgroundImgEnSrc, backgroundImgFrSrc, null, beforeCountdown, -1, null, 
                null, cancelZone, null, null, compressFile)
        {
            
        }
        protected override void OnLoad()
        {
            base.OnLoad();

            if (!VideoZone.HasValue)
            {
                throw new System.Exception("No zone specified for video playback.");
            }

            if (!(ExternalVideoFile?.Exists ?? false))
            {
                throw new System.Exception("No valid file specified for video playback.");
            }

            var config = ServicesContainer.GetService<NomadConfiguration>();
            var data = FfmpegUtil.GetVideoFileMetadata(config.FfprobeLocation, ExternalVideoFile);

            TimeToRecordDefault = (int)Math.Ceiling(data.GetMainVideoStreamData().DurationSeconds);
            
            _vlcControl = new VlcControl
            {
                Size = VideoZone.Value.Size,
                Location = VideoZone.Value.Location.X < 0 ? new Point((PictureBox.Width - VideoZone.Value.Width) / 2, VideoZone.Value.Location.Y) : VideoZone.Value.Location,
                VlcLibDirectory = null,
                BackColor = SystemColors.ButtonShadow,
                Name = "myVlcControl"
            };
            _vlcControl.VlcLibDirectoryNeeded += VlcUtil.GetLibDirectory;
            Controls.Add(_vlcControl);
            
            // Méthode "EndInit" très importante! Normalement automatiquement invoquée dans un Designer, mais il faut le faire manuellement ici
            // et le contrôle VLC ne fonctionne simplement pas si on n'invoque pas cette méthode.
            _vlcControl.EndInit(); 
            _vlcControl.BringToFront();
            _vlcControl.EndReached += OnVideoFinished;
            _vlcControl.Visible = false;
            log.Trace("VLC video control initialized.");
        }
        
        private void OnVideoFinished(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            if (!_activated) return;
            this.SafeCall(() =>
            {
                log.Trace("Video playback has finished.");
                if (_vlcControl.IsPlaying)
                {
                    _vlcControl.Stop();
                    _vlcControl.Visible = false;
                    log.Trace("COntrol return from stop command.");
                }
                StopRecording();
            });
        }

        protected override void OnRecordStart()
        {
            PlayVideo();
        }

        protected override void OnVideoRecordStop(bool error = false)
        {
            if (error)
            {
                log.Debug("Vlc control stopped because of machine state error.");
            }
            this.SafeCall(() =>
            {
                if (!_vlcControl.IsPlaying) return;
                _vlcControl.Stop();
                _vlcControl.Visible = false;
                log.Trace("Control return from stop command.");
            });
        }

        private void PlayVideo()
        {
            this.SafeCall(() => 
            {
                if (!_activated) return;
                _vlcControl.Visible = true;
                _vlcControl.ResetMedia();
                log.Trace("Control return from ResetMedia command.");
                _vlcControl.Play(ExternalVideoFile);
                log.Trace("Control return from play command.");
            });
        }
        
        protected override void PrepareToClose()
        {
            this.SafeCall(() =>
            {
                if (!_vlcControl.IsPlaying) return;
                _vlcControl.Stop();
                _vlcControl.Visible = false;
                log.Trace("Control return from stop command.");
            });
            Recycle();
        }

        public override void Recycle()
        {
            base.Recycle();
            this.SafeCall(() => {
                if (!_vlcControl.IsPlaying) return;
                _vlcControl.Stop();
                _vlcControl.Visible = false;
                log.Trace("Control return from stop command.");
            });
        }

        public override void SetActivated()
        {
            this.SafeCall(() =>
            {
                base.SetActivated();
                Refresh();
            });
        }
    }
}