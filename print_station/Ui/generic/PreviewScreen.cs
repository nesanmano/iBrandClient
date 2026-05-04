using NLog;
using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.PrintStation.Ui.generic
{
    public class PreviewScreen : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Rectangle _nextArea;
        
        private readonly Rectangle _cancelArea;

        protected readonly PictureBox _picture;

        protected string _photoFileName;

        public PreviewScreen(string backgroundImageSrc, PrintStationScreenConfig sc,
            Rectangle cancelRectangle, Rectangle nextRectangle, int pictureYposition,
            Size pictureSize)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            _picture = new PictureBox
            {
                Size = pictureSize,
                Location = new Point((PictureBox.Width - pictureSize.Width) / 2, pictureYposition),
                BackColor = Color.White,
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Controls.Add(_picture);

            _picture.BringToFront();

            // Assignation des rectangles.
            _cancelArea = cancelRectangle;
            _nextArea = nextRectangle;
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End(); // On ne renvoi pas au premier écran, on termine.
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _cancelArea))
            {
                log.Trace("Cancel Area clicked");
                Close();
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
              
                log.Trace("Next Area clicked");
                Next();
                return false;
            }

            return true;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            log.Trace("SetActivated Called");

            try
            {
                log.Trace("Initializing Share Screen with new image.");
                if (_photoFileName != null) return;

                _photoFileName = GetSelectedPicture().Src;
 
                _picture.Image?.Dispose();
                _picture.ImageLocation = _photoFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;

            _photoFileName = null;
        }
    }
}
