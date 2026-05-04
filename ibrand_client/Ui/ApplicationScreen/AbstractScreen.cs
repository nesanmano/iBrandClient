using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Print;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro.SynchFinishers;
using NomadLogic.iBrandDesktop.Synchro.SynchFinishers.Juju;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public abstract partial class AbstractScreen : UserControl, IAbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        /// Temps minimal pour une tentattive d'impression (empêche certaines "race conditions" d'un appel de processus externe).
        [Obsolete]
        protected const int MINIMUM_PRINT_WAIT_TIME = 500;

        /// <summary>
        /// Retourne une variante d'une image modifiée procéduralement pour créer une très fréquente variation: image du sablier blanc sur noir.
        /// Les seuls paramètres intéressant sont les dimensions de l'image.
        /// </summary>
        /// <param name="size">La taille de l'image.</param>
        /// <returns>La définition de modification précédurale.</returns>
        [PublicAPI]
        public static ProceduralExtendedImageDefinition GetBasicHourglassImageDefinition(Size size)
        {
            return GetBasicHourglassImageDefinition(size.Width, size.Height, Color.Black);
        }

        /// <summary>
        /// Retourne une variante d'une image modifiée procéduralement pour créer une très fréquente variation: image du sablier blanc sur noir.
        /// Les seuls paramètres intéressant sont les dimensions de l'image.
        /// </summary>
        /// <param name="width">La largeur de l'image.</param>
        /// <param name="height">La hauteur de l'image.</param>
        /// <param name="backColor">La couleur du fond.</param>
        /// <returns>La définition de modification précédurale.</returns>
        [PublicAPI]
        public static ProceduralExtendedImageDefinition GetBasicHourglassImageDefinition(int width, int height, Color backColor)
        {
            return new ProceduralExtendedImageDefinition(width, height, backColor);
        }

        public bool InvokeRequiredForm()
        {
            return InvokeRequired;
        }

        public new Control ParentForm
        {
            get => Parent;
            set => Parent = value;
        }
        public event EventHandler OnFirstScreen;
        public event EventHandler OnClose;

        public event ChangePageEvent OnNext;
        public event ChangePageEvent OnBack;
        public event ChangePageEvent OnGoTo;
        public event ChangeToPseudoModalEvent OnChangeToPseudoModal;

        public bool IsSaveOnThisPage { get; set; }

        private readonly Bitmap _bitmapFr;
        private readonly Bitmap _bitmapEn;

        protected bool _flagForClosing;

        protected string _activeLg;

        public string ActiveLanguage => _activeLg;

        public int ProjectId => _screenConfig.ProjectId;

        public int QuestionnaireId => _screenConfig.QuestionnaireId;

        public int TerminalId => _screenConfig.TerminalId;

        private readonly Timer _onTouchSinceTimer;
        
        protected bool _activated;

        /// <summary>
        /// Un identifiant non-contraignant pour identifier un écran parmis les autres écrans existants.
        /// </summary>
        public string ScreenTag
        {
            get => Tag?.ToString();

            set => Tag = value;
        }

        private readonly Rectangle _configurationMainArea = new Rectangle(new Point(0, 0), new Size(70, 70));
        protected readonly ScreenConfig _screenConfig;

        [Obsolete]
        internal ScreenConfig ScreenConfiguration => _screenConfig;

        public bool IsRespondentAlreadySaved { get; protected set; }

        /// <summary>
        /// Référence interne aux enfant de cette classe pour avoir accès au PictureBox principal, qui gère les événements souris.
        /// </summary>
        protected PictureBox PictureBox { get; private set; }

        /// <summary>
        /// Liste d'actions supplémentaires indépendantes des qactions habituelles qu'il est possible de faire sur un écran en particulier.
        /// </summary>
        [PublicAPI]
        public IEnumerable<CustomClickAction> CustomClickActions { get; set; }

        [Obsolete("Il faut utiliser le constructeur avec les identifiants de projet désormais.")/*, true*/] // true pour forcer une erreur de compilation.
        protected AbstractScreen(string backgroundImgEnSrc, string backgroundImageSrc) : this(backgroundImgEnSrc, backgroundImageSrc, new ScreenConfig(-1, -1, -1))
        {
            // rien
        }

        /// <summary>
        /// Ce constructeur est réservé aux écrans ne possédant pas des images de fond d'écran ou ne devant pas réagir automatiquement sur des clics de souris dans
        /// l'ensemble de sa surface.
        /// </summary>
        /// <param name="size">Taille de l'écran.</param>
        /// <param name="screenConf">Objet de configuration.</param>
        protected AbstractScreen(Size size, ScreenConfig screenConf) : this()
        {
            if (size == null)
            {
                throw new System.Exception();
            }

            _screenConfig = screenConf;

            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            _activeLg = configuration.PreferedLanguage.Equals("en") ? Constants.EN : Constants.FR;

            Size = size;
        }

        /// <summary>
        /// Constructeur principal pour des écrans de Photobooth.
        /// </summary>
        /// <param name="backgroundImgEnSrc"></param>
        /// <param name="backgroundImageFrSrc"></param>
        /// <param name="screenConf"></param>
        protected AbstractScreen(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig screenConf) : this()
        {
            _screenConfig = screenConf;
            InitBackgroundImage(backgroundImageFrSrc ?? backgroundImgEnSrc, out _bitmapFr);
            InitBackgroundImage(backgroundImgEnSrc, out _bitmapEn);

            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            Bitmap bitmap;
            if (configuration.PreferedLanguage.Equals("en"))
            {
                bitmap = _bitmapEn;
                _activeLg = Constants.EN;
            }
            else
            {
                bitmap = _bitmapFr;
                _activeLg = Constants.FR;
            }

            if (bitmap != null)
            {
                PictureBox.Image = bitmap;
                PictureBox.Width = bitmap.Width;
                PictureBox.Height = bitmap.Height;
                Size = bitmap.Size;
            }

            PictureBox.Location = new Point(0, 0);
            Location = new Point(0, 0);
        }

        protected void SuspendTimer()
        {
            log.Trace("Timer stopped: screen tag: {0}", Tag);
            _onTouchSinceTimer.Stop();
        }

        protected void ResumeTimer()
        {
            log.Trace("Timer resumed: screen tag: {0}", Tag);
            _onTouchSinceTimer.Start();
        }

        [Obsolete("Ne pas utiliser ce constructeur si possible. Utiliser un constructeur avec ScreenConfig", false)]
        protected AbstractScreen(string backgroundImgSrc) : this()
        {
            InitBackgroundImage(backgroundImgSrc, out _bitmapEn);

            if (_bitmapEn == null) return;

            PictureBox.Image = _bitmapEn;
            PictureBox.Width = _bitmapEn.Width;
            PictureBox.Height = _bitmapEn.Height;

            _activeLg = Constants.EN;

            Size = _bitmapEn.Size;
        }

        protected AbstractScreen(Size size) : this()
        {
            Size = size;
        }

        protected AbstractScreen()
        {
            InitializeComponent();

            _onTouchSinceTimer = new Timer { Interval = 1 * 60 * 2000 };
            _onTouchSinceTimer.Tick += OnTimerEvent;

            PictureBox.MouseDown += MouseDownHandlerListener;
            PictureBox.Paint += PaintEventHandlerListener;
            Load += (s, e) => OnLoad();
        }

        /// <summary>
        /// Méthode à surcharger pour initialiser les éléments lors de la première activation de l'écran.
        /// Il faut mettre les éléments graphiques ici pour les initialiser lors de la première activation de l'écran.
        /// </summary>
        protected virtual void OnLoad()
        {
            // TODO Déplacer les instanciation d'images ici
            // Rien par défaut
        }

        /// <summary>
        /// Modifie l'intervale, en millisecondes, où l'on détermine que l'usager est inactif et que l'on termine l'usager.
        /// </summary>
        [PublicAPI]
        public int InactivityTimerInterval {
            get => _onTouchSinceTimer.Interval;

            set {
                if (value < 1000) return; // Minimum une seconde.
                if (_activated) _onTouchSinceTimer.Stop();
                _onTouchSinceTimer.Interval = value;
                if (_activated) _onTouchSinceTimer.Start();
            }
        }

        /// <summary>
        /// Méthode principale pour écouter les événements de desseins.
        /// </summary>
        /// <param name="sender">L'envoyeur.</param>
        /// <param name="e">L'événement de dessein de la forme.</param>
        protected void PaintEventHandlerListener(object sender, PaintEventArgs e)
        {
            if (!_activated) return;

            OnPaintEvent(e.Graphics);
        }

        protected virtual void OnPaintEvent(Graphics g)
        {
            // rien par défaut.
        }

        protected Bitmap GetBitmapDo(string lgCode, string fileName)
        {
            Bitmap bitmap = null;

            string path;
            if (_screenConfig != null)
            {
                path = DirectoryUtil.GetLocalIbrandTerminalDataDir(ProjectId, QuestionnaireId, TerminalId, 
                    DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, 
                    DirectoryUtil.ProjectDataDirectoriesModEnum.none) + lgCode + Path.DirectorySeparatorChar + fileName;

                if (File.Exists(path))
                {
                    bitmap = FileUtil.LoadBitmap(path);
                }
            }

            if (bitmap != null) return bitmap;

            // On essaye à nouveau avec le répertoire par défaut.
            path = DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir(lgCode) + fileName;
            if (File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                bitmap = new Bitmap(stream);
                stream.Close();
            }

            if (_screenConfig == null) return bitmap;


            if (_screenConfig.ForceFileCheck && bitmap == null)
            {
                throw new FileNotFoundException("Could not find image file: " + path);
            }

            return bitmap;
        }

        /// <summary>
        /// Obtient un Bitmap avec le code de langue par défaut (actuel en fait).
        /// <br/>Cherche avant tout dans le répertoire du questionnaire, puis dans les fichiers par défaut.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à chercher.</param>
        /// <returns>Une instance de Bitmap si trouvée.</returns>
        protected virtual Bitmap GetBitmap(string fileName)
        {
            return GetBitmapDo(_activeLg, fileName);
        }

        /// <summary>
        /// Obtient un Bitmap avec un code de langue donné.
        /// <br/>Cherche avant tout dans le répertoire du questionnaire, puis dans les fichiers par défaut.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à chercher.</param>
        /// <param name="lg">Le code de langue voulu.</param>
        /// <returns>Une instance de Bitmap si trouvée.</returns>
        protected Bitmap GetBitmap(string fileName, string lg)
        {
            return GetBitmapDo(lg, fileName);
        }

        /// <summary>
        /// Obtient un Bitmap dans le répertoire agnostique à la langue.
        /// <br/>Cherche avant tout dans le répertoire du questionnaire, puis dans les fichiers par défaut.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à chercher.</param>
        /// <returns>Une instance de Bitmap si trouvée.</returns>
        protected virtual Bitmap GetNoneLanguageBitmap(string fileName)
        {
            return GetBitmapDo(null, fileName);
        }

        protected virtual void OnTimerEvent(object sender, EventArgs e)
        {
            if (!DisableTimer)
            {
                OnFirstScreen?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                log.Trace("Event received but, timer disabled. Screen => {0}", Tag);
            }
        }

        /// <summary>
        /// Désactive le timer d'inactivité (si true) complètement, à mois qu'une classe ne surcharge la méthode "OnTimerEvent"
        /// </summary>
        [PublicAPI]
        public bool DisableTimer { get; set; }

        public virtual void SetActivatedFlag()
        {
            _activated = true;
        }

        public virtual void SetActivated()
        {
            _onTouchSinceTimer.Start();
            _activated = true;
            log.Trace("Screen activates ({0})", Tag);
        }

        public bool IsActivated()
        {
            return _activated;
        }

        public void ResetTimer()
        {
            _onTouchSinceTimer.Stop();
            _onTouchSinceTimer.Start();
        }

        public virtual void Recycle()
        {
            _onTouchSinceTimer.Stop();
        }

        /// <summary>
        /// Invoquez cette méthode pour que la langue du répondant et des écrans soit modifiée.
        /// Si le code de langue est similaire à celui actuel, la méthode sort immédiatement.
        /// </summary>
        /// <param name="language">Le code de langue.</param>
        /// <return>Retourne vrai s'il y a effectivement eu un changement de langue.</return>
        public virtual bool ChangeLanguage(string language)
        {
            if (language.Equals(_activeLg)) return false;

            if (language.Equals(Constants.EN))
            {
                if (_bitmapEn != null)
                {
                    PictureBox.Image = _bitmapEn;
                    PictureBox.Width = _bitmapEn.Width;
                    PictureBox.Height = _bitmapEn.Height;
                }
                _activeLg = Constants.EN;
            }
            else
            {
                if (_bitmapFr != null)
                {
                    PictureBox.Image = _bitmapFr;
                    PictureBox.Width = _bitmapFr.Width;
                    PictureBox.Height = _bitmapFr.Height;
                }
                _activeLg = Constants.FR;
            }

            PictureBox.Invalidate();

            return true;
        }
        
        /// <summary>
        /// Avance tellement loin dans les pages que l'on doit finir le répondant. Provoque la sauvegarde du questionnaire.
        /// </summary>
        public void End()
        {
            Next(1000);
        }

        /// <summary>
        /// Avance d'un nombre de pages spécifique dans l'ordre des écrans du questionnaire.
        /// <br/>Si l'on dépasse la dernière page du questionnaire, on provoque la sauvegarde du répondant et le
        /// retour à la première page.
        /// </summary>
        /// <param name="nbrPage">Le nombre de page à avancer.</param>
        public void Next(int nbrPage = 1)
        {
            if (!_activated) return;
            if (OnNext == null) return;

            _activated = false;
            _onTouchSinceTimer.Stop();
            log.Trace("Leaving page: screen tag: {0}", Tag);
            OnNext(nbrPage);
        }

        [PublicAPI]
        public void SwitchToPseudoModal(string tag)
        {
            if (OnChangeToPseudoModal == null) return;

            _activated = false;
            _onTouchSinceTimer.Stop();
            log.Trace("Leaving page (to pseudomodal): screen tag: {0}", Tag);
            OnChangeToPseudoModal(tag);
        }

        protected void Close()
        {
            OnClose?.Invoke(this, EventArgs.Empty);
        }

        public void GoTo(int pageNo)
        {
            if (OnGoTo == null) return;

            _activated = false;
            _onTouchSinceTimer.Stop();
            log.Trace("Leaving page: screen tag: {0}", Tag);
            OnGoTo(pageNo);
        }

        public void Back()
        {
            if (_activated)
                Back(1);
        }

        /// Annule le répondant actuel et retourne au début des écrans.
        public virtual void FirstScreen()
        {
            if (!_activated) return;
            if (OnFirstScreen == null) return;

            _activated = false;
            OnFirstScreen(this, EventArgs.Empty);
        }

        public void Back(int nbrPage)
        {
            if (OnBack == null)
            {
                log.Error("No 'Back' callback method is defined for this screen: " + Tag);
                return;
            }

            _activated = false;
            _onTouchSinceTimer.Stop();
            log.Trace("Leaving page: screen tag: {0}", Tag);
            OnBack(nbrPage);
        }

        private static void InitBackgroundImage(string bkg, out Bitmap target)
        {
            target = null;
            if (bkg == null) return;
            if (File.Exists(bkg))
            {
                target = FileUtil.LoadBitmap(bkg);
            }
            else
            {
                throw new FileNotFoundException("Background image file not found: " + bkg);
            }
        }

        protected bool InArea(int x, int y, Rectangle area)
        {
            return MathUtil.InArea(x, y, area);
        }

        protected bool InArea(int x, int y, Rectangle? area)
        {
            return area != null && InArea(x, y, area.Value);
        }

        public new virtual void Dispose()
        {
            base.Dispose();
            _activated = false;
        }

        /// <summary>
        /// Méthode principale pour écouter les événement souris venant du canevas d'un écran.
        /// Il provoque l'appel de la méthode "OnMouseDownHandler" si l'écran peut bel et bien agir sur un clic de souris.
        /// </summary>
        /// <param name="sender">L'envoyeur.</param>
        /// <param name="e">L'événement souris provoqué.</param>
        protected void MouseDownHandlerListener(object sender, [NotNull] MouseEventArgs e)
        {
            if (_flagForClosing) return;
            if (!_activated) return;

            OnMouseDownHandler(e.X, e.Y);
        }

        /// <summary>
        /// Gère l'événement de souris, lors d'un clic.
        /// Il faut absolumeent que les enfant appelle le "base" de cette méthode. Quand cette méthode retourne faux, c'est pour indiquer
        /// que l'événement a provoqué un effet, et qu'il ne faut PAS continuer à traiter cet événement.
        /// Cela vient du contrat de cette classe qui dit qu'un clic ne peut provoquer qu'un et seulement un événement par clic.
        /// </summary>
        /// <returns>True, si et seulement si aucun effet n'a été provoqué par ce clic. Faux, si on a traité cet événement (on a touché un zone).</returns>
        protected virtual bool OnMouseDownHandler(int x, int y)
        {
            if (_flagForClosing)
            {
                log.Trace("Closing sequenced detected. Mouse down event discarded. Screen => {0}", Tag);
                return false;
            }

            if (!_activated)
            {
                log.Trace("Inactive screen received mouse event. Mouse down event discarded. Screen => {0}", Tag);
                return false;
            }
            
            if (InArea(x, y, _configurationMainArea))
            {
                log.Trace("Main configuration area hit. From screen {1}, ScreenType={0}", GetType(), Tag);
                var pwDlg = new PasswordDialog(_activeLg, _screenConfig.ShowKeyboardOnPasswordDialog);
                pwDlg.ShowDialog();
                if (pwDlg.DialogResult == DialogResult.OK)
                {
                    new TouchModeConfigurationDlg(_screenConfig).ShowDialog();
                }

                return false;
            }

            if (CustomClickActions == null)
            {
                log.Trace("Base Raw mouse event received ({0}, {1}). Screen => {2}", x, y, Tag);
                return true;
            }
            foreach (var cca in CustomClickActions.Where(cca => InArea(x, y, cca.ClickZone)))
            {
                log.Trace("Custom click action area hit. Action type={0}", cca.GetType());
                cca.ToExecute.Invoke();
                return false;
            }

            log.Trace("Base Raw mouse untreated event received ({0}, {1}). Screen => {2}", x, y, Tag);
            return true;
        }
        
        /// <summary>
        /// Imprime le fichier d'image contenu dans la réponse texte de la question et choix donné.
        /// </summary>
        /// <param name="questionNo">Le numéro de question.</param>
        /// <param name="choiceNo">Le numéro du choix.</param>
        /// <returns>TRUE si l'impression à été envoyé à l'imprimante.</returns>
        protected bool PrintAnswerFile(int questionNo, int choiceNo)
        {
            var questionnaire = GetCurrentQuestionnaire();
            var success = false;
            
            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could not print. Unavailable question no = {0}", questionNo);
                return false;
            }

            var tmp = question.GetChoice(choiceNo);
            if (tmp == null)
            {
                log.Warn("Could not print. Unavailable choice no = {0}", choiceNo);
                return false;
            }

            try
            {
                var choiceText = (ChoiceText)tmp;
                var fileName = choiceText.GetAnswerText();
                success = PrintUtil.Print(fileName, null, this);
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not print.");
            }

            return success;
        }
        
        /// <summary>
        /// Prend la réponse texte d'une question/choix, et y incrémente une valeur entière que l'on y lit.
        /// <br/>Quand la réponse est vide, on y met la valeur "1". Par la suite, on incrément cette valeur entière et
        /// on remplace la réponse.
        /// </summary>
        /// <param name="questionNo">Le numéro de la question.</param>
        /// <param name="choiceNo">Le numéro du choix.</param>
        /// <param name="respondentToDatabaseUpdate">Le répondant dans la BD à mettre à jour avec la réponse.</param>
        /// <returns>Si la réponse à été modifiée correctement.</returns>
        protected bool IncrementEventInQuestionnaire(int questionNo, int choiceNo = 1, int? respondentToDatabaseUpdate = null)
        {
            var questionnaire = GetCurrentQuestionnaire();
            var success = false;

            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could not increment event. Unavailiable question no = {0}", questionNo);
                return false;
            }

            var tmp = question.GetChoice(choiceNo);
            if (tmp == null)
            {
                log.Warn("Could not increment event. Unavailiable choice no = {0}", choiceNo);
                return false;
            }

            string text = null;
            try
            {
                var choiceText = (ChoiceText)tmp;

                var answer = new Answer {QuestionNo = question.No, ChoiceNo = choiceText.No};
                text = choiceText.GetAnswerText();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var nbrPrint = int.Parse(text);
                    nbrPrint++;
                    answer.Response = nbrPrint.ToString();
                    choiceText.SetAnswer(answer);
                }
                else
                {
                    answer.Response = "1";
                    choiceText.SetAnswer(answer);
                }

                if (respondentToDatabaseUpdate.HasValue)
                {
                    UpdateAnswerInDatabase(answer, respondentToDatabaseUpdate.Value);
                }

                success = true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not increment event. Text={0}", text);
            }

            return success;
        }
        
        /// <summary>
        /// Assigne une image à un PictureBox, image issue d'une question
        /// </summary>
        /// <param name="box">La pictureBox à assigne rl'image.</param>
        /// <param name="questionNo">Le numéro de question</param>
        /// <param name="choiceNo">Le numéro de choix.</param>
        /// <param name="centerCrop">Indique si on veut faire le même effet que la méthode MakeImageForView</param>
        /// <param name="modifySize">Indique s'il faut modifier la taille du pictureBox à la taille de l'image. Uniquement applicable si "centerCrop" est faux.</param>
        protected void DisplayQuestionnaireAnswerInPictureBox(PictureBox box, int questionNo, int choiceNo, bool centerCrop, bool modifySize = false)
        {
            if (box == null)
            {
                return;
            }

            var questionnaire = GetCurrentQuestionnaire();

            // Traitement de l'image
            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could not Set image. Unavailiable question no = {0}", questionNo);
                return;
            }

            var tmp = question.GetChoice(choiceNo);
            if (tmp == null)
            {
                log.Warn("Could not Set image. Unavailiable choice no = {0}", choiceNo);
                return;
            }

            string photoFileName = null;
            try
            {
                var choiceText = (ChoiceText)tmp;
                photoFileName = choiceText.GetAnswerText();

                if (string.IsNullOrEmpty(photoFileName)) return;

                var tmpImage = FileUtil.LoadBitmap(photoFileName);

                if (centerCrop)
                {
                    box.Image = tmpImage.ResizeForView(box.Width, box.Height);
                }
                else
                {
                    box.SizeMode = modifySize ? PictureBoxSizeMode.AutoSize : PictureBoxSizeMode.StretchImage;
                    box.Image = tmpImage;
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not Set image. Text={0}", photoFileName);
            }
        }


        public bool SaveAnswerToQuestion(params Answer[] answers)
        {
            return answers != null && answers.All(SaveAnswerToQuestion);
        }

        public bool SaveAnswers(IEnumerable<Answer> answers)
        {
            return answers != null && answers.All(SaveAnswerToQuestion);
        }
        
        /// <summary>
        /// Sauvegarde une réponse sans composante texte dans le questionnaire actuel.
        /// </summary>
        /// <param name="qi">l'identifiant de la question</param>
        /// <returns>TRUE si la sauvegarde s'est bien effectuée.</returns>
        [PublicAPI]
        public bool SaveAnswerToQuestion(QuestionIdentifier qi)
        {
            var questionnaire = GetCurrentQuestionnaire();
            var success = false;

            var question = questionnaire.GetQuestion(qi.QuestionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailable question no = {0}", qi.QuestionNo);
                return false;
            }

            var tmp = question.GetChoice(qi.ChoiceNo);
            if (tmp == null)
            {
                log.Warn("Could not save answer. Unavailable choice no = {0}", qi.ChoiceNo);
                return false;
            }

            try
            {
                var answer = new Answer {QuestionIdentifier = qi};
                tmp.SetAnswer(answer);
                log.Trace("Answer saved. A={0}", answer.ToString());
                success = true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not save answer: " + qi);
            }

            return success;
        }

        /// <summary>
        /// Sauvegarde une valeur String comme réponse pour une question du questionnaire actuel.
        /// </summary>
        /// <param name="qi">l'identifiant de la réponse.</param>
        /// <param name="responseValue">La valeur à sauvegarder.</param>
        /// <returns>TRUE si la sauvegarde s'est bien effectuée.</returns>
        [PublicAPI]
        public bool SaveAnswerToQuestion(QuestionIdentifier qi, string responseValue)
        {
            var questionnaire = GetCurrentQuestionnaire();
            var success = false;

            var question = questionnaire.GetQuestion(qi.QuestionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailiable question no = {0}", qi.QuestionNo);
                return false;
            }

            var tmp = question.GetChoice(qi.ChoiceNo);
            if (tmp == null)
            {
                log.Warn("Could not save answer. Unavailiable choice no = {0}", qi.ChoiceNo);
                return false;
            }

            try
            {
                var choiceText = (ChoiceText) tmp;

                var answer = new Answer{QuestionIdentifier = qi, Response = responseValue};
                choiceText.SetAnswer(answer);
                success = true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not save answer. Text={0}", responseValue);
            }

            return success;
        }

        protected bool IsQuestionAnswered(int questionNo)
        {
            var questionnaire = GetCurrentQuestionnaire();
            
            var question = questionnaire.GetQuestion(questionNo);
            if (question != null) return question.IsAnswered();
            log.Warn("Could not Get answer. Unavailiable question no = {0}", questionNo);
            return false;
        }

        protected void ResetQuestionAnswers(int questionNo)
        {
            var questionnaire = GetCurrentQuestionnaire();
            
            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could not Get answer. Unavailiable question no = {0}", questionNo);
                return;
            }

            question.ResetAnswers();

        }

        /// <summary>
        /// Obtient le questionnaire actuel.
        /// </summary>
        /// <returns>Le questionnaire actuel de l'application.</returns>
        public virtual Questionnaire GetCurrentQuestionnaire()
        {
            return ScreensControllerAccessor.ScreensController.Questionnaire;
        }

        /// <summary>
        /// Retourne une instance du répondant actuel avec réponses telles que lues dans la base de données.
        /// </summary>
        /// <returns>Une référence vers le répondant actuel.</returns>
        public virtual IRespondent GetCurrentRespondentWithAnswers() {
            return ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent();
        }

        /// <summary>
        /// Sauvegarde une réponse complète pour une question du questionnaire actuel.
        /// </summary>
        /// <param name="answer">La réponse complète à sauvegarder.</param>
        /// <returns>TRUE si la sauvegarde s'est bien effectuée.</returns>
        [PublicAPI]
        public bool SaveAnswerToQuestion(Answer answer)
        {
            if (answer == null) return false;
            var questionnaire = GetCurrentQuestionnaire();
            var success = false;

            var question = questionnaire.GetQuestion(answer.QuestionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailiable question no = {0}", answer.QuestionNo);
                return false;
            }

            var tmp = question.GetChoice(answer.ChoiceNo);
            if (tmp == null)
            {
                log.Warn("Could not save answer. Unavailiable choice no = {0}", answer.ChoiceNo);
                return false;
            }

            try
            {
                tmp.SetAnswer(answer);
                success = true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not save answer. answer={0}", answer);
            }

            return success;
        }

        /// <summary>
        /// Sauvegarde une valeur positive ou négatice à une question de type Check.
        /// Si le numéro de question est invalide, on ne fait rien et on retourne true.
        /// </summary>
        /// <param name="questionNo">Le numéro de la question à sauvegarder.</param>
        /// <param name="choiceNo">Le choix de la question à sauvegarder.</param>
        /// <param name="check">Indique s'il faut cocher ou non la réponse.</param>
        /// <returns>TRUE si la sauvegarde s'est bien effectuée.</returns>
        [PublicAPI]
        public bool SaveAnswerToQuestionCheck(int questionNo, int choiceNo, bool check)
        {
            if (questionNo < 1 || choiceNo < 1) return true;

            var questionnaire = GetCurrentQuestionnaire();
            var success = false;

            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailiable question no = {0}", questionNo);
                return false;
            }

            var tmp = question.GetChoice(choiceNo);
            if (!(tmp is ChoiceCheck cc))
            {
                log.Warn("Could not save answer. Unavailiable choice no = {0}", choiceNo);
                return false;
            }

            try
            {
                var answer = new Answer {QuestionType = QuestionTypeEnum.Check, QuestionNo = question.No, ChoiceNo = choiceNo};
                cc.SetAnswer(answer);

                success = true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not save answer. check={0}. qNo={1}, cNo={2}\n", check, questionNo, choiceNo);
            }

            return success;
        }

        public bool SaveAnswerToQuestionCheck(QuestionIdentifier qi, bool check)
        {
            return GetCurrentQuestionnaire()?.SaveAnswerToQuestionCheck(qi, check) ?? false;
        }

        public bool SaveDynamicHeader(DynamicHeader dh)
        {
            return GetCurrentQuestionnaire()?.SaveDynamicHeader(dh) ?? false;

        }

        public bool SaveDynamicHeaderCollection(ICollection<DynamicHeader> dhc)
        {
            return GetCurrentQuestionnaire()?.SaveDynamicHeaderCollection(dhc) ?? false;
        }

        public bool ClearDynamicHeaderCollection(params string[] dataKeyPurge)
        {
            return GetCurrentQuestionnaire()?.ClearDynamicHeaderCollection(dataKeyPurge) ?? false;
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, DataValue dv)
        {
            return GetCurrentQuestionnaire()?.AddToFirstMatchingKeyHeader(dataKey, dv) ?? false;
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, List<DataValue> dvList)
        {
            return GetCurrentQuestionnaire()?.AddToFirstMatchingKeyHeader(dataKey, dvList) ?? false;
        }

        /// <summary>
        /// Obtient la réponse en format texte d'un choix d'une question du questionnaire actuel.
        /// Si le choix de la question n'est pas un sous-type de type texte (ChoiceText), il retournera null,
        /// réponse similaire à si la question n'a pas été répondue.
        /// </summary>
        /// <param name="questionNo">Le numéro de question.</param>
        /// <param name="choiceNo">Le choix de la question.</param>
        /// <returns>La réponse obtenue. NUll si non répondue ou si le type de choix n'est pas texte.</returns>
        protected string GetTextAnswerFromQuestion(int questionNo, int choiceNo = 1)
        {
            var questionnaire = GetCurrentQuestionnaire();

            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could Get answer. Unavailiable question no = {0}", questionNo);
                return null;
            }

            var tmp = question.GetChoice(choiceNo);
            if (tmp == null)
            {
                log.Warn("Could Get answer. Unavailiable choice no = {0}", choiceNo);
                return null;
            }

            try
            {
                var choiceText = tmp as ChoiceText;
                return choiceText?.GetAnswerText();
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not Get answer.");
            }

            return null;
        }
        
        /// <summary>
        /// Retourne si oui ou non la réponse que la question donnée au choix donnée est booléennement vrai ou faux.
        /// Si le type de choix n'est pas un ChoiceCheck, retourne NULL. SI oui, retourne soit TRUE ou FALSE.
        /// </summary>
        /// <param name="questionNo">Le numéro de question à interroger.</param>
        /// <param name="choiceNo">Le numéro de choix à interroger.</param>
        /// <returns>NULL si la question n'est pas du bon type ou si un ou l'autre de la question ou du choix n'existe pas. Sinion, retourne TRUE
        /// si et seulement si le choix est répondu comme coché.</returns>
        [PublicAPI]
        protected bool? GetBooleanAnswerFromQuestionCheck(int questionNo, int choiceNo = 1)
        {
            var questionnaire = GetCurrentQuestionnaire();

            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could Get answer. Unavailiable question no = {0}", questionNo);
                return null;
            }

            var tmp = question.GetChoice(choiceNo);
            if (tmp == null)
            {
                log.Warn("Could Get answer. Unavailiable choice no = {0}", choiceNo);
                return null;
            }

            try
            {
                var choice = tmp as ChoiceCheck;
                return choice?.IsChecked();
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not Get answer.");
            }

            return null;
        }

        /// <summary>
        /// Valide la question identifiée par le premier paramètre, avec le texte en paramètre (si elle ne contient pas déjà une réponse) comme une addresse
        /// courriel valide.
        /// </summary>
        /// <param name="questionNo">Le numéro de la question à vérifier.</param>
        /// <param name="text">Le texte`à valider par la question.</param>
        /// <returns>TRUE si le texte est une addresse courriel valide.</returns>
        [PublicAPI]
        protected bool ValidateEmailAddress(int questionNo, string text, int choiceNo = 1)
        {
            var questionnaire = GetCurrentQuestionnaire();

            var question = questionnaire.GetQuestion(questionNo);
            var answer = new Answer {QuestionNo = question.No, ChoiceNo = 1, Response = text};
            MessageDialog mBox;
            if (!(question.GetChoice(choiceNo) is ChoiceText choiceText))
            {
                log.Warn($"No valid choice number in question detected: {questionNo}/{choiceNo}");
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR)
                    ? "Erreur dans le questionnaire. Question invalide."
                    : "Error in the questionnaire. Invalid Question.");
                return false;
            }
            choiceText.SetAnswer(answer);

            if (!string.IsNullOrEmpty(text) && StringUtil.IsValidEmail(text)) return true;

            mBox = new MessageDialog();
            mBox.ShowMessage(_activeLg.Equals(Constants.FR)
                ? "L'adresse courriel est non-valide."
                : "Email address not valid.");

            answer.Response = "";

            choiceText.SetAnswer(answer);

            return false;
        }

        /// <summary>
        /// Indique s'il faut sauter par-dessus la séquence de SubmitActions, pour quelque raison que ce soit.
        /// </summary>
        public bool SkipSubmitActions { get; set; }
        
        /// <summary>
        /// Callback sur un événement qui change le texte d'une réponse. Se conforme à un callback d'événement standard de forme pour
        /// être utilisé sur le plus de contrôles possible.
        /// <br/>Par défaut, remet le timer d'inactivité à zéro.
        /// </summary>
        /// <param name="sender">L'envoyeur d'événement. (pas utilisé)</param>
        /// <param name="e">Les paramètre d'événement. (pas utilisé)</param>
        public virtual void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
            {
                ResetTimer();
            }
        }

        /// <summary>
        /// Valide qu'une réponse texte est un numéro de téléphone valide.
        /// </summary>
        /// <param name="questionNo">Le numéro de question.</param>
        /// <param name="choiceNo">Le numéro de choix.</param>
        /// <param name="textToValidate">Le texte à valider.</param>
        /// <returns>TRUE si le texte est valide et que le résultat ait été mit dans la question.</returns>
        protected bool ValidatePhoneNumber(int questionNo, int choiceNo, string textToValidate)
        {
            var questionnaire = GetCurrentQuestionnaire();
            MessageDialog mBox;
            var questionPhone = questionnaire.GetQuestion(questionNo);
            if (!(questionPhone.GetChoice(choiceNo) is ChoiceText choicePhone))
            {
                log.Error($"No valid question/choice detected for identifier: {questionNo}/{choiceNo}");
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr") ? "Question invalide." : "Invalid question.");
                return false;
            }

            var answerPhone = new Answer { QuestionNo = questionPhone.No, ChoiceNo = choicePhone.No, Response = textToValidate };
            choicePhone.SetAnswer(answerPhone);

            var result = StringUtil.IsValidPhoneNumber(textToValidate);

            if (result) return true;

            mBox = new MessageDialog();
            mBox.ShowMessage(_activeLg.Equals("fr") ? "Téléphone invalide." : "Invalid Phone number");

            answerPhone.Response = "";

            choicePhone.SetAnswer(answerPhone);

            return false;
        }

        public void InvokeForm(ChangeScreenDelegate changeScreenDelegate, int index)
        {
            Invoke(changeScreenDelegate, index);
        }

        public void InvokeForm(Action showFirstScreenDelegate)
        {
            Invoke(showFirstScreenDelegate);
        }

        /// <summary>
        /// Créé un fichier d'image à partir d'une image cadre déjà existante, et place la photo prise par dessus.
        /// </summary>
        /// <param name="frameFile">Le fichier cadre.</param>
        /// <param name="pictureTaken">L'image prise.</param>
        /// <param name="percentageOffset">Le pourcentage de l'image cadre à utiliser comme espace entre lkes limites. Si null, la valeur sera 0.05F.</param>
        /// <param name="extraOffsetX">Une valeur de pixels d'extra pour déplacer la position de la photo sur le cadre.</param>
        /// <param name="extraOffsetY">Une valeur de pixels d'extra pour déplacer la position de la photo sur le cadre.</param>
        /// <param name="outputFileName">En sortir, le nom du fichier résultant. NUll si une erreur s'est produite.</param>
        /// <param name="frameOverPicture">Indique s'il faut dessiner le frame par dessus l'image (inverse de la normale)</param>
        /// <returns>TRUE si tout s'est bien déroulé.</returns>
        // TODO À déplacer dans une classe utilitaire.
        [Obsolete]
        // ReSharper disable once UnusedMember.Global
        protected bool MakeSquareFramePicture(string frameFile, string pictureTaken, float? percentageOffset, int extraOffsetX, int extraOffsetY, out string outputFileName, bool frameOverPicture = false)
        {
            if (!File.Exists(frameFile))
            {
                log.Warn("Frame file does not exists.");
                outputFileName = null;
                return false;
            }

            outputFileName = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                _screenConfig.QuestionnaireId,
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.print)
                                        + GenerateUniqueId.GetId() + ".jpg";

            using (var frame = FileUtil.LoadBitmap(frameFile))
            {
                var offset = (int)(frame.Width * (percentageOffset ?? 0.05F));
                var imageSize = frame.Width - (2 * offset);

                using (var image = FileUtil.LoadBitmap(pictureTaken))
                {
                    using (var b = image.Frame(imageSize, imageSize))
                    {
                        using (var result = BitmapExtentions.CreateFramedPicture(frame, b, offset + extraOffsetX, offset + extraOffsetY, frameOverPicture))
                        {
                            result.Save(95, outputFileName);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Effectue un élaguage carré sur un fichier d'image existant, et créé un fichier modifié sur disque.
        /// </summary>
        /// <param name="pictureTaken">Le nom de l'image à modifier.</param>
        /// <param name="outputFileName">Le nom du fichier résultant. NULL si un problème à eu lieu.</param>
        /// <returns>TRUE si tout s'est bien passé.</returns>
        // TODO À déplacer dans une classe utilitaire.
        [Obsolete]
        // ReSharper disable once UnusedMember.Global
        protected bool CropPicture(string pictureTaken, out string outputFileName)
        {
            outputFileName = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                 _screenConfig.QuestionnaireId,
                 DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none)
                                      + GenerateUniqueId.GetId() + ".jpg";

            using (var image = FileUtil.LoadBitmap(pictureTaken))
            {
                var size = Math.Min(image.Width, image.Height);
                using (var b = image.Frame(size, size))
                {
                    b.Save(95, outputFileName);
                }
            }

            return true;
        }

        /// <summary>
        /// Cette méthode créer un fichier d'impression pour impression avec une série d'images (qui sont habituellement la base d'un GIF).
        /// On utilise une image de cadre passé en paramètre pour y placer les images. Les images sont redimensionnées et élaguées en carrés selon la taille
        /// de l'image cadre, avec un espace (offset) stable pour chaque image.
        /// </summary>
        /// <param name="frameFile">L'image de cadre utilisée comem base pour le fichier d'impression.</param>
        /// <param name="picturesTaken">La liste d'image utilisés.</param>
        /// <param name="percentageOffset">Le poucentage de l'image cadre utilisé comme espacement. Si nul, la valeur sera de 5% (0.05F)</param>
        /// <param name="nbrColumn">Le nombre de colonnes voulues pour espacer les images à imprimer.</param>
        /// <param name="extraTopOffset">Nombre de pixel supplémentaire à mettre comme offset en Y.</param>
        /// <param name="extraSideOffset">Nombre de pixel supplémentaire à mettre comme offset en X.</param>
        /// <param name="outputFileName">Le nom de fichier généré par la méthode. Null, s'il y a eu un problème.</param>
        /// <returns>TRUE si tout s'est bien déroulé.</returns>
        // TODO À déplacer dans une classe utilitaire.
        [Obsolete]
        // ReSharper disable once UnusedMember.Global
        protected bool MakePrintSquareFrameMultiple(string frameFile, IList<string> picturesTaken, float? percentageOffset,
            int nbrColumn, int extraTopOffset, int extraSideOffset, out string outputFileName)
        {
            if (!File.Exists(frameFile))
            {
                log.Warn("Frame file does not exists.");
                outputFileName = null;
                return false;
            }

            outputFileName = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                _screenConfig.QuestionnaireId,
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.print)
                                        + GenerateUniqueId.GetId() + ".jpg";

            try
            {
                using (var frame = FileUtil.LoadBitmap(frameFile)) {
                    using (var result = frame.PutMultiplePictureOnFrame(picturesTaken, percentageOffset, nbrColumn, extraTopOffset, extraSideOffset)) {
                        return result.Save(95, outputFileName);
                    }
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during print picture multiple generation.");
                outputFileName = null;
                return false;
            }
        }

        /// <summary>
        /// Créer un GIF avec une série de fichiers. On peut donner ici les paramètres de GIF que l'on veut.
        /// </summary>
        /// <param name="picturesTaken">La séquence d'image avec laquelle créer le GIF.</param>
        /// <param name="size">La taille du GIF (il s'agit d'une carré, donc, une seule dimension suffit.)</param>
        /// <param name="repeat">Indique si on veut que le GIF se répète.</param>
        /// <param name="quality">Sa qualité. 1 est le minimum et indique une qualité supérieure.</param>
        /// <param name="delayMili">Le délai entre chaque frame, en milisecondes.</param>
        /// <param name="outputFileName">Le fichier gif résultant.</param>
        /// <param name="overlay">Un fichier de overlay pour chaque trame</param>
        /// <param name="crop">Indique si on doit élaguer les côtés de l'image lors d'un resize. Vrai par défaut.</param>
        /// <param name="loopBack">Indique si on veut l'effet boomerang sur les images. Faux par défaut.</param>
        /// <returns>TRUE si tout s'est bien déroulé.</returns>
        // TODO À déplacer dans une classe utilitaire.
        [Obsolete]
        // ReSharper disable once UnusedMember.Global
        protected bool MakeGif(IList<string> picturesTaken, int size, bool repeat, int quality, int delayMili, out string outputFileName, string overlay,
            bool crop = true, bool loopBack = false)
        {
            outputFileName = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                _screenConfig.QuestionnaireId,
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.gif)
                                        + GenerateUniqueId.GetId() + ".gif";

            return ImageUtil.CreateGif(picturesTaken, outputFileName, repeat, loopBack, quality, delayMili, overlay, new Size(size, size), crop);
        }

        /// <summary>
        /// Met à jour le PictureBox de présentation de l'image.
        /// <br/>Fait les vérifications nécessaires selon le type d'image pour un chargement optimal.
        /// </summary>
        /// <param name="pictureBoxToSet">L'instance de picture Box à modifier.</param>
        /// <param name="questionNumber">Le numéro de la question qui contient le chemin absolu de l'image à afficher.</param>
        /// <param name="disposeOld">Indique s'il faut détruire l'ancienne image.</param>
        protected void SetPreviewPicture(PictureBox pictureBoxToSet, int questionNumber, bool disposeOld)
        {
            if (pictureBoxToSet == null)
            {
                return;
            }

            var fileLocation = GetTextAnswerFromQuestion(questionNumber);
            if (!File.Exists(fileLocation)) return;

            if (disposeOld) pictureBoxToSet.Image?.Dispose();

            if (fileLocation.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase))
            {
                pictureBoxToSet.ImageLocation = fileLocation;
            }
            else
            {
                var bmp = FileUtil.LoadBitmap(fileLocation);
                pictureBoxToSet.Image = bmp.ResizeForView(pictureBoxToSet.Width, pictureBoxToSet.Height);
            }
        }

        /// <summary>
        /// Met à jour une image dans un PictureBox. On peut donner une valeur nulle, ce qui à pour effet de vider le contenant.
        /// Quand on change d'image, on détruit (Dispose) l'ancienne automatiquement, sauf dans deux cas.
        /// Le premier cas est si on donne une référence vers une image à ne pas toucher, et que les références sont identiques.
        /// Le deuxième est quand l'image à mettre à jour est déjà celle présente dans le contrôle.
        /// </summary>
        /// <param name="pictBox">Le contrôle PictireBox à mettre à jour.</param>
        /// <param name="safeBitmap">Optionel. Une référence vers une image à ne jamais détruire.</param>
        /// <param name="newImage">La nouvelle image à afficher. Peut être nul (ce qui vide le contrôle).</param>
        protected void SetPreviewPicture(PictureBox pictBox, Image safeBitmap, Image newImage)
        {
            if (pictBox == null)
            {
                return;
            }

            Invoke((MethodInvoker)delegate
            {
                if (pictBox.Image != safeBitmap && pictBox.Image != newImage)
                {

                    // On ne détruit (dispose) l'image que si ce n'est pas une référence vers une image critique (comme le "Freeze"),
                    // et que l'image est différente de celle déjà présente. Cette vérification se fait par pointeur.
                    pictBox.Image?.Dispose();
                    pictBox.Image = null;
                }

                try
                {
                    if (newImage == null || newImage.Width <= 0) return;

                    // Image valide, on remplace
                    pictBox.Image = newImage;
                    pictBox.Refresh();
                }
                catch (System.Exception ex)
                {
                    log.Error(ex, "Update of picture box image failed.");
                }
            });
        }

        /// <summary>
        /// Met à jour une image dans un PictureBox. On peut donner une valeur nulle, ce qui à pour effet de vider le contenant.
        /// Quand on change d'image, on détruit (Dispose) l'ancienne automatiquement, sauf dans deux cas.
        /// Le premier cas est si on donne une liste de références vers des images à ne pas toucher, et que cette liste contienne l'image à enlever.
        /// Le deuxième est quand l'image à mettre à jour est déjà celle présente dans le contrôle.
        /// </summary>
        /// <param name="pictBox">Le contrôle PictireBox à mettre à jour.</param>
        /// <param name="safeBitmaps">Optionel. Une liste de références d'images à ne jamais détruire.</param>
        /// <param name="newImage">La nouvelle image à afficher. Peut être nul (ce qui vide le contrôle).</param>
        protected void SetPreviewPicture(PictureBox pictBox, List<Image> safeBitmaps, Image newImage)
        {
            if (pictBox == null)
            {
                return;
            }

            this.SafeCall(() => { 
                if (!safeBitmaps.Contains(pictBox.Image) && pictBox.Image != newImage)
                {
                    // On ne détruit (dispose) l'image que si ce n'est pas une référence vers une image critique (comme le "Freeze"),
                    // et que l'image est différente de celle déjà présente. Cette vérification se fait par pointeur.
                    pictBox.Image?.Dispose();
                }

                pictBox.Image = null;

                try
                {
                    if (newImage == null || newImage.Width <= 0) return;

                    // Image valide, on remplace
                    pictBox.Image = newImage;
                }
                catch (System.Exception ex)
                {
                    log.Error(ex, "Update of picture box image with new image failed.");
                }

                pictBox.Refresh();
            });
        }

        /// <summary>
        /// Met à jour une réponse dans la base de données. Si la réponse n'existe pas au préalable, elle sera créée.
        /// </summary>
        /// <param name="answer">La réponse à mettre à jour. Ne doit pas être nulle.</param>
        /// <param name="respondentId">Le répondant associé à la réponse.</param>
        private void UpdateAnswerInDatabase(Answer answer, int respondentId)
        {
            if (answer == null)
            {
                return;
            }

            var db = ServicesContainer.GetService<IbrandDatabase>();

            db.DeleteAnswer(respondentId, answer);
            db.InsertAnswer(respondentId, answer);
            log.Trace("Answer for respondent {0} updated to: {1}", respondentId, answer);
        }

        /// <summary>
        /// Imprime directement le fichier donné en paramètre, et tente par la suite d'incrémenter une question
        /// en relation, si elle est donnée en paramètre.
        /// </summary>
        /// <param name="filePath">Le fichier à imprimer.</param>
        /// <param name="questionNbrPrint">Optionnel. Le numéro de question pour garder en mémoire le nombre d'impression.</param>
        /// <param name="respondentToUpdateInDatabase">Optionnel. Le numéro de répondant à mettre à jour directement dans la BD avec la réponse obtenue.</param>
        protected void PrintAndIncrement(string filePath, int? questionNbrPrint, int? respondentToUpdateInDatabase)
        {
            if (!PrintUtil.Print(filePath, null, this))
            {
                log.Warn("Could not print file: " + filePath);
                return;
            }

            if (!questionNbrPrint.HasValue) return;

            if (!IncrementEventInQuestionnaire(questionNbrPrint.Value, 1, respondentToUpdateInDatabase))
            {
                log.Warn("Could not increment print number.");
            }
        }

        public void FlagForClosing()
        {
            _flagForClosing = true;
            PrepareToClose();
        }

        protected virtual void PrepareToClose()
        {
            // Rien par defaut.
        }

        /// <summary>
        /// Cette méthode retourne "false" si et seulement si un des éléments est un "opt-in" obligatoire
        /// et n'est pas coché au moment de la vérification. Donc, une liste vide ou nulle retourne true.
        /// </summary>
        /// <param name="zones">La liste de zones à tester.</param>
        /// <returns>FALSE si et seulement si au moins un check obligatoire n'est pas coché.</returns>
        protected static bool AllMandatoryChecks(IEnumerable<ScreenCheckOption> zones)
        {
            if (zones == null) return true;
            return !zones.Any(z => z.Mandatory && !z.AnswerState);
        }
        
        /// <summary>
        /// Font par défaut pour les boîtes de texte.
        /// </summary>
        public static readonly Font DEFAULT_TEXT_BOX_FONT = new Font(FontFamily.GenericSansSerif , 26);
        
        /// <summary>
        /// Permet d'initialiser un objet de type TextBox à partir d'une référence. La référence sera modifiée.
        /// </summary>
        /// <param name="box">La référence du TextBox</param>
        /// <param name="index">L'indice de tabulation</param>
        /// <param name="x">Position en X</param>
        /// <param name="y">Position en Y</param>
        /// <param name="w">Largeur</param>
        /// <param name="h">Hauteur</param>
        /// <param name="fontSize">Taille de la fonte. Par défaut 18</param>
        protected void SetTextBox(out TextBox box, int index, int x, int y, int w, int h, int fontSize = 18)
        {
            box = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                TabIndex = index,
                Font = new Font(FontFamily.GenericSansSerif, fontSize)
            };
            box.TextChanged += OnTextChanged;
            Controls.Add(box);
            box.BringToFront();
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        protected void InitAddedKeyboard(KeyboardControl keyboard)
        {
            keyboard.Parent = PictureBox;
            keyboard.BringToFront();
            keyboard.Show();
        }

        /// <summary>
        /// Ajouter un clavier avec accents sur un écran requiert plusieures étapes et modification de parent, si l'on veut gérer la transparence.
        /// Version générique, pour tous les sous-types de clavier.
        /// Expliqué en détail ici: http://stackoverflow.com/questions/9387267/transparent-control-over-picturebox
        /// </summary>
        /// <param name="x">Position en X du clavier.</param>
        /// <param name="y">Position en Y du clavier</param>
        /// <param name="width">Largeur du clavier.</param>
        /// <param name="height">Hauteur du clavier.</param>
        [PublicAPI]
        public T AddKeyboardToControls<T>(int x, int y, int width, int height) where T : KeyboardControl, new()
        {
            var keyboard = new T();
            keyboard.SetLocation(x, y);
            keyboard.Size = new Size(width, height);
            InitAddedKeyboard(keyboard);
            return keyboard;
        }
        
        /// <summary>
        /// Ajouter un clavier avec accents sur un écran requiert plusieures étapes et modification de parent, si l'on veut gérer la transparence.
        /// Version générique avec instance déjà créée. SI null, retourne null et fait un No-Op.
        /// Expliqué en détail ici: http://stackoverflow.com/questions/9387267/transparent-control-over-picturebox
        /// </summary>
        /// <param name="kb">Référence vers le clavier</param>
        [PublicAPI]
        public T AddKeyboardToControls<T>(T kb) where T : KeyboardControl
        {
            if (kb == null) return null;
            InitAddedKeyboard(kb);
            return kb;
        }

        /// <summary>
        /// Ajouter un clavier avec accents sur un écran requiert plusieures étapes et modification de parent, si l'on veut gérer la transparence.
        /// Expliqué en détail ici: http://stackoverflow.com/questions/9387267/transparent-control-over-picturebox
        /// </summary>
        /// <param name="x">Position en X du clavier.</param>
        /// <param name="y">Position en Y du clavier</param>
        /// <param name="width">Largeur du clavier.</param>
        /// <param name="height">Hauteur du clavier.</param>
        [PublicAPI, Obsolete]
        protected KeyboardControl AddKeyboardToControls(int x, int y, int width, int height)
        {
            return AddKeyboardToControls<KeyboardControl>(x, y, width, height);
        }
        
        /// <summary>
        /// Ajouter un clavier numérique sur un écran requiert plusieures étapes et modification de parent, si l'on veut gérer la transparence.
        /// Expliqué en détail ici: http://stackoverflow.com/questions/9387267/transparent-control-over-picturebox
        /// </summary>
        /// <param name="x">Position en X du clavier.</param>
        /// <param name="y">Position en Y du clavier</param>
        /// <param name="width">Largeur du clavier.</param>
        /// <param name="height">Hauteur du clavier.</param>
        [PublicAPI, Obsolete]
        protected NumbersKeyboardControl AddKeyboardToControlsNumeric(int x, int y, int width, int height) {
            
            return AddKeyboardToControls<NumbersKeyboardControl>(x, y, width, height);
        }

        /// <summary>
        /// Ajouter un clavier USA sur un écran requiert plusieures étapes et modification de parent, si l'on veut gérer la transparence.
        /// Expliqué en détail ici: http://stackoverflow.com/questions/9387267/transparent-control-over-picturebox
        /// </summary>
        /// <param name="x">Position en X du clavier.</param>
        /// <param name="y">Position en Y du clavier</param>
        /// <param name="width">Largeur du clavier.</param>
        /// <param name="height">Hauteur du clavier.</param>
        [PublicAPI, Obsolete]
        protected UsaKeyboardControl AddKeyboardToControlsUsa(int x, int y, int width, int height) {
            return AddKeyboardToControls<UsaKeyboardControl>(x, y, width, height);
        }

        /// <inheritdoc />
        public void InvokeRefresh()
        {
            this.SafeCall(Refresh);
        }

        [Obsolete]
        protected static void DrawImage([NotNull] Graphics g, Image img, Rectangle zone)
        {
            ScreenUtil.DrawImage(g, img, zone);
        }

        [Obsolete]
        protected static void DrawImage([NotNull] Graphics g, Image img, Point location)
        {
            ScreenUtil.DrawImage(g, img, location);
        }

        [Obsolete]
        protected static void DrawImage([NotNull] Graphics g, Image img, Rectangle? zone)
        {
            ScreenUtil.DrawImage(g, img, zone);
        }

        [Obsolete]
        protected static void DrawImage([NotNull] Graphics g, Image img, Point? location)
        {
            ScreenUtil.DrawImage(g, img, location);
        }

        /// <summary>
        /// Peinture une liste de d'options de Checkbox.
        /// </summary>
        /// <param name="g">L'élément graphique de l'image.</param>
        /// <param name="checkBoxZones">La liste de zones à option.</param>
        /// <param name="checkmarkImage">L'image de crochet. Unique pour la collection de zone.</param>
        protected static void PaintCheckBoxList([NotNull] Graphics g, IList<ScreenCheckOption> checkBoxZones, [NotNull] Image checkmarkImage)
        {
            if (checkBoxZones == null) return;

            foreach (var zone in checkBoxZones)
            {
                if (zone.AnswerState)
                {
                    DrawImage(g, checkmarkImage, zone.CheckLocation);
                }
            }
        }

        /// <summary>
        /// Peinture une liste d'images basée sur des conditions de questionnaire (ou de répondants). Assume que la variable
        /// "QuestionnaireAnswerImage.ConditionCheck" à été mise à jour.
        /// </summary>
        /// <param name="g">L'élément graphique de l'image.</param>
        /// <param name="qaiList">Liste d'images basée sur le questionnaire à afficher.</param>
        protected static void PaintQuestionnaireAnswerImageList([NotNull] Graphics g, IList<QuestionnaireAnswerImage> qaiList)
        {
            if (qaiList == null) return;

            foreach (var qai in qaiList)
            {
                if (qai.ConditionCheck)
                {
                    DrawImage(g, qai.ImageInstance, qai.ImagePoint);
                }
            }
        }


        /// <summary>
        /// Évalue et modifie au besoin l'état du questionnaire et des zone d'options lors d'un événement souris.
        /// </summary>
        /// <param name="x">Position X du clic</param>
        /// <param name="y">Position Y du clic</param>
        /// <param name="checkBoxZones">La liste d'options de checks. Peut être nul ou vide, si c'est le cas provoque un No-Op.</param>
        /// <returns>TRUE si aucune zone n'a été touchée. False sinon.</returns>
        protected bool EvaluateCheckBoxsMouseDown(int x, int y, IList<ScreenCheckOption> checkBoxZones)
        {
            if (checkBoxZones == null) return true;

            foreach (var zone in checkBoxZones)
            {
                if (!InArea(x, y, zone.Position)) continue;
                zone.Toggle();
                zone.SaveToAllModels(this, zone.AnswerState);
                Refresh();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Recycle l'état d'une liste d'options de checks.
        /// </summary>
        /// <param name="checkBoxZones"></param>
        [Obsolete]
        protected void RecycleCheckBoxs(IList<ScreenCheckOption> checkBoxZones)
        {
            if (checkBoxZones == null) return;

            foreach (var zone in checkBoxZones)
            {
                zone.Recycle();
            }
        }

        /// <summary>
        /// Remplit un contrôle avec la réponse texte du questionnaire, si existante et non-vide.
        /// </summary>
        /// <param name="ctrl">L'instance de contrôle à mettre à jour.</param>
        /// <param name="questionNo">Le numéro de question.</param>
        /// <param name="choiceNo">Le numéro de choix. 1 par défaut</param>
        protected void FillControlWithTextAnswer(TextBox ctrl, int questionNo, int choiceNo = 1)
        {
            if (ctrl == null) return;
            var resp = GetCurrentRespondentWithAnswers();

            var str = resp.GetAnswerText(questionNo, choiceNo);
            if (!string.IsNullOrWhiteSpace(str))
            {
                this.SafeCall(() => ctrl.Text = str);
            }
        }
        
        protected void PreloadCheckBoxDefaultAnswers(IList<ScreenCheckOption> checkOptions)
        {
            foreach (var zone in checkOptions)
            {
                if (!(zone.DataModels is DataModelQuestionnaire dmq)) continue;
                var q = GetCurrentQuestionnaire().GetQuestion(dmq.Identifier.QuestionNo);
                if (!q.HasDefaultAnswer()) continue;
                zone.AssignInitialValue(true); // Existence d'une répons eest considéré comme "true" pour un check box.
                Refresh();
            }
        }

        public virtual bool Init()
        {
            return true; // Rien par défaut.
        }

        /// <summary>
        /// Va chercher le répertoire des assets du terminal associé à la configuration actuelle de l'écran.
        /// Raccourci de l'utilisation de la méthode correcte venant de la classe DirectoryUtil.
        /// </summary>
        /// <returns>Le chemin absolu du répertoire.</returns>
        public string GetTerminalDirectory()
        {
            return DirectoryUtil.GetLocalIbrandTerminalDir(ProjectId, QuestionnaireId, TerminalId);
        }

        /// <summary>
        /// Construit un chemin vers un fichier contenu dans le répertoire de données du terminal de l'écran.
        /// Raccourci de l'utilisation de la méthode correcte venant de la classe DirectoryUtil.
        /// </summary>
        /// <returns>Le chemin absolu du fichier.</returns>
        public string GetTerminalDirectoryDataFile(string fileName, DirectoryUtil.ProjectDataDirectoriesEnum dataDir, DirectoryUtil.ProjectDataDirectoriesModEnum mod = DirectoryUtil.ProjectDataDirectoriesModEnum.none)
        {
            return DirectoryUtil.GetLocalIbrandTerminalDataDir(ProjectId, QuestionnaireId, TerminalId, dataDir, mod) + fileName;
        }

        /// <summary>
        /// Va chercher le répertoire des assets du terminal associé à la configuration actuelle de l'écran.
        /// Raccourci de l'utilisation de la méthode correcte venant de la classe DirectoryUtil.
        /// </summary>
        /// <returns>Le chemin absolu du répertoire.</returns>
        public string GetTerminalDirectory(DirectoryUtil.ProjectDataDirectoriesEnum dataDir, DirectoryUtil.ProjectDataDirectoriesModEnum mod = DirectoryUtil.ProjectDataDirectoriesModEnum.none)
        {
            return DirectoryUtil.GetLocalIbrandTerminalDataDir(ProjectId, QuestionnaireId, TerminalId, dataDir, mod);
        }
    }
}
