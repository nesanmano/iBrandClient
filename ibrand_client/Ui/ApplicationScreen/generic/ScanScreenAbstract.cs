using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Ui.NomadScreen.ScreenAction;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Print;
using NomadLogic.Common.Util.RfidReader;
using NomadLogic.Common.Util.String;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Datasource;
using NomadLogic.iBrandDesktop.Resources;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Ce type sous-type d'écran gère une lecture via un périphérique externe, branché à la tablette.
    /// Les classe qui héritent doivent gérer le type exact de périphérique, mais une fois la valeur lue, elles passent toutes
    /// par les même étapes: vérification, affichage d'erreur, gestion d'événements et autres. Toutes ces opération communes sont donc dans
    /// cette classe.
    /// </summary>
    [PublicAPI]
    public abstract class ScanScreenAbstract : AbstractScreen, ILanguageScreenInterface
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private AnimatedImageSequence _waitSequence;
        private readonly TextBox _scanTextBox;
        private HashSet<string> _fullSet;
        private Task _nextAction;
        protected Bitmap _errorBitmap;
        protected bool _showError;
        private volatile bool _block;
        private PictureBox _picture;
        private string _regexString;
        private Regex _regexValidation;
        
        private WebRequestFacade _facadeInstance;

        public event LanguageSelectionEventHandler OnLanguageSelection;

        /// <summary>
        /// On assume que l'enregistrement de la réponse de question à été fait dans le Mapper de la facade. Voir:
        /// <see cref="WebRequestFacade.SaveDataToQuestionnaire(ISaveLocalRespondentData, object)" />
        /// </summary>
        /// <param name="langCode">Le nouveau code de langue.</param>
        public void LanguageSelection(string langCode)
        {
            log.Trace("Changing touch screens language to {0}.", langCode);
            OnLanguageSelection?.Invoke(langCode);

            var resp = GetCurrentRespondentWithAnswers();
            resp.Language = langCode;

            log.Trace("Changing current questionnaire language to {0}.", langCode);
            GetCurrentQuestionnaire().LanguageCurrent = langCode;
        }

        public int? QuestionLanguageNo { get; set; }
        public int QuestionLanguageChoiceNoFr { get; set; }
        public int QuestionLanguageChoiceNoEn { get; set; }
        public bool IsQuestionLanguageRadio { get; set; }
        public Rectangle? NextLocation { get; set; }
        public Rectangle? PreviousLocation { get; set; }
        public Rectangle? CancelLocation { get; set; }
        public Rectangle? ValidateCode { get; set; }

        /// <summary>
        /// Outil pour créer un préfixe au code à chaque lecture (après transformation par le lecteur).
        /// Utilisé pour créer un sel ou un préfixe programmatique (comme un format de date) à chaque code.
        /// </summary>
        [Obsolete("Fonctionalité à l'intérieure de la classe RfidReaderBase")]
        public DatePrefixGenerator DatePrefixGenerator { get; set; }

        /// <summary>
        /// Si on veut vérifier qu'un code lu (avant préfixe, mais après transformation par le lecteur) possède une forme précise vérifiable par REGEX.
        /// Si null, aucune vérification n'est faite.
        /// Cette étape est préalable à la vérification d'une liste pré-établie, et quoique un peu redondante, peut être utilisé simultanément (par exemple, pour éviter une lourde recherche si le format est clairement invalide).
        /// </summary>
        [Obsolete("Fonctionalité à l'intérieure de la classe RfidReaderBase")]
        public string CodeValidationRegex
        {
            get => _regexString;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _regexString = null;
                    _regexValidation = null;
                }
                else
                {
                    if (value.Trim().Equals(_regexString)) return;

                    _regexString = value.Trim();
                    _regexValidation = new Regex(_regexString, RegexOptions.Compiled);
                }
            }
        }

        /// <summary>
        /// Si n'étend pas le code pour le résultat web, on peut dire ici qu'un code problématique provoque un problème.
        /// </summary>
        public bool FailOnNullResult { get; set; } = true;

        /// <summary>
        /// Nom du fichier de configuration de facade. Doit etre dans le repertoire de base du questionnaire.
        /// </summary>
        public virtual string WebFacadeConfigFile { get; set; } = "webfacade_config.json";
        
        /// <summary>
        /// Définition de la méthode à invoquer dans la facade web instanciée par réflexion lors de la lecture d'un code RFID.
        /// On ne peut appeller qu'une seule méthode.
        /// </summary>
        public WebRequestFacadeAction CardReadAction { get; set; }

        /// Liste des zones configurées pour afficher du contenu du questionnaire, sous forme de Labels.
        public IList<QuestionnaireAnswerLabel> QuestionnaireLabels { get; set; }

        /// Liste des zones configurées pourr afficher une image selon le contenu du questionnaire.
        public IList<QuestionnaireAnswerImage> QuestionnaireImages { get; set; }

        /// <summary>
        /// En cas d'erreur, on affiche cette image à l'écran au lieu d'une boîte de message.
        /// </summary>
        public IImageGenerator ErrorImage { get; set; }

        /// <summary>
        /// Position de l'image d'erreur, si on en utilise une.
        /// </summary>
        public Point? ErrorImagePosition { get; set; }

        /// Force l'usage de la zone de validation manuelle (Next) pour passer au prochain écran. L'événement de lecture venant d'un scanner
        /// sera ignoré.
        public bool ForceUseOfNextZone { get; set; }

        /// <summary>
        /// Via le bouton Next, indique que le bouton ne fait pas de validation de code.
        /// Utiliser cette valeur à TRUE et la valeur de "ForceUseOfNextZone" à TRUE implique
        /// que l'action de faire un scan ne provoque d'aucune façon un changement d'écran. Il faut utiliser le bouton.
        /// </summary>
        public bool DontCheckCodeOnButton { get; set; }

        /// <summary>
        /// Le numéro de question qui contient l'image à afficher.
        /// </summary>
        public int? PreviewQuestion { get; set; }

        /// <summary>
        /// Si on veut afficher un preview d'image, on doit indiquer la position et la taille de l'image via un rectangle.
        /// </summary>
        public Rectangle? PreviewLocation { get; set; }

        /// <summary>
        /// Méthode de base pour réagir sur un nouveau code valide.
        /// </summary>
        /// <param name="code">La code à traiter.</param>
        protected void UpdateWithScanValue(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            SuspendTimer();
            var newText = AppendScanValue ? $"{_scanTextBox.Text};{code}" : code;

            this.SafeCall(() =>
            {
                _scanTextBox.Text = newText;
                CheckCodeEvent(!ForceUseOfNextZone);
            });
        }

        /// <summary>
        /// Séquence animée lorsque on attend que la question de code barre soit répondue.
        /// </summary>
        public AnimatedImageSequence WaitSequence
        {
            get => _waitSequence;
            set
            {
                if (_waitSequence != null)
                {
                    _waitSequence.StopSequence();
                    _waitSequence.UpdateDraw -= InvokeRefresh;
                    _waitSequence.Dispose();
                    _waitSequence = null;
                }

                _waitSequence = value;
                if (_waitSequence != null)
                {
                    _waitSequence.UpdateDraw += InvokeRefresh;
                }
            }
        }

        /// <summary>
        /// Valeur en milisecondes du délai entre la réception d'un code valide et de l'action pour passer au prochain écran.
        /// Si une zone Next existe, cette valeur n'est pas utilisée.
        /// </summary>
        public int AutomaticNextDelay { get; set; } = 2000;

        /// <summary>
        /// Indique si on doit ajouter le code lu après la séquence lue précédemment, sans la remplacer.
        /// </summary>
        public bool AppendScanValue { get; set; }

        /// <summary>
        /// Numéro de question dans laquelle il faut faire une sauvegarde de la lecture. Si plus petit que 1, ne fait aucune sauvegarde de question.
        /// </summary>
        public int? ScanQuestionNo { get; set; }
        
        protected int _scanTriggerOnCharacterNumber;
        private Task _task;
        private CancellationTokenSource _cts;
        private readonly object _lock = new object();
        private bool _enterDetected;
        private bool _scanOnEnter;

        /// Dès que la zone de texte reçoit ce nombre de caractère (ou plus), cela provoque une vérification du code.
        public int ScanTriggerOnCharacterNumber {
            get => _scanTriggerOnCharacterNumber;
            set
            {
                _scanTriggerOnCharacterNumber = value;
                UpdateScanCharacterTriggerEvent();
            }
        }

        public bool ScanTriggerOnReturn {
            get => _scanOnEnter;
            set
            {
                _scanTextBox.KeyDown -= OnScanTextBoxOnKeyDown;
                _scanTextBox.KeyPress -= OnScanTextBoxOnKeyPress;
                _scanOnEnter = value;

                if (_scanOnEnter)
                {
                    log.Trace("Scan box Key detection event enabled.");
                    _scanTextBox.KeyDown += OnScanTextBoxOnKeyDown;
                    _scanTextBox.KeyPress += OnScanTextBoxOnKeyPress;
                }

                UpdateScanCharacterTriggerEvent();
            }
        }

        private void OnScanTextBoxOnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!_enterDetected) return;

            // On défait le event.
            _enterDetected = false;
            e.Handled = true;

            if (ScanTriggerOnCharacterNumber > 0)
            {
                OnTextChangedScanTrigger(sender, e);
            }
            else
            {
                log.Debug("Return key pressed. Launching code validation event.");
                this.SafeCall(() => { CheckCodeEvent(!ForceUseOfNextZone); });
            }
        }

        private void OnScanTextBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            log.Trace("Enter key pressed.");
            _enterDetected = true;
        }

        /// Quand on fait une validation automatique, on va accéder à cette liste pour l'opération de changement d'écran.
        /// Par défaut: Next(1)
        public ScreenChangeOptions OnAutoScreenChange { get; set; } = ScreenChangeOptions.GenerateBasicNextOptions();
        
        protected ScanScreenAbstract(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc ?? backgroundImgEnSrc, sc)
        {
            var font = new Font(FontFamily.GenericSansSerif, 18);

            this.InitTextBox(out _scanTextBox, textboxLocation ?? new Rectangle(PictureBox.Width / 2 - 150, PictureBox.Height / 2 - 150, 300, 30), font, 1);
            _scanTextBox.TextAlign = HorizontalAlignment.Center;
            
            _scanTextBox.Select();
        }

        protected override void OnLoad()
        {
            UpdateScanCharacterTriggerEvent();

            if (_waitSequence != null)
            {
                _waitSequence.ClearCurrentImages();
                _waitSequence.CreateImages(_activeLg);
            }

            _errorBitmap = ErrorImage?.GenerateImage(_activeLg);

            if (PreviewLocation.HasValue)
            {
                _picture = new PictureBox
                {
                    Location = new Point(PreviewLocation.Value.X, PreviewLocation.Value.Y),
                    Size = new Size(PreviewLocation.Value.Width, PreviewLocation.Value.Height),
                    BackColor = Color.Black,
                    Visible = true
                };

                Controls.Add(_picture);
                _picture.BringToFront();
            }

            CustomLoad();
        }

        /// <summary>
        /// Ajoute ou enlève un événement levé quand la boîte de texte atteint un certain nombre de caractère.
        /// </summary>
        protected void UpdateScanCharacterTriggerEvent()
        {
            _scanTextBox.TextChanged -= OnTextChangedScanTrigger;
            if (_scanOnEnter || _scanTriggerOnCharacterNumber > 0)
            {
                _scanTextBox.TextChanged += OnTextChangedScanTrigger;
            }
        }

        private void OnTextChangedScanTrigger(object sender, EventArgs e)
        {
            var check = false;

            // Vérification nombre de caractères non-invisible
            var code = _scanTextBox.Text;
            var visCount = code.Trim().Length;

            log.Trace("Analysing scan text: {1}", code);
            if (_scanTriggerOnCharacterNumber > 0 && visCount >= _scanTriggerOnCharacterNumber)
            {
                log.Debug("Automatic code validation launched by minimal number of caracter.");
                check = true;
            }

            // Les événement de claviers sont traités ailleurs (voir plus haut: ScanTriggerOnReturn)

            if (check)
            {
                this.SafeCall(() => { CheckCodeEvent(!ForceUseOfNextZone); });
            }
        }

        /// <summary>
        /// Si on doit ajouter un chargement personnalisé (initialisation de périphérique, notamment),
        /// on peut surcharger cette méthode.
        /// </summary>
        protected virtual async void CustomLoad()
        {
            if (QuestionnaireLabels != null && QuestionnaireLabels.Any())
            {
                foreach (var qal in QuestionnaireLabels)
                {
                    if (!qal.Init() || qal.LabelInstance == null)
                    {
                        throw new System.Exception("Could not intiialize Questionnaire Label: " + qal);
                    }
                    qal.LabelInstance.Parent = PictureBox;
                    qal.LabelInstance.BringToFront();
                }
            }

            if (!await InitializeFacade())
            {
                MsgBox.Show(this, "Configuration file was not found. Contact system admin.", iBrand.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected virtual void InitBitmap()
        {
            if (QuestionnaireImages != null && QuestionnaireImages.Any())
            {
                foreach (var qai in QuestionnaireImages)
                {
                    if (!qai.InitImage(null, _activeLg) || qai.ImageInstance == null)
                    {
                        throw new System.Exception("Could not intiialize Questionnaire Image: " + qai);
                    }
                }
            }
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_showError && ErrorImagePosition.HasValue)
            {
                ScreenUtil.DrawImage(g, _errorBitmap, ErrorImagePosition.Value);
            }

            _waitSequence?.Paint(g);
        }

        public override bool ChangeLanguage(string language)
        {
            
            var change = base.ChangeLanguage(language);
            if (!change) return false;
            
            if (_waitSequence == null) return true;

            _block = true;
            _waitSequence.ClearCurrentImages();
            _waitSequence.CreateImages(_activeLg);

            _errorBitmap?.Dispose();
            _errorBitmap = ErrorImage?.GenerateImage(_activeLg);
            _block = false;

            return true;
        }

        public override void Recycle()
        {
            base.Recycle();

            lock (_lock)
            {
                _cts?.Cancel();
                _cts = null;
                _task = null;
            }

            this.SafeCall(() => {
                if (_picture != null)
                {
                    _picture.Image?.Dispose();
                    _picture.Image = null;
                }

                _nextAction = null;
                _scanTextBox.Text = "";
                _block = false;
            });
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, CancelLocation))
            {
                _nextAction = null;
                FirstScreen();
                return false;
            }

            if (InArea(x, y, NextLocation))
            {
                if (!DontCheckCodeOnButton)
                {
                    CheckCodeEvent(true);
                }
                else
                {
                    _nextAction = null;
                    Next();
                }

                return false;
            }

            if (InArea(x, y, PreviousLocation))
            {
                _nextAction = null;
                Back();
                return false;
            }

            if (InArea(x, y, ValidateCode))
            {
                if ( !_block)
                {
                    this.SafeCall(() => CheckCodeEvent(!ForceUseOfNextZone));
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Assigne un (ou plusieurs) fichier contenant les codes entiers de validation. Si on passe une liste vide ou inexistante, cela a pour effet d'enlever les
        /// valeurs de validation et donc de ne faire aucune validation.
        /// </summary>
        /// <param name="sources">Liste de fichiers qui contiennent les données à lire.</param>
        public void SetValidationSetFromFile(params FileInfo[] sources)
        {
            if (sources == null || !sources.Any())
            {
                _fullSet?.Clear();
                _fullSet = null;
                return;
            }

            _fullSet = new HashSet<string>();
            
            if (!string.IsNullOrWhiteSpace(DebugValidationValue))
            {
                _fullSet.Add(DebugValidationValue);
            }
            
            foreach (var fi in sources)
            {
                if (fi == null) continue;
                
                if (string.Equals(fi.Extension, ".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Format XML de NomadLogic de DataSource
                    var dataInfo = new DatasourceInfo("validation", fi.FullName, "in_memory");
                    var dataSource = new InMemoryDatasource(dataInfo, new Project(-1, ""));
                    dataSource.Load();
                    
                    foreach (var value in dataSource.GetValues("code"))
                    {
                        _fullSet.Add(value);
                    }
                }
                else if (string.Equals(fi.Extension, ".json", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new NotImplementedException("Validation via JSON pas encore implémentée.");
                }
                else if (string.Equals(fi.Extension, ".esh", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Extension spéciale pour les fichiers envoyé par EShots pour leurs codes QR.
                    using (var fileReader = new StreamReader(fi.FullName))
                    {
                        var line = fileReader.ReadLine();
                        if (line == null) return;
                        while ((line = fileReader.ReadLine()) != null)
                        {
                            if (line.StartsWith("#")) continue;
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var result = line.Split('\t');
                            _fullSet.Add(result[0] + result[1]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Pour ajouter des identifiant supplémentaires
        /// </summary>
        /// <param name="extraSet"></param>
        protected void CustomExtraValidationSetData(IEnumerable<string> extraSet)
        {
            if (extraSet == null) return;

            if (_fullSet == null)
            {
                _fullSet = new HashSet<string>();
            }

            foreach (var id in extraSet)
            {
                _fullSet.Add(id);
            }
        }

        /// <summary>
        /// Cette valeur, nulle par défaut, est toujours vrai lors d'une validation avec fichiers.
        /// Permet d'ajouter une valeur "debug", utile en terme de tests ou de sécurité lors d'une activation.
        /// Cette valeur est AJOUTÉE aux valeurs lues à partir d'un fichier, lors de l'initialisation des fichiers.
        /// </summary>
        public string DebugValidationValue { get; set; }

        /// <summary>
        /// Vérifie que le numéro de scan fait partie d'une liste pré-établie.
        /// </summary>
        private bool ValidateScanValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return _fullSet == null || _fullSet.Contains(value);
        }

        /// Plus une option de débogage, quand on ne détecte aucune option de changement valide, on annule le répondant.
        /// Cela représente un cas d'état invalide (il manque des informations dans le questionnaire). 
        public bool OnMissingScreenChangeDoCancel { get; set; }

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();
            SuspendTimer();
            
            ResumeTimer();

            this.SafeCall(() =>
            {
                if (PreviewQuestion.HasValue)
                {
                    _picture.SizeMode = PictureBoxSizeMode.StretchImage;
                    _picture.ImageLocation = GetTextAnswerFromQuestion(PreviewQuestion.Value);
                }

                _scanTextBox.Focus();
                
                if (QuestionnaireLabels != null && QuestionnaireLabels.Any())
                {
                    var dico = GetCurrentQuestionnaire().GetAnswersDictionary();
                    foreach (var qal in QuestionnaireLabels)
                    {
                        var finalString = StringUtil.CreateStringFromTokenizedPatternRespondent(qal.TokenString, dico);
                        qal.LabelInstance.Text = finalString;
                    }
                }

                if (QuestionnaireImages != null && QuestionnaireImages.Any())
                {
                    var resp = GetCurrentRespondentWithAnswers();
                    foreach (var qai in QuestionnaireImages)
                    {
                        qai.Update(resp);
                    }
                }

                Refresh();
            });
        }

        /// <summary>
        /// Dans certaines circonstance (avec certains types de lecteurs) il faut transformer la séquence de caractère lus avant de processer le tout.
        /// </summary>
        /// <param name="sequenceOriginal">La séquence telle que présente dans la boîte de texte.</param>
        /// <returns>La séquence transformée, à envoyer dans les système subséquents.</returns>
        protected virtual RfidTransformationResult TransformCode(string sequenceOriginal)
        {
            return new RfidTransformationResult
            {
                RawCode = sequenceOriginal,
                StringCode = sequenceOriginal,
                FinalCode = sequenceOriginal
            };
        }

        /// <summary>
        /// Cette méthode enveloppe l'appel vers la vérification du code dans une tâche asynchrone, unique. On ne peut avoir qu'une seule demande
        /// de vérification, ce qui empêche les appels répétitifs.
        /// Cette tâche est annulée si on recycle ou dispose l'écran, ce qui a pour effet d'empêcher les dialogues de s'ouvrir le cas échéant.
        /// </summary>
        /// <param name="nextAfterCheck">Si on passe automatiquement à l'écran suivant lors d'une complétion positive de la vérification.</param>
        protected async void CheckCodeEvent(bool nextAfterCheck)
        {
            Task tmp;
            lock (_lock)
            {
                if (_task != null)
                {
                    return;
                }

                _cts = new CancellationTokenSource();
                _task = new Task(() =>
                {
                    this.SafeCall(async () => await ExecuteCodeCheck(nextAfterCheck, _cts.Token));
                }, _cts.Token);
                tmp = _task.ContinueWith(t =>
                {
                    _task = null;
                    _cts = null;
                });

                _task.Start();
            }

            await tmp;
        }

        private async Task ExecuteCodeCheck(bool nextAfterCheck, CancellationToken ct)
        {
            if (_block) return;

            if (_scanTextBox == null) return;
            _block = true;

            try
            {
                var codeRez = TransformCode(_scanTextBox.Text.Trim());
                log.Trace("Sequence transformed: {0}", codeRez);
                bool valid;

                if (!string.IsNullOrWhiteSpace(codeRez.StringCode))
                {
                    valid = ValidateScanValue(codeRez.StringCode);
                    log.Trace("Code validation: {0}", valid);

                    // TODO Redondant: fait partie de la classe RfidReaderBase désormais.
                    // 2a- Validation du format avec un REGEX, si existant
                    if (_regexValidation != null)
                    {
                        var rez = _regexValidation.Match(codeRez.StringCode);
                        if (!rez.Success)
                        {
                            valid = false;
                        }
                    }

                    // TODO Redondant: fait partie de la classe RfidReaderBase désormais.
                    if (valid && DatePrefixGenerator != null)
                    {
                        codeRez.FinalCode = DatePrefixGenerator.ModifyString(codeRez.FinalCode);
                    }

                    valid &= await OnValidateAction(codeRez.FinalCode);
                    log.Trace("Code validation after action: {0}", valid);
                }
                else
                {
                    log.Error("Code was empty or null after transformation. Check Rfid Reader configurations.");
                    valid = false;
                }

                if (ct.IsCancellationRequested) return;

                _scanTextBox.ForeColor = valid ? Color.Black : Color.Red;
                
                if (valid)
                {
                    if (ScanQuestionNo.HasValue && ScanQuestionNo.Value > 0)
                    {
                        SaveAnswerToQuestion(new Answer {QuestionNo = ScanQuestionNo.Value, Response = codeRez.FinalCode});
                    }

                    _showError = false;
                    if (nextAfterCheck)
                    {
                        _nextAction = new Task(() =>
                        {
                            try
                            {
                                if (HasCustomSuccessAction)
                                {
                                    ExecuteCustomSuccessAction(codeRez.FinalCode);
                                }
                                else
                                {
                                    if (ct.IsCancellationRequested) return;
                                    this.SafeCall(() => { _scanTextBox.Text = ""; });
                                    _waitSequence?.StartSequence();
                                    Task.Delay(AutomaticNextDelay, ct);
                                }
                            }
                            finally
                            {
                                if (!ct.IsCancellationRequested)
                                {
                                    // Si next action est mise à null avant qu'on arrive ici, c'est qu'il ne faut pas l'exécuter (dans le cas d'un timeout ou cancellation par exemple).
                                    _waitSequence?.StopSequence();
                                    if (_nextAction != null) ExecuteNext();
                                }
                            }
                        });
                        _nextAction.Start();
                    }
                    else
                    {
                        ResumeTimer();
                    }
                }
                else
                {
                    if (ct.IsCancellationRequested) return;

                    if (_facadeInstance != null && _facadeInstance.ContinueOnFail)
                    {
                        ExecuteNext();
                        return;
                    } 
                    else if (_errorBitmap == null)
                    {
                        this.SafeCall(() =>
                        {
                            var message = _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase)
                                ? "Le code lu est inconnu/invalide. Veuillez réessayer ou vérifier la connexion externe."
                                : "Read code is unknown or invalid. Please try again or check external connection.";
                            var result = MessageBox.Show(this, message, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
                            log.Debug($"Bad scan: {result}");
                            ResetScanArea();
                        });
                    }
                    else
                    {
                        _showError = true;
                    }

                    ResumeTimer();
                }
            }
            finally
            {
                _block = false;
            }

            Refresh();
        }

        /// <summary>
        /// Éxécute une action spécifique à une classe enfant en cas de succès lors de la validation du code.
        /// Cette méthode ne roulera que si ce succès est confirmé, et juste avant le changement d'écran.
        /// </summary>
        /// <param name="code">La code RFID validé.</param>
        protected virtual void ExecuteCustomSuccessAction(string code)
        {
            // rien par défaut;
        }

        protected virtual bool HasCustomSuccessAction => false;

        private void ExecuteNext()
        {
            var resp = GetCurrentRespondentWithAnswers();
            log.Trace("Evaluating respondent for screen change: resp={0}", resp);
            var sco = OnAutoScreenChange?.Evaluate(resp);
            if (sco == null)
            {
                if (OnMissingScreenChangeDoCancel)
                {
                    log.Warn(
                        "No valid ScreenChange option detected. Check screen configuration. Cancelling respondent.");
                    this.SafeCall(() =>
                    {
                        var message =
                            _activeLg.Equals(Constants.FR,
                                StringComparison.InvariantCultureIgnoreCase)
                                ? "Une erreur de logique a été détectée. Impossible de continuer. Ce répondant sera annulé."
                                : "Logic error detected. Cannot continue. This respondent will be cancelled.";
                        MessageBox.Show(this, message, @"iBrand", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        FirstScreen();
                    });
                }
                else
                {
                    log.Warn("No valid ScreenChange option detected. Check screen configuration.");
                }
            }
            else
            {
                Next(sco.Step);
            }
        }

        /// Permet au code d'exécuter du code supplémentaire quand on a fini la validation de l'identifiant lu.
        /// Incluant la vérification via exécution de façade web.
        /// <param name="sequenceRead">Le texte lu à valider.</param>
        protected virtual async Task<bool> OnValidateAction(string sequenceRead)
        {
            if (_facadeInstance != null) return await ExecuteFacade(sequenceRead);
            return true;
        }

        protected void ResetScanArea()
        {
            this.SafeCall(() =>
            {
                if (_scanTextBox == null) return;
                _scanTextBox.Text = "";
                _scanTextBox.Focus();
            });
        }
        
        /// <summary>
        /// Cette méthode appelle la facade et les fonctions connexes avec le ID de la carte lue.
        /// </summary>
        /// <param name="sequenceRead">Le code saisi par l'écran.</param>
        protected async Task<bool> ExecuteFacade(string sequenceRead)
        {
            if (_facadeInstance == null || string.IsNullOrWhiteSpace(sequenceRead)) return false;

            if (ScanQuestionNo > 0)
            {
                SaveAnswerToQuestion(new Answer {QuestionNo = ScanQuestionNo.Value, Response = sequenceRead});
            }
            else
            {
                log.Warn("No question assigned to scan event. Web requests will probably fail. Correct this in executable.");
            }

            var resp = GetCurrentRespondentWithAnswers();
            var dic = GetCurrentQuestionnaire().GetAnswersDictionary();
            DialogResult diagResult;

            do
            {
                log.Trace("Launching facade method.");
                var res = await _facadeInstance.ConstructAndInvokeMethod(resp, dic);

                // Sauvegarde le résultat de l'appel via les configurations de "mapping" de réponse à questionnaire contenues dans la configuration de la facade.
                log.Trace("Saving data from request.");
                _facadeInstance.SaveDataToQuestionnaire(this, res);

                // TODO Avoir un système de vérification supplémentaire (genre tel type de retour provoque une rreur, comme dans le cas de TD PayWall (DIA-2018-02)

                diagResult = ExecuteFacadeCustomCode(sequenceRead, res, _facadeInstance.WebRequestAction.ReturnType);
                log.Trace("Result from custom code: {0}", diagResult);

            } while (diagResult == DialogResult.Retry); // Si l'usager veut recommencer, on le fait immédiatement.

            return diagResult == DialogResult.OK && ExecuteScreenActions(_facadeInstance.ScreenActions);
        }

        /// <summary>
        /// Méthode pour crochet de code personnalisé à la fin d'une exécution de requête qui ne peut être gérée totalement par la façade.
        /// </summary>
        /// <param name="cardUid"></param>
        /// <param name="res">l'instance de l'objet résultat venant de la requête.</param>
        /// <param name="returnType">Le type de cet objet.</param>
        /// <returns>OK si tout s'est bien passé. Sinon, un retour de dialogue indiquant ce qu'il faut faire.</returns>
        protected abstract DialogResult ExecuteFacadeCustomCode(string cardUid, object res, Type returnType);

        /// <summary>
        /// Cette méthode vérifie si une configuration est disponible pour la façade, et si oui tente d'instancier et valider l'instance de facade.
        /// Si oui, l'objet de facade sera non-nul comme effet de bord.
        /// </summary>
        /// <returns>TRUE si une configuration valide et correctement utilisable existe.</returns>
        private async Task<bool> InitializeFacade()
        {
            if (ScanQuestionNo == null)
            {
                log.Debug("No question assigned to scan ID. Web requests will probably not work.");
            }

            var baseDir = GetTerminalDirectory();
            var configFile = baseDir + WebFacadeConfigFile;
            var facadeConfigFilePath = new FileInfo(configFile);
            log.Trace("Trying to locate configuration file: {0}", configFile);

            if (facadeConfigFilePath.Exists)
            {
                try
                {
                    log.Trace("Converting configuration file to facade instance.");
                    _facadeInstance = JsonUtil.ReadJsonFile<WebRequestFacade>(facadeConfigFilePath);
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Could not instanciate class from facade config file.");
                    return false;
                }

                if (_facadeInstance == null)
                {
                    log.Error("Impossible to initialize facade with config file. Check JSON config.");
                    return false;
                }

                try
                {
                    log.Trace("Initializing facade instance: {0}", _facadeInstance.WebExecutorClassName);
                    if (!await InitializeFacadeCustom(_facadeInstance))
                    {
                        log.Error("Facade initialisation requested but failed.");
                        return false;
                    }
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Error while initializing facade instance.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Dans le cas où il faut une initialisation personnalisée, on peut invoquer cette méthode.
        /// Basé sur le concept de Template method.
        /// </summary>
        /// <param name="facade"></param>
        /// <returns></returns>
        protected virtual async Task<bool> InitializeFacadeCustom([NotNull] WebRequestFacade facade)
        {
            return await facade.Init();
        }

        public override void Dispose()
        {
            base.Dispose();

            lock (_lock)
            {
                _cts?.Cancel();
                _cts = null;
                _task = null;
            }

            _facadeInstance?.Dispose();

            if (QuestionnaireImages == null) return;
            foreach (var qi in QuestionnaireImages)
            {
                qi?.Dispose();
            }
        }

        protected virtual bool ExecuteScreenActions(IList<IScreenAction> actions)
        {
            if (actions == null || !actions.Any()) return true;

            foreach (var act in actions)
            {
                var loopBreak = false;
                switch (act)
                {
                    case PrintScreenAction print:
                        var path = print.FixedPrintPicture;
                        if (path == null && print.QuestionPrintPicture.HasValue)
                        {
                            path = GetCurrentRespondentWithAnswers().GetAnswerText(print.QuestionPrintPicture.Value);
                        }

                        if (!File.Exists(path))
                        {
                            path = GetTerminalDirectory() + path;
                        }

                        if (File.Exists(path))
                        {
                            if (!PrintUtil.Print(path))
                            {
                                if (print.ShowErrorMessage)
                                {
                                    this.SafeCall(() =>
                                    {
                                        var message = _activeLg.Equals(Constants.FR)
                                            ? print.ErrorMessageFr
                                            : print.ErrorMessageEn;
                                        MsgBox.Show(this, message, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                    });
                                }

                                if (print.IsCritical)
                                {
                                    log.Error("Print attempt has failed. Check printer and/or configuration.");
                                    loopBreak = true;
                                }
                            }
                            else if (print.QuestionNumberOfPrint.HasValue)
                            {
                                IncrementEventInQuestionnaire(print.QuestionNumberOfPrint.Value);
                            }
                        }
                        break;
                        // TODO AJouter les autres classes ET/OU mettre cette logique dans une méthode polymorphique.
                }

                if (loopBreak) break; // À cause du "switch"
            }

            return true;
        }
    }
}