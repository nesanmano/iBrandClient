using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc09 : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly PictureBox _picture;
        private string _pictureFile;

        public Rbc09(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;
            _picture = new PictureBox
            {
                Location = new Point(98, 178),
                Size = new Size(407, 271),
                BackColor = Color.White,
                Visible = true
            };

            Controls.Add(_picture);

            _picture.BringToFront();
            
            _nextArea = new Rectangle(new Point(552, 495), new Size(175, 57));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        End();
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();
            _pictureFile = null;
            _picture.Image?.Dispose();
            _picture.Image = null;
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

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End();
        }
    }
}