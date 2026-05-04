using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.CitizenRelations
{
    public class ShareScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly PictureBox _picture;
        private readonly Rectangle _mailArea;
        private readonly Rectangle _smsArea;


        private string _photoFileName;

        public ShareScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {

            //PictureBox.MouseDown += MouseDownHandler;
            _picture = new PictureBox
            {
                Location = new Point((PictureBox.Width - 480) / 2, 118),
                Size = new Size(480, 480),
                BackColor = Color.White,
                Visible = true
            };
            //PictureBox.Paint += PaintEvent;

            Controls.Add(_picture);

            _picture.BringToFront();

            _mailArea = new Rectangle(new Point(906, 249), new Size(87, 87));
            _smsArea = new Rectangle(new Point(906, 393), new Size(87, 87));

            _nextArea = new Rectangle(new Point(512, 614), new Size(256, 97));
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End(); // On ne renvoi pas au premier écran, on termine.
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        End();
        //    }
        //    else if (InArea(e.X, e.Y, _mailArea))
        //    {
        //        SuspendTimer();
        //        Next(1);
        //    }
        //    else if (InArea(e.X, e.Y, _smsArea))
        //    {
        //        SuspendTimer();
        //        Next(2);
        //    }
            

        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;

            _photoFileName = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            try
            {
                if (_photoFileName != null) return;
                if (_screenConfig.QuestionGifNo == null) return;

                _photoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionGifNo.Value, 1);
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
