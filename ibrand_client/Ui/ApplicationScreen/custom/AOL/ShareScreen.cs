using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.AOL
{
    public class ShareScreen : AbstractScreen
    {
        //private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly PictureBox _picture;
        private readonly Rectangle _mailArea;
        private readonly Rectangle _smsArea;
        //private readonly Rectangle _facebookArea;
        //private readonly Rectangle _twitterArea;
        private readonly Rectangle _printArea;
        //private readonly ShareUrlToFacebookPseudomodal _facebookShare;
        //private readonly UploadPictureToTwitterLeNomadPseudomodal _twitterShare;

        //private string _thumbnailFileName;
        //private bool _ibrandUploadDone;
        //private bool _ibrandThumbnailDone;
        //private bool _ibrandUploadProblem;
        //private bool _ibrandThumbnailProblem;
        //private string _facebookImageUrl;
        //private string _facebookThumbnailUrl;
        private string _photoFileName;

        //private readonly Bitmap _socialInactive;
        //private readonly Bitmap _uploadProblemImage;
        //private readonly string _twitterCustomText;

        public ShareScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            //_twitterCustomText = twitterCustomText;

            //PictureBox.MouseDown += MouseDownHandler;
            _picture = new PictureBox
            {
                Location = new Point((PictureBox.Width - 480) / 2, 118),
                Size = new Size(480, 480),
                BackColor = Color.White,
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            //PictureBox.Paint += PaintEvent;

            Controls.Add(_picture);

            _picture.BringToFront();

            //_twitterShare = twitterShare;
            //_twitterShare.OnUploadFinished += OnUploadFinishedTwitter;

            //_facebookShare = facebookShare;
            //_facebookShare.OnUploadFinished += OnUploadFinishedFacebook;

            _mailArea = new Rectangle(new Point(906, 326), new Size(87, 87));
            _smsArea = new Rectangle(new Point(906, 458), new Size(87, 87));
            _printArea = new Rectangle(new Point(906, 193), new Size(87, 87));
            //_facebookArea = new Rectangle(new Point(906, 368), new Size(87, 87));
            //_twitterArea = new Rectangle(new Point(906, 505), new Size(87, 87));

            _nextArea = new Rectangle(new Point(512, 628), new Size(254, 97));

            //_socialInactive = GetNoneLanguageBitmap("clock.png");
            //_uploadProblemImage = GetNoneLanguageBitmap("not_availiable.png");
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End(); // On ne renvoi pas au premier écran, on termine.
        }

        /*
        public new void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (!_ibrandUploadDone && !_ibrandThumbnailDone)
            {
                e.Graphics.DrawImageUnscaled(_socialInactive, new Point(995, 406));
            }
            else if (_ibrandUploadProblem || _ibrandThumbnailProblem)
            {
                e.Graphics.DrawImageUnscaled(_uploadProblemImage, new Point(995, 509));
            }
        }
        */

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
        //    else if (InArea(e.X, e.Y, _printArea))
        //    {
        //        if (_screenConfig.QuestionPictureToPrintNo == null) return;
        //        if (!PrintAnswerFile(_screenConfig.QuestionPictureToPrintNo.Value, 1)) return;

        //        if (_screenConfig.QuestionNumberOfPrintNo != null)
        //        {
        //            IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value, 1);
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _smsArea))
        //    {
        //        SuspendTimer();
        //        Next(2);
        //    }
        //    /*
        //    else if (InArea(e.X, e.Y, _facebookArea))
        //    {
        //        if (!(_ibrandUploadDone && !_ibrandUploadProblem && !_ibrandThumbnailProblem && _ibrandThumbnailDone)
        //            || _photoFileName == null || !File.Exists(_photoFileName))
        //            return;

        //        SuspendTimer();

        //        _facebookShare.BrowserSize = Size;
        //        _facebookShare.Url = _facebookImageUrl;
        //        _facebookShare.ThumbnailUrl = _facebookThumbnailUrl;
        //        _facebookShare.Language = _active_lg;

        //        _facebookShare.DoWork();
        //    }
        //    else if (InArea(e.X, e.Y, _twitterArea))
        //    {
        //        SuspendTimer();

        //        if (_screen_config.QuestionFirstPictureNo != null)
        //        {
        //            var fileName = GetTextAnswerFromQuestion(_screen_config.QuestionFirstPictureNo.Value, 1);
        //            if (fileName == null) return;

        //            _twitterShare.FileName = fileName;

        //            if (!string.IsNullOrWhiteSpace(_twitterCustomText))
        //            {
        //                _twitterShare.CustomText = _twitterCustomText;
        //            }
        //            _twitterShare.DoWork();
        //        }
        //    }
        //    */
        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;

            //_thumbnailFileName = null;
            //_ibrandUploadDone = false;
            //_ibrandThumbnailDone = false;
            //_ibrandUploadProblem = false;
            //_ibrandThumbnailProblem = false;
            //_facebookImageUrl = null;
            //_facebookThumbnailUrl = null;
            _photoFileName = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            //if (_facebookImageUrl != null) return; // Pour ne pas faire la transformation et l'envoi facebook plusieurs fois.

            try
            {
                if (_photoFileName != null) return;
                if (_screenConfig.QuestionGifNo == null) return;

                _photoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionGifNo.Value, 1);

                _picture.Image?.Dispose();
                _picture.ImageLocation = _photoFileName;

                /* Pas besoin car on n'utilise pas Facebook
                // Envoi de l'image vers notre serveur
                var files = new[] { new FileInfo(_photoFileName) };
                var uploadPicture = new UploadFilesToIbrandQuestionnaire(files, _screen_config.ProjectId.ToString(),
                    _screen_config.QuestionnaireId.ToString(), "facebook");
                uploadPicture.OnUploadFinished += OnUploadFinishedIbrand;

                var t = new Thread(uploadPicture.DoWork);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                */
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        /*
        private void OnUploadFinishedIbrand(bool success, Dictionary<string, string> results)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegateIbrand(OnUploadFinishedIbrand), success, results);
                return;
            }

            if (success && results.Count > 0)
            {
                _ibrandUploadDone = true;

                var key = Path.GetFileName(_photoFileName);
                if (key != null && results.ContainsKey(key))
                    _facebookImageUrl = results[key];

                if (_facebookImageUrl == null)
                {
                    _ibrandUploadProblem = true;
                }
                else
                {
                    // On obtient le nom du fichier téléversé (n'est pas le même que le fichier d'origine pour empêcher les doublons).
                    _thumbnailFileName = _facebookImageUrl.Substring(_facebookImageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    // On tente ensuite de chercher le lien URL du thumbnail en utilisant GetFacebookPictureURL
                    var thumbnailPicture = new GetFacebookPictureURL(_facebookImageUrl);
                    thumbnailPicture.OnQueryFinished += OnThumbnailFinished;

                    var t = new Thread(thumbnailPicture.DoWork);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                }
            }
            else
            {
                _ibrandUploadProblem = true;
            }

            Refresh();
        }

        private void OnThumbnailFinished(bool success, Dictionary<string, string> results)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegateIbrand(OnThumbnailFinished), success, results);
                return;
            }

            if (success && results.Count > 0)
            {
                _ibrandThumbnailDone = true;

                var key = Path.GetFileName(_thumbnailFileName);
                if (key != null && results.ContainsKey(key))
                    _facebookThumbnailUrl = results[key];

                if (_facebookThumbnailUrl == null)
                {
                    _ibrandThumbnailProblem = true;
                }
            }
            else
            {
                _ibrandThumbnailProblem = true;
            }

            Refresh();
        }

        private void OnUploadFinishedFacebook(bool success)
        {
            // Ne pas faire un résumé de timer ici. Il y a des possibilité de bogue avec les appels de pseudi modal, et
            // de toute façon, le timer est réactivé avec le "setActivated" quand on revient sur l'écran de partage
            //ResumeTimer();

            if (!success) return;

            if (_screen_config.QuestionFacebookNo == null) return;

            IncrementEventInQuestionnaire(_screen_config.QuestionFacebookNo.Value, 1, null);
        }

        private void OnUploadFinishedTwitter(bool success)
        {
            // Ne pas faire un résumé de timer ici. Il y a des possibilité de bogue avec les appels de pseudi modal, et
            // de toute façon, le timer est réactivé avec le "setActivated" quand on revient sur l'écran de partage
            //ResumeTimer();

            if (!success) return;

            if (_screen_config.QuestionTwitterNo == null) return;

            IncrementEventInQuestionnaire(_screen_config.QuestionTwitterNo.Value, 1, null);
        }
        */
    }
}
