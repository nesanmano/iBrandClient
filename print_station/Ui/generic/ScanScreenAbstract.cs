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
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop;
using NomadLogic.iBrandDesktop.Datasource;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.PrintStation.Ui.generic
{
    /// <summary>
    /// Copie de la classe de base pour les scans dans le Printstation
    /// </summary>
    [PublicAPI]
    public class ScanScreenPrintstationAbstract : AbstractPrintStationScreen, ILanguageScreenInterface
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
        private Task _task;
        private CancellationTokenSource _cts;
        private readonly object _lock = new object();

        public event LanguageSelectionEventHandler OnLanguageSelection;
        public void LanguageSelection(string lg)
        {
            OnLanguageSelection?.Invoke(lg);
        }

        public int? QuestionLanguageNo { get; set; }
        public int QuestionLanguageChoiceNoFr { get; set; }
        public int QuestionLanguageChoiceNoEn { get; set; }
        public bool IsQuestionLanguageRadio { get; set; }

        public Rectangle? NextLocation { get; set; }
        public Rectangle? PreviousLocation { get; set; }
        public Rectangle? CancelLocation { get; set; }
        public Rectangle? ValidateRfid { get; set; }

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
        /// Dès que la zone de texte reçoit ce nombre de caractère (ou plus), cela provoque une vérification du code.
        public int ScanTriggerOnCharacterNumber {
            get => _scanTriggerOnCharacterNumber;
            set
            {
                _scanTriggerOnCharacterNumber = value;
                UpdateScanCharacterTriggerEvent();
            }
        }

        /// Quand on fait une validation automatique, on va accéder à cette liste pour l'opération de changement d'écran.
        /// Par défaut: Next(1)
        public ScreenChangeOptions OnAutoScreenChange { get; set; } = ScreenChangeOptions.GenerateBasicNextOptions();
        
        protected ScanScreenPrintstationAbstract(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc, Rectangle? textboxLocation = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc ?? backgroundImgEnSrc, sc)
        {
            _scanTextBox = new TextBox();
            var font = new Font(FontFamily.GenericSansSerif, 18);

            this.InitTextBox(_scanTextBox, textboxLocation ?? new Rectangle(PictureBox.Width / 2 - 150, PictureBox.Height / 2 - 150, 300, 30), font, 1);
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

        protected void UpdateScanCharacterTriggerEvent()
        {
            _scanTextBox.TextChanged -= OnTextChangedScanTrigger;
            if (ScanTriggerOnCharacterNumber > 0)
            {
                _scanTextBox.TextChanged += OnTextChangedScanTrigger;
            }
        }

        private void OnTextChangedScanTrigger(object sender, EventArgs e)
        {
            if (_scanTextBox.Text.Length < ScanTriggerOnCharacterNumber) return;
            this.SafeCall(() =>
            {
                CheckCodeEvent(!ForceUseOfNextZone);
            });
        }

        /// <summary>
        /// Si on doit ajouter un chargement personnalisé (initialisation de périphérique, notamment),
        /// on peut surcharger cette méthode.
        /// </summary>
        protected virtual void CustomLoad()
        {
            // Rien par défaut.
        }

        /// <inheritdoc />
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_showError && ErrorImagePosition.HasValue)
            {
                DrawImage(g, _errorBitmap, ErrorImagePosition.Value);
            }

            _waitSequence?.Paint(g);
        }

        /// <inheritdoc />
        public override bool ChangeLanguage(string language)
        {
            _block = true;
            var change = base.ChangeLanguage(language);
            if (!change) return false;
            
            if (_waitSequence == null) return true;

            _waitSequence.ClearCurrentImages();
            _waitSequence.CreateImages(_activeLg);

            _errorBitmap?.Dispose();
            _errorBitmap = ErrorImage?.GenerateImage(_activeLg);
            _block = false;

            return true;
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();

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

        /// <inheritdoc />
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

            if (InArea(x, y, ValidateRfid))
            {
                UpdateWithScanValue(_scanTextBox.Text);
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
        /// Cette valeur, nulle par défaut, est toujours vrai lors d'une validation avec fichiers.
        /// Permet d'ajouter une valeur "debug", utile en terme de tests ou de sécurité lors d'une activation.
        /// Cette valeur est AJOUTÉE aux valeurs lues à partir d'un fichier, lors de l'initialisation des fichiers.
        /// </summary>
        public string DebugValidationValue { get; set; }

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
                Refresh();
            });
        }

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
            _block = true;

            try
            {
                var code = _scanTextBox.Text;
                var valid = ValidateScanValue(code);

                valid &= await OnValidateAction(code);

                _scanTextBox.ForeColor = valid ? Color.Black : Color.Red;

                if (valid)
                {
                    if (ScanQuestionNo.HasValue && ScanQuestionNo.Value > 0)
                    {
                        //SaveAnswerToQuestion(ScanQuestionNo.Value, 1, code);
                    }

                    _showError = false;
                    if (nextAfterCheck)
                    {
                        _nextAction = new Task(() =>
                        {
                            try
                            {
                                this.SafeCall(() => { _scanTextBox.Text = ""; });
                                _waitSequence?.StartSequence();
                                Thread.Sleep(AutomaticNextDelay);
                            }
                            finally
                            {
                                // Si next action est mise à null avant qu'on arrive ici, c'est qu'il ne faut pas l'exécuter (dans le cas d'un timeout ou cancellation par exemple).
                                _waitSequence?.StopSequence();
                                if (_nextAction != null)
                                {
                                    var resp = GetCurrentRespondentWithAnswers();
                                    log.Trace("Evaluating respondent for screen change: resp={0}", resp);
                                    var sco = OnAutoScreenChange?.Evaluate(resp);
                                    if (sco == null)
                                    {
                                        if (OnMissingScreenChangeDoCancel)
                                        {
                                            log.Warn("No valid ScreenChange option detected. Check screen configuration. Cancelling respondent.");
                                            this.SafeCall(() =>
                                            {
                                                var message = _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase)
                                                    ? "Une erreur de logique a été détectée. Impossible de continuer. Ce répondant sera annulé."
                                                    : "Logic error detected. Cannot continue. This respondent will be cancelled.";
                                                MessageBox.Show(this, message, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                    if (_errorBitmap == null)
                    {
                        MsgBox.Show(this, _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase) ? "Lecture invalide" : "Invalid scan", "iBrand",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        this.SafeCall(() =>
                        {
                            var message = _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase)
                                ? "Le code lu est inconnu/invalide. Veuillez réessayer ou vérifier la connexion externe."
                                : "Read code is unknown or invalid. Please try again or check external connection.";
                            MessageBox.Show(this, message, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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

        /// Permet au code d'exécuter du code supplémentaire quand on a fini la validation de l'identifiant lu.
        /// <param name="text">Le texte validé.</param>
        protected virtual Task<bool> OnValidateAction(string text)
        {
            return Task.Run(() => true);
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
    }
}