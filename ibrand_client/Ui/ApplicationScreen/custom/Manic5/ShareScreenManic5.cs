using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.Social.Facebook;
using NomadLogic.iBrandDesktop.Util.Social.Twitter;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public partial class ShareScreenManic5 : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);

        readonly Rectangle _nextArea;
        readonly Rectangle _end;
        readonly Rectangle _facebookRect;
        readonly Rectangle _twitterRect;
        readonly Rectangle _courriel;
        readonly Rectangle _print;
        readonly PictureBox _picture;
        
        private string _photoFileName;
        private string _thumbnailFileName;
        private bool _ibrandUploadDone;
        private bool _ibrandThumbnailDone;
        private readonly Bitmap _facebookInactive;
        private readonly Bitmap _ibrandUploadProblemImage;
        private string _facebookImageUrl;
        private string _facebookThumbnailUrl;
        private bool _ibrandUploadProblem;
        private bool _ibrandThumbnailProblem;

        private readonly ShareUrlToFacebookPseudomodal _facebookShare;
        private readonly UploadPictureToTwitterLeNomadPseudomodal _twitterShare;
        private bool _printDone;

        public ShareScreenManic5(string backgroundImgSrc, ScreenConfig sc, ShareUrlToFacebookPseudomodal facebookShare, UploadPictureToTwitterLeNomadPseudomodal twitterShare)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            IsSaveOnThisPage = false;

            _picture = new PictureBox
            {
                Location = new Point(673, 249),
                Size = new Size(575, 575),
                Visible = true
            };
            Controls.Add(_picture);
            _picture.BringToFront();

            _facebookShare = facebookShare;
            _facebookShare.OnUploadFinished += OnUploadFinishedFacebook;
            _twitterShare = twitterShare;
            _twitterShare.OnUploadFinished += OnUploadFinishedTwitter;

            _facebookInactive = GetNoneLanguageBitmap("clock.png");
            _ibrandUploadProblemImage = GetNoneLanguageBitmap("not_availiable.png");

            _facebookRect = new Rectangle(new Point(1285, 251), new Size(98, 95));
            _twitterRect = new Rectangle(new Point(1285, 367), new Size(98, 95));

            _nextArea = new Rectangle(new Point(1285, 727), new Size(285, 97));
            _end = new Rectangle(new Point(786, 879), new Size(350, 97));

            _courriel = new Rectangle(new Point(1285, 488), new Size(519, 97));
            _print = new Rectangle(new Point(1285, 608), new Size(172, 97));
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);
        //    if (!_ibrandUploadDone && !_ibrandThumbnailDone)
        //    {
        //        if (_facebookInactive != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_facebookInactive, new Point(1446, 447));
        //        }
        //    }
        //    else if (_ibrandUploadProblem || _ibrandThumbnailProblem)
        //    {
        //        if (_ibrandUploadProblemImage != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_ibrandUploadProblemImage, new Point(1446, 447));
        //        }
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();

            _printDone = false;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _end))
        //    {
        //        End();
        //    }
        //    else if (InArea(e.X, e.Y, _print))
        //    {
        //        if (_printDone) return;

        //        SuspendTimer();

        //        try
        //        {
        //            if (_screenConfig.QuestionPictureToPrintNo == null)
        //            {
        //                log.Warn("No print question given. Aborting print sequence.");
        //            }
        //            else
        //            {
        //                if (!PrintAnswerFile(_screenConfig.QuestionPictureToPrintNo.Value, 1))
        //                {
        //                    log.Trace("Could not send to printer spool. Aborting print sequence.");
        //                }
        //                else if (_screenConfig.QuestionNumberOfPrintNo != null)
        //                {
        //                    _printDone = true;
        //                    log.Trace("Incrementing Print number question.");
        //                    IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value, 1);
        //                }
        //            }
        //        }
        //        catch (System.Exception err)
        //        {
        //            log.Error(err, "Error during print sequence.");
        //        }

        //        Cursor.Current = Cursors.Default;

        //        ResumeTimer();
        //    }
        //    else if (InArea(e.X, e.Y, _courriel))
        //    {
        //        SuspendTimer();

        //        var emailDlg = new EmailDialog();

        //        emailDlg.ShowDialog();

        //        if (emailDlg.DialogResult == DialogResult.OK)
        //        {
        //            var emailList = emailDlg.GetEmailList();

        //            if (emailList.Count > 0)
        //            {
        //                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
        //                if (_screenConfig.QuestionEmailToSendNo != null)
        //                {
        //                    var question = questionnaire.GetQuestion(_screenConfig.QuestionEmailToSendNo.Value);
        //                    var choiceText = (ChoiceText)question.GetChoice(1);

        //                    var answer = new Answer(-1, question.NumberInt, choiceText.No);

        //                    if (!string.IsNullOrWhiteSpace(choiceText.GetAnswerText()))
        //                    {
        //                        answer.Response = choiceText.GetAnswerText() + ":";
        //                    }

        //                    for (var i = 0; i < emailList.Count; i++)
        //                    {
        //                        var email = emailList[i];
        //                        answer.Response += email;
        //                        if (i < emailList.Count - 1)
        //                            answer.Response += ":";
        //                    }

        //                    choiceText.SetAnswer(answer);
        //                }
        //            }
        //        }
        //        ResumeTimer();
        //    }
        //    else if (InArea(e.X, e.Y, _facebookRect))
        //    {
        //        // facebook
        //        if (!_ibrandUploadDone || _ibrandUploadProblem || _ibrandThumbnailProblem ||
        //            !_ibrandThumbnailDone) return;

        //        if (_photoFileName == null || !File.Exists(_photoFileName)) return;

        //        SuspendTimer();
                
        //        _facebookShare.BrowserSize = Size;
        //        _facebookShare.MultimediaItemUrl = _facebookImageUrl;
        //        _facebookShare.ThumbnailUrl = _facebookThumbnailUrl;
        //        _facebookShare.Language = _activeLg;
                
        //        var t = new Thread(_facebookShare.DoWork);
        //        t.SetApartmentState(ApartmentState.STA);
        //        t.Start();
        //    }
        //    else if (InArea(e.X, e.Y, _twitterRect))
        //    {
        //        // twitter
        //        if (_photoFileName == null || !File.Exists(_photoFileName)) return;

        //        SuspendTimer();
                
        //        var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

        //        if (_screenConfig.QuestionGifNo == null) return;

        //        var question = questionnaire.GetQuestion(_screenConfig.QuestionGifNo.Value);
        //        var choiceText = (ChoiceText) question.GetChoice(1);

        //        _twitterShare.FileName = choiceText.GetAnswerText();

        //        var t = new Thread(_twitterShare.DoWork);
        //        t.SetApartmentState(ApartmentState.STA);
        //        t.Start();
        //    }
        //}

        private void OnUploadFinishedFacebook(bool success)
        {
            ResumeTimer();

            if (!success) return;
            
            if (_screenConfig.QuestionFacebookNo != null)
            {
                IncrementEventInQuestionnaire(_screenConfig.QuestionFacebookNo.Value, 1);
            }
        }

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
                    /*
                    // On obtient le nom du fichier téléversé (n'est pas le même que le fichier d'origine pour empêcher les doublons).
                    _thumbnailFileName = _facebookImageUrl.Substring(_facebookImageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    // On tente ensuite de chercher le lien URL du thumbnail en utilisant GetFacebookPictureURL
                    var thumbnailPicture = new GetFacebookPictureUrl(_facebookImageUrl, null);
                    thumbnailPicture.OnQueryFinished += OnThumbnailFinished;

                    var t = new Thread(thumbnailPicture.DoWork);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    */
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
        }

        private void OnUploadFinishedTwitter(bool success)
        {
            ResumeTimer();

            if (!success) return;

            if (_screenConfig.QuestionTwitterNo != null)
            {
                IncrementEventInQuestionnaire(_screenConfig.QuestionTwitterNo.Value, 1);
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            // Traitement de l'image
            if (_screenConfig.QuestionGifNo != null)
            {
                var question = questionnaire.GetQuestion(_screenConfig.QuestionGifNo.Value);
                var choiceText = (ChoiceText)question.GetChoice(1);
                _photoFileName = choiceText.GetAnswerText();

                if (!string.IsNullOrEmpty(_photoFileName))
                {
                    _picture.Image = Image.FromFile(_photoFileName);
                    _picture.SizeMode = PictureBoxSizeMode.StretchImage;
                }

                // Envoi de l'image vers notre serveur
                question = questionnaire.GetQuestion(_screenConfig.QuestionGifNo.Value);
                choiceText = (ChoiceText)question.GetChoice(1);
                _photoFileName = choiceText.GetAnswerText();
            }

            /*
            var uploadPicture = new UploadFilesToIbrandQuestionnaire(new[] {new FileInfo(_photoFileName)},
                 _screenConfig.ProjectId.ToString(), _screenConfig.QuestionnaireId.ToString(), "facebook");
            uploadPicture.OnUploadFinished += OnUploadFinishedIbrand;

            var t = new Thread(uploadPicture.DoWork);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            */
        }

        // On termine en sauvant ce qui a été fait si on timeout sur cette page.
        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            if (_activated)
            {
                End();
            }
        }
    }
}
