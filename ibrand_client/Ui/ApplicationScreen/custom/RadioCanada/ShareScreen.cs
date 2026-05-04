using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class ShareScreen : AbstractScreen
    {
        private const string TEXT_NO_CHECK = "Oups! Pour envoyer un courriel ou un message texte vous devez accepter les conditions mentionnées ci-bas.";

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly PictureBox _picture;
        private readonly Rectangle _mailArea;
        private readonly Rectangle _smsArea;
        private readonly Rectangle _consentArea;

        private bool _isConsentChecked;
        private readonly Bitmap _checkmark;
        private string _pictureFile;

        public ShareScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;
            _picture = new PictureBox
            {
                Location = new Point(161, 181),
                Size = new Size(420, 280),
                BackColor = Color.White,
                Visible = true
            };

            Controls.Add(_picture);

            _picture.BringToFront();

            _mailArea = new Rectangle(new Point(659, 230), new Size(215, 215));
            _smsArea = new Rectangle(new Point(904, 230), new Size(215, 215));
            _nextArea = new Rectangle(new Point(506, 680), new Size(272, 95));

            _consentArea = new Rectangle(new Point(155, 475), new Size(1004, 168));

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_isConsentChecked)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(163, 476));
        //    }
        //}

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
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
        //            mBox.ShowMessage(TEXT_NO_CHECK);
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _consentArea))
        //    {
        //        _isConsentChecked = !_isConsentChecked;
        //        SaveAnswerToQuestionCheck(9, 1, _isConsentChecked);
        //        Refresh();
        //    }
            
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

            var fileToSHow = GetTextAnswerFromQuestion(7, 1);

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
