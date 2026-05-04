using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Util;


// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.G7_Selena
{
    public class ShareScreen : AbstractScreen
    {
        private const int MINIMUM_PRINT_WAIT_TIME = 500;


        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;

        private readonly Rectangle? _mailArea;

        readonly PictureBox _picture;

        private string _photoFileName;

        public ShareScreen(string backgroundImageSrc, ScreenConfig sc,
           Rectangle? mailRectangle, Rectangle endRectangle, Size previewSize, int previewYposition, int previewXposition)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {

            //PictureBox.Paint += PaintEvent;
            //PictureBox.MouseDown += MouseDownHandler;

            _picture = new PictureBox
            {
                Size = previewSize,
                Location = new Point(previewXposition, previewYposition),
                BackColor = Color.White,
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Controls.Add(_picture);

            _picture.BringToFront();

            // Assignation des rectangles.
            _mailArea = mailRectangle;

            _nextArea = endRectangle;
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End(); // On ne renvoi pas au premier écran, on termine.
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (_flagForClosing) return;

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        log.Trace("Next Area clicked");
        //        End();
        //    }

        //    else if (InArea(e.X, e.Y, _mailArea))
        //    {
        //        log.Trace("Mail Area clicked");

        //        SuspendTimer();
        //        Next(1);
        //    }

        //}

        public override void Recycle()
        {
            base.Recycle();
            _photoFileName = null;

            _picture.Image?.Dispose();
            _picture.Image = null;
        }



        public override void SetActivated()
        {
            base.SetActivated();

            try
            {
                log.Trace("Initializing Share Screen with new image.");
                if (_photoFileName != null) return;

                _photoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionFirstPictureNo.Value, 1);

                if (!File.Exists(_photoFileName))
                {
                    _photoFileName = null;
                    return;
                }

                _picture.Image?.Dispose();
                _picture.ImageLocation = _photoFileName;

                
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }

}
