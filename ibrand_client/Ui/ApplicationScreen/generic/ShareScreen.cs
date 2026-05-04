using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.Social.Facebook;
using NomadLogic.iBrandDesktop.Util.Social.Twitter;

// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Classe complexe pour gérer les envois vers les options sociales une fois les questions essentielles du questionnaire fait par les autres écrans.
    /// </summary>
    public class ShareScreen : AbstractScreen
    {
        private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);
        private delegate void UiDelegateIbrandOneFile(bool success, string result);

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly PictureBox _picture;
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
        private string _facebookImageUrl;
        private string _facebookThumbnailUrl;
        private string _photoFileName;

        private readonly Bitmap _twitterInactiveImage;
        private readonly Bitmap _facebookInactiveImage;
        private readonly Bitmap _mailInactiveImage;
        private readonly Bitmap _smsInactiveImage;
        private readonly Bitmap _printInactiveImage;
        //private readonly Bitmap _printImageClick;
        //private Bitmap _printBtnImg;
        private bool _socialBlocked;
        private readonly Dictionary<string, string> _twitterCustomText;
        private Bitmap _checkmark;
        private readonly List<ScreenCheckOption> _zones = new List<ScreenCheckOption>();
        /// Empêche d'imprimer deux fois le même fichier pour un même répondant sur une impression sur activation.
        private bool _earlyPrintSent;

        private ShareSocialOption _optionsUsed;
        //private UploadFilesToIbrandQuestionnaire _uploadPicture;
        //private GetFacebookPictureUrl _thumbnailUpload;
        private Bitmap _facebookImageWait;
        private int _photoFileNameWidth;
        private int _photoFileNameHeight;

        [NotNull] public AdvancedScreenPrintConfiguration PrintoutCounter { get; set; } = new AdvancedScreenPrintConfiguration();
        private int _indexPrint;
        public bool _print;


        /// <summary>
        /// Fichier spécifique pour le "checkmark" des options "checks", si existantes.
        /// </summary>
        public string CheckmarkImagePath { get; set; }

        /// <summary>
        /// force l'envoie a facebook d'un seul fichier. par defaut false
        /// </summary>
        public bool SingleFileForFacebook { get; set; }

        /// <summary>
        /// Écran final pour les options de partage social. Par définition, toutes ces options sont optionnelles et le prochain écran est 
        /// toujours la fin du questionnaire. À cet effet, si le "timer" s'écoule dans cette page, on ferme le questionnaire, on ne revient pas au premier
        /// écran comme c'est le cas avec la plupart des autre écrans.
        /// Aussi, cet écran envoi une version "thumbnail" de L'image sur notre serveur immédiatement à fins de visualisation par les différenets services sociaux
        /// (notamment Facebook), car à cette étape il n'existe pas encore de répondant sur le serveur.
        /// Les actions de SMS et de Courriel vont tout deux envoyer des écrans plus loin, écrans qui ne servent qu'à saisir l'information et à revenir à cet écran. Il est
        /// important de bien indique quel est l'intervalle d'écran à naviguer (avant) et utilise le même nombre pour chaque action quand on détermine les écrans par la suite.
        /// Habituellement on utilise 1 pour le courriel et 2 pour le SMS. mais quand on n'utilise que le SMS, il faut utiliser la valeur 1, car il n'y a pas d'élément nul dans la
        /// liste d'écrans.
        /// Chaque option principale (Twitter, Facebook, SMS, Courriel et Impression) possède une image par défaut inactive, qui se met par dessus chaque option quand on désactive
        /// les boutons (ce qui arrive à chaque fois que l'on active une option de partage, même si l'effet est presque invisiable à l'oeil dans certains cas). Ces boutons
        /// se dessinent à partir du même endroit que la zone d'action de chaque option. Si on ne veut pas utiliser les images de base, il faut les remplacer dans les images
        /// custom du projet dans le répertoire "photobooth_ui". Leur nom est fixe, et donc il est important de bien utiliser les mêmes nom de fichier.
        /// </summary>
        /// <param name="backgroundImageSrc">L'image de fond</param>
        /// <param name="sc">Les paramètre d'écran.</param>
        /// <param name="facebookShare">L'objet de partage social pour facebook. Optionnel.</param>
        /// <param name="twitterShare">L'objet de partage social pour twitter. Optionnel.</param>
        /// <param name="twitterCustomText">Le texte à envoyer avec Twitter. Optionnel.</param>
        /// <param name="printRectangle">La zone d'action pour l'impression. Optionnel.</param>
        /// <param name="mailRectangle">La zone d'action pour le courriel. Optionnel.</param>
        /// <param name="smsRectangle">La zone d'action pour le SMS.</param>
        /// <param name="facebookRectangle">La zone d'action pour Facebook.</param>
        /// <param name="twitterRectangle">La zone d'action pour Twitter.</param>
        /// <param name="endRectangle">La zone pour terminer le répondant.</param>
        /// <param name="pictureYposition">L'espace entre le haut et l'image de présentation. Elle est toujours centrée.</param>
        /// <param name="pictureSize">La taille de l'image de présentation.</param>
        /// <param name="backgroundImageFrSrc">Image de fond en français</param>
        public ShareScreen(
            string backgroundImageSrc,
            ScreenConfig sc,
            ShareUrlToFacebookPseudomodal facebookShare,
            UploadPictureToTwitterLeNomadPseudomodal twitterShare,
            Dictionary<string, string> twitterCustomText,
            Rectangle? printRectangle,
            Rectangle? mailRectangle,
            Rectangle? smsRectangle,
            Rectangle? facebookRectangle,
            Rectangle? twitterRectangle,
            Rectangle endRectangle,
            int pictureYposition,
            Size pictureSize,
            string backgroundImageFrSrc = null)
            : this(backgroundImageSrc, sc, facebookShare, twitterShare, twitterCustomText, printRectangle, mailRectangle,
                  smsRectangle, facebookRectangle, twitterRectangle, endRectangle, new Rectangle(new Point(-1, pictureYposition), pictureSize), null, backgroundImageFrSrc)
        {
            // rien de plus
        }

        /// <summary>
        /// Écran final pour les options de partage social. Par définition, toutes ces options sont optionnelles et le prochain écran est 
        /// toujours la fin du questionnaire. À cet effet, si le "timer" s'écoule dans cette page, on ferme le questionnaire, on ne revient pas au premier
        /// écran comme c'est le cas avec la plupart des autre écrans.
        /// Aussi, cet écran envoi une version "thumbnail" de L'image sur notre serveur immédiatement à fins de visualisation par les différenets services sociaux
        /// (notamment Facebook), car à cette étape il n'existe pas encore de répondant sur le serveur.
        /// Les actions de SMS et de Courriel vont tout deux envoyer des écrans plus loin, écrans qui ne servent qu'à saisir l'information et à revenir à cet écran. Il est
        /// important de bien indique quel est l'intervalle d'écran à naviguer (avant) et utilise le même nombre pour chaque action quand on détermine les écrans par la suite.
        /// Habituellement on utilise 1 pour le courriel et 2 pour le SMS. mais quand on n'utilise que le SMS, il faut utiliser la valeur 1, car il n'y a pas d'élément nul dans la
        /// liste d'écrans.
        /// Chaque option principale (Twitter, Facebook, SMS, Courriel et Impression) possède une image par défaut inactive, qui se met par dessus chaque option quand on désactive
        /// les boutons (ce qui arrive à chaque fois que l'on active une option de partage, même si l'effet est presque invisiable à l'oeil dans certains cas). Ces boutons
        /// se dessinent à partir du même endroit que la zone d'action de chaque option. Si on ne veut pas utiliser les images de base, il faut les remplacer dans les images
        /// custom du projet dans le répertoire "photobooth_ui". Leur nom est fixe, et donc il est important de bien utiliser les mêmes nom de fichier.
        /// </summary>
        /// <param name="backgroundImageSrc">L'image de fond</param>
        /// <param name="sc">Les paramètre d'écran.</param>
        /// <param name="facebookShare">L'objet de partage social pour facebook. Optionnel.</param>
        /// <param name="twitterShare">L'objet de partage social pour twitter. Optionnel.</param>
        /// <param name="twitterCustomText">Le texte à envoyer avec Twitter. Optionnel.</param>
        /// <param name="printRectangle">La zone d'action pour l'impression. Optionnel.</param>
        /// <param name="mailRectangle">La zone d'action pour le courriel. Optionnel.</param>
        /// <param name="smsRectangle">La zone d'action pour le SMS.</param>
        /// <param name="facebookRectangle">La zone d'action pour Facebook.</param>
        /// <param name="twitterRectangle">La zone d'action pour Twitter.</param>
        /// <param name="endRectangle">La zone pour terminer le répondant.</param>
        /// <param name="preview">Rectangle de position pour l'Image de preview. Si la valeur X est inférieure à 0, on calcule au centre.</param>
        /// <param name="optInOptions">la liste des zones de checkbox. L'option obligatoire ne sera pas pris en compte car cela contrevient au contrat de cet ecran.</param>
        /// <param name="backgroundImageFrSrc">Image de fond en français</param> 
        public ShareScreen(string backgroundImageSrc, ScreenConfig sc,
            ShareUrlToFacebookPseudomodal facebookShare, UploadPictureToTwitterLeNomadPseudomodal twitterShare,
            Dictionary<string, string> twitterCustomText, Rectangle? printRectangle, Rectangle? mailRectangle, Rectangle? smsRectangle,
            Rectangle? facebookRectangle, Rectangle? twitterRectangle, Rectangle endRectangle, Rectangle? preview = null,
            IEnumerable<ScreenCheckOption> optInOptions = null, string backgroundImageFrSrc = null)
            : base(backgroundImageSrc, backgroundImageFrSrc ?? backgroundImageSrc, sc)
        {
            if (optInOptions != null)
            {
                _zones.AddRange(optInOptions);
            }
            
            _twitterCustomText = twitterCustomText;
            
            if (preview.HasValue)
            {
                var rect = new Rectangle(preview.Value.Location, preview.Value.Size); // copie
                if (rect.X < 0) rect.X = (PictureBox.Width - rect.Width) / 2;
                _picture = new PictureBox
                {
                    Size = rect.Size,
                    Location = rect.Location,
                    BackColor = Color.White,
                    Visible = true,
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                Controls.Add(_picture);

                _picture.BringToFront();
            }

            /* Mise en place des images inactives */
            _twitterShare = twitterShare;
            if (_twitterShare != null)
            {
                _twitterInactiveImage = GetNoneLanguageBitmap("twitter_button_inactive.png");
                _twitterShare.OnUploadFinished += OnUploadFinishedTwitter;
            }

            _facebookShare = facebookShare;
            if (_facebookShare != null)
            {
                _facebookInactiveImage = GetNoneLanguageBitmap("facebook_button_inactive.png");
                _facebookShare.OnUploadFinished += OnUploadFinishedFacebook;
            }

            if (mailRectangle != null)
            {
                _mailInactiveImage = GetNoneLanguageBitmap("email_button_inactive.png");
            }

            if (smsRectangle != null)
            {
                _smsInactiveImage = GetNoneLanguageBitmap("sms_button_inactive.png");
            }

            if (printRectangle != null)
            {

                //_printImageClick = GetBitmap("print_button_click.png")??GetNoneLanguageBitmap("print_button_click.png");

                _printInactiveImage = GetNoneLanguageBitmap("print_button_inactive.png");

               // _printBtnImg = _printImageClick;

            }

            // Assignation des rectangles.
            _mailArea = mailRectangle;
            _smsArea = smsRectangle;
            _facebookArea = facebookRectangle;
            _twitterArea = twitterRectangle;
            _printArea = printRectangle;
            _nextArea = endRectangle;
        }
        
        /// <summary>
        /// Indique s'il faut sauter un nombre X d'écrans au lieu de simplement terminer. NULL par défaut.
        /// Cette valeur agit sur un timeout et sur l'action de Next.
        /// Si la valeur est non-nulle mais inférieure à un, on agit comme si c'était nul.
        /// </summary>
        public int? OverrideEndBySkip { get; set; }

        internal void CheckForEndOrSkip()
        {
            ClearEventsFromUploadReferences();

            if (OverrideEndBySkip.HasValue && OverrideEndBySkip.Value > 0)
            {
                Next(OverrideEndBySkip.Value);
            }
            else
            {
                End(); // On ne renvoi pas au premier écran, on termine.
            }
        }

        private void ClearEventsFromUploadReferences()
        {
            // TODO Activier partage facebook à partir du client.
            /*
            if (_uploadPicture != null)
            {
                _uploadPicture.OnUploadFinished -= OnUploadFinishedIbrand;
                _uploadPicture.OnUploadOneFileFinished -= OnUploadOneFileFinishedIbrand;
                _uploadPicture = null;
            }

            if (_thumbnailUpload != null)
            {
                _thumbnailUpload.OnQueryFinished -= OnThumbnailFinished;
                _thumbnailUpload = null;
            }
            */
        }

        /// <inheritdoc/>
        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            CheckForEndOrSkip();
        }

        /// <summary>
        /// Indique qu'il ne faut pas afficher les images inactives quand les fonctions sociales devraient être
        /// désactivée. Mettre cette option à faux va faire en sorte que les boutons ne changeront pas d'aspect quand inutilisables.
        /// </summary>
        public bool DontShowInactiveImages { get; set; }

        /// <summary>
        /// Contient des options d'affichage et d'action selon si certaines options sociales ont été choisie par l'usager.
        /// Une fois que l'on revient, avec succès ou non, des options de partage, on peut afficher certains boutons supplémentaires,
        /// ou même déclencher des actions selon les états choisis.
        /// Les zones de click actives ont précédence sur les boutons existants (et peuvent donc remplacer la zone de clic).
        /// </summary>
        public IList<ShareScreenOptionTrigger> OptionalSharingTriggers { get; set; }

        #region Overrides of UserControl

        /// <inheritdoc />
        protected override void OnLoad()
        {
            base.OnLoad();

            if (_zones.Any())
            {
                _checkmark = string.IsNullOrWhiteSpace(CheckmarkImagePath) ? 
                    GetNoneLanguageBitmap("checkmark.png") : 
                    FileUtil.LoadBitmap(CheckmarkImagePath);
            }

            if (PreviewSizeMode.HasValue && _picture != null) _picture.SizeMode = PreviewSizeMode.Value;

            if (FacebookUploadWaitImagePath != null && FacebookUploadWaitImagePoint != null) {

                if (!File.Exists(FacebookUploadWaitImagePath))
                    log.Error("Could not find file: " + FacebookUploadWaitImagePath);

                _facebookImageWait = FileUtil.LoadBitmap(FacebookUploadWaitImagePath);
            }
            
            if (OptionalSharingTriggers == null || !OptionalSharingTriggers.Any()) return;

            using (var iter = OptionalSharingTriggers.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var trig = iter.Current;
                    if (trig != null) trig.Bitmap = FileUtil.LoadBitmap(trig.ImageFile);
                }
            }
        }

        /// <summary>
        /// Indique que le PictureBox de la prévisualisation à un mode d'affichage qui diffère du mode par défaut, qui est
        /// PictureBoxSizeMode.StretchImage
        /// </summary>
        public PictureBoxSizeMode? PreviewSizeMode { get; set; }

        /// <summary>
        /// Indique le chemin d'un fichier d'image optionnel si un veut afficher un message lors du chargement de Facebook.
        /// Doit être utilisé avec la propriété "FacebookUploadWaitImagePoint" si on veut placer l'image.
        /// </summary>
        public string FacebookUploadWaitImagePath { get; set; }

        /// <summary>
        /// Endroit où placer l'image d'attente Facebook, si utilisée.
        /// </summary>
        public Point? FacebookUploadWaitImagePoint { get; set; }

        #endregion

        /// <inheritdoc/>
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            PaintCheckBoxList(g, _zones, _checkmark);
            
            if (OptionalSharingTriggers != null) {
                // Affichage des triggers.
                foreach (var trig in OptionalSharingTriggers) {
                    if (trig.CheckFlags(_optionsUsed) && trig.ClickZone.HasValue)
                    {
                        DrawImage(g, trig.Bitmap, trig.ClickZone.Value);
                    }
                }
            }

            if (_printArea!=null && _print)
            {
                PrintoutCounter.Draw(g,_indexPrint);
                _print = false;
            }

            // Boutons inactifs
            if (DontShowInactiveImages) return;
            
            if (_socialBlocked || (_photoFileName != null && File.Exists(_photoFileName) &&
                !(_ibrandUploadDone && !_ibrandUploadProblem && !_ibrandThumbnailProblem && _ibrandThumbnailDone)))
            {
                if (_facebookArea != null) {
                    DrawImage(g, _facebookInactiveImage, _facebookArea.Value);
                }

                if (FacebookUploadWaitImagePath != null && FacebookUploadWaitImagePoint != null) {
                    DrawImage(g, _facebookImageWait, FacebookUploadWaitImagePoint.Value);
                }
            }
            
            // Toutes les options suivantes sont dépendantes uniquement de savoir si les options sociales sont bloquées.
            if (!_socialBlocked) return;
            DrawImage(g, _printInactiveImage, _printArea);
            DrawImage(g, _twitterInactiveImage, _twitterArea);
            DrawImage(g, _smsInactiveImage, _smsArea);
            DrawImage(g, _mailInactiveImage, _mailArea);
        }

        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (OptionalSharingTriggers != null)
            {
                foreach (var trig in OptionalSharingTriggers)
                {
                    if (trig.ClicAction == null) continue;
                    if (!InArea(x, y, trig.ClickZone) || !trig.CheckFlags(_optionsUsed)) continue;
                    // Si on a cliqué sur la zone, que la condition est remplie ET que l'on possède une action à exécuter sur Clic
                    trig.ClicAction.Invoke();
                    return false; // Une seule action par clic.
                }
            }

            if (InArea(x, y, _nextArea))
            {
                if (_screenConfig.QuestionNumberOfPrintNo != null)
                {
                    SaveAnswerToQuestion(new Answer
                        {
                            QuestionNo = _screenConfig.QuestionNumberOfPrintNo.GetValueOrDefault(),
                            ChoiceNo = 1,
                            Response = _indexPrint.ToString()
                        });
                }

                if (_socialBlocked)
                {
                    log.Trace("Next function currently blocked. Aborting print sequence.");
                    return false;
                }

                log.Trace("Next Area clicked");
                CheckForEndOrSkip();
                return false;
            }

            if (InArea(x, y, _printArea))
            {

               // _printBtnImg = _printInactiveImage;
               //DontShowInactiveImages = false;
                

                log.Trace("Print Area Clicked");

                if (_socialBlocked)
                {
                    log.Trace("Social function currently blocked. Aborting print sequence.");
                    return false;
                }

                if (PrintoutCounter.MaxPrintOuts != -1 && _indexPrint == PrintoutCounter.MaxPrintOuts)
                {
                    var mBox = new MessageDialog();               
                    if(GetCurrentQuestionnaire().LanguageCurrent.Equals("en"))
                    {
                        mBox.ShowMessage("You have reached the maximum allowed for printouts.");
                        return false;
                    }
                    
                    mBox.ShowMessage("Vous avez atteint le seuil maximal pour l'impression.");
                    return false;
                }
                //SetSocialBlock(true);
                SendPrintThread();
                _optionsUsed |= ShareSocialOption.Print;
                CheckTriggersFlag();
                return false;
            }

            if (InArea(x, y, _mailArea))
            {
                log.Trace("Mail Area clicked");

                if (_socialBlocked)
                {
                    log.Trace("Social function currently blocked. Aborting Mail sequence.");
                    return false;
                }

                SuspendTimer();
                Next(MailScreenSkip);
                _optionsUsed |= ShareSocialOption.Email;
                return false;
            }

            if (InArea(x, y, _smsArea))
            {
                log.Trace("SMS Area clicked");

                if (_socialBlocked)
                {
                    log.Trace("Social function currently blocked. Aborting SMS sequence.");
                    return false;
                }

                SuspendTimer();
                Next(SmsScreenSkip);
                _optionsUsed |= ShareSocialOption.Sms;
                return false;
            }

            if (InArea(x, y, _facebookArea))
            {
                log.Trace("Facebook Area clicked");
                if (SingleFileForFacebook)
                {
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
                        log.Trace(@"Facebook function not enabled. Flags:\n\t-Upload done: {0}\n\t-Upload problem: {1}\n\t-Thumbnail done: {2}\n\t-Thumbnail problem: {3}\n\t-Social block: {4}",
                            _ibrandUploadDone, _ibrandUploadProblem, _ibrandThumbnailDone, _ibrandThumbnailProblem, _socialBlocked);
                        return false;
                    }
                }

                SuspendTimer();

                _facebookShare.BrowserSize = Size;
                _facebookShare.MultimediaItemUrl = _facebookImageUrl;

                _facebookShare.ThumbnailUrl = SingleFileForFacebook
                    ? _facebookThumbnailUrl
                    : _facebookImageUrl;

                _facebookShare.Language = _activeLg;
                _facebookShare.ImageWidth = _photoFileNameWidth;
                _facebookShare.ImageHeight = _photoFileNameHeight;
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

                if (_socialBlocked)
                {
                    log.Trace("Social block enabled. Twitter sequence aborted.");
                    return false;
                }

                SuspendTimer();

                // D'abord GIF, ensuite, Photo Transformée, puis première image.
                var questionNo = GetQuestionImageNo();

                var fileName = GetTextAnswerFromQuestion(questionNo);

                if (!File.Exists(fileName))
                {
                    log.Trace("No file found for first picture. Twitter sequence aborted.");
                    return false;
                }

                _twitterShare.FileName = fileName;

                if (_twitterCustomText != null)
                {
                    _twitterCustomText.TryGetValue(_activeLg, out var text);

                    if (text == null && _twitterCustomText.Count > 0)
                    {
                        // On prend la langue par défaut dans ce cas.
                        text = _twitterCustomText.First().Value;
                    }

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        _twitterShare.CustomText = text;
                    }
                }

                SetSocialBlock(true);

                log.Trace("Begin Twitter social share sequence.");
                var t = new Thread(_twitterShare.DoWork);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                return false;
            }

            // On regarde pour les zones de Waiver.
            if (WaiverDialogZones != null && WaiverDialogZones.Length > 0)
            {
                foreach (var waiverZone in WaiverDialogZones)
                {
                    if (!InArea(x, y, waiverZone.ClickZone)) continue;
                    var dialog = new WaiverHtmlDialog(waiverZone.HtmlFile.FullName);
                    dialog.ShowDialog();
                    return false;
                }
            }

            return EvaluateCheckBoxsMouseDown(x, y, _zones);
        }

        private void SendPrintThread(bool dontUnblockSocial = false)
        {
            _indexPrint++;
            if (!dontUnblockSocial) SetSocialBlock(true);

            log.Trace("Begin Print sequence.");
            var t = new Thread(() => { SendPrint(dontUnblockSocial); });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            _print = true;
        }

        /// <summary>
        /// Zones où il est possible de faire apparaître un dialogue "Waiver". Chaque zone point vers un fichier.
        /// </summary>
        [PublicAPI]
        public WaiverDialogClickZone[] WaiverDialogZones { get; set; }

        /// <summary>
        /// Nombre d'écran à sauter pour aller à l'écran de saisie du numéro de téléphone (ou SMS). 2 par défaut.
        /// </summary>
        public int SmsScreenSkip { get; set; } = 2;

        /// <summary>
        /// Nombre d'écran à sauter pour aller à l'écran de saisie de courriel. 1 par défaut.
        /// </summary>
        public int MailScreenSkip { get; set; } = 1;

        /// <summary>
        /// Indique si on doit tenter une impression de fichier dès que l'on arrive sur cette page.
        /// Si le numéro de question pour l'incrémentation du nombre d'impression existe, il sera aussi incrémenté.
        /// </summary>
        public bool ForcePrintOnActivation { get; set; }

        private int GetQuestionImageNo()
        {
            return _screenConfig.QuestionGifNo ??
                                _screenConfig.QuestionPictureTransformedNo ??
                                _screenConfig.QuestionFirstPictureNo ??
                                _screenConfig.QuestionPictureToPrintNo ??
                                -1;
        }

        private void SendPrint(bool dontTouchBlock = false)
        {
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                if (_screenConfig.QuestionPictureToPrintNo == null)
                {
                    log.Warn("No print question given. Aborting print sequence.");
                }
                else
                {
                    if (!PrintAnswerFile(_screenConfig.QuestionPictureToPrintNo.Value, 1))
                    {
                        log.Warn("Could not send to printer spool. Aborting print sequence.");
                    }
                    else if (_screenConfig.QuestionNumberOfPrintNo != null)
                    {
                        log.Trace("Incrementing Print number question.");
                        IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value);
                    }
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during print sequence.");
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds < PrintoutCounter.PrintTimeout)
            {
                Thread.Sleep(PrintoutCounter.PrintTimeout - (int)watch.ElapsedMilliseconds);
            }

            //_printBtnImg = _printImageClick;
            //DontShowInactiveImages = true;
            if (!dontTouchBlock) 
                SetSocialBlock(false);
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();

            ClearEventsFromUploadReferences();
            
            foreach (var zone in _zones)
            {
                zone.Recycle();
            }
            if (_picture != null)
            {
                _picture.Image?.Dispose();
                _picture.Image = null;
            }

            _earlyPrintSent = false;
            _socialBlocked = false;
            _thumbnailFileName = null;
            _ibrandUploadDone = false;
            _ibrandThumbnailDone = false;
            _ibrandUploadProblem = false;
            _ibrandThumbnailProblem = false;
            _facebookImageUrl = null;
            _facebookThumbnailUrl = null;
            _photoFileName = null;
            _photoFileNameWidth = 0;
            _photoFileNameHeight = 0;
            _indexPrint = 0;

            // Remet tous les drapeaux à rien du tout.
            _optionsUsed = 0;
            if (OptionalSharingTriggers == null || !OptionalSharingTriggers.Any()) return;
            // Remet les drapeaux d'exécutions à faux.
            using (var iter = OptionalSharingTriggers.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var trig = iter.Current;
                    if (trig != null) trig.HasTriggered = false;
                }
            }
        }

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();

            Invalidate();

            log.Trace("SetActivated Called");

            if (_facebookImageUrl != null) return; // Pour ne pas faire la transformation et l'envoi facebook plusieurs fois.

            SetSocialBlock(false);

            try
            {
                // Vérifie l'état des flags
                CheckTriggersFlag();

                log.Trace("Initializing Share Screen with new image.");
                if (_photoFileName != null) return;

                _photoFileName = GetTextAnswerFromQuestion(GetQuestionImageNo());

                if (!File.Exists(_photoFileName))
                {
                    _photoFileName = null;
                    return;
                }

                if (_picture != null)
                {
                    _picture.Image?.Dispose();
                    var bmp = FileUtil.LoadBitmap(_photoFileName);
                    _picture.Image = !PreviewCrop ? bmp : bmp.Frame(_picture.Width, _picture.Height);

                    _photoFileNameWidth = bmp.Width;
                    _photoFileNameHeight = bmp.Height;
                }

                // On fait l'envoi vers le serveur et le thumbnail UNIQUEMENT si on en a besoin I.E. Si facebook est actif!
                /*
                if (_facebookArea != null)
                {
                    // Envoi de l'image vers notre serveur
                    var files = new[] { new FileInfo(_photoFileName) };
                    _uploadPicture = new UploadFilesToIbrandQuestionnaire(files, _screenConfig.ProjectId.ToString(),
                                                                             _screenConfig.QuestionnaireId.ToString(), "facebook", null, SingleFileForFacebook);

                    if (!SingleFileForFacebook) { _uploadPicture.OnUploadFinished += OnUploadFinishedIbrand; }
                    else { _uploadPicture.OnUploadOneFileFinished += OnUploadOneFileFinishedIbrand; }

                    var t = new Thread(_uploadPicture.DoWork);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    log.Trace("Sending image to server to prepare for Facebook social sharing.");
                }
                */

                // On regarde maintenant si on doit tenter une impression.
                if (!ForcePrintOnActivation || _earlyPrintSent) return;
                _earlyPrintSent = true;
                SendPrintThread(true);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        /// <summary>
        /// Si mis à vrai (faux par défaut), on n'utilise pas le fichier directement pour le review, mais une image avec le cropping, comme pour les
        /// previews dans autre types d'écrans.
        /// </summary>
        public bool PreviewCrop { get; set; }

        private void CheckTriggersFlag()
        {
            Invalidate();
            if (OptionalSharingTriggers == null || !OptionalSharingTriggers.Any()) return;
            using (var iter = OptionalSharingTriggers.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var trig = iter.Current;
                    if (trig?.TriggerAction == null || trig.HasTriggered || trig.CheckFlags(_optionsUsed)) continue;
                    trig.HasTriggered = true;
                    trig.TriggerAction.Invoke();
                }
            }
        }

        /// <summary>
        /// Callback pour indiquer que l'upload sur le serveur est terminé et que l'on peut utiliser Facebook, si c'est un succès.
        /// </summary>
        /// <param name="success">Indique si l'Opération s'est soldée par un succès.</param>
        /// <param name="result">Le URL du fichier envoyé vers le serveur.</param>
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
                _facebookImageUrl = result;
                _ibrandUploadDone = true;
                // Il n'y a pas de thumbnail dans ce cas-ci, mais on met la variable a true pour ne pas bloquer les reste.
                _ibrandThumbnailDone = true;
                

            }
            else
            {
                log.Trace("Problem during iBrand upload sequence.");
                _ibrandUploadProblem = true;
            }

            Refresh();
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
                    _thumbnailUpload = new GetFacebookPictureUrl(_facebookImageUrl, null);
                    _thumbnailUpload.OnQueryFinished += OnThumbnailFinished;

                    var t = new Thread(_thumbnailUpload.DoWork);
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

            Refresh();
        }

        private void OnUploadFinishedFacebook(bool success)
        {
            log.Trace("Facebook share sequence completed with signal: {0}", success);

            SetSocialBlock(false);
            // Ne pas faire un résumé de timer ici. Il y a des possibilité de bogue avec les appels de pseudo modal, et
            // de toute façon, le timer est réactivé avec le "setActivated" quand on revient sur l'écran de partage
            //ResumeTimer();

            if (!success) { return; }

            if (_screenConfig.QuestionFacebookNo == null) return;

            log.Trace("Incrementing Facebook question number.");
            IncrementEventInQuestionnaire(_screenConfig.QuestionFacebookNo.Value);
            _optionsUsed |= ShareSocialOption.Facebook;

            this.SafeCall(() =>
            {
                Refresh();
                CheckTriggersFlag();
            });
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
            _optionsUsed |= ShareSocialOption.Twitter;
            this.SafeCall(() =>
            {
                Refresh();
                CheckTriggersFlag();
            });
        }

        private void SetSocialBlock(bool enableBlock)
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
    }
}
