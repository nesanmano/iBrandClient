using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class VideoScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        VlcInstance _vlcInstance;
        VlcMediaPlayer _vlcPlayer;

        readonly string _videoNameEn;

        readonly Panel _videoPanel;

        public VideoScreen(string backgroundImgEnSrc, string videoNameEn)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            _videoNameEn = videoNameEn;

            _videoPanel = new Panel();
            _videoPanel.Size = new Size(864, 486);
            _videoPanel.Location = new Point(528, 250);
            Controls.Add(_videoPanel);

            _videoPanel.BringToFront();

            InitVideoPlayer();
        }

        private void InitVideoPlayer()
        {

            string[] args = {
                "--ignore-config",
                "--no-disable-screensaver",
                "--no-ffmpeg-hurry-up",
                "--no-video-title-show"
                            };


            _vlcInstance = new VlcInstance(args);

            _vlcPlayer = new VlcMediaPlayer(_vlcInstance, _videoPanel.Handle);
            _vlcPlayer.OnMediaEnded += OnVideoEnded;

        }

        private void PlayVideo()
        {
            var media = new VlcMedia(_vlcInstance, _videoNameEn);
            _vlcPlayer.Load(media);

            media.Dispose();

            _vlcPlayer.Play();
        }

        public override void SetActivated()
        {
            try
            {
                PlayVideo();
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
            }
        }

        public void OnVideoEnded(object sender, EventArgs e)
        {
            Next();
        }
    }
}
