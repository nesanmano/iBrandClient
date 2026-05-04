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
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.RfidReader;
using NomadLogic.Common.Util.String;
using NomadLogic.Common.Util.Win32;
using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    [PublicAPI]
    public class ScreenDefinitionRfidMultipleScan : ScreenFieldAbstract, IJsonConfig, IExternalFileConfig, IPaint, IRecyclable, IActivateScreen, IOnLoad, 
        IChangeLanguage, IImageInitialisable, ISaveToQuestionnaire, IEvaluateDefinitionsState
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Textes destinés à l'usager par défaut pour une boîte de texte.
        /// </summary>
        public static readonly IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> DefaultUserMessages;

        private TextBox _scanTextBox;
        private HashSet<string> _fullSet;
        private volatile bool _block;
        private IRfidReader _rfidReader;
        private AbstractScreen _parentScreen;
        private Task _task;
        private CancellationTokenSource _cts;
        private readonly object _lock = new object();
        private bool _enterDetected;
        private bool _showError;
        private Bitmap _errorBitmap;
        private Button _checkButton;
        private string _activeLg;
        private AnimatedImageSequence _waitSequence;
        private TextBox _scanCountBox;
        private readonly HashSet<string> _currentScans = new HashSet<string>();
        private TextBoxKeyboardEventsWrapper _scanTextBoxWrapper;
        private ActivityLoggerService _activityLogger;

        static ScreenDefinitionRfidMultipleScan()
        {
            // Initialisation statique.
            // TODO À remplacer par un véritable système i18n

            var dico = new Dictionary<UserErrorEnum, IReadOnlyDictionary<string, string>>
            {
                {UserErrorEnum.Empty, new Dictionary<string, string>
                {
                    {Constants.EN, "The field '{0}' cannot be empty."}, {Constants.FR, "le champ '{0}' ne peut être vide."}
                }}
                ,{UserErrorEnum.Mandatory, new Dictionary<string, string>
                {
                    {Constants.EN, "You must scan at least one code field: {0}"}, {Constants.FR, "Vous devez lire au moins un code pour ce champ: {0}"}
                }}
                ,{UserErrorEnum.Validation, new Dictionary<string, string>
                {
                    {Constants.EN, "You cannot proceed. Invalid number ({0}) of scan executed."}, {Constants.FR, "Vous n'avez pas un nombre adéquat ({0}) de valeurs lues."}
                }}
                ,{UserErrorEnum.System, new Dictionary<string, string>
                {
                    {Constants.EN, "System configuration error. Please contact an administrator."}, {Constants.FR, "Erreur de configuration système. Contactez un administrateur."}
                }}
            };

            DefaultUserMessages = dico;
        }

        #region Properties

        public string DefaultConfigFileName => "screen_rfid_scan_multiple.json";
        public string ConfigFileName { get; set; }

        public string RfidReaderConfigFile { get; set; } = "rfid_reader_config.json";

        public int ScanTriggerOnCharacterNumber => _rfidReader?.ScanTriggerOnCharacterNumber ?? 0;
        
        public bool ScanTriggerOnReturn { get; set; }
        
        /// <summary>
        /// En cas d'erreur, on affiche cette image à l'écran au lieu d'une boîte de message.
        /// </summary>
        public IImageGenerator ErrorImage { get; set; }
        
        public Rectangle? ErrorImagePosition { get; set; }

        public FontDefinitionInfo FontDefinition { get; set; }

        public Rectangle? ScanCountTextBoxZone { get; set; }

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
        /// Indique le nom du fichier qui contient la liste des code RFID qui doivent être utilisée.
        /// </summary>
        public string CrossCheckListFileName { get; set; }
        
        public string CheckImageFileName { get; set; }
        
        public Point? CheckImagePosition { get; set; }

        /// <summary>
        /// Indique le nombre minimal de scan correctement effectués pour considérer pouvoir passer au prochain écran.
        /// </summary>
        public uint MinimumScanCount { get; set; } = 1;

        /// <summary>
        /// Indique la limite maximale de code à conserver en mémoire.
        /// Une fois la limite atteinte, on stoppe la saisie.
        /// </summary>
        public uint? MaximumScanCount { get; set; }

        public bool ClearOnError { get; set; }
        
        /// <summary>
        /// Quand on atteint le maximum de valeurs, on exécute ces options de changement, comme si on avait cliqué sur une zone de navigation.
        /// </summary>
        public ScreenChangeOptions MaximumScanCountTriggerNavigationAction { get; set; }
        

        #endregion

        #region Overloads
        
        public void Paint(Graphics g, bool debug = false)
        {
            if (_showError && ErrorImagePosition.HasValue)
            {
                ScreenUtil.DrawImage(g, _errorBitmap, ErrorImagePosition);
            }
            
            _waitSequence?.Paint(g);
        }

        public void Recycle(Control parent)
        {
            ResetScanArea();
            _currentScans.Clear();
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            ResetScanArea();
            UpdateCountBox();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _cts?.Cancel();
                _cts = null;
                _task = null;
            }
            _errorBitmap?.Dispose();
            _errorBitmap = null;

            if (_rfidReader is IRfidEvents iRfidEvents)
            {
                iRfidEvents.CardRead -= ScanManagerOnCardRead;
                iRfidEvents.HardWareDisconnected -= ScanManagerOnHardWareDisconnected;
                iRfidEvents.InvalidCardRead -= ScanManagerOnInvalidCardRead;
                iRfidEvents.Dispose();
            }

            _scanTextBoxWrapper?.Dispose();
        }

        public bool InitImage(string baseDir, string languageCode)
        {
            if (_waitSequence != null)
            {
                _waitSequence.ClearCurrentImages();
                _waitSequence.CreateImages(_activeLg);
            }

            _errorBitmap = ErrorImage?.GenerateImage(_activeLg);

            return true;
        }
        
        public void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            _activeLg = languageCode;
        }
        
        public ScreenEvaluationResult EvaluateDefinitionsState(string activeLanguage)
        {
            if (_currentScans.Count >= MinimumScanCount)
            {
                // Valide
                return new ScreenEvaluationResult(true);
            }

            //Invalide
            var msg = $"{_currentScans.Count} / {MinimumScanCount}";
            return GetErrorResult(_activeLg, UserErrorEnum.Validation, DefaultUserMessages, msg);
        }

        public bool Save(ISaveLocalRespondentData saveTo, string activeLanguage)
        {
            if (!_currentScans.Any()) return true; // Rien a faire. donc réussite.

            var res = SaveToAllModels(saveTo, _currentScans);

            if (res)
            {
                _currentScans.Clear();
            }
            
            if (_parentScreen is GenericUpdateServerScreen initScreen)
            {
                var resp = _parentScreen.GetCurrentRespondentWithAnswers();

                var code = DataModels.First().GetActivationTextValue(resp);
                
                // Charge données du serveur. Il faut garder le répondant en mémoire pour le modifier.
                res &= initScreen.LoadFromServer(CancellationToken.None, code, resp, null, DebugFile).GetAwaiter().GetResult();

                if (!res)
                {
                    log.Error("Could not load respondent data from server.");
                    return false;
                }

                // Pour l'envoi, il faut épurer le répondant pour enlever les question/valeurs dynamique en extra..
                var q = initScreen.GetCurrentQuestionnaire();
                var sendResp = q.ActualizeRespondentData(resp);

                // Envoi des données au serveur.
                res &= initScreen.SendToServer(CancellationToken.None, sendResp, null, DebugFile).GetAwaiter().GetResult();
                _activityLogger.LogActivity(_parentScreen.TerminalId, ActivityLoggerActionType.DirectSave, res ? "Success" : "Error");
            }

            return res;
        }

        public FileInfo DebugFile { get; set; }

        public bool OnLoad(AbstractScreen screen)
        {
            _activityLogger = ServicesContainer.GetService<ActivityLoggerService>();
            _activeLg = screen.ActiveLanguage;
            var baseDir = screen.GetTerminalDirectory();

            // Charger le lecteur RFID. S'il n'y a pas de fichier de configuration, on assume que c'est un lecteur "léger" (qui ne requiert pas de configuration complexe).
            var configFilePath = new FileInfo(baseDir + RfidReaderConfigFile);
            _rfidReader = configFilePath.Exists ? JsonUtil.ReadJsonFile<IRfidReader>(configFilePath) : new Simple_RfidReader();

            if (_rfidReader == null)
            {
                throw new System.Exception("Error during marshalling RFID reader instance. Check JSON config file format.");
            }

            _parentScreen = screen;

            if (_rfidReader.ConnectReader())
            {
                ScanTriggerOnReturn = _rfidReader.ScanTriggerOnEnter;

                // Vérification de spécialisation d'instance.
                if (_rfidReader is IRfidEvents rfidEvent)
                {
                    rfidEvent.CardRead += ScanManagerOnCardRead;
                    rfidEvent.HardWareDisconnected += ScanManagerOnHardWareDisconnected;
                    rfidEvent.InvalidCardRead += ScanManagerOnInvalidCardRead;
                }
            }
            else
            {
                return false;
            }

            var font = FontDefinition != null ? 
                new Font(FontDefinition.FamilyName, FontDefinition.Size, FontDefinition.ParseStyleValueToEnum(), FontDefinition.GraphicsUnit) : 
                // ReSharper disable once PossibleLossOfFraction
                new Font("Times", Position.Height / 3);

            screen.InitTextBox(out _scanTextBox, Position, font, 1);
            _scanTextBoxWrapper = new TextBoxKeyboardEventsWrapper(_scanTextBox, CheckCodeEvent, ScanTriggerOnReturn, ScanTriggerOnCharacterNumber);
            
            // Remplissage du fichier de vérification de RFID.
            if (!string.IsNullOrWhiteSpace(CrossCheckListFileName))
            {
                var filePath = Path.Combine(baseDir, CrossCheckListFileName);

                if (File.Exists(filePath))
                {
                    _fullSet = new HashSet<string>();
                    if (filePath.EndsWith("csv", StringComparison.InvariantCultureIgnoreCase) || 
                        filePath.EndsWith("txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Séparation via ',' ou fin de ligne
                        foreach (var ln in File.ReadAllLines(filePath))
                        {
                            foreach (var chunk in ln.Split(','))
                            {
                                _fullSet.Add(chunk.Trim());
                            }
                        }
                    }
                    else if (filePath.EndsWith("json", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Liste de chaîne
                        foreach (var val in JsonUtil.ReadJsonFile<List<string>>(new FileInfo(filePath)))
                        {
                            _fullSet.Add(val);
                        }
                    }
                    else
                    {
                        log.Warn("Unknown file type for stocking tags.");
                    }

                    if (!_fullSet.Any())
                    {
                        _fullSet = null;
                    }
                }
            }

            if (CheckImagePosition.HasValue)
            {
                var filePath = Path.Combine(baseDir, CheckImageFileName);
                if (File.Exists(filePath))
                {
                    var bmp = FileUtil.LoadBitmap(filePath);
                    _checkButton = new Button
                    {
                        Location = CheckImagePosition.Value,
                        Size = bmp.Size,
                        Image = bmp
                    };
                    _checkButton.MouseClick += (sender, args) => CheckCodeEvent(_scanTextBox.Text.Trim());
                    screen.Controls.Add(_checkButton);
                    _checkButton.BringToFront();
                }
            }

            if (ScanCountTextBoxZone.HasValue)
            {
                if (!screen.InitTextBox(out _scanCountBox, ScanCountTextBoxZone.Value, font))
                {
                    return false;
                }

                _scanCountBox.TextAlign = HorizontalAlignment.Center;
                _scanCountBox.Enabled = false;
            }

            UpdateCountBox();

            return true;
        }

        #endregion

        private void UpdateCountBox()
        {
            _scanCountBox?.SafeCall(() =>
            {
                _scanCountBox.Text = _currentScans.Count.ToString();
            });
        }
        
        protected RfidTransformationResult TransformCode([NotNull] string sequenceOriginal)
        {
            return _rfidReader.ExecuteSequenceAnalisys(sequenceOriginal);
        }
        
        private void ScanManagerOnInvalidCardRead(object sender, RfidReadEventArgs args)
        {
            if (!_parentScreen.IsActivated()) return;
            log.Debug("Invalid card read.");
            _scanTextBox.SafeCall(() =>
            {
                if (_errorBitmap == null)
                {
                    MsgBox.Show(_parentScreen, Resources.iBrand.BarcodeScreen_InvalidBarcode, Resources.iBrand.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
            });
        }

        private void ScanManagerOnHardWareDisconnected(object sender, EventArgs eventArgs)
        {
            log.Warn("Hardware disconnected event.");

            _scanTextBox.SafeCall(() =>
            {
                if (_errorBitmap == null)
                {
                    MsgBox.Show(_parentScreen, Resources.iBrand.BarcodeScreen_PeripheralDisconnect, Resources.iBrand.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
            });
        }

        private void ScanManagerOnCardRead(object sender, RfidReadEventArgs args)
        {
            if (!_parentScreen.IsActivated()) return;
            log.Debug("Card read event : " + args?.CardUid);
            UpdateWithScanValue(args?.CardUid);
        }
        
        /// <summary>
        /// Cette méthode enveloppe l'appel vers la vérification du code dans une tâche asynchrone, unique. On ne peut avoir qu'une seule demande
        /// de vérification, ce qui empêche les appels répétitifs.
        /// Cette tâche est annulée si on recycle ou dispose l'écran, ce qui a pour effet d'empêcher les dialogues de s'ouvrir le cas échéant.
        /// </summary>
        protected async void CheckCodeEvent(string code)
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
                    _scanTextBox.SafeCall(async () => await ExecuteCodeCheck(_cts.Token, code));
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

        /// <summary>
        /// Vérifie si la valeur RFID fait partie d'un lot valide.
        /// </summary>
        private bool ValidateScanFromSet(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return _fullSet == null || _fullSet.Contains(value);
        }

        private async Task ExecuteCodeCheck(CancellationToken ct, string text)
        {
            _parentScreen.ResetTimer();
            if (_block) return;
            _block = true;

            try
            {
                // 1- Transformation de la lecture selon la configuration du lecteur.

                var codeResults = TransformCode(text);
                log.Trace("Sequence analysed: {0}", codeResults);
                
                var valid = codeResults.IsRegexValid;

                // 2- Validation du code selon une liste locale et pré-établie.
                if (!string.IsNullOrWhiteSpace(codeResults.StringCode))
                {
                    valid &= ValidateScanFromSet(codeResults.StringCode);
                    log.Trace("Code validation: {0}", valid);
                }
                else
                {
                    log.Error("Code was empty or null after transformation. Check Rfid Reader configurations.");
                    valid = false;
                }
                
                // 3- Ajout de préfixes configurables
                if (ct.IsCancellationRequested) return;

                _scanTextBox.ForeColor = valid ? Color.Black : Color.Red;
                
                if (valid)
                {
                    _showError = false;
                    if (MaximumScanCount == null || _currentScans.Count < MaximumScanCount.Value)
                    {
                        _currentScans.Add(codeResults.FinalCode);
                    }

                    ResetScanArea();
                    UpdateCountBox();
                    _parentScreen.Invalidate();
                    
                    if (MaximumScanCountTriggerNavigationAction != null &&MaximumScanCount.HasValue && _currentScans.Count == MaximumScanCount.Value)
                    {
                        var action = MaximumScanCountTriggerNavigationAction.Evaluate(_parentScreen.GetCurrentRespondentWithAnswers());
                        if (action != null)
                        {
                            if (_parentScreen is GenericScreen genParent)
                            {
                                var evalResult = new ScreenEvaluationResult
                                {
                                    // Agit comme une fonction "Next"
                                    ActionsToExecute = new List<Action> {
                                        () =>
                                        {
                                            if (action.Cancel)
                                            {
                                                _parentScreen.FirstScreen();
                                                return;
                                            }

                                            _parentScreen.Next(action.Step);
                                        }
                                    }
                                };

                                genParent.ProvokeScreenEvaluation(evalResult);
                            }
                        }
                        else
                        {
                            log.Warn("No valid actions were selected upon activating 'maximum reached' navigation trigger. Disregarding.");
                        }
                    }
                }
                else
                {
                    if (ct.IsCancellationRequested) return;

                    if (_errorBitmap == null)
                    {
                        _scanTextBox.SafeCall(() =>
                        {
                            var message = _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase)
                                ? "Le code lu est inconnu/invalide. Veuillez réessayer ou vérifier la connexion externe."
                                : "Read code is unknown or invalid. Please try again or check external connection.";
                            var result = MessageBox.Show(_parentScreen, message, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
                            log.Debug($"Bad scan: {result}");
                            ResetScanArea();
                        });
                    }
                    else
                    {
                        if (ClearOnError)
                        {
                            ResetScanArea();
                        }
                        _showError = true;
                        
                    }
                }

                _parentScreen.Refresh();
            }
            finally
            {
                _block = false;
            }
        }
        
        protected void ResetScanArea()
        {
            _scanTextBoxWrapper.ResetTextBox();
        }

        protected void UpdateWithScanValue(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            
            _scanTextBox.SafeCall(() =>
            {
                _scanTextBox.Text = code;
                CheckCodeEvent(code);
            });
        }

        private void InvokeRefresh()
        {
            _parentScreen?.InvokeRefresh();
        }

    }
}