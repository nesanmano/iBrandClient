using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc06 : AbstractPrintStationScreen
    {
        private const string TEXT_NO_CHECK = "Oops! To send an email or SMS you must agree to the terms noted below.";

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _previousArea;
        readonly PictureBox _picture;
        private readonly Rectangle _mailArea;
        private readonly Rectangle _smsArea;
        private readonly Rectangle _consentArea;
        //private readonly Rectangle _waiverArea;
        private bool _isConsentChecked;
        private readonly Bitmap _checkmark;
        private string _pictureFile;

        public Rbc06(string backgroundImgEnSrc, PrintStationScreenConfig sc)
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

            _mailArea = new Rectangle(new Point(457, 179), new Size(205, 205));
            _smsArea = new Rectangle(new Point(700, 179), new Size(205, 205));
            _nextArea = new Rectangle(new Point(650, 589), new Size(175, 57));
            _previousArea = new Rectangle(new Point(454, 589), new Size(175, 57));

            _consentArea = new Rectangle(new Point(108, 402), new Size(180, 60));

            //_waiverArea = new Rectangle(new Point(795, 475), new Size(243, 40));

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_isConsentChecked)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(120, 410));
        //    }
        //}

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _previousArea))
        //    {
        //        Back();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next(3);
        //    }
        //    else if (InArea(e.X, e.Y, _mailArea))
        //    {
        //        if (_isConsentChecked)
        //        {
        //            Next(1);
        //        }
        //        else
        //        {
        //            var mBox = new MessageDialog();
        //            mBox.ShowMessage(TEXT_NO_CHECK);
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _smsArea))
        //    {
        //        if (_isConsentChecked)
        //        {
        //            Next(2);
        //        }
        //        else
        //        {
        //            var mBox = new MessageDialog();
        //            mBox.ShowMessage("Oops! To send an email or SMS you must agree to the terms noted below.");
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _consentArea))
        //    {
        //        _isConsentChecked = !_isConsentChecked;
        //        SaveAnswerToQuestionCheck(11, 1, _isConsentChecked);
        //        Refresh();
        //    }
        //    /*else if (InArea(e.X, e.Y, _waiverArea))
        //    {
        //        SuspendTimer();

        //        Bitmap release = null;
        //        if (_active_lg.Equals("en"))
        //            release = GetNoneLanguageBitmap("agreement_en.png");
        //        else
        //            release = GetNoneLanguageBitmap("agreement_fr.png");
        //        if (release != null)
        //        {
        //            new WaiverDialog(release).ShowDialog();
        //            release.Dispose();
        //        }

        //        ResumeTimer();
        //    }*/
        //}

        public override void Recycle()
        {
            base.Recycle();
            
            _picture.Image?.Dispose();
            _picture.Image = null;

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
                _pictureFile = fileToSHow;
            }
        }
    }
}