using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util.Win32;
using NomadLogic.Multimedia.Video.Vlc;
using Vlc.DotNet.Core;
using Vlc.DotNet.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Classe de présentation d'un video, avec son.
    /// Peut soit utiliser une question comme source de vidéo, ou alors un fichier externe fixe.
    /// </summary>
    public class PresentationVideoScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private VlcControl _vlcControl;
        private FileInfo _videoFile;

        public PresentationVideoScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc) { }

        protected override void OnLoad()
        {
            if (!VideoZone.HasValue)
            {
                throw new System.Exception("No zone specified for video playback.");
            }
            
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
        }

        [PublicAPI, CanBeNull]
        public FileInfo ExternalVideoFile { get; set; }

        [PublicAPI, CanBeNull]
        public int? QuestionVideo { get; set; }

        /// Doit être assigné avant le chargement de l'écran, sinon lance une exception.
        [PublicAPI, CanBeNull]
        public Rectangle? VideoZone { get; set; }

        /// On annule tout le répondant.
        [PublicAPI, CanBeNull]
        public Rectangle? CancelZone { get; set; }
        
        /// On passe au prochain écran.
        [PublicAPI, CanBeNull]
        public Rectangle? NextZone { get; set; }
        
        /// On revient en arrière.
        [PublicAPI, CanBeNull]
        public Rectangle? PreviousZone { get; set; }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, CancelZone))
            {
                _vlcControl.Stop();
                FirstScreen();
                return false;
            }

            if (InArea(x,y, NextZone))
            {
                _vlcControl.Stop();
                Next();
                return false;
            }

            if (InArea(x, y, PreviousZone))
            {
                _vlcControl.Stop();
                Back();
                return false;
            }

            return true;
        }

        private void OnVideoFinished(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            if (!_activated) return;
            
            Task.Run(() =>
            {
                Thread.Sleep(1000); // Dans les deux cas on fait une petite pause.

                // Dans le cas où l'écran n'est plus actif après la pause de une seconde.
                // Ceci est un "race condition".
                if (!_activated) return; 
                
                if (AutomaticNextOnVideoFinish)
                {
                     this.SafeCall(() => Next());
                }
                else
                {
                    PlayVideo();
                }
            });
        }

        /// Indique si on va automatiquement au prochain écran une fois le vidéo terminé.
        /// Sinon, le vidéo recommence automatiquement.
        [PublicAPI]
        public bool AutomaticNextOnVideoFinish { get; set; }

        public override void SetActivated()
        {
            base.SetActivated();
            
            // Vérification de l'existence du fichier vidéo.

            FileInfo videoFile;
            if (QuestionVideo.HasValue)
            {
                var str = GetTextAnswerFromQuestion(QuestionVideo.Value);
                videoFile = new FileInfo(str);
            }
            else
            {
                videoFile = ExternalVideoFile;
            }
            
            if (videoFile != null && videoFile.Exists)
            {
                _videoFile = videoFile;
                PlayVideo();
            }
            else
            {
                log.Error("No video file found: {0}", QuestionVideo?.ToString() ?? ExternalVideoFile?.ToString());
                Invoke(new Action(() =>
                {
                    MessageBox.Show(Resources.iBrand.Copyright, Resources.iBrand.NoVideoSource);
                }));
            }
        }

        private void PlayVideo()
        {
            Invoke(new Action(() =>
            {
                if (!_activated) return;
                _vlcControl.Play(_videoFile);
            }));
        }
        
        protected override void PrepareToClose()
        {
            _vlcControl.Stop();
            Recycle();
        }
    }
}
