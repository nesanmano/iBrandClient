using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.PrintStation.Services;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.Social.Facebook;
using NomadLogic.iBrandDesktop.Util.Social.Twitter;
#pragma warning disable 612

// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.PrintStation.Ui.generic
{
    /// TODO Refactoriser le classe originale et celle-ci pour ne pas dupliquer de code.
    public class ShareScreen : AbstractPrintStationScreen
    {
        private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);
        private delegate void UiDelegateIbrandOneFile(bool success, string result);

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        protected readonly PictureBox _picture;
        private readonly Rectangle? _printArea;
        private readonly Rectangle? _mailArea;
        private readonly Rectangle? _smsArea;
        private readonly Rectangle? _facebookArea;
        private readonly Rectangle? _twitterArea;
        private readonly ShareUrlToFacebookPseudomodal _facebookShare;
        private readonly UploadPictureToTwitterLeNomadPseudomodal _twitterShare;

        private string _thumbnailFileName;
        private bool _ibrandUploadDone;
        private bool _ibrandThumbnailDone;
        private bool _ibrandUploadProblem;
        private bool _ibrandThumbnailProblem;
        protected string _facebookImageUrl;
        private string _facebookThumbnailUrl;
        protected string _photoFileName;
        protected string _printFileName;

        private Bitmap _twitterInactiveImage;
        private Bitmap _facebookInactiveImage;
        private Bitmap _mailInactiveImage;
        private Bitmap _smsInactiveImage;
        private Bitmap _printInactiveImage;
        private bool _socialBlocked;
        private readonly string _twitterCustomTextEn;
        private readonly string _twitterCustomTextFr;

        public bool AutoPrint { get; set; }

        public Rectangle? CropRectangle { get; set; }

        private bool _printFinished;
        
        public IList<int> PicturesToTransform { get; set; }

        public int PreviewPictureQuestion { get; set; }

        private ImageListTransformOptions _imageListTransform;

        /// <summary>
        /// Séquence d'images optionnelle quand on fait la transformation.
        /// </summary>
        protected AnimatedImageSequence _creatingSequence;

        // Référence vers le panneau de prévisuation. Peut être nul si on n'utilise pas la prévisualisation.
        //protected PictureBox _panelPreview;

        /// Référence interne vers les images de base d'affichage, qui ne doivent pas être "garbage collected" quand on itère dans l'affichage du panneau de prévisualisation.
        protected readonly List<Image> _safeBitmaps = new List<Image>();


        /// Cette variable est une sémaphone multi-thread pour indiquer que la classe travaille en arrière plan (soit attends une photo, soit est en train
        /// de transformer quelque chose). Si mis à vrai, ne devrait pas réagir sur certains événements, comme le UI.
        protected volatile bool _backgroundWorkActive;

        private CancellationTokenSource _cts;
        private Task _task;

        public ShareScreen(string backgroundImageSrcEn, string backgroundImageSrcFr, PrintStationScreenConfig sc,
            ShareUrlToFacebookPseudomodal facebookShare, UploadPictureToTwitterLeNomadPseudomodal twitterShare,
            string twitterCustomTextEn, string twitterCustomTextFr, Rectangle? printRectangle, Rectangle? mailRectangle, Rectangle? smsRectangle,
            Rectangle? facebookRectangle, Rectangle? twitterRectangle, Rectangle endRectangle, int pictureYposition = -1,
            Size? pictureSize = null, bool showHourGlass = false)
            : base(backgroundImageSrcEn, backgroundImageSrcFr, sc)
        {
            _twitterCustomTextEn = twitterCustomTextEn;
            _twitterCustomTextFr = twitterCustomTextFr;

            if (pictureSize != null)
            {
                _picture = new PictureBox
                {
                    Size = pictureSize.Value,
                    Location = new Point((PictureBox.Width - pictureSize.Value.Width) / 2, pictureYposition),
                    BackColor = Color.White,
                    Visible = true,
                    SizeMode = PictureBoxSizeMode.StretchImage
                };


                if (showHourGlass)
                {
                    if (!File.Exists(GetTerminalDirectory() + @"photobooth_ui\clock.png"))
                        _picture.ImageLocation =
                            DirectoryUtil.GetLocalNomadLogicAssetsDir() + @"photobooth_ui\clock.png";
                    else
                        _picture.ImageLocation = GetTerminalDirectory() + @"photobooth_ui\clock.png";
                }

                Controls.Add(_picture);

                _picture.BringToFront();
            }

            _twitterShare = twitterShare;
            _facebookShare = facebookShare;

            // Assignation des rectangles.
            _mailArea = mailRectangle;
            _smsArea = smsRectangle;
            _facebookArea = facebookRectangle;
            _twitterArea = twitterRectangle;
            _printArea = printRectangle;
            _nextArea = endRectangle;
        }

        protected virtual void InitBitmap()
        {
            if (_creatingSequence != null)
            {
                _creatingSequence.ClearCurrentImages();
                _creatingSequence.CreateImages(_activeLg);
            }
        }
        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            if (_creatingSequence == null) return true;

            _creatingSequence.ClearCurrentImages();
            _creatingSequence.CreateImages(_activeLg);
            return true;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            InitBitmap();

            /* Mise en place des images inactives */
            if (DontUseInactiveImages) return;

            if (_twitterShare != null)
            {
                _twitterInactiveImage = GetNoneLanguageBitmapFromIbrand("twitter_button_inactive.png", _screenConfig.ProjectId, _screenConfig.QuestionnaireId);
                _twitterShare.OnUploadFinished += OnUploadFinishedTwitter;
            }
            
            if (_facebookShare != null)
            {
                _facebookInactiveImage = GetNoneLanguageBitmapFromIbrand("facebook_button_inactive.png", _screenConfig.ProjectId, _screenConfig.QuestionnaireId);
                _facebookShare.OnUploadFinished += OnUploadFinishedFacebook;
            }

            if (_mailArea != null)
            {
                _mailInactiveImage = GetNoneLanguageBitmapFromIbrand("email_button_inactive.png", _screenConfig.ProjectId, _screenConfig.QuestionnaireId);
            }

            if (_smsArea != null)
            {
                _smsInactiveImage = GetNoneLanguageBitmapFromIbrand("sms_button_inactive.png", _screenConfig.ProjectId, _screenConfig.QuestionnaireId);
            }

            if (_printArea != null)
            {
                _printInactiveImage = GetNoneLanguageBitmapFromIbrand("print_button_inactive.png", _screenConfig.ProjectId, _screenConfig.QuestionnaireId);
            }
        }

        /// Indique qu'il ne faut pas utiliser les images inactives.
        public bool DontUseInactiveImages { get; set; }
        
        /// <summary>
        /// ignore l'assignation de pictureToPrint. empeche d'ecraser la valeur precedente de cette question 
        /// dans la situation d'un ecran precedent ou on a assigne une reponse a cette question
        /// </summary>
        public bool IgnorePictureToPrintQuestion { get; set; }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            if (EndJump.HasValue) Next(EndJump.Value);
            else End();
        }

        /// <summary>
        /// Indique si on envoi un ou plusieurs fichiers vers Facebook (influe sur le choix de end-point web).
        /// </summary>
        public bool SingleFileForFacebook { get; set; }
        
        /// <summary>
        /// Indique si on a une zone pour canceller dans cette page. Nul par défaut.
        /// </summary>
        public Rectangle? CancelArea { get; set; }

        /// <summary>
        /// Séquence d'image optionnelle qui joue lors de la transformation.
        /// </summary>
        public AnimatedImageSequence CreatingImageSequence
        {
            get => _creatingSequence;
            set
            {
                if (_creatingSequence != null)
                {
                    _creatingSequence.StopSequence();
                    _creatingSequence.UpdateDraw -= InvokeRefresh;
                    _creatingSequence.Dispose();
                    _creatingSequence = null;
                }

                _creatingSequence = value;
                if (_creatingSequence != null)
                {
                    _creatingSequence.UpdateDraw += InvokeRefresh;
                }
            }
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            _creatingSequence?.Paint(g);

            if (!_socialBlocked) return;
            ScreenUtil.DrawImage(g, _twitterInactiveImage, _twitterArea);
            ScreenUtil.DrawImage(g, _smsInactiveImage, _smsArea);
            ScreenUtil.DrawImage(g, _mailInactiveImage, _mailArea);
            ScreenUtil.DrawImage(g, _printInactiveImage, _printArea);
            ScreenUtil.DrawImage(g, _facebookInactiveImage, _facebookArea);
        }

        public int MailJump { get; set; } = 1;

        public int SmsJump { get; set; } = 2;

        /// <summary>
        /// Indique qu'au lieu de fermer le questionnaire, on saute quelques écrans par l'avant.
        /// </summary>
        public int? EndJump { get; set; }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            if (_socialBlocked) return false;

            if (InArea(x, y, CancelArea))
            {
                log.Trace("Cancel Area clicked");
                Close();
                return false;
            }

            if(InArea(x, y, _nextArea))
            {
                log.Trace("Next Area clicked");
                if (AutoPrint && _printFinished)
                {
                    _printFinished = false;
                    if (EndJump.HasValue) Next(EndJump.Value);
                    else End();
                }

                if (!AutoPrint)
                {
                    if (EndJump.HasValue) Next(EndJump.Value);
                    else End();
                }

                return false;
            }

            if (InArea(x, y, _printArea))
            {
                log.Trace("Print Area Clicked");
                SetSocialBlock(true);

                log.Trace("Beguin Print sequence.");
                var t = new Thread(SendPrint);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                return false;
            }

            if (InArea(x, y, _mailArea))
            {
                log.Trace("Mail Area clicked");
                SuspendTimer();
                Next(MailJump);
                return false;
            }

            if (InArea(x, y, _smsArea))
            {
                log.Trace("SMS Area clicked");
                SuspendTimer();
                Next(SmsJump);
                return false;
            }

            if (InArea(x, y, _facebookArea))
            {
                log.Trace("Facebook Area clicked");
                if (SingleFileForFacebook) {
                    if (!(_ibrandUploadDone && !_ibrandUploadProblem)
                        || _photoFileName == null || !File.Exists(_photoFileName) || _socialBlocked)
                    {
                        log.Trace(@"Facebook function not enabled. Flags:\n\t-Upload done: {0}\n\t-Upload problem: {1}\n\t-Social block: {2}",
                            _ibrandUploadDone, _ibrandUploadProblem, _socialBlocked);
                        return false;
                    }

                }
                else
                {
                    if (!(_ibrandUploadDone && !_ibrandUploadProblem && !_ibrandThumbnailProblem && _ibrandThumbnailDone)
                        || _photoFileName == null || !File.Exists(_photoFileName) || _socialBlocked)
                    {
                        log.Trace(@"Facebook function not enabled. Flags:\n\t-Upload done: {0}\n\t-Upload problem: {1}\n\t-Social block: {2}",
                            _ibrandUploadDone, _ibrandUploadProblem, _socialBlocked);

                        return false;
                    }
                }

                SuspendTimer();

                _facebookShare.BrowserSize = Size;
                _facebookShare.MultimediaItemUrl = _facebookImageUrl;
                _facebookShare.ThumbnailUrl = SingleFileForFacebook
                    ? _facebookImageUrl
                    : _facebookThumbnailUrl;
                 
                _facebookShare.Language = _activeLg;

                SetSocialBlock(true);

                log.Trace("Beguin Facebook share sequence.");
                var t = new Thread(_facebookShare.DoWork);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                return false;
            }

            if (InArea(x, y, _twitterArea))
            {
                log.Trace("Twitter Area clicked");

                if (_twitterShare == null)
                {
                    log.Warn("No Twitter class assigned to share screen.");
                    return false;
                }

                SuspendTimer();

                if (!_twitterShare.ShareFromRespondentNo)
                {
                    // D'abord GIF, ensuite, Photo Transformée, puis première image.
                    var questionNo = GetQuestionImageNo();

                    var fileName = GetTextAnswerFromQuestion(questionNo);

                    if (!File.Exists(fileName))
                    {
                        log.Warn("No file found for first picture. Twitter sequence aborted.");
                        return false;
                    }

                    _twitterShare.FileName = fileName;
                }
                else
                {
                    if (int.TryParse(GetTextAnswerFromQuestion(_twitterShare.RespondentNoQuestionNo), out var tmp))
                    {
                        _twitterShare.RespondentNo = tmp;
                    }
                    else
                    {
                        log.Warn("No valid respondent ID was in the questionnaire for question no=" + _twitterShare.RespondentNoQuestionNo);
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(_twitterCustomTextEn) && _activeLg.Equals("en"))
                {
                    _twitterShare.CustomText = _twitterCustomTextEn;
                } else if (!string.IsNullOrWhiteSpace(_twitterCustomTextFr))
                {
                    _twitterShare.CustomText = _twitterCustomTextFr;
                }

                SetSocialBlock(true);

                log.Trace("Begin Twitter social share sequence.");
                var t = new Thread(_twitterShare.DoWork);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                return false;
            }

            // On regarde pour les zones de Waiver.
            if (WaiverDialogZones == null || WaiverDialogZones.Length <= 0) return true;
            foreach (var waiverZone in WaiverDialogZones)
            {
                if (!InArea(x, y, waiverZone.ClickZone)) continue;
                var dialog = new WaiverHtmlDialog(waiverZone.HtmlFile.FullName);
                dialog.ShowDialog();
                break;
            }

            return false;
        }

        private int GetQuestionImageNo()
        {

            return _screenConfig.QuestionGifNo ??
                                _screenConfig.QuestionPictureTransformedNo ??
                                _screenConfig.QuestionFirstPictureNo ??
                                ForceTwitterToUseQuestionNo ??
                                - 1;
        }
        
        private void SendPrint()
        {
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var database = ServicesContainer.GetService<PrintStationDatabase>();

                var picture = MainForm.GetInstance().SelectedPicture;

                database.AddPrintCount(picture.ServerPostId);

                string fileToPrint;

                if (_printFileName != null)
                {
                    fileToPrint = _printFileName;
                }
                else if (_screenConfig.QuestionPictureToPrintNo.HasValue)
                {
                    fileToPrint = GetTextAnswerFromQuestion(_screenConfig.QuestionPictureToPrintNo.Value);
                }
                else
                {
                    fileToPrint = DirectoryUtil.GetLocalIbrandTerminalDataDir(ProjectId, QuestionnaireId, TerminalId,
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                        DirectoryUtil.ProjectDataDirectoriesModEnum.print) + StringUtil.GetFileName(picture.Src);

                    if (!File.Exists(fileToPrint))
                    {
                        var result = PrintStationUtil.CreateFramedPhoto(picture.Src, picture, fileToPrint);
                        result.Save(95, fileToPrint);
                        result.Dispose();
                    }
                }

                if (PrintUtil.Print(fileToPrint, null, this))
                {
                    if (_screenConfig.QuestionNumberOfPrintNo != null)
                    {
                        IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value);
                    }

                    NewPrintEvent.GetInstance().NewItemEvent.Set();
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error during print sequence.");
            }

            watch.Stop();
            _printFinished = true;
            if (watch.ElapsedMilliseconds < MINIMUM_PRINT_WAIT_TIME)
            {
                Thread.Sleep(MINIMUM_PRINT_WAIT_TIME - (int)watch.ElapsedMilliseconds);
            }
            SetSocialBlock(false);
        }

        public override void Dispose()
        {
            base.Dispose();

            _creatingSequence?.Dispose();

            Recycle();
        }

        public override void Recycle()
        {
            base.Recycle();

            if (_cts != null && _task != null)
            {
                _cts.Cancel();

                _task.ContinueWith(t => {
                    RecycleLastStep();
                });

                _cts = null;
                _task = null;
            }
            else
            {
                RecycleLastStep();
            }
        }

        private void RecycleLastStep()
        {
            if (_picture != null)
            {
                _picture.Image?.Dispose();
                _picture.Image = null;
            }

            _socialBlocked = false;
            _ibrandUploadDone = false;
            _ibrandUploadProblem = false;
            _facebookImageUrl = null;
            _photoFileName = null;
            _printFileName = null;
        }

        /// <summary>
        /// Action externe supplémentaire pour gérer la situation d'un hypothétique Bundle de fichier.
        /// </summary>
        [Obsolete("Ne pas utiliser si possible. L'assignation de fichier via SendToPrintstation se fait mieux désormais.")]
        public Action<string> AdditionalProcessForFileBundle { get; set; }

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();

            log.Trace("SetActivated Called");

            if (_facebookImageUrl != null) return; // Pour ne pas faire la transformation et l'envoi facebook plusieurs fois.

            SetSocialBlock(false);
            
            try
            {
                if (SaveSelectedPictureToQuestionNo.HasValue)
                {
                    //SaveAnswerToQuestion(SaveSelectedPictureToQuestionNo.Value, 1, GetSelectedPicture().Src);
                }

                IList<string> pics =new List<string>();
                pics.Add(GetSelectedPicture().Src);

                _cts = new CancellationTokenSource();
                _task = Task.Run(() => ImageAndSocialProcessingTask(pics, _cts.Token), _cts.Token);

            }
            catch (Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        /// <summary>
        /// Avec un questionnaire, on ne fait pas de transformation de cadre sur l'image automatiquement. Si cette propriété est vrai, on va
        /// créer une version de l'image uniquement à des fins d'impression. Si l'option d'écran "QuestionPictureToPrintNo" existe, on va sauvegarder
        /// cette image dans cette question. Sinon aucune modification permanente au questionnaire par rapport à l'image modifiée ne sera faite.
        /// </summary>
        public bool ApplyTransformationToSelectedImage { get; set; }

        /// <summary>
        /// Indique qu'il faut sauvegarder l'image sélectionnée dans le numéro de question donné en paramètre lors
        /// du chargement de la page.
        /// </summary>
        [Obsolete] // Utiliser SaveSelectedPictureToQuestionNo de questionnaireUtils 2019-10-24
        public int? SaveSelectedPictureToQuestionNo { get; set; }

        /// <summary>
        /// Zones où il est possible de faire apparaître un dialogue "Waiver". Chaque zone point vers un fichier.
        /// </summary>
        public WaiverDialogClickZone[] WaiverDialogZones { get; set; }

        /// <summary>
        /// force twitter a utiliser un numéro de question spécifique comme source d'image.
        /// 
        /// </summary>
        public int? ForceTwitterToUseQuestionNo { get; set; }
        
        protected void OnUploadFinishedIbrand(bool success, Dictionary<string, string> results)
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
                    log.Trace("Problem during iBrand upload sequence. File missing.");
                    _ibrandUploadProblem = true;
                }
                else 
                {
                    log.Trace("Beguin facebook thumbnail generation.");
                    // On obtient le nom du fichier téléversé (n'est pas le même que le fichier d'origine pour empêcher les doublons).
                    _thumbnailFileName = _facebookImageUrl.Substring(_facebookImageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    // On tente ensuite de chercher le lien URL du thumbnail en utilisant GetFacebookPictureURL
                    /*
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
                log.Trace("Problem during iBrand upload sequence.");
                _ibrandUploadProblem = true;
            }

            Refresh();
        }

        protected void OnUploadOneFileFinishedIbrand(bool success, string result)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegateIbrandOneFile(OnUploadOneFileFinishedIbrand), success, result);
                return;
            }

            if (success && result != null)
            {
                _ibrandUploadDone = true;


                _facebookImageUrl = result;

            }
            else
            {
                log.Trace("Problem during iBrand upload sequence.");
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
                    log.Trace("Problem during thumbnail generation sequence. Thumbnail url missing.");
                    _ibrandThumbnailProblem = true;
                }
                else
                {
                    log.Trace("Thumbnail generation completed successfully.");
                }
            }
            else
            {
                log.Trace("Problem during thumbnail generation sequence.");
                _ibrandThumbnailProblem = true;
            }

            SetSocialBlock(false);
        }

        private void OnUploadFinishedFacebook(bool success)
        {
            log.Trace("Facebook share sequence completed with signal: {0}", success);

            SetSocialBlock(false);
            // Ne pas faire un résumé de timer ici. Il y a des possibilité de bogue avec les appels de pseudo modal, et
            // de toute façon, le timer est réactivé avec le "setActivated" quand on revient sur l'écran de partage
            //ResumeTimer();

            if (!success)
            {
                return;
            }

            if (_screenConfig.QuestionFacebookNo == null) return;

            log.Trace("Incrementing Facebook question number.");
            IncrementEventInQuestionnaire(_screenConfig.QuestionFacebookNo.Value);
        }

        private void OnUploadFinishedTwitter(bool success)
        {
            log.Trace("Twitter share sequence completed with signal: {0}", success);

            SetSocialBlock(false);
            // Ne pas faire un résumé de timer ici. Il y a des possibilité de bogue avec les appels de pseudi modal, et
            // de toute façon, le timer est réactivé avec le "setActivated" quand on revient sur l'écran de partage
            //ResumeTimer();

            if (!success) return;

            if (_screenConfig.QuestionTwitterNo == null) return;

            log.Trace("Incrementing Twitter question number.");
            IncrementEventInQuestionnaire(_screenConfig.QuestionTwitterNo.Value);
        }

        protected void SetSocialBlock(bool enableBlock)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { SetSocialBlock(enableBlock); }));
                return;
            }

            _socialBlocked = enableBlock;
            Refresh();
            log.Trace("Social block Set to {0}", enableBlock);
        }

        private async Task ImageAndSocialProcessingTask(IList<string> pictures, CancellationToken ct)
        {
            try
            {
                _creatingSequence?.StartSequence();

                if (!ct.IsCancellationRequested)
                {
                    await Task.Run(() => { LaunchTransformationSequenceDo(pictures); }, ct);
                }
            
                if (!_backgroundWorkActive && !(ct.IsCancellationRequested))
                {
                    log.Trace("Initializing Share Screen with new image.");
                    if (_photoFileName != null) return;

                    var pictureInfo = GetSelectedPicture();

                    _photoFileName = pictureInfo.Src;

#pragma warning disable 618
                    AdditionalProcessForFileBundle?.Invoke(_photoFileName);
#pragma warning restore 618

                    if (ApplyTransformationToSelectedImage)
                    {
                        var user = ((PrintStationScreenConfig) _screenConfig).PrintStationUser;

                        var image = FileUtil.LoadBitmap(_photoFileName);

                        var minSize = Math.Min(image.Height, image.Width);
                        var croppedPicture = image.Crop(minSize, minSize);

                        var tmpFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId()) + ".jpg";
                        croppedPicture.Save(95, tmpFile);
                        croppedPicture.Dispose();
                        image.Dispose();

                        PrintStationUtil.TransformPicture(user, pictureInfo, tmpFile, 0, out _printFileName);
                    }
                    else
                    {
                        _printFileName = null; // SI null, on prendra l'image originale pour l'impression.
                    }
                    
                    // Si on a une information de première image (image d'affichage), on utilise l'information venant du questionnaire.

                    if (_screenConfig.QuestionGifNo.HasValue)
                    {
                        _photoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionGifNo.Value);
                    }
                    else if (_screenConfig.QuestionFirstPictureNo.HasValue)
                    {
                        _photoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionFirstPictureNo.Value);
                    }

                    if (_picture != null)
                    {
                        _picture.Image?.Dispose();
                        _picture.Image = null;

                        if (CropRectangle != null)
                        {
                            var x = CropRectangle.Value.X;
                            var y = CropRectangle.Value.Y;
                            var h = CropRectangle.Value.Height;
                            var w = CropRectangle.Value.Width;
                            var img = GetSelectedPicture();
                            _printFileName = img.Src; 
                            var bmp = FileUtil.LoadBitmap(img.Src);
                            _picture.Image = bmp.Crop(new Point(x,y),w,h);
                        }
                        else
                        {
                            var ans = GetCurrentQuestionnaire().GetQuestionAnswers(PreviewPictureQuestion);
                            if (ans != null && ans.Any()) _picture.ImageLocation = ans[0].Response;
                        }
                    }
                    // On n'envoi pas l'information thumbnail Facebook si on n'a pas de fonction Facebook.
                    if (_facebookArea != null)
                    {
                        /*
                        // Envoi de l'image vers notre serveur
                        var files = new[] {new FileInfo(_photoFileName)};
                        var uploadPicture = new UploadFilesToIbrandQuestionnaire(files,
                            _screenConfig.ProjectId.ToString(),
                            _screenConfig.QuestionnaireId.ToString(), "facebook", null, SingleFileForFacebook);

                        if (!SingleFileForFacebook)
                        {
                            uploadPicture.OnUploadFinished += OnUploadFinishedIbrand;
                        }
                        else
                        {
                            uploadPicture.OnUploadOneFileFinished += OnUploadOneFileFinishedIbrand;
                        }

                        var transfo = new Thread(uploadPicture.DoWork);
                        transfo.SetApartmentState(ApartmentState.STA);
                        transfo.Start();
                        log.Trace("Sending image to server to prepare for Facebook social sharing.");
                        */
                    }
                }

                if (!_backgroundWorkActive && !ct.IsCancellationRequested && AutoPrint)
                {
                    log.Trace("Print Area Clicked");
                    SetSocialBlock(true);

                    log.Trace("Beguin Print sequence.");
                    var print = new Thread(SendPrint);
                    print.SetApartmentState(ApartmentState.STA);
                    print.Start();
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            finally
            {
                _creatingSequence?.StopSequence();
               
            }
        }
        
        /// <summary>
        /// Contient l'ensemble des options de transformations avec des fichiers d'image.
        /// Si cette variable n'est pas explicitement donnée, on tente de lire un fichier dans le répertoire du projet, qu'il existe ou non, et on met à jour
        /// la configuration selon le contenu de ce fichier.
        /// </summary>
        public ImageListTransformOptions ImageListTranformOptions
        {
            get
            {
                if (_imageListTransform != null) return _imageListTransform;

                var transform = new ImageListTransformOptions();
                JsonUtil.UpdateFromJson(
                    new FileInfo(GetTerminalDirectory() + transform.DefaultConfigFileName),
                    transform);
                return transform;
            }
            set => _imageListTransform = value;
        }

        /// <summary>
        /// Sauvegarde l'image prise par la caméra, avant transformations, dans un numéro de question.
        /// </summary>
        public int? SaveOriginalImageToQuestion { get; set; }

        private void LaunchTransformationSequenceDo(IList<string> pictures)
        {
            Exception error = null;
            try
            {
                _backgroundWorkActive = true;
                if (SaveOriginalImageToQuestion.HasValue)
                {
                    var copyPath = DirectoryUtil.GetLocalIbrandTerminalDataDir(ProjectId,QuestionnaireId,TerminalId,
                                       DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                       DirectoryUtil.ProjectDataDirectoriesModEnum.none) + GenerateUniqueId.GetId() +
                                   ".jpg";

                    File.Copy(pictures[0], copyPath);
                    //SaveAnswerToQuestion(SaveOriginalImageToQuestion.Value, 1, copyPath, 11);
                }

                ImageListTransform.TransformImageList(this, pictures, _screenConfig, ImageListTranformOptions);
            }
            catch (Exception e)
            {
                log.Error(e);
                error = e;
            }
            finally
            {
                _creatingSequence?.StopSequence();
                _backgroundWorkActive = false;
            }

            if (error != null)
            {
                var result = MessageBox.Show(
                    @"There was an error during transformation. Do you wish to continue?\nMessage:\n" + error.Message,
                    @"iBrand TM", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes) Next();
            }
           
        }
    }
}
