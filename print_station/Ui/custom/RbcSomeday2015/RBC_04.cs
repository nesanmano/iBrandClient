using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc04 : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _previousArea;
        readonly PictureBox _picture;
        private string _pictureFile;

        public Rbc04(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _picture = new PictureBox
            {
                Location = new Point(360, 120),
                Size = new Size(560, 373),
                Visible = true,
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Controls.Add(_picture);
            _picture.BringToFront();

            _nextArea = new Rectangle(new Point(650, 495), new Size(175, 60));
            _previousArea = new Rectangle(new Point(455, 495), new Size(175, 60));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _previousArea))
        //    {
        //        Back();
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();
            _picture.Image?.Dispose();
            _picture.Image = null;
            _pictureFile = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            var fileToSHow = GetTextAnswerFromQuestion(10, 1);

            if (fileToSHow.Equals(_pictureFile)) return;

            using (var originale = FileUtil.LoadBitmap(fileToSHow))
            {
                _picture.Image?.Dispose();
                _picture.Image = originale.Resize(_picture.Width, _picture.Height);
                _pictureFile = fileToSHow;
            }
        }

    }
}