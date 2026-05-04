using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class PreviewScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;

        readonly PictureBox _picture;


        public PreviewScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _picture = new PictureBox
            {
                Location = new Point((PictureBox.Size.Width - 720) / 2, 95),
                Size = new Size(720, 480),
                Visible = true
            };

            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(362, 617), new Size(239, 94));
            _nextArea = new Rectangle(new Point(674, 617), new Size(239, 94));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _backArea))
        //    {
        //        Back();
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

            //SetPreviewPicture(_picture, 7, false);
            var photoName = GetTextAnswerFromQuestion(7, 1);
            var originale = FileUtil.LoadBitmap(photoName);

            _picture.Image = originale;
            _picture.SizeMode = PictureBoxSizeMode.StretchImage;

        }
    }
}
