using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.ParcCanada
{
    public partial class ShareScreenParcCanada : AbstractScreen
    {
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        private delegate void UiDelegate();
        private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);

        readonly Rectangle _nextArea;
        readonly Rectangle _facebookRect;
        readonly Rectangle _twitterRect;
        readonly PictureBox _picture;

        volatile bool _facebookDone;
        volatile bool _twitterDone;

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
        private Respondent _resp;

        public ShareScreenParcCanada(string backgroundImgEnSrc, string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            IsSaveOnThisPage = false;

            _picture = new PictureBox();
            _picture.Location = new Point(486, 223);
            _picture.Size = new Size(950, 643);
            _picture.Visible = true;
            Controls.Add(_picture);
            _picture.BringToFront();

            _facebookInactive = GetNoneLanguageBitmap("clock.png");
            _ibrandUploadProblemImage = GetNoneLanguageBitmap("not_availiable.png");

            _facebookRect = new Rectangle(new Point(1446, 447), new Size(82, 82));
            _twitterRect = new Rectangle(new Point(1446, 547), new Size(82, 82));

            _nextArea = new Rectangle(new Point(878, 905), new Size(167, 83));
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

            if (_picture.Image != null)
                _picture.Image.Dispose();

            _facebookDone = false;
            _twitterDone = false;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _facebookRect))
        //    {
        //        // facebook
        //        if (_ibrandUploadDone && !_ibrandUploadProblem && !_ibrandThumbnailProblem && _ibrandThumbnailDone)
        //        {
        //            if (_photoFileName != null && File.Exists(_photoFileName))
        //            {
        //                SuspendTimer();
                    
        //                MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, false)));
        //                /* Obsolète. Utiliser les écrans pseudo-modal désormais
        //                ShareUrlToFacebook share = new ShareUrlToFacebook(_facebookImageUrl, _facebookThumbnailUrl, "cipgmow8j1", "579", _active_lg);
        //                share.OnUploadFinished += OnUploadFinishedFacebook;

        //                Thread t = new Thread(share.DoWork);
        //                t.SetApartmentState(ApartmentState.STA);
        //                t.Start();
        //                */
        //            }
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _twitterRect))
        //    {
        //        // twitter
        //        if (_photoFileName != null && File.Exists(_photoFileName))
        //        {
        //            SuspendTimer();
                    
        //            MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, false)));

        //            string text = null;
        //            if (_activeLg.Equals(Constants.EN))
        //            {
        //                text = "Being on the Fortifications is awesome! It could be you...";
        //            }
        //            else if (_activeLg.Equals(Constants.FR))
        //            {
        //                text = "Et si c’était vous... sur les fortifications de Québec, c’est grandiose!";
        //            }
        //            /* Obsolète. Utiliser les écrans pseudo-modaux désormais
        //            UploadPictureToTwitterLeNomad uploadPictureTwitter = new UploadPictureToTwitterLeNomad(_screen_config);
        //            uploadPictureTwitter.FileName = _photoFileName;
        //            uploadPictureTwitter.CustomText = text;
        //            uploadPictureTwitter.OnUploadFinished += OnUploadFinishedTwitter;

        //            Thread t = new Thread(uploadPictureTwitter.DoWork);
        //            t.SetApartmentState(ApartmentState.STA);
        //            t.Start();
        //            */
        //        }
        //    }
        //}

        private void OnUploadFinishedFacebook(bool success)
        {
            ResumeTimer();
            
            _facebookDone = success;

            if (success) {
                var db = ServicesContainer.GetService<IbrandDatabase>();
                
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                var question = questionnaire.GetQuestion(5);
                var choiceText = (ChoiceText)question.GetChoice(1);

//                var answer = new Answer(-1, question.No, choiceText.No);
//                answer.Response = "sent";
//
//                db.InsertAnswer(_resp.Id, answer);
            }

            MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, true)));
            SingleApplication.SwitchToCurrentInstance();
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
                _facebookImageUrl = results[Path.GetFileName(_photoFileName)];
                if (_facebookImageUrl == null)
                {
                    _ibrandUploadProblem = true;
                }
                else
                {
                    /*
                    // On obtient le nom du fichier téléversé (n'est pas le même que le fichier d'origine pour empêcher les doublons).
                    _thumbnailFileName = _facebookImageUrl.Substring(_facebookImageUrl.LastIndexOf("/") + 1);

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
                _facebookThumbnailUrl = results[Path.GetFileName(_thumbnailFileName)];
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

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            Next();
        }

        private void OnUploadFinishedTwitter(bool success)
        {
            ResumeTimer();
            _twitterDone = success;

            if (success)
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();

                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                var question = questionnaire.GetQuestion(6);
                var choiceText = (ChoiceText)question.GetChoice(1);

//                var answer = new Answer(-1, question.No, choiceText.No);
//                answer.Response = "sent";
//
//                db.InsertAnswer(_resp.Id, answer);
            }

            MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, true)));
            SingleApplication.SwitchToCurrentInstance();
        }

        public override void SetActivated()
        {
            base.SetActivated();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var db = ServicesContainer.GetService<IbrandDatabase>();

            // Sauvegarde du répondant dans notre questionnaire.
            _resp = ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent() as Respondent;
            _resp.Answers = questionnaire.GetAnswersToSaveInDb();

            var repId = db.InsertRespondent(_resp, true);
            IsRespondentAlreadySaved = true;
            _resp.Id = repId;

            // Traitement de l'image
            var question = questionnaire.GetQuestion(4);
            var choiceText = (ChoiceText)question.GetChoice(1);
            _photoFileName = choiceText.GetAnswerText();

            if (_photoFileName != null && _photoFileName.Length > 0)
            {
                _picture.Image = Image.FromFile(_photoFileName);
                _picture.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            // Envoi de l'image vers notre serveur
            /*
            var uploadPicture = new UploadFilesToIbrandQuestionnaire(new[] {new FileInfo(_photoFileName)}, "592", "579", "facebook");
            uploadPicture.OnUploadFinished += OnUploadFinishedIbrand;

            var t = new Thread(uploadPicture.DoWork);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            */
        }
    }
}
