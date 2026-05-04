using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.MonsterEnergy
{
    public class PreviewScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;
        readonly Rectangle _closeArea;

        readonly PictureBox _picture;
        private readonly int _questionPreview;

        public PreviewScreen(string backgroundImgEnSrc, ScreenConfig sc, int questionNbrToPreview)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _questionPreview = questionNbrToPreview;

            _picture = new PictureBox
            {
                Location = new Point(401, 65),
                Size = new Size(480, 480),
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(398, 600), new Size(235, 95));
            _nextArea = new Rectangle(new Point(643, 600), new Size(235, 95));
            _closeArea = new Rectangle(new Point(1190, 20), new Size(85, 67));
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
        //    else if (InArea(e.X, e.Y, _closeArea))
        //    {
        //        FirstScreen();
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
            var tmp = FileUtil.LoadBitmap(GetTextAnswerFromQuestion(_questionPreview));
            _picture.Image = tmp.ResizeForView(_picture.Width, _picture.Height);
        }

    }
}
