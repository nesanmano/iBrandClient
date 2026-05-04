using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.iBrand;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;
using Timer = System.Timers.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cette classe fait plusieures choses:
    /// 1- Elle ouvre un canal de communication avec le serveur principal pour chercher de façon constante les réponses d'un questionnaire précis, sous forme condensée, via un
    ///     exécuteur web.
    /// 2- Elle stocke ces valeurs localement
    /// 3- Quand on active l'écran, on va chercher un sous-ensemble des répondants potentiels et remplit un combobox de choix.
    /// 4- Quand l'usager choisit un répondant, on va chercher les réponses qui lui sont associées et on les copies dans le questionnaire en cours.
    /// Plusieures optiosn existe pour la fréquence des recherches, ainsi que la façon que l'on "map" les questions, leur format, et si on doit réutiliser les données.
    /// On peut aussi limiter les données choisies selon une heure limite.
    /// </summary>
    [PublicAPI]
    public class CondensedAnswerSelectionScreen : AbstractScreen
    {
        #region Fields

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private WebRequestFacade _facadeInstance;
        [CanBeNull] private Timer _facadeTimer;
        private ComboBox _comboRespondents;
        private IbrandDatabase _db;
        private IList<SectionSkipScreenData> _zones;
        /// Si de nouvelles données ont été ajoutée à la BD (ou enlevées)
        private bool _dataChanged = true;
        private List<CondensedRespondentReference> _refs;
        private int _insertingMax = -1;
        private int _insertingCurrent = -1;

        /* Valeur de débogage, possiblement affichées à l'écran. */
        private DateTime _debugLastCall;
        private int _debugLastNumber;
        private bool _debugFacadeError;

        /* Référence vers les bitmaps de débogage */
        private Bitmap _imageDebugLastCall;
        private Bitmap _imageDebugLastNumber;
        private Bitmap _imageDebugFacadeError;
        private Bitmap _imageDebugCurrentSelection;
        private volatile bool _inExecution;

        /// Variable de configuration supplémentaire, lue à partir d'in fichier.
        private CondensedAnswerSelectionScreenConfig _config;

        #endregion

        #region properties

        public Rectangle? ComboBoxRectangle { get; set; }

        public ushort WebRequestTimerSecond { get; set; } = 60;

        /// Nom du fichier de configuration de la façade d'exécution web.
        public string WebExecutorConfigFile { get; set; } = "webexecutor_config.json";
        /// Nom du fichier de configuration du comportement des réponses condensées.
        public string CondensedAnswerSelectionScreenConfigFile { get; set; } = "condensed_screen.json";

        /// Indique qu'il faut sauvegarder le numéro de répondant original comme réponse à une question.
        /// Peut être utilisé par un Post-Submit, ou simplement par référence dans la BD.
        public int? SaveRespondentNumberToQuestionNo { get; set; }
        
        /// <summary>
        /// Assigne ou va chercher un tableau de définition de zones.
        /// Cette valeur ne peut jamais être vide ou nulle, et dans cette situation on lui assignera une valeur 
        /// par défaut (Tout l'écran, va à la prochaine page) la prochaine fois qu'on demandera sa valeur.
        /// </summary>
        [PublicAPI]
        public IList<SectionSkipScreenData> ZoneList
        {
            set => _zones = value;
            get => _zones ?? (_zones = new List<SectionSkipScreenData>
                       {
                           new SectionSkipScreenData
                           {
                               Zone = new Rectangle(new Point(0, 0), new Size(PictureBox.Image.Width, PictureBox.Image.Height)),
                               PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                           }
                       }
                   );
        }

        /// Information pour le fonte du combo box. SI null, on va utiliser des valeurs par défaut.
        public FontDefinitionInfo FontInfo { get; set; }

        #endregion

        #region Initialisation

        public CondensedAnswerSelectionScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc) :
            base(backgroundImgEnSrc, backgroundImgFrSrc, sc) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            var configFile = new FileInfo(GetTerminalDirectory() + CondensedAnswerSelectionScreenConfigFile);
            if (!configFile.Exists)
            {
                throw new System.Exception($"The config file '{configFile}' does not exists. Cannot load screen.");
            }

            _config = JsonUtil.ReadJsonFile<CondensedAnswerSelectionScreenConfig>(configFile);
            if (_config == null)
            {
                throw new System.Exception($"The config file '{configFile}' could not be loaded as class. Cannot load screen.");
            }

            if (ComboBoxRectangle == null)
            {
                throw new System.Exception("No combobox rectangle defined. Check configuration.");
            }

            var font = FontInfo != null ? 
                new Font(FontInfo.FamilyName, FontInfo.Size, FontInfo.ParseStyleValueToEnum(), FontInfo.GraphicsUnit) : 
                new Font("Times", ComboBoxRectangle.Value.Height / 3f);

            _comboRespondents = new ComboBox
            {
                Location = ComboBoxRectangle.Value.Location,
                Size = ComboBoxRectangle.Value.Size,
                Font = font,
                DropDownHeight = ComboBoxRectangle.Value.Height * 10
            };
            Controls.Add(_comboRespondents);
            _comboRespondents.BringToFront();

            _db = ServicesContainer.GetService<IbrandDatabase>();

            var t = InitializeFacade();
            t.Wait();
            var initSuccess = t.Result;

            if (!initSuccess)
            {
                this.SafeCall(() => {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                        "Could not initialize web request facade. Check internet connection." :
                        "Façade de requête web incapable d'initialisation. Regardez la connexion web.");
                });
                return;
            }

            var interval = WebRequestTimerSecond * 1000;
            if (interval < 1000) interval = 1000;

            _facadeTimer = new Timer
            {
                AutoReset = true,
                Interval = interval,
                Enabled = true
            };
            _facadeTimer.Elapsed += async (o, args) =>
            {
                if (_inExecution)
                {
                    log.Trace("Currently using facade. Skipping.");
                    return;
                }

                _inExecution = true;

                await Task.Run(async () => await TriggerFacadeRequest());

                _inExecution = false;
            };

            InitBitmaps();
        }

        private void InitBitmaps()
        {
            var tGen = _config?.TextGenerator;
            if (tGen != null)
            {
                this.SafeCall(() =>
                {
                    _imageDebugCurrentSelection?.Dispose();
                    _imageDebugLastNumber?.Dispose();
                    if (_config?.DebugLastNumberInfo != null)
                    {
                        tGen.BitmapSize = _config.DebugLastNumberInfo.Value.Size;
                        _imageDebugLastNumber = ImageUtil.CreateProceduralTextBitmap(tGen,
                            "Number: " + _debugLastNumber);
                    }
                    if (_config?.DebugCurrentSelectionInfo != null)
                    {
                        tGen.BitmapSize = _config.DebugCurrentSelectionInfo.Value.Size;
                        _imageDebugCurrentSelection = ImageUtil.CreateProceduralTextBitmap(tGen,
                            _refs != null ? "Nbr: " + _refs.Count : "N/A");
                    }

                    _imageDebugLastCall?.Dispose();
                    _imageDebugFacadeError?.Dispose();

                    if (_config?.DebugLastCallInfo != null)
                    {
                        tGen.BitmapSize = _config.DebugLastCallInfo.Value.Size;
                        _imageDebugLastCall = ImageUtil.CreateProceduralTextBitmap(tGen,
                            _debugLastCall.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    if (_config?.DebugFacadeInfo != null)
                    {
                        tGen.BitmapSize = _config.DebugFacadeInfo.Value.Size;
                        _imageDebugFacadeError = ImageUtil.CreateProceduralTextBitmap(tGen,
                            _activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                                ? "Call Error"
                                : "Erreur d'appel");
                    }

                    Refresh();

                }, err => { log.Error(err, "Could not create image files for screen."); });
            }
        }

        #endregion

        #region Class Code

        private async Task TriggerFacadeRequest()
        {
            if (_facadeInstance == null || _config == null) return;

            var qId = _config.SourceQuestionnaireId;

            _debugLastCall = DateTime.Now;
            _debugLastNumber = _db.GetLastRespondentIdFromCondensedAnswers(qId);

            InitBitmaps();

            if (log.IsTraceEnabled)
            {
                log.Trace($"Facade call triggered ({_screenConfig.QuestionnaireId},{_debugLastNumber}): {_debugLastCall.ToLocalTime()}");
            }

            var webParams = new Dictionary<string, object>
            {
                { "questionnaireId", qId.ToString() },
                { "lastRespondentNumber", _debugLastNumber.ToString() }
            };
            var res = await _facadeInstance.LaunchMethod<List<IBrandCondensedQuestionnaireAnswer>>("GetCondensedQuestionnaire", webParams);
            if (res == null)
            {
                // Peut arriver.
                _debugFacadeError = true;
                this.SafeCall(Refresh);
                return;
            }

            if (log.IsTraceEnabled)
            {
                if (res.Count > 0)
                {
                    log.Trace("Facade call completed. No new data.");
                }
                else
                {
                    log.Trace("Facade call completed: \n" + res.Count);
                }
            }

            // L'appel a été un succès et on met à jour la BD. S'il y a des nouveaux répondants.
            _debugFacadeError = false;
            if (res.Count <= 0) return;
            _insertingMax = res.Count;
            var parts = res.PartitionEnumerable(20);
            _insertingCurrent = 0;

            foreach (var part in parts)
            {
                var check = _db.InsertCondensedAnswersData(qId, part);
                _insertingCurrent += check;
                this.SafeCall(() =>
                {
                    RefreshComboBox();
                });
            }

            var tmp = _db.GetLastRespondentIdFromCondensedAnswers(qId);
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (tmp != _debugLastNumber)
            {
                _debugLastNumber = tmp;
            }

            if (_insertingCurrent != res.Count)
            {
                this.SafeCall(() =>
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                        ? "Could not initialize web request facade. Check internet connection."
                        : "Façade de requête web incapable d'initialisation. Regardez la connexion web.");
                });
            }

            _dataChanged = true;
            RefreshComboBox();

            _insertingCurrent = -1;
            _insertingMax = -1;
        }

        /// <summary>
        /// Cette méthode vérifie si une configuration est disponible pour la façade, et si oui tente d'instancier et valider l'instance de facade.
        /// Si oui, l'objet de facade sera non-nul comme effet de bord.
        /// </summary>
        /// <returns>TRUE si une configuration valide et correctement utilisable existe.</returns>
        private async Task<bool> InitializeFacade()
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId, _screenConfig.TerminalId);
            var facadeConfigFilePath = new FileInfo(baseDir + WebExecutorConfigFile);

            if (!facadeConfigFilePath.Exists) return false;

            try
            {
                _facadeInstance = JsonUtil.ReadJsonFile<WebRequestFacade>(facadeConfigFilePath);
                return await _facadeInstance.Init();
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not read from facade config file.");
                _facadeInstance = null;
                return false;
            }
        }

        ~CondensedAnswerSelectionScreen()
        {
            Dispose(true);
        }

        private bool CheckSelection()
        {
            var selInd = _comboRespondents.SelectedIndex;
            if (selInd < 1) return false;

            /*
            // Vérifie si on est encore en train de charger des données ou si aucune données utile n'existe.
            if (_comboRespondents.SelectedText is string shown)
            if (shown.StartsWith("--"))
            {
                return false;
            }
            */

            var num = _refs?.FirstOrDefault(r => r.Index == selInd-1);
            if (num == null) return false;

            var answers = _db.GetAnswersFromCondensedTable(num.Numero);

            // On map les réponses au questionnaire existant.
            foreach (var a in answers)
            {
                SaveAnswerToQuestion(a);
            }

            if (SaveRespondentNumberToQuestionNo != null)
            {
                SaveAnswerToQuestion(new Answer {QuestionNo = SaveRespondentNumberToQuestionNo.Value, Response = num.Numero.ToString()});
            }

            return true;
        }

        private void RefreshComboBox(bool fromSetActivated = false)
        {
            if (_config == null) return; // Pas de config minimale, pas de traitement.

            this.SafeCall(() =>
            {
                // On réinitialise le comboBox
                if (!_dataChanged && _config?.SourceQuestionnaireTimeSpanCutOff == null) return;
                log.Trace("Refresh combo box initiated.");

                DateTime? minDate = null;
                if (_config?.SourceQuestionnaireTimeSpanCutOff != null)
                {
                    minDate = DateTime.Now - _config?.SourceQuestionnaireTimeSpanCutOff.Value;
                }

                _refs = _db.GetCondensedRespondentReferences(_config.SourceQuestionnaireId, _config.SourceQuestionnaireQuestionIdentifiers, minDate);
                if (_refs == null || !_refs.Any())
                {
                    log.Trace($"No data found for qId={_config.SourceQuestionnaireId}");

                    _comboRespondents.BeginUpdate();
                    _comboRespondents.Items.Clear();

                    if (_insertingCurrent != -1 && _insertingMax != -1)
                    {
                        _comboRespondents.Items.Add($"-- Loading {_insertingCurrent}/{_insertingMax}");
                    }
                    else
                    {
                        _comboRespondents.Items.Add("-- No data");
                    }

                    _comboRespondents.EndUpdate();
                    _comboRespondents.SelectedIndex = 0;

                    return;
                }

                _comboRespondents.BeginUpdate();
                _comboRespondents.Items.Clear();
                _comboRespondents.Items.Add("-- Please select");
                foreach (var reference in _refs)
                {
                    _comboRespondents.Items.Add(reference.Identifier);
                }

                _comboRespondents.EndUpdate();
                _comboRespondents.SelectedIndex = 0;
                _dataChanged = false;
                log.Trace("Refresh combo box finished.");

                if (fromSetActivated)
                {
                    _debugLastNumber = _db.GetLastRespondentIdFromCondensedAnswers(_config.SourceQuestionnaireId);
                    _debugLastCall = DateTime.Now;
                    InitBitmaps();
                }

                Refresh();
            });
        }

        #endregion

        #region Overrides and implementations

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_imageDebugCurrentSelection != null && _config?.DebugCurrentSelectionInfo != null)
            {
                DrawImage(g, _imageDebugCurrentSelection, _config.DebugCurrentSelectionInfo.Value);
            }

            if (_debugFacadeError && _imageDebugFacadeError != null && _config?.DebugFacadeInfo != null)
            {
                DrawImage(g, _imageDebugFacadeError, _config.DebugFacadeInfo.Value);
            }

            if (_imageDebugLastCall != null && _config?.DebugFacadeInfo != null)
            {
                DrawImage(g, _imageDebugLastCall, _config.DebugFacadeInfo.Value);
            }

            if (_imageDebugLastNumber != null && _config?.DebugLastNumberInfo != null)
            {
                DrawImage(g, _imageDebugLastNumber, _config.DebugLastNumberInfo.Value);
            }
        }

        public override bool ChangeLanguage(string language)
        {
            var state = base.ChangeLanguage(language);
            if (!state) return false;
            InitBitmaps();
            return true;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            RefreshComboBox(true);
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            return this.EvaluateZonesAndValidation(x, y, ZoneList, null, CheckSelection);
        }

        protected override void Dispose(bool disposing)
        {
            _facadeInstance?.Dispose();
            if (_facadeTimer != null) _facadeTimer.Enabled = false;
        }

        public override void Recycle()
        {
            if (_comboRespondents != null) _comboRespondents.SelectedIndex = -1;
        }

        #endregion
    }
}