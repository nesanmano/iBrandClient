using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc07 : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _previousArea;
        readonly PictureBox _picture;
        private readonly Rectangle _printArea;
        private string _pictureFile;

        public Rbc07(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;
            _picture = new PictureBox
            {
                Location = new Point(96, 178),
                Size = new Size(312, 208),
                BackColor = Color.White,
                Visible = true
            };

            Controls.Add(_picture);

            _picture.BringToFront();

            _printArea = new Rectangle(new Point(457, 179), new Size(205, 205));
            _nextArea = new Rectangle(new Point(650, 495), new Size(175, 57));
            _previousArea = new Rectangle(new Point(454, 495), new Size(175, 57));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _previousArea))
        //    {
        //        Back(3);
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next(1);
        //    }
        //    else if (InArea(e.X, e.Y, _printArea))
        //    {
        //        if (PrintAnswerFile(10, 1))
        //        {
        //            IncrementEventInQuestionnaire(9, 1);
        //        }
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