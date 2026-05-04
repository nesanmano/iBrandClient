using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc08 : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _previousArea;
        readonly PictureBox _picture;
        readonly PictureBox _pictureMini;
        private readonly Rectangle _legalArea;
        private readonly Rectangle _consentArea;
        private bool _isConsentChecked;
        private readonly Bitmap _checkmark;
        private string _pictureFile;

        public Rbc08(string backgroundImgEnSrc, PrintStationScreenConfig sc)
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

            _pictureMini = new PictureBox
            {
                Location = new Point(658, 207),
                Size = new Size(119, 80),
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(_pictureMini);
            _pictureMini.BringToFront();

            _legalArea = new Rectangle(new Point(223, 542), new Size(256, 30));
            _nextArea = new Rectangle(new Point(650, 584), new Size(175, 57));
            _previousArea = new Rectangle(new Point(454, 584), new Size(175, 57));
            _consentArea = new Rectangle(new Point(100, 474), new Size(978, 65));

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_isConsentChecked)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(104, 481));
        //    }
        //}

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _previousArea))
        //    {
        //        Back(1);
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next(2);
        //    }
        //    else if (InArea(e.X, e.Y, _legalArea))
        //    {
        //        Next(1);
        //    }
        //    else if (InArea(e.X, e.Y, _consentArea))
        //    {
        //        _isConsentChecked = !_isConsentChecked;
        //        SaveAnswerToQuestionCheck(12, 1, _isConsentChecked);
        //        Refresh();
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;

            _pictureMini.Image?.Dispose();
            _pictureMini.Image = null;

            _pictureFile = null;

            _isConsentChecked = false;
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

                _pictureMini.Image?.Dispose();
                _pictureMini.Image = originale.Resize(_pictureMini.Width, _pictureMini.Height);

                _pictureFile = fileToSHow;
            }
        }
    }
}