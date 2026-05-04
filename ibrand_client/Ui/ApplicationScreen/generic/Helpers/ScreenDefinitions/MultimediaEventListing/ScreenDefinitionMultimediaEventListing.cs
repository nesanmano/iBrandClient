using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    public class ScreenDefinitionMultimediaEventListing : IJsonConfig, IExternalFileConfig, IOnLoad, IActivateScreen, IRecyclable, ISaveToQuestionnaire, IInitialisable, IPaint, IImageInitialisable,
        IChangeLanguage, IProgress<RespondentBundleDownloadReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private TableLayoutPanel _mainLayout;
        private List<MultimediaEventGalleryRow> _rowList;
        private DirectoryInfo _assetsDir;
        private Image _textItemSelectedImage;
        private int _nbrItemsSelected;
        private AbstractScreen _parent;
        private MultimediaEventListRawConverter _converterInstance;
        private RespondentBundleFinalData _lastData;
        private RespondentEventDataDownloadService _downloadService;
        private MultimediaEventListingStateEnum _presentState;
        private Image _stateMessageImage;
        private PictureBox _backButton;
        private PictureBox _nextButton;

        private delegate void SafeCallDelegatePopulate(List<MultimediaEventGalleryRow> rowList);
        private delegate void SafeCallDelegateReset();

        #region Properties

        [JsonIgnore] public string DefaultConfigFileName => "capture_event_gallery_config.json";

        public string ConfigFileName { get; set; }

        public string CustomWorkFileDirectory { get; set; } = DirectoryUtil.GetLocalApplicationWorkDir();

        public string CustomErrorPlaceHolderFileName { get; set; }

        public MultimediaEventItemPresentation PresentationData { get; set; }

        /// <summary>
        /// La classe qui convertit les données brutes (et possiblement incomplètes) du service de téléchargement du répondant choisi.
        /// SI absente au moment de la configuration, on utilise la classe de base de conversion.
        /// Si on veut utiliser des conversions ésotériques ou des vakleurs fixes de tests, c'est ici qu'il faut spécifier une instance.
        /// </summary>
        public MultimediaEventListRawConverter EventDataConverter { get; set; }

        /// <summary>
        /// Si on affiche le nombre d'items sélectionnés, cette instance sera utilisée.
        /// Doit être jumelée à une location.
        /// </summary>
        public ImageGeneratorFromText NumberSelectedTextImageGenerator { get; set; }

        /// <summary>
        /// L'endroit ou rendre le texte du nombre d'items sélectionnés.
        /// Doit être jumelé à un générateur d'image texte.
        /// </summary>
        public Rectangle? NumberSelectedTextLocation { get; set; }

        /// <summary>
        /// Endroit optionel pour afficher des messages basé sur l'état de la gallerie.
        /// </summary>
        public Rectangle? StateMessageLocation { get; set; }

        /// <summary>
        /// Définition d'image à utiliser avec le texte d'état. Optionel. Si manquant, une définition par défaut sera utilisée.
        /// </summary>
        public ProceduralImageTextInfo StateMessageImageDefinition { get; set; }

        /// <summary>
        /// Textes à utiliser pour l'affichage d'état de l'écran. Si vide (ou inapproprié), on utilisera des textes par défaut (en anglais).
        /// </summary>
        public IDictionary<MultimediaEventListingStateEnum, IDictionary<string, string>> StateMessageCustomTexts { get; set; }

        /// <summary>
        /// Indique la location du bouton de retour. Si absent, on ne peut pas retourner en arrière, sauf avec les options de navigations contruites dans l'écran.
        /// </summary>
        public Rectangle? BackButtonZone { get; set; }

        /// <summary>
        /// Le nom du fichier du bouton de retour, relatif au répertoire des fichiers de terminal.
        /// </summary>
        public string BackButtonImageFileName { get; set; }

        /// <summary>
        /// Indique que l'on veut afficher le bouton de retour en tout temps, même s'il y a des images dans la gallerie.
        /// </summary>
        public bool BackButtonAlwaysShow { get; set; }

        /// <summary>
        /// Donne une façon supplémentaire de voyager au prochain écran. Ce bouton ne s'affiche que quand on sélectionne au moins un élément de la gallerie.
        /// </summary>
        public Rectangle? NextButtonZone { get; set; }

        /// <summary>
        /// Le nom du fichier du bouton d'avancement, relatif au répertoire des fichiers de terminal.
        /// </summary>
        public string NextButtonImageFileName { get; set; }

        /// <summary>
        /// Indique que l'on veut afficher le bouton d'avancement en tout temps, même si aucune sélection n'a été faite..
        /// </summary>
        public bool NextButtonAlwaysShow { get; set; }

        /// <summary>
        /// Nombre de pages à sauter quand on pèse sur le bouton.
        /// </summary>
        public int NextButtonPageSkip { get; set; } = 1;

        /// <summary>
        /// Cette information sera utilisée pour grouper les réponses dynamiques ensemble. Si existante, cette valeur remplacera le datakey des valeurs sélectionnées par
        /// celle-ci.
        /// </summary>
        public string SaveDynamicHeadersCommonDataKey { get; set; }

        /// <summary>
        /// Dans le cas où une valeur de groupe dynamique représente une image, on peut choisir de remplacer la valeur originale (qui ressemble à "file:jasgsdfjas") par
        /// le chemin du fichier local qui a été téléchargé. Utilisé à des fins d'utilisation local, comme l'impression.
        /// </summary>
        public bool SaveDynamicHeadersReplaceOriginalValueWithLocalFile { get; set; }

        /// <summary>
        /// Indique que l'on doit "activer" un changement d'écran quand un nombre minimum d'objets sélectionné à été atteint.
        /// </summary>
        public int? TriggerChangeOnMinimumSelectedItems { get; set; }

        /// <summary>
        /// Cette propriété indique qu'il faut ré-initialiser l'état de sélection de tous les éléments à chaque fois que l'on va sur l'écran (Activate).
        /// </summary>
        public bool ResetSelectionOnActivation { get; set; }

        #endregion Properties

        public bool OnLoad(AbstractScreen screen)
        {
            _parent = screen;
            _assetsDir = new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDir(screen.ProjectId, screen.QuestionnaireId, screen.TerminalId));
            log.Trace("Load call. Assets dir: {0}", _assetsDir);

            if (BackButtonZone != null)
            {
                var bbl = BackButtonZone.Value;
                var backImagePath = Path.Combine(_assetsDir.FullName, BackButtonImageFileName);
                if (File.Exists(backImagePath))
                {
                    _backButton = new PictureBox
                    {
                        Size = bbl.Size,
                        Location = bbl.Location,
                        Image = FileUtil.LoadImage(backImagePath)
                    };
                    _backButton.BringToFront();
                    _backButton.MouseClick += (sender, args) =>
                    {
                        if (!_backButton.Visible) return;
                        _backButton.Visible = false;
                        // Va provoquer un recyclage, donc un arrêt du téléchargement si applicable.
                        _parent.FirstScreen(); // TODO Voir si on peut mettre d'autres options sur ce bouton.
                    };

                    _backButton.Visible = BackButtonAlwaysShow;
                    _parent.Controls.Add(_backButton);
                }
                else
                {
                    log.Error("Could not find back button image file: " + backImagePath);
                }
            }

            if (NextButtonZone != null)
            {
                var nbl = NextButtonZone.Value;
                var nextImagePath = Path.Combine(_assetsDir.FullName, NextButtonImageFileName);
                if (File.Exists(nextImagePath))
                {
                    _nextButton = new PictureBox
                    {
                        Size = nbl.Size,
                        Location = nbl.Location,
                        Image = FileUtil.LoadImage(nextImagePath)
                    };
                    _nextButton.BringToFront();
                    _nextButton.MouseClick += (sender, args) =>
                    {
                        if (!_nextButton.Visible) return;
                        _nextButton.Visible = false;
                        // Va provoquer un recyclage, donc un arrêt du téléchargement si applicable.
                        _parent.Next(NextButtonPageSkip); // TODO Voir si on peut mettre d'autres options sur ce bouton.
                    };

                    _nextButton.Visible = NextButtonAlwaysShow;
                    _parent.Controls.Add(_nextButton);
                }
            }

            _converterInstance = EventDataConverter ?? new MultimediaEventListRawConverterBase();
            _converterInstance.SetScreenInstance(screen);
            _converterInstance.SelectionChangedEventRef = ItemOnSelectionChangedEvent;
            _converterInstance.SetPresentationData(PresentationData);
            _converterInstance.SetAssetsDirectory(_assetsDir);
            _converterInstance.SetWorkDirectory(CustomWorkFileDirectory);
            _converterInstance.SetPlaceHolderImageFilePath(CustomErrorPlaceHolderFileName);

            _downloadService = ServicesContainer.GetService<RespondentEventDataDownloadService>();
            if (_downloadService == null)
            {
                log.Error("No download service exists in this instance. A previous screen must initialize and launch respondent download to work.");
                screen.SafeCall(() => { MessageBox.Show(@"Required download service does no exists. You must close the application.", @"iBrand", MessageBoxButtons.OK); });
                return false;
            }

            _downloadService.AddObserver(this);

            return true;
        }

        private delegate void ConvertDataAndLaunchDelegate(IReadOnlyDictionary<RequestRespondentDataDynamicValue, RespondentBundleDataGroup> rawData);

        private void ConvertDataAndLaunch(IReadOnlyDictionary<RequestRespondentDataDynamicValue, RespondentBundleDataGroup> rawData)
        {
            log.Trace("Respondent bundle data received. Conversion started.");
            ChangeState(MultimediaEventListingStateEnum.ProcessingData);
            _converterInstance.SetRawData(rawData);
            _converterInstance.StartConversion(PopulateMainLayout);
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            if (_mainLayout == null)
            {
                _mainLayout = new TableLayoutPanel
                {
                    Location = PresentationData.LayoutRectangle.Location,
                    Size = PresentationData.LayoutRectangle.Size,
                    MaximumSize = PresentationData.LayoutRectangle.Size,
                    ColumnCount = 1,
                    GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                    AutoSize = false,
                    Visible = true,
                    Enabled = true,
                    BackColor = PresentationData.MainLayoutBackgroundColor ?? Color.White
                };
                parent.Controls.Add(_mainLayout);
                _mainLayout.BringToFront();
            }

            if (_backButton != null) _backButton.Visible = BackButtonAlwaysShow;

            if (_nextButton != null) _nextButton.Visible = NextButtonAlwaysShow;

            // On ne cherche et convertit les données qu'une seulle fois.
            _lastData = _downloadService.GetLastRespondentData();
            if (!_converterInstance.IsConversionFinished)
            {
                if (_lastData == null)
                {
                    // Erreur!
                    // TODO Faire un message à l'usager pour revenir au début.
                    throw new System.Exception("Data is null. This should never happen. Check screen that launches the respondent download sequence.");
                }

                if (_lastData.DataAvailiable)
                {
                    DataOnAllDataAvailiable(this, null);
                }
                else
                {
                    log.Trace("Data is not ready on activation. Configuring async callback.");
                    _lastData.AllDataAvailiable += DataOnAllDataAvailiable;
                }
            }
            
            if (ResetSelectionOnActivation)
            {
                ResetSelectionMainLayout();
            }
        }

        private void DataOnAllDataAvailiable(object sender, EventArgs e)
        {
            if (_lastData == null) return;
            var dataRef = _lastData.GetFinalDataReference();
            if (dataRef == null)
            {
                log.Error("Data availiable event received but data seems missing");
                ChangeState(MultimediaEventListingStateEnum.Error);
                return;
            }

            log.Debug("Data availiable event for conversion");
            _lastData.AllDataAvailiable -= DataOnAllDataAvailiable; // Okazou que l'on vient du event async.

            /*
            if (_lastData.IsEmpty)
                ChangeState(MultimediaEventListingStateEnum.NoData);
            else*/
                ConvertDataAndLaunch(dataRef);
        }

        private void PopulateMainLayout(List<MultimediaEventGalleryRow> rowList)
        {
            if (_mainLayout.InvokeRequired)
            {
                var deleg = new SafeCallDelegatePopulate(PopulateMainLayout);
                _mainLayout.Invoke(deleg, rowList);
                return;
            }

            _rowList = rowList;
            var stateNext = MultimediaEventListingStateEnum.Ready;

            if (_rowList == null)
            {
                stateNext = MultimediaEventListingStateEnum.Error;
                _parent.SafeCall(() => { MessageBox.Show(@"Major error during image manipulation. You must close the application.", @"iBrand", MessageBoxButtons.OK); });
            } 
            else if (_rowList.Any())
            {
                log.Trace("MainLayout populate (Start): controls: {0}, rows: {1}, styles={2}", _mainLayout.Controls.Count, _mainLayout.RowCount, _mainLayout.RowStyles.Count);
                var rowIndex = 0;
                try
                {
                    foreach (var galleryItem in _rowList)
                    {
                        _mainLayout.Controls.Add(galleryItem.GetControlToDisplay(), 0, rowIndex);
                        _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        rowIndex++;
                    }
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Error during assignation of controls to main layout. Cleaning.");
                    stateNext = MultimediaEventListingStateEnum.Error;
                    _mainLayout.Controls.Clear();
                    _mainLayout.RowStyles.Clear();
                    rowIndex = 0;
                }

                _mainLayout.RowCount = rowIndex;

                // Un peu de "magie" avec Winforms pour faire disparaître la barre horizontale....
                // https://stackoverflow.com/questions/5489273/how-do-i-disable-the-horizontal-scrollbar-in-a-panel
                _mainLayout.HorizontalScroll.Maximum = 0;
                _mainLayout.AutoScroll = false;
                _mainLayout.AutoScroll = true;
                _mainLayout.HorizontalScroll.Visible = false;

                log.Trace("MainLayout populate (End): controls: {0}, rows: {1}, styles={2}", _mainLayout.Controls.Count, _mainLayout.RowCount, _mainLayout.RowStyles.Count);
            }
            else
            {
                stateNext = MultimediaEventListingStateEnum.NoData;
            }

            if (stateNext != MultimediaEventListingStateEnum.Ready)
            {
                // Aucun élément trouvé, on doit permettre aux usagers de retourner au tout début des écrans en annulant.
                if (_backButton != null)
                {
                    _backButton.Visible = true;
                    _backButton.BringToFront();
                    _parent.Invalidate();
                }
            }

            ChangeState(stateNext);
            _mainLayout.Refresh();
        }

        private void ResetSelectionMainLayout()
        {
            if (_mainLayout.InvokeRequired)
            {
                var deleg = new SafeCallDelegateReset(ResetSelectionMainLayout);
                _mainLayout.Invoke(deleg);
                return;
            }

            if (_rowList == null || !_rowList.Any()) return;

            var nbr = _rowList.Sum(c => c.ResetSelection());

            log.Trace("Selection reset completed with {0} elements changed.", nbr);
            if (nbr > 0) _mainLayout.Refresh();
        }

        private void ItemOnSelectionChangedEvent(object sender, object o)
        {
            _nbrItemsSelected = _rowList.Sum(f => f.GetSelectedNumber());
            log.Trace("Event -> Nombre d'éléments sélectionné modifié: {0}", _nbrItemsSelected);

            if (TriggerChangeOnMinimumSelectedItems != null && _nbrItemsSelected >= TriggerChangeOnMinimumSelectedItems.Value)
            {
                _parent.SafeCall(() =>
                {
                    // Va provoquer un recyclage, donc un arrêt du téléchargement si applicable.
                    _parent.SaveAllProperties();
                    _parent.Next(NextButtonPageSkip); // TODO Voir si on peut mettre d'autres options sur ce bouton.
                });
                return; // On stoppe le reste de la méthode si on fait next.
            }

            GenerateNumberSelectedImage();

            if (_nextButton != null)
                _parent.SafeCall(() =>
                {
                    _nextButton.Visible = _nbrItemsSelected > 1 || NextButtonAlwaysShow;
                    _nextButton.BringToFront();
                    _parent.Invalidate();
                });
        }

        public void Recycle(Control parent)
        {
            _downloadService?.StopDownload(); // Ne fait rien si les infos sont complètement téléchargées.

            ChangeState(MultimediaEventListingStateEnum.WaitingForData);
            if (_lastData != null)
            {
                _lastData.AllDataAvailiable -= DataOnAllDataAvailiable;
                _lastData = null;
            }

            if (_mainLayout != null)
            {
                _mainLayout.Controls.Clear();
                _mainLayout.RowStyles.Clear();
                _mainLayout.RowCount = 0;
            }

            if (_rowList != null)
            {
                foreach (var i in _rowList) i.Dispose();

                _rowList.Clear();
                _rowList = null;
            }

            _converterInstance.Clean();
        }

        public bool Save(ISaveLocalRespondentData saveTo, string activeLanguage)
        {
            if (_rowList == null || !_rowList.Any()) return true;

            if (log.IsTraceEnabled)
            {
                var resp = _parent.GetCurrentRespondentWithAnswers();
                log.Trace("Save: Before Header list correction: List => \n" + StringUtil.FormatEnumerable(resp.DynamicHeaders));
            }

            // TODO Cet algorithme devrait être standard. À déplacer dans un classe utilitaire ou dans le Questionnaire lui-même.
            // Ici, il faut mettre dans le questionnaire les éléments de groupes dynamique.
            // Comme ces informations forment une liste non structurée, on ne peut pas simplement remplacer cette structure; il faut trier, élaguer et remplacer
            // les informations contenues dans le questionnaire.
            
            // 1- Obtenir les groupes à ajouter
            var addGroups = _rowList.Select(row => row.GetDynamicHeaderToSave(SaveDynamicHeadersReplaceOriginalValueWithLocalFile)).Where(toSave => toSave != null).ToList();

            if (!addGroups.Any()) return true;

            // TODO On aggrège les valeur en un seul groupe dynamique. On pourrait les laisser dans leur propre groupe au besoin avec une simple config.
            var finalGroup = new DynamicHeader
            {
                DataKey = SaveDynamicHeadersCommonDataKey ?? addGroups[0].DataKey,
                DateCreated = DateTime.Now
            };

            foreach (var dyna in addGroups) {
                // Pas de remplacement, on utilise les valeurs comme telles.
                finalGroup.DataValues.AddRange(dyna.DataValues.Select(c => c)); // Transfert de liste
            }

            // 2- Enlever les anciens groupes de la liste du questionnaire
            saveTo.ClearDynamicHeaderCollection(finalGroup.DataKey);

            // 3- Ajouter ce qui reste de la liste originale et les groupes actuels.
            saveTo.SaveDynamicHeader(finalGroup);

            if (log.IsTraceEnabled)
            {
                var resp = _parent.GetCurrentRespondentWithAnswers();
                log.Trace("Save: After Header list correction: List => \n" + StringUtil.FormatEnumerable(resp.DynamicHeaders));
            }

            return true;
        }

        public void Dispose()
        {
            if (_lastData != null)
            {
                _lastData.AllDataAvailiable -= DataOnAllDataAvailiable;
                _lastData = null;
            }

            if (_mainLayout != null)
            {
                _mainLayout.Dispose();
                _mainLayout = null;
            }

            if (_rowList != null)
            {
                foreach (var i in _rowList) i.Dispose();

                _rowList.Clear();
                _rowList = null;
            }
        }

        public bool InitImage(string baseDir, string languageCode)
        {
            var state = true;

            // Création des images de overlay pour les données de présentation.
            if (PresentationData == null)
            {
                log.Warn("No valid presentation data found. Cannot initialize.");
                return false;
            }

            state &= PresentationData.Init();
            state &= GenerateStateMessageImage();
            state &= GenerateNumberSelectedImage();
            return state;
        }

        private bool GenerateNumberSelectedImage()
        {
            if (NumberSelectedTextImageGenerator == null || NumberSelectedTextLocation == null) return true;

            NumberSelectedTextImageGenerator.CustomStrings = new[] {_nbrItemsSelected.ToString()};
            _textItemSelectedImage = NumberSelectedTextImageGenerator.GenerateImage(_parent.ActiveLanguage);
            _parent.Invalidate();
            return true;
        }

        private bool GenerateStateMessageImage()
        {
            if (StateMessageLocation == null) return true;

            // Assignation d'une définition d'image texte par défaut.
            if (StateMessageImageDefinition == null)
                StateMessageImageDefinition = new ProceduralImageTextInfo
                {
                    BitmapSize = StateMessageLocation.Value.Size,
                    Color = Color.Black,
                    FontStyle = FontStyle.Bold,
                    FontGraphicUnit = GraphicsUnit.Pixel,
                    FontSize = 50,
                    FontFamilly = "Helvetica"
                };

            string txt = null;
            if (StateMessageCustomTexts == null)
                StateMessageCustomTexts = new Dictionary<MultimediaEventListingStateEnum, IDictionary<string, string>>
                {
                    {MultimediaEventListingStateEnum.Ready, new Dictionary<string, string> {{"en", "Gallery loaded"}}},
                    {MultimediaEventListingStateEnum.NoData, new Dictionary<string, string> {{"en", "No data exists. It is impossible to proceed."}}},
                    {MultimediaEventListingStateEnum.WaitingForData, new Dictionary<string, string> {{"en", "Data is being fetched from servers. Please wait."}}},
                    {MultimediaEventListingStateEnum.Error, new Dictionary<string, string> {{"en", "An error has occured. Please try again."}}}
                };

            if (StateMessageCustomTexts.ContainsKey(_presentState)) txt = StateMessageCustomTexts[_presentState]?.GetDefaultValue(_parent.ActiveLanguage);

            _stateMessageImage?.Dispose();
            _stateMessageImage = txt == null ? null : ImageUtil.CreateProceduralTextBitmap(StateMessageImageDefinition, txt);

            log.Trace("State image updated");

            return true;
        }

        public void Paint(Graphics g, bool debug = false)
        {
            ScreenUtil.DrawImage(g, _textItemSelectedImage, NumberSelectedTextLocation);
            ScreenUtil.DrawImage(g, _stateMessageImage, StateMessageLocation);
        }

        public bool Init()
        {
            return PresentationData?.Init() ?? false;
        }

        public void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            GenerateNumberSelectedImage();
            GenerateStateMessageImage();
            screen.SafeCall(screen.Refresh);
        }

        public void Report(RespondentBundleDownloadReport value)
        {
            // Reçoit les information du service de téléchargement.
            if (log.IsDebugEnabled) log.Debug("RespDownBundleService report: " + value.ToStringPublicProperties());
        }

        private void ChangeState(MultimediaEventListingStateEnum newState)
        {
            if (_presentState == newState) return;

            log.Trace("State changed from '{0}' to '{1}'.", _presentState, newState);
            _presentState = newState;

            if (StateMessageLocation != null)
                _parent.SafeCall(() =>
                {
                    GenerateStateMessageImage();
                    _parent.SafeCall(_parent.Refresh);
                });
        }
    }
}