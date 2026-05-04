using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using MCollections;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Win32;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    [PublicAPI]
    public class ScreenDefinitionGalleryFromFiles : 
        ScreenDefinitionSelectionBase<AnswerZone>,
        IOnLoad, IActivateScreen, IRecyclable, ISaveToQuestionnaire, IEvaluateScreenZones, IEvaluateDefinitionsState, IImageInitialisable, IPaint, 
        IPaginationMonitor<ComparableFileInfo>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        [JsonRequired]
        public IGallerySource<ComparableFileInfo> GallerySource { get; set; }
        
        [JsonRequired]
        public Rectangle GalleryZone { get; set; }

        private List<AnswerZone> _shownPictures = new List<AnswerZone>();

        [JsonIgnore]
        public override IList<AnswerZone> SelectionZones
        {
            get => _shownPictures;
            set => throw new System.Exception("Cannot assign zones to a Gallery.");
        }

        private CancellationTokenSource _cts;
        private Task _loadTask;
        private AbstractScreen _parent;
        private IRespondent _respondent;

        public override string DefaultConfigFileName { get; set; } = "screen_definition_gallery_index.json";
        
        /// <summary>
        /// Message à afficher quand on charge les images en mémoire.
        /// </summary>
        public AnimatedImageSequence LoadingImageSequence { get; set; }

        /// <summary>
        /// Message à afficher quand on n'a pas pu charger toutes les images.
        /// </summary>
        public AnimatedImageSequence ErrorImageSequence { get; set; }

        /// <summary>
        /// Force un ratio d'image à afficher à l'intérieur des zones d'affichage.
        /// Si null, on tente d'obtenir le ratio de la première image.
        /// </summary>
        public float? GalleryImageRatio { get; set; }
        
        /// <summary>
        /// Options de changements d'écran quand on appuie sur une image.
        /// Cette option sera la même pour chaque image, et elles agiront comme un bouton si cette valeur est non nulle.
        /// Si cette valeur est nulle, il faut spécifier un mouvement d'écran ailleurs dans l'écran.
        /// </summary>
        public ScreenChangeOptions SelectionPageChangeOptions { get; set; }
        
        /// <summary>
        /// Indique le pourcentage de taille (de la zone totale) à utiliser comme valeur de "padding"
        /// entre chaque élément. Le nombre fois que cette valeur sera utilisée est le nombre d'éléments par axe, moins 1.
        /// Par défaut, 2%. Il s'agit d'une valeur fractionnaire.
        /// </summary>
        public double PaddingRatio { get; set; } = 0.02;

        /// <summary>
        /// Indique si on doit utiliser le système de génération de thumbnails pour afficher les éléments.
        /// À utiliser si il est possible de voir plusieurs fois les mêmes fichiers dans la gallerie, particulièrement
        /// s'il y a de la pagination à afficher.
        /// </summary>
        public bool UseThumbnails { get; set; } = true;

        /// <summary>
        /// Modificateur d'index pour fin de réponses. Si cette valeur est de zéro, les indices sont basé sur zéro.
        /// </summary>
        public int FinalIndexOffset { get; set; } = 1;

        /// <summary>
        /// Configuration des éléments de navigation visuels.
        /// </summary>
        public PaginationUiElements<ComparableFileInfo> PaginationUi { get; set; }

        public bool ShowReversed
        {
            get => IsReversed;
            set
            {
                if (IsReversed != value)
                {
                    Reverse();
                }
            }
        }

        //private readonly object _locker = new object();

        public event EventHandler ShouldRefresh;

        public event EventHandler UiRefresh;

        private int _projectId;
        private int _questionnaireId;
        private DirectoryInfo _thumbDir;
        private readonly Dictionary<ComparableFileInfo, AnswerZone> _dicAnswerZones = new Dictionary<ComparableFileInfo, AnswerZone>();

        public bool OnLoad(AbstractScreen screen)
        {
            _parent = screen;
            
            if (ErrorImageSequence != null)
            {
                ErrorImageSequence.UpdateDraw = screen.InvokeRefresh;
            }
            if (LoadingImageSequence != null)
            {
                LoadingImageSequence.UpdateDraw = screen.InvokeRefresh;
            }

            _projectId = screen.ProjectId;
            _questionnaireId = screen.QuestionnaireId;
            _thumbDir = new DirectoryInfo(screen.GetTerminalDirectory(
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail));
            _baseDir = screen.GetTerminalDirectory();
            
            ShouldRefresh += OnShouldRefresh;
            
            if (GallerySource == null)
            {
                log.Error("Gallery source is null. Loading of gallery screen has failed.");
                return false;
            }

            if (!GallerySource.Init())
            {
                log.Error("Gallery source could not initialize correctly. Loading of gallery screen has failed.");
                return false;
            }

            if (!GallerySource.AddElementMonitor(this))
            {
                log.Error("Could not assign screen as gallery monitor. Cancelling initialization setup.");
                return false;
            }

            if (!GallerySource.StartUpdates())
            {
                log.Error("Gallery source could not start updating correctly. Loading of gallery screen has failed.");
                ShowErrorScreen(_parent);
                return false;
            }

            return PaginationUi?.OnLoad(screen) ?? true;
        }

        public override void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            base.ChangeLanguage(screen, languageCode);

            PaginationUi?.ChangeLanguage(screen, languageCode);

            InitImage(_baseDir, languageCode);
        }

        private void OnShouldRefresh(object sender, EventArgs e)
        {
            InitiateImageUpdate();
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            _respondent = respondent;

            if (GallerySource is IActivateScreen gsa)
            {
                gsa.Activate(_parent, respondent);
            }

            InitiateImageUpdate();
        }

        protected void InitiateImageUpdate()
        {
            if (_parent == null || _respondent == null) return;
            if (!_parent.IsActivated()) return;

            LoadingImageSequence?.StartSequence();
            PaginationUi?.DisableSwitch(true);

            if (_cts != null && _loadTask != null) return;
            
            _cts = new CancellationTokenSource();
            _loadTask = Task.Run(() => LoadImagesToPictureBox(_cts.Token), _cts.Token).ContinueWith(t =>
            {
                LoadingImageSequence?.StopSequence();

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    _parent.SafeCall(_parent.Refresh);
                }

                _cts = null;
                _loadTask = null;
                PaginationUi?.DisableSwitch(false);
                UiRefresh?.Invoke(this, null);
            });
        }

        protected void LoadImagesToPictureBox(CancellationToken ct)
        {
            if (!NeedRefresh) return;
            
            UpdateAnswerZones(ct, GetCurrentPageElements());
        }

        private void UpdateAnswerZones(CancellationToken ct, [NotNull] IReadOnlyList<(ComparableFileInfo, int)> elements)
        {
            if (GalleryImageRatio == null && elements.Any())
            {
                var tmp = elements.First().Item1.FileInfoRef;
                if (ImageMagickCliUtil.GetImageDimensions(tmp, out var size))
                {
                    GalleryImageRatio = size.Width/(float)size.Height;
                }
                else
                {
                    log.Error("Cannot proceed. No image ration was given and first file candidate is invalid: " + tmp.FullName);
                    return;
                }
            }

            if (GalleryImageRatio == null)
            {
                log.Debug("Gallery ratio is unknown (until files arrive). Skipping.");
                return;
            }

            _coordsCache = MathUtil.CalculatePresentationCoordinates(_coordsCache, PresentationSize, GalleryZone, PaddingRatio, GalleryImageRatio.Value, true);
            var coords = _coordsCache.ElementCoordinates;
            if (ct.IsCancellationRequested) return;

            // Ajoute les zones dans le dictionnaire d'éléments déjà instanciés
            var indexList = elements.Select(i => i.Item1).ToList();
            var newElemsFileInfo = indexList.Except(_dicAnswerZones.Keys).ToList();
            
            if (newElemsFileInfo.Any())
            {
                var newElems = elements.Where(e => newElemsFileInfo.Contains(e.Item1));
                var zoneSize = coords.First().Size;

                foreach (var (comparableFileInfo, item2) in newElems)
                {
                    if (ct.IsCancellationRequested) break;
                    
                    var answers = GallerySource.SaveFormatter.CreateAnswers(item2, comparableFileInfo);
                    
                    var az = new AnswerZone
                    {
                        // On recalcule le rectangle de présentation plus tard.
                        PageChangeOptions = SelectionPageChangeOptions,
                        BackgroundImageFilePath = comparableFileInfo.FileInfoRef.FullName,
                        AnswersUponSelection = answers
                    };

                    if (!UseThumbnails)
                    {
                        az.BackgroundImageInstance = FileUtil.LoadBitmap(comparableFileInfo.FileInfoRef.FullName);
                    }
                    else
                    {
                        if (ImageMagickCliUtil.LoadImageFromThumbnail(comparableFileInfo.FileInfoRef, _thumbDir, zoneSize, out var bmp, true, Color.Transparent))
                        {
                            az.BackgroundImageInstance = bmp;
                        }
                        else
                        {
                            log.Error("Could not create thumbnail for file: " + comparableFileInfo.FileInfoRef.FullName);
                        }
                    }

                    log.Trace("Answer zone added to main zone dictionary. Bitmap file: {0}", StringUtil.GetFileName(az.BackgroundImageFilePath));
                    _dicAnswerZones.Add(comparableFileInfo, az);
                }
            }

            if (!ct.IsCancellationRequested)
            {
                lock (_shownPictures)
                {
                    var tmp = new List<AnswerZone>();
                    for (var i = 0; i < elements.Count && i < coords.Count; i++)
                    {
                        var (comparableFileInfo, index) = elements[i];
                        var az = _dicAnswerZones[comparableFileInfo];
                        tmp.Add(az);
                        az.Zone = coords[i];
                        log.Trace("Adding following anwser zone to gallery presentation: Index={0}, Bitmap file {1}", index, StringUtil.GetFileName(az.BackgroundImageFilePath));
                    }

                    _shownPictures = tmp;
                }
            }
            else
            {
                _shownPictures = new List<AnswerZone>();
                return;
            }

            // On libère de la mémoire, au besoin. On tolère jusqu'à quatre fois la taille de la présentation.
            if (_dicAnswerZones.Count < 4 * coords.Count) return;

            var unsusedKeys = _dicAnswerZones.Keys.Except(indexList).ToList();

            foreach (var key in unsusedKeys)
            {
                var az = _dicAnswerZones[key];
                if (!_dicAnswerZones.Remove(key)) continue; // Pas supposé, mais bon.
                log.Trace("Disposing unused answer zone. Key={1}, Bitmap file: {0}", StringUtil.GetFileName(az.BackgroundImageFilePath), key);
                az.Dispose();
            }
        }
        
        /// <summary>
        /// Dans le cas où une erreur se produit pour ce répondant, on change la configuration de l'écran pour afficher un message
        /// approprié. L'écran ne peut plus continuer, mais on peut revenir en arrière ou annuler le répondant.
        /// </summary>
        private void ShowErrorScreen(Control parent)
        {
            ErrorImageSequence?.StartSequence();
            parent.SafeCall(parent.Refresh);
        }
        
        public new void Recycle(Control parent)
        {
            base.Recycle(parent);

            LoadingImageSequence?.StopSequence();
            ErrorImageSequence?.StopSequence();
            
            _respondent = null;
            _cts?.Cancel();
            _cts = null;
            _loadTask = null;
        }

        public new void Dispose()
        {
            base.Dispose();

            lock(_shownPictures) _shownPictures = new List<AnswerZone>();

            ErrorImageSequence?.Dispose();
            LoadingImageSequence?.Dispose();

            GallerySource?.StopUpdates();
            GallerySource?.RemoveElementMonitor(this);
            GallerySource?.Dispose();

            PaginationUi?.Dispose();

            // Listes de pagination
            if (GallerySource != null)
            {
                GallerySource.StopUpdates();
                GallerySource.RemoveElementMonitor(this);
                GallerySource.Dispose();
            }

            _galleryFilesComplete.Clear();
            _currentElements = new List<(ComparableFileInfo, int)>();

            _cts?.Cancel();
            _cts = null;
            _loadTask = null;

            log.Trace("Disposed of the instance of Gallery from files.");
        }

        public new bool InitImage(string baseDir, string languageCode)
        {
            // On ne fait pas l'initialisation de la pagination ici.

            LoadingImageSequence?.ClearCurrentImages();
            LoadingImageSequence?.CreateImages(languageCode);

            ErrorImageSequence?.ClearCurrentImages();
            ErrorImageSequence?.CreateImages(languageCode);
            
            return true;
        }

        public bool Save(ISaveLocalRespondentData saveTo, string activeLanguage)
        {
            return CurrentlySelectedZone != null && CurrentlySelectedZone.AnswersUponSelection.All(saveTo.SaveAnswerToQuestion);
        }

        public ScreenEvaluationResult EvaluateDefinitionsState(string activeLanguage)
        {
            if (!Mandatory && CurrentlySelectedZone == null)
            {
                return new ScreenEvaluationResult
                {
                    Result = EvaluationResult.Valid
                };
            }

            var result = CurrentlySelectedZone != null ? EvaluationResult.Valid : EvaluationResult.Invalid;
            var messages = new List<string>();
            if (result != EvaluationResult.Valid)
            {
                messages.Add(DefaultUserMessages[UserErrorEnum.Mandatory][activeLanguage]);
            }

            return new ScreenEvaluationResult
            {
                Result = result,
                UserMessages = messages
            };
        }

        public new void Paint(Graphics g, bool debug = false)
        {
            lock (_currentElements)
            {
                base.Paint(g, debug);
            }

            LoadingImageSequence?.Paint(g, debug);
            ErrorImageSequence?.Paint(g, debug);
            PaginationUi?.Paint(g, debug);

            if (debug) ScreenUtil.DrawDebugLines(g, ScreenUtil.DebugBlackPen, _coordsCache?.DebugLines);
        }

        public new ScreenEvaluationResult EvaluateZones(AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            var result = base.EvaluateZones(screen, x, y, validationMethod);
            var pageResults = PaginationUi?.EvaluateZones(screen, x, y, validationMethod);

            return ScreenEvaluationResult.AggregateResults(result, pageResults);
        }

        #region Pagination Interface
        
        private Size _presentationSize = new Size(0,0);
        private bool _loadElems;
        /// <summary>
        /// Cette liste contient tous les fichiers qui forment la gallerie, même si la plupart d'entre eux ne sont pas visible dû à la pagination.
        /// </summary>
        private IndexedSet<ComparableFileInfo> _galleryFilesComplete = new IndexedSet<ComparableFileInfo>(natural);
        private IReadOnlyList<ValueTuple<ComparableFileInfo, int>> _currentElements = new List<ValueTuple<ComparableFileInfo, int>>();
        private string _baseDir;
        
        /// <summary>
        /// Comparateur d'ordre Naturel
        /// </summary>
        private static readonly IComparer<ComparableFileInfo> natural = Comparer<ComparableFileInfo>.Create((a, b) =>
        {
            if (a == null) return b == null ? 0 : -1;
            if (b == null) return 1;
            return a.CompareTo(b);
        });
        /// <summary>
        /// Comparateur d'ordre Inverse
        /// </summary>
        private static readonly IComparer<ComparableFileInfo>  inverse = Comparer<ComparableFileInfo>.Create((a, b) =>
        {
            if (b == null) return a == null ? 0 : -1;
            if (a == null) return 1;
            return b.CompareTo(a);
        });

        private PresentationCoordinates _coordsCache;

        public bool HasNextPage => CurrentPage < MaxPage;

        public bool HasPreviousPage => CurrentPage > 0;

        public uint TotalElements => (uint) _galleryFilesComplete.Count;

        public Size PresentationSize
        {
            get => _presentationSize;
            set
            {
                if (_presentationSize.Equals(value)) return;
                _presentationSize = value;
                NeedRefresh = true;
                log.Trace("Pagination presentation changed to {0}x{1}", _presentationSize.Width, _presentationSize.Height);
                UiRefresh?.Invoke(this, null);
                ShouldRefresh?.Invoke(this, null);
            }
        }

        public ComparableFileInfo CurrentPresentedElement { get; private set; }

        public uint MaxPage { get; private set; }

        public uint CurrentPage { get; private set; }

        public bool HasUpdated { get; private set; }

        public bool NeedRefresh { get; private set; }

        public bool IsReversed { get; private set; }

        public bool Reverse()
        {
            IsReversed = !IsReversed;

            var perPage = _presentationSize.Width * _presentationSize.Height;
            if (perPage == 0) return false;
            
            // Inversion de la liste. Il faut ré-insérer tous les éléments.
            var tmp = new IndexedSet<ComparableFileInfo>(IsReversed ? inverse : natural);
            foreach (var cfi in _galleryFilesComplete)
            {
                tmp.Add(cfi);
            }
            _galleryFilesComplete.Clear();
            _galleryFilesComplete = tmp;

            var oldPage = CurrentPage;
            if (CurrentPage == MaxPage)
            {
                CurrentPage = 0;
            }
            else
            {
                var maxIndex = perPage * (CurrentPage+1);
                var newIndex = _galleryFilesComplete.Count - maxIndex;
                CurrentPage = (uint)Math.Floor(newIndex / (double)perPage);
            }
            
            log.Trace("Reversing pagination list. Old page = {0}, new page = {1}", oldPage, CurrentPage);

            NeedRefresh = true;
            _loadElems = true;
            UiRefresh?.Invoke(this, null);
            ShouldRefresh?.Invoke(this, null);

            return true;
        }

        public bool GotoPage(uint pageIndex)
        {
            if (CurrentPage == pageIndex) return true;
            if (pageIndex > MaxPage) return false;
            
            log.Trace("Page change. Going to index {0}", pageIndex);

            _loadElems = true;
            NeedRefresh = true;
            CurrentPage = pageIndex;
            CurrentPresentedElement = null;

            UiRefresh?.Invoke(this, null);
            ShouldRefresh?.Invoke(this, null);

            return true;
        }

        public bool GotoElement(ComparableFileInfo element, IComparer<ComparableFileInfo> comparer = null)
        {
            if (element == null) return false;

            var ind = FindIndexInGallery(element, comparer);

            if (ind < 0) return false;

            var perPage = PresentationSize.Width * PresentationSize.Height;
            if (perPage == 0) return false;
            
            var newPage = (uint)Math.Floor(ind / (float) perPage);

            return newPage != CurrentPage && GotoPage(newPage);
        }

        private int FindIndexInGallery(ComparableFileInfo element, IComparer<ComparableFileInfo> comparer)
        {
            var ind = -1;
            for (var i = 0; i < _galleryFilesComplete.Count; i++)
            {
                var elem = _galleryFilesComplete[i];
                if (comparer != null && comparer.Compare(element, elem) == 0)
                {
                    ind = i;
                    break;
                }

                if (element.CompareTo(elem) == 0)
                {
                    ind = i;
                    break;
                }
            }

            return ind;
        }

        public IReadOnlyList<(ComparableFileInfo, int)> GetCurrentPageElements()
        {
            var tmpList = new List<(ComparableFileInfo, int)>();
            var perPage = _presentationSize.Width * _presentationSize.Height;
            if (perPage == 0)
            {
                return tmpList;
            }

            // Obtenir les X éléments (max) à afficher
            var totalFiles = _galleryFilesComplete.Count;
            var startIndex = (int) (perPage * CurrentPage);
            var maxIndex = Math.Min(totalFiles, perPage + startIndex) - 1;

            log.Trace("GetCurrentPageElements: Calculating elements (index {0} to {1}) for potential initialisation.", startIndex, maxIndex);

            for (var i = startIndex; i <= maxIndex; i++)
            {
                tmpList.Add(new ValueTuple<ComparableFileInfo, int>(_galleryFilesComplete[i], i));
            }

            // Vérifie si on a besoin de charger à nouveau les éléments en mémoire
            if (!_loadElems && !NeedRefresh && tmpList.SequenceEqual(_currentElements))
            {
                log.Trace("GetCurrentPageElements: No update needed for current page elements. Returning old list.");
                return _currentElements;
            }

            _currentElements = tmpList;
            log.Trace("GetCurrentPageElements: Targeted {0} elements for current page.", _currentElements.Count);

            return _currentElements;
        }

        public int GetElementIndex(ComparableFileInfo element)
        {
            if (element == null) return -1;
            var ind = FindIndexInGallery(element, null);
            
            return ind;
        }

        public void UpdateElements(IndexedSet<ComparableFileInfo> updatedElements)
        {
            // On remet les trigger booléean à faux.
            HasUpdated = false;
            _loadElems = false;
            NeedRefresh = false;

            var oldGallCount = _galleryFilesComplete.Count;

            var perPage = _presentationSize.Width * _presentationSize.Height;
            if (perPage == 0)
            {
                log.Error("UpdateElements: Cannot calculate pagination: invalid presentation Size: " + perPage);
                return;
            }

            if (!updatedElements.Any()) //  On vide la liste
            {
                log.Trace("UpdateElements: Empty update list. No element to show.");
                _galleryFilesComplete.Clear();
                MaxPage = 0;
                HasUpdated = true;
                NeedRefresh = true;
            }
            else if (_galleryFilesComplete.Count == 0 && updatedElements.Any()) // Chargement initial
            {
                foreach (var elem in updatedElements)
                {
                    _galleryFilesComplete.Add(elem);
                }

                MaxPage = (uint)Math.Ceiling(_galleryFilesComplete.Count / (double) perPage) - 1;
                CurrentPage = IsReversed ? 0 : MaxPage;

                // Tout doit être chargé, au minimum.
                HasUpdated = true;
                NeedRefresh = true;
                _loadElems = true;

                log.Trace("UpdateElements: Initial update: Received {0} files for pagination.", _galleryFilesComplete.Count);
            }
            else // Tout les autres check.
            {
                // Si ce check est faux, alors on a pas le choix de remplacer la gallerie au complet.
                if (!updatedElements.SetEquals(_galleryFilesComplete))
                {
                    StringBuilder sb = null;
                    if (log.IsTraceEnabled)
                    {
                        sb = new StringBuilder($"UpdateElements: New set not a subset of current list. Taking new set and rebuilding pagination. Gal={oldGallCount}, Upd={updatedElements.Count}");
                    }

                    var wasOnLast = CurrentPage == MaxPage;
                    var lastMax = MaxPage;

                    _galleryFilesComplete.Clear();
                    _galleryFilesComplete = new IndexedSet<ComparableFileInfo>(IsReversed ? inverse : natural);
                    foreach (var el in updatedElements)
                    {
                        _galleryFilesComplete.Add(el);
                    }

                    MaxPage = (uint)Math.Ceiling(_galleryFilesComplete.Count / (double) perPage) - 1;

                    if (wasOnLast && lastMax != MaxPage && !IsReversed) // On veut rester sur la dernière page quand on n'est pas en mode inverse.
                    {
                        sb?.Append($"\n\tAutomatic page switch. Was on last ({lastMax}) before max page change ({MaxPage}) in normal order.");
                        CurrentPage = MaxPage;
                    }

                    var oldSeq = _currentElements;
                    var tmp = GetCurrentPageElements();
                    if (!ReferenceEquals(oldSeq, tmp))
                    {
                        sb?.Append("\n\tCurrent element sequence has changed.");
                        NeedRefresh = true;
                        _loadElems = true;
                    }

                    HasUpdated = true;

                    if (sb != null)
                    {
                        log.Trace(sb.ToString());
                    }
                }
                else
                {
                    // Les deux listes sont strictement égales
                    log.Trace("UpdateElements: No new elements were detected. Current={0}", updatedElements.Count);
                }
            }

            if (oldGallCount != _galleryFilesComplete.Count)
            {
                log.Trace("Gallery total elements changed. Gal={0}, upd={1}", oldGallCount, updatedElements.Count);
            }

            // Vérifie les indices
            if (CurrentPage > MaxPage)
            {
                log.Trace("UpdateElements: CurrentPage index exceeded maximal page. Updating and refreshing elements.");
                // Correctif de la page actuelle, au besoin.
                CurrentPage = MaxPage;
                NeedRefresh = true;
                _loadElems = true;
            }

            if (HasUpdated)
            {
                UiRefresh?.Invoke(this, null);
            }

            if (NeedRefresh)
            {
                ShouldRefresh?.Invoke(this, null);
            }

            if ((HasUpdated || NeedRefresh) && log.IsTraceEnabled)
            {
                log.Trace($"UpdateElements: Update finished. (HasUpdate={HasUpdated}, NeedRefresh={NeedRefresh}). Indexes recalculated:\tMax Page => {MaxPage}\tCurrent Page => {CurrentPage}\tTotal => {_galleryFilesComplete.Count}\tReversed => {IsReversed}");
            }
        }

        #endregion
    }
}
