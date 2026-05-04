using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Networking.Ftp;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using WinSCP;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Cette classe va tenter de trouver un fichier d'image via un nom (sans extension) donné par une boîte texte.
    /// Si le fichier est trouvé, on peut le sauvegarder dans le questionnaire et automatiquement lancer un passage au prochain écran (ou se réinitialiser).
    /// S'il n'est pas trouvé, on peut lancer un message d'erreur.
    /// </summary>
    [PublicAPI]
    public class ScreenDefinitionFtpFileFinder : ScreenFieldAbstract, IJsonConfig, IExternalFileConfig, IOnLoad,
        IActivateScreen, ISaveToQuestionnaire, IRecyclable, IEvaluateScreenZones, IImageInitialisable, IPaint, IChangeLanguage
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private TextBox _textBox;
        private CancellationTokenSource _cts;
        private readonly object _lock = new object();
        private Image _validateImage;
        private FileInfo _localFtpFile;
        private SessionOptions _ftpSessionOptions;
        private DirectoryInfo _localWorkDirectory;
        private bool _useRemoteName;
        private ImageListTransformOptions _imageListTransform;
        private string _terminalDir;
        private AbstractScreen _screen;
        protected AnimatedImageSequence _workSequence;
        private bool _successFileOperation;
        private bool _initFtpSuccess;
        private bool _initFtpErrorDetected;
        private bool _working;
        private TextBoxKeyboardEventsWrapper _textBoxEventWrapper;
        private ActivityLoggerService _activityLogger;

        public string DefaultConfigFileName => "ftp_file_finder.json";
        public string ConfigFileName { get; set; }

        #region Properties

        /// <summary>
        /// Indique qu'une option de sauvegarde additionnelle est faite pour le fichier avant modifications.
        /// Peut être requis dans certaines circonstances, comme un traitement via ImageMagick, qui requiert
        /// un accès direct à un questionnaire plutôt qu'à des valeurs dynamiques.
        /// </summary>
        public DataModelBase OriginalFileDataModel { get; set; }

        /// <summary>
        /// Indique qu'il faut valider la connexion FTP à chaque activation. À utiliser seulement quand
        /// le réseau est instable.
        /// </summary>
        public bool CheckFtpOnActivate { get; set; }

        /// <summary>
        /// Séquence animée lorsque on attend que le traitement soit fini.
        /// </summary>
        public AnimatedImageSequence WorkSequence
        {
            get => _workSequence;
            set
            {
                if (_workSequence != null)
                {
                    _workSequence.StopSequence();
                    _workSequence.UpdateDraw -= InvokeScreenRefresh;
                    _workSequence.Dispose();
                    _workSequence = null;
                }

                _workSequence = value;
                if (_workSequence != null)
                {
                    _workSequence.UpdateDraw += InvokeScreenRefresh;
                }
            }
        }

        /// <summary>
        /// Si mis à vrai, remet à zéro l'état de recherche interne, comme un recyclage, à chaque fois que l'on reviens sur cet écran.
        /// </summary>
        public bool ResetOnActivate { get; set; }

        public FontDefinitionInfo DefaultFontDefinition { get; set; }

        [JsonRequired] public string AutomaticFileExtension { get; set; } = "jpg";

        /// <summary>
        /// Provoque une recherche quand on perçoit au moins ce nombre de caractères dans la boîte de recherche.
        /// </summary>
        public uint? TriggerOnMinCharacters { get; set; }

        /// <summary>
        /// Provoque une recherche quand on détecte le caractère "retour" dans la boîte de texte.
        /// </summary>
        public bool TriggerOnReturn { get; set; }

        public Rectangle? ValidateButtonPosition { get; set; }

        public string ValidateButtonFileName { get; set; }

        /// <summary>
        /// Object de configuration FTP
        /// </summary>
        [JsonRequired]
        public FtpConfigInfo FtpConfiguration { get; set; }
        
        public DirectoryInfo DebugWorkDirectory { get; set; }
        
        public uint? MinimumLength { get; set; }

        public char PadCharacter { get; set; } = '0';

        /// <summary>
        /// Contient l'ensemble des options de transformations avec des fichiers d'image.
        /// Si cette variable n'est pas explicitement donnée, on tente de lire un fichier dans le répertoire du projet, qu'il existe ou non, et on met à jour
        /// la configuration selon le contenu de ce fichier.
        /// </summary>
        public ImageListTransformOptions ImageListTransformOptions
        {
            get
            {
                if (_imageListTransform != null) return _imageListTransform;

                var transform = new ImageListTransformOptions();
                JsonUtil.UpdateFromJson(
                    new FileInfo(_terminalDir + transform.DefaultConfigFileName),
                    transform);
                return transform;
            }
            set => _imageListTransform = value;
        }

        /// <summary>
        /// Action de changement d'écran optionnelle, au moment où le traitement du fichier est terminé.
        /// </summary>
        public ScreenChangeOptions ActionOnSuccess { get; set; }

        #endregion

        private void InvokeScreenRefresh()
        {
            _screen?.InvokeRefresh();
        }

        public bool OnLoad(AbstractScreen screen)
        {
            _activityLogger = ServicesContainer.GetService<ActivityLoggerService>();
            
            _terminalDir = screen.GetTerminalDirectory();
            if (!Directory.Exists(_terminalDir))
            {
                log.Warn("Terminal directory from screen does not exists: " + _terminalDir);
                return false;
            }

            if (_textBox == null)
            {
                var fontToUse = StaticCustomFontRepository.CreateCustomFont(DefaultFontDefinition);
                screen.InitTextBox(out _textBox, Position, fontToUse, 1);

                _textBoxEventWrapper = new TextBoxKeyboardEventsWrapper(_textBox, TextBoxScanTrigger, TriggerOnReturn, (int?)TriggerOnMinCharacters);

                _textBox.Enabled = false; // Mis active quand FTP est confirmé valide.
                _textBox.BringToFront();
            }
            
            if (DebugWorkDirectory != null && DebugWorkDirectory.Exists)
            {
                _localWorkDirectory = DebugWorkDirectory;
                _useRemoteName = true;
            }
            else
            {
                _localWorkDirectory = new DirectoryInfo(Path.GetTempPath());
            }

            _screen = screen;

            _initFtpSuccess = false;
            Task.Run(ValidateFtpConnection); //  Met dans une tâche async pour ne pas bloquer le UI.
            
            return true;
        }

        private void ValidateFtpConnection()
        {
            // FTP
            try
            {
                if (FtpConfiguration == null)
                {
                    throw new System.Exception("Null config instance.");
                }

                // Teste le contenu de la configuration pour la façon la plus sécurisée possible
                _ftpSessionOptions = FtpWebUtil.CreateSessionOptions(FtpConfiguration); 
                
                // test de connexion.
                using (var session = new Session())
                {
                    session.Open(_ftpSessionOptions);
                    if (!FtpWebUtil.TestSession(session))
                    {
                        log.Error("Could not open FTP session.");
                        _screen.SafeCall(ShowFtpUnavailableWarning);
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Invalid FTP session configuration.");
                _screen.SafeCall(ShowFtpUnavailableWarning);
                return;
            }

            _initFtpSuccess = true;
            _initFtpErrorDetected = false;
            _screen.SafeCall(UpdateUiState);
        }

        private void UpdateUiState()
        {
            if (_working)
            {
                _textBox.Text = "";
                _textBox.Enabled = false;
            }
            else
            {
                _textBox.Enabled = _initFtpSuccess && !_initFtpErrorDetected;
            }

            if (_textBox.Enabled)
            {
                _textBox.Focus();
            }

            _screen.InvokeRefresh();
        }

        private void ShowFtpUnavailableWarning()
        {
            _initFtpErrorDetected = true;
            MsgBox.Show(_screen,
                "Image source (FTP) not available. Disabling screen. Please quit application and correct the problem.",
                "Image source unavailable.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Hand);
            _screen.SafeCall(UpdateUiState);
        }
        
        private void TextBoxScanTrigger(string text)
        {
            _screen.ResetTimer();
            LaunchWorkTask(text);
        }
        
        public void Activate(Control parent, IRespondent respondent)
        {
            _working = false;
            _workSequence?.StopSequence();
            if (ResetOnActivate)
            {
                Recycle(_screen);
            }

            if (CheckFtpOnActivate)
            {
                _screen.SafeCall(() =>
                {
                    _initFtpSuccess = false;
                    _initFtpErrorDetected = false;
                    UpdateUiState();
                    Task.Run(ValidateFtpConnection).ConfigureAwait(false);
                });
            }
            else
            {
                _screen.SafeCall(UpdateUiState);
            }
        }

        public bool Save(ISaveLocalRespondentData saveTo, string activeLanguage)
        {
            if (_localFtpFile == null) return true; // No-Op, pas de problème

            if (!_localFtpFile.Exists) return false; //  Fichier référencé mais inexistant. Problème.
            
            return _successFileOperation && SaveToAllModels(saveTo, _localFtpFile.FullName);
        }

        public void Recycle(Control parent)
        {
            parent.SafeCall(() => {
                _textBox.Text = "";
                _localFtpFile = null;
                _successFileOperation = false;
                _workSequence?.StopSequence();
            });
        }

        public ScreenEvaluationResult EvaluateZones(AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            if (_initFtpSuccess && MathUtil.InArea(x, y, ValidateButtonPosition))
            {
                LaunchWorkTask(_textBox.Text.Trim());
            }

            return new ScreenEvaluationResult(true);
        }

        private bool LaunchWorkTask(string text)
        {
            lock (_lock)
            {
                if (_cts != null) return false;
                // Lancer la validation.
                _cts = new CancellationTokenSource(FtpConfiguration.MaxSearchTime);
                Task.Run(() => { CheckFtp(_cts.Token, text); },
                    _cts.Token).ContinueWith(task =>
                {
                    lock (_lock)
                    {
                        _cts = null;
                    }
                });
            }
            return true;
        }

        private void CheckFtp(CancellationToken ctsToken, string text)
        {
            _working = true;
            _screen.SafeCall(UpdateUiState);

            _successFileOperation = false;
            _workSequence?.StartSequence();

            // Créer le nom de fichier
            var sb = new StringBuilder();

            // On vérifie si on "pad" de caractères si pas assez long.
            var txt = text.Trim();
            if (MinimumLength.HasValue)
            {
                if (txt.Length < MinimumLength.Value)
                {
                    txt = txt.PadLeft((int)MinimumLength.Value, PadCharacter);
                }
            }

            sb.Append(txt);
            if (AutomaticFileExtension != null)
            {
                if (!AutomaticFileExtension.StartsWith("."))
                {
                    sb.Append('.');
                }

                sb.Append(AutomaticFileExtension);
            }

            var remPath = sb.ToString();

            try
            {
                using (var session = new Session
                {
                    Timeout = FtpConfiguration.MaxSearchTime
                })
                {
                    session.Open(_ftpSessionOptions);
                    var exists = FtpWebUtil.CheckIfExists(session, remPath);
                    if (!exists)
                    {
                        _screen.SafeCall(CallBackOnNoExists);
                        return;
                    }

                    _localFtpFile = FtpWebUtil.DownloadFile(session, remPath, _localWorkDirectory, _useRemoteName);
                    if (_localFtpFile == null || !_localFtpFile.Exists)
                    {
                        _activityLogger.LogActivity(_screen.TerminalId,ActivityLoggerActionType.FtpFileSelected, "Error" );
                        Task.Run(CallBackOnError, CancellationToken.None);
                    }
                    Task.Run(CallbackOnSuccess, CancellationToken.None);
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during call.");
                Task.Run(CallBackOnError, CancellationToken.None);
                _working = false;
                _screen.SafeCall(UpdateUiState);
            }
        }
        
        private void CallbackOnSuccess()
        {
            if (OriginalFileDataModel != null)
            {
                if (!OriginalFileDataModel.SaveData(_screen, _localFtpFile.FullName))
                {
                    log.Error("Could not save image file ('{1}') to disk. Save model: {0}", 
                        OriginalFileDataModel.ToStringPublicProperties(),
                        _localFtpFile.FullName);
                }
            }

            if (ImageListTransformOptions != null)
            {
                try
                {
                    ImageListTransform.TransformImageList(_screen, new List<string>{_localFtpFile.FullName}, _screen.ScreenConfiguration, ImageListTransformOptions);
                    _successFileOperation = true;
                }
                catch (System.Exception e)
                {
                    log.Error(e, "error during file transformation.");
                }
            }
            else
            {
                _successFileOperation = true;
            }

            _workSequence?.StopSequence();

            // Pour déboguer les problèmes de threading.
            //Thread.Sleep(8000);

            // Vérifie si on a une action automatique à faire une fois que l'opération est un succès.
            if (ActionOnSuccess != null)
            {
                _screen.SafeCall(() =>
                {
                    var resp = _screen.GetCurrentRespondentWithAnswers();
                    var action = ActionOnSuccess.Evaluate(resp);

                    // Va provoquer un recyclage, donc un arrêt du téléchargement si applicable.
                    _screen.SaveAllProperties();
                    _screen.Next(action.Step); // TODO Voir si on peut mettre d'autres options sur ce bouton.
                });
            }

            _working = false;
            _screen.SafeCall(UpdateUiState);
        }

        private void CallBackOnError()
        {
            _workSequence?.StopSequence();
            _working = false;
            _screen.SafeCall(() =>
            {
                MsgBox.Show(_screen, @"Could not fetch file from server because an error occurred.", "Error while fetching file.", MessageBoxButtons.OK);
                UpdateUiState();
            });
            
            
        }

        private void CallBackOnNoExists()
        {
            _workSequence?.StopSequence();
            _working = false;
            _screen.SafeCall(() =>
            {
                MsgBox.Show(_screen, @"File does not exists on server.", "File not found", MessageBoxButtons.OK);
                UpdateUiState();
            });
        }

        public void Dispose()
        {
            _textBoxEventWrapper?.Dispose();
            _textBox?.Dispose();
            _cts?.Cancel();
            _validateImage?.Dispose();
        }

        public bool InitImage(string baseDir, string languageCode)
        {
            if (_workSequence != null)
            {
                _workSequence.ClearCurrentImages();
                _workSequence.CreateImages(languageCode);
            }

            // Clic zone
            if (ValidateButtonPosition == null) return true;
            var filePath = Path.Combine(baseDir, ValidateButtonFileName);
            _validateImage = FileUtil.LoadImage(filePath);
            return _validateImage != null;
        }

        public void Paint(Graphics g, bool debug = false)
        {
            if (!_initFtpSuccess) return;
            
            _workSequence?.Paint(g, debug);

            if (ValidateButtonPosition != null)
            {
                ScreenUtil.DrawImage(g, _validateImage, ValidateButtonPosition);
                if (debug)
                {
                    ScreenUtil.DrawDebugRectangle(g, ScreenUtil.DebugBluePen, ValidateButtonPosition);
                }
            }
        }

        public void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            if (_workSequence == null) return;

            _workSequence.ClearCurrentImages();
            _workSequence.CreateImages(languageCode);
        }
    }
}