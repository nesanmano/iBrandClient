using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.InfoPresse
{
    public class ShareScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly PictureBox _picture;
        private readonly Rectangle _mailArea;

        public ShareScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            _picture = new PictureBox
            {
                Location = new Point((PictureBox.Width - 675) / 2, 140),
                Size = new Size(675, 450),
                BackColor = Color.White,
                Visible = true
            };
            //PictureBox.Paint += PaintEvent;

            Controls.Add(_picture);

            _picture.BringToFront();

            _mailArea = new Rectangle(new Point(1013, 304), new Size(88, 88));
            _nextArea = new Rectangle(new Point(523, 631), new Size(234, 69));
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End(); // On ne renvoi pas au premier écran, on termine.
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);
        //}

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
        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            if (_screenConfig.QuestionGifNo == null) return;

            var photoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionGifNo.Value, 1);
            _picture.Image?.Dispose();
            var tmp = FileUtil.LoadBitmap(photoFileName);
            _picture.Image = tmp.Frame(_picture.Width, _picture.Height);
        }
    }
}
