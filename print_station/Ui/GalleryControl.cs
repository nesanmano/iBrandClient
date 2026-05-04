using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Win32;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation.Ui
{
    public partial class GalleryControl : UserControl
    {
        /// <summary>
        /// Structure utilisée pour stocker les valeurs d'espacement pour les images, lors de la présentation.
        /// </summary>
        private class GallerySpacingData
        {
            internal int ImageWidth;
            internal int ImageHeight;
            internal int LeftStart;
            internal int TopStart;
            internal int RowPadding;
            internal int RowNumber;
            internal int ColumnPadding;
            internal int ColumnNumber;
        }

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private GallerySpacingData _spacingData;
        protected readonly PrintStationDatabase _database;
        protected KeyboardControl _keyboard;
        protected readonly Bitmap _logo;
        protected long _maxId;
        protected long _minId;
        protected int _nbrPictureByPage = -1;
        protected Rectangle _nextArea;
        protected bool _nextButtonVisible;
        protected Bitmap _nextButton;

        //Si pas d'interaction pendant 20 secondes et pas à la dernière page -> on retourne a la derniere page. FSF
        private Timer _onTouchSinceTimer;
        protected List<Post> _postsToShow;
        protected Rectangle _previousArea;
        protected Bitmap _previousButton;
        protected bool _previousButtonVisible;
        private Rectangle _searchArea;
        private Bitmap _searchButton;
        protected Bitmap _searchText;
        protected readonly Bitmap _background;
        protected Point _searchTextLocation;
        protected TextBox _searchTextbox;
        protected readonly PrintStationUser _user;
        public RemovePicturesControlEvent _onRemovePicturesControl;
        public ShowPicturesControlEvent _onShowPicturesControl;
        public EventHandler _showToolStrip;

        /// <summary>
        /// Classe principale pour l'affichage des Posts à l'écran. Inclu (ou non) les principaux éléments de navigation aussi
        /// et de présentation.
        /// </summary>
        /// <param name="user">L'usager printstation en cours.</param>
        /// <param name="width">La largeur en pixels de la forme.</param>
        /// <param name="height">La heuteur en pixel de la forme.</param>
        public GalleryControl(PrintStationUser user, int width, int height)
        {
            var conf = RunningFeatureConfig.GetInstance();
            InitializeComponent();
            
            var showSearch = conf.GetConfiguration<bool>(FeatureConfigEnum.IsGalleryShowSearch);

            BackColor = ColorTranslator.FromHtml("#" + RunningFeatureConfig.GetInstance().GetConfiguration<string>(FeatureConfigEnum.BackgroundColor));

            var backgroundFileName = UiFilesUtil.GetFileName(UiFilesUtil.Background, user.Id);

            if (File.Exists(backgroundFileName))
            {
                _background = FileUtil.LoadBitmap(backgroundFileName);
                // Copie pour libérer le "handle" de fichier et gossage avecles référence.
                var tmp = new Bitmap(_background);
                _background.Dispose();
                _background = tmp;
            }

            _user = user;

            Size = new Size(width, height);

            _searchArea = new Rectangle(new Point(0, 0), new Size(70, 70));

            _database = ServicesContainer.GetService<PrintStationDatabase>();

            var logoFileName = UiFilesUtil.GetFileNameWhitoutDefault(UiFilesUtil.Logo, user.Id);

            if (logoFileName != null)
            {
                _logo = FileUtil.LoadBitmap(logoFileName);
            }

            if (showSearch)
                AddSearchElements();

            Paint += PaintEvent;

            MouseDown += MouseDownHandler;

            Initialize();
        }

        public PrintStationUser User => _user;

        protected void ResetTimer()
        {
            _onTouchSinceTimer.Stop();
            _onTouchSinceTimer.Start();
        }

        protected virtual void AddSearchElements()
        {
            var conf = RunningFeatureConfig.GetInstance();

            var searchBoxWidth = conf.GetConfiguration<int>(FeatureConfigEnum.GallerySearchBoxWidth);
            var searchBoxTextSize = conf.GetConfiguration<int>(FeatureConfigEnum.GallerySearchBoxTextSize);
            var panelHeight = conf.GetConfiguration<int>(FeatureConfigEnum.GallerySearchPanelHeight);
            var searchOffset = conf.GetConfiguration<int>(FeatureConfigEnum.GallerySearchOffset);

            var seacrhButtonFileName = UiFilesUtil.GetFileName(UiFilesUtil.SearchButton, _user.Id);
            var searchTextFileName = UiFilesUtil.GetFileName(UiFilesUtil.SearchText, _user.Id);

            if (!File.Exists(seacrhButtonFileName) || !File.Exists(searchTextFileName)) return;

            _searchText = FileUtil.LoadBitmap(searchTextFileName);

            _searchTextbox = new TextBox
            {
                Size = new Size(searchBoxWidth, 100),
                Font = new Font(FontFamily.GenericSansSerif, searchBoxTextSize)
            };
            _searchTextbox.Click += OnTextBoxGotFocus;
            _searchTextbox.LostFocus += OnTextBoxLostFocus;

            _searchButton = FileUtil.LoadBitmap(seacrhButtonFileName);

            var x = Width / 2 - (_searchText.Width + 5 + _searchTextbox.Width + 5 + _searchButton.Width) / 2;
            var y = (panelHeight / 2) - (_searchText.Height / 2) + searchOffset;

            _searchTextLocation = new Point(x, y);

            x = _searchTextLocation.X + _searchText.Width + 5;
            y = (panelHeight / 2) - (_searchTextbox.Height / 2) + searchOffset;

            _searchTextbox.Location = new Point(x, y);

            x = _searchTextbox.Location.X + _searchTextbox.Width + 5;
            y = (panelHeight / 2) - (_searchButton.Height / 2) + searchOffset;

            _searchArea = new Rectangle(new Point(x, y), _searchButton.Size);
        }

        private void InitPostsToShow()
        {
            _postsToShow = _database.GetLastReceivedPosts(_nbrPictureByPage, _user.EventId);

            _postsToShow = _postsToShow.OrderBy(post => post.Id).ToList();

            _minId = _database.GetMinPostId(_user.EventId);

            if (_postsToShow.Count > 0)
                _maxId = _postsToShow[_postsToShow.Count - 1].Id;
        }

        public void ShowPictures()
        {
            InitPostsToShow();

            ShowPictures(_postsToShow);

            ShowNavigationButtons();
        }

        public void RefreshLastScreen()
        {
            //if last posts are presented 
            if (!_nextButtonVisible)
            {
                ShowLastScreen();
            }
        }

        public void ShowLastScreen()
        {
            InitPostsToShow();

            ShowPictures(_postsToShow);

            ShowNavigationButtons();
        }

        private void Initialize()
        {
            var conf = RunningFeatureConfig.GetInstance();
            var refreshTimer = conf.GetConfiguration<int>(FeatureConfigEnum.GalleryRefreshTimer);
            var configBottomPanelHeight = conf.GetConfiguration<int>(FeatureConfigEnum.GalleryBottomPanelHeight);
            var topStart = RunningFeatureConfig.GetInstance().GetConfiguration<int>(FeatureConfigEnum.GalleryPanelTopStart);

            if (topStart <= 0)
            {
                topStart = 0;
            }

            CalculateSpacingInfo();

            var sidePanelWidth = conf.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth);

            var nextButtonFileName = UiFilesUtil.GetFileName(UiFilesUtil.NextButton, _user.Id);



            if (File.Exists(nextButtonFileName))
            {
                _nextButton = FileUtil.LoadBitmap(nextButtonFileName);

                var panelLeftStart = conf.GetConfiguration<int>(FeatureConfigEnum.GalleryPanelLeftStart);

                var x = panelLeftStart + GetPicturesAndSideAvailableWidth() - sidePanelWidth + ((sidePanelWidth - _nextButton.Width) / 2);
                var y = (GetPicturesAndSideAvailableHeight(configBottomPanelHeight) / 2) - (_nextButton.Height / 2) + topStart;

                _nextArea = new Rectangle(new Point(x, y), _nextButton.Size);
            }

            var backButtonFileName = UiFilesUtil.GetFileName(UiFilesUtil.BackButton, _user.Id);

            if (File.Exists(backButtonFileName))
            {
                _previousButton = FileUtil.LoadBitmap(backButtonFileName);

                var panelLeftStart = conf.GetConfiguration<int>(FeatureConfigEnum.GalleryPanelLeftStart);

                var x = (sidePanelWidth - _previousButton.Width) / 2 + panelLeftStart;
                var y = (GetPicturesAndSideAvailableHeight(configBottomPanelHeight) / 2) - (_previousButton.Height / 2) + topStart;
                _previousArea = new Rectangle(new Point(x, y), _previousButton.Size);
            }

            _nbrPictureByPage = _spacingData.RowNumber*_spacingData.ColumnNumber;

            _onTouchSinceTimer = new Timer { Interval = refreshTimer * 1000 };
            _onTouchSinceTimer.Tick += OnTimerEvent;
        }

        protected virtual void OnTimerEvent(object sender, EventArgs e)
        {
            if (_nextButtonVisible)
            {
                ShowLastScreen();
            }
        }

        public void ShowMenu()
        {
            _showToolStrip?.Invoke(this, EventArgs.Empty);
        }

        private void ShowSearchBox()
        {
            if (_searchTextbox == null) return;
            Controls.Add(_searchTextbox);
            _searchTextbox.BringToFront();
        }

        protected void OnTextBoxGotFocus(object sender, EventArgs e)
        {
            AddKeyboard();
        }

        protected void OnTextBoxLostFocus(object sender, EventArgs e)
        {
            CloseKeyboard();
        }

        private void AddKeyboard()
        {
            try
            {
                if (_keyboard != null)
                {
                    FormUtil.SafeCall(Parent, () =>
                    {
                        _keyboard.Show();
                    });
                    return;
                }
                var conf = RunningFeatureConfig.GetInstance();
                var w = conf.GetConfiguration<int>(FeatureConfigEnum.GalleryKeyboardWidth);
                var h = conf.GetConfiguration<int>(FeatureConfigEnum.GalleryKeyboardHeight);

                _keyboard = new KeyboardControl
                {
                    Size = new Size(w, h),
                    BackColor = Color.White
                };
                _keyboard.OnEnterClicked += OnKeyboardEnterClicked;

                Controls.Add(_keyboard);

                _keyboard.BringToFront();
                _keyboard.Show();

                var x = Width / 2 - _keyboard.Width / 2;
                var y = Height - _keyboard.Height;

                _keyboard.Location = new Point(x, y);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void OnKeyboardEnterClicked(object sender, EventArgs e)
        {
            SearchClick();
            CloseKeyboard();
        }

        private void CloseKeyboard()
        {
            if (_keyboard != null)
            {
                FormUtil.SafeCall(Parent, () =>
                 {
                     _keyboard.Hide();

                     /*
                     Controls.Remove(_keyboard);
                     _keyboard.Dispose();
                     Invalidate();
                     _keyboard = null;
                     */
                 });
            }
        }

        /// <summary>
        /// Méthode pour gérer les événements souris.
        /// </summary>
        /// <param name="sender">L'appelant de l'événement.</param>
        /// <param name="e">Les paramètre de l'événement.</param>
        protected virtual void MouseDownHandler(object sender, MouseEventArgs e)
        {
            ResetTimer();

            //_show_tool_strip_timer.Start();

            if (InArea(e.X, e.Y, _previousArea))
            {
                if (_previousButtonVisible)
                {
                    long minId = 0;
                    if (_postsToShow.Count > 0)
                        minId = _postsToShow[0].Id;

                    _postsToShow = _database.GetPosts(minId - 1, false, _nbrPictureByPage, _user.EventId);
                    _postsToShow = _postsToShow.OrderBy(post => post.Id).ToList();
                    ShowPictures(_postsToShow);

                    ShowNavigationButtons();
                }
            }
            else if (InArea(e.X, e.Y, _nextArea))
            {
                if (_nextButtonVisible)
                {
                    long maxId = 0;
                    if (_postsToShow.Count > 0)
                        maxId = _postsToShow[_postsToShow.Count - 1].Id;

                    _postsToShow = _database.GetPosts(maxId + 1, true, _nbrPictureByPage, _user.EventId);
                    _postsToShow = _postsToShow.OrderBy(post => post.Id).ToList();
                    ShowPictures(_postsToShow);

                    ShowNavigationButtons();
                }
            }
            else if (InArea(e.X, e.Y, _searchArea))
            {
                var conf = RunningFeatureConfig.GetInstance();

                if (conf.GetConfiguration<bool>(FeatureConfigEnum.IsGalleryShowSearch))
                    SearchClick();
                else
                    ShowMenu();
            }

            CloseKeyboard();
        }

        /// <summary>
        /// Quand on clique pour lancer une recherche sur un répondant en particulier.
        /// </summary>
        protected void SearchClick()
        {
            var userName = _searchTextbox.Text;

            if (!string.IsNullOrEmpty(userName))
            {
                if (userName.Equals("app-menu"))
                {
                    ShowMenu();
                    return;
                }

                var postsToShow = _database.GetLastReceivedPosts(userName, _nbrPictureByPage, _user.EventId);
                if (postsToShow.Count <= 0) return;

                postsToShow = postsToShow.OrderBy(post => post.Id).ToList();
                var searchPictureControl = new SearchPicturesControl(userName, _user, Width, Height);
                searchPictureControl._onRemovePicturesControl += RemoveSearchPicturesControl;
                searchPictureControl._onShowPicturesControl += ShowSearchPicturesControl;
                searchPictureControl.InitPostsToShow(postsToShow);
                searchPictureControl.ShowPictures(postsToShow);
                searchPictureControl.ShowNavigationButtons();

                _onShowPicturesControl?.Invoke(searchPictureControl);
            }
        }

        private void RemoveSearchPicturesControl(SearchPicturesControl searchPictureControl)
        {
            ClearTextBox();

            _onRemovePicturesControl?.Invoke(searchPictureControl);
        }

        private void ShowSearchPicturesControl(SearchPicturesControl searchPictureControl)
        {
            _onShowPicturesControl?.Invoke(searchPictureControl);
        }

        /// <summary>
        /// Affiche les boutons pour naviguer dans les pages de posts.
        /// </summary>
        protected void ShowNavigationButtons()
        {
            if (_postsToShow.Count > 0)
            {
                _previousButtonVisible = _postsToShow[0].Id != _minId;
                _nextButtonVisible = _postsToShow[_postsToShow.Count - 1].Id != _maxId;
            }
            else
            {
                _previousButtonVisible = false;
                _nextButtonVisible = false;
            }
        }

        protected bool InArea(int x, int y, Rectangle area)
        {
            if (x <= area.Location.X || x >= area.Location.X + area.Width) return false;
            return y < area.Location.Y + area.Height && y > area.Location.Y;
        }

        private void OnPictureControlClick(object sender, EventArgs e)
        {
            SuspendTimer();
        }

        private void OnPictureControlDialogClosed(long postId)
        {
            if (!RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HidePostOnceClicked))
                return;

            var database = ServicesContainer.GetService<PrintStationDatabase>();
            database.HidePost(postId);

            ShowLastScreen();
        }
        

        // ReSharper disable once UnusedParameter.Global
        public void OnPictureControlClosed(object sender, EventArgs e)
        {
            ResumeTimer();
        }

        protected void SuspendTimer()
        {
            _onTouchSinceTimer.Stop();
        }

        protected void ResumeTimer()
        {
            _onTouchSinceTimer.Start();
        }

        /// <summary>
        /// Événement pour dessiner le contenu de la forme.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void PaintEvent(object sender, PaintEventArgs e)
        {

            if (_background != null)
            {
                e.Graphics.DrawImage(_background, 
                    new Rectangle(0, 0, _background.Width, _background.Height),
                    0, 0, _background.Width, _background.Height,
                    GraphicsUnit.Pixel);
            }

            if (_logo != null)
            {
                var x = Width / 2 - _logo.Width / 2;
                var y = Height - _logo.Height;
                e.Graphics.DrawImage(_logo, 
                    new Rectangle(x, y, _logo.Width, _logo.Height),
                    0, 0, _logo.Width, _logo.Height,
                    GraphicsUnit.Pixel);


                /* var a = Height/ _logo.Height;
                 var y = (a * (_logo.Height))-_logo.Height;
                 var x = 0;
                 e.Graphics.DrawImage(_logo,x,y);
                var y = Height -_logo.Height;
                e.Graphics.DrawImage(_logo,
                    new Rectangle(0, Height-200,Width,200),
                    0, 0, _logo.Width,_logo.Height,
                    GraphicsUnit.Pixel);*/


            }

            if (_searchText != null)
            {
                e.Graphics.DrawImage(_searchText, 
                    new Rectangle(_searchTextLocation, _searchText.Size),
                    0, 0, _searchText.Width, _searchText.Height,
                    GraphicsUnit.Pixel);
            }

            if (_previousButton != null)
            {
                if (_previousButtonVisible)
                {
                    e.Graphics.DrawImage(_previousButton, 
                        new Rectangle(_previousArea.Location, _previousButton.Size), 
                        0, 0, _previousButton.Width, _previousButton.Height,
                        GraphicsUnit.Pixel);
                }
            }

            if (_nextButton != null)
            {
                if (_nextButtonVisible)
                    e.Graphics.DrawImage(_nextButton,
                        new Rectangle(_nextArea.Location, _nextButton.Size),
                        0, 0, _nextButton.Width, _nextButton.Height,
                        GraphicsUnit.Pixel);
            }


            if (_searchButton != null)
            {
                e.Graphics.DrawImage(_searchButton,
                    new Rectangle(_searchArea.Location, _searchButton.Size),
                    0, 0, _searchButton.Width, _searchButton.Height,
                    GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Détruit les éléments visuels de ce contrôle immédiatement.
        /// </summary>
        public void DisposeUiElements()
        {
            _logo?.Dispose();

            _searchText?.Dispose();

            _previousButton?.Dispose();

            _nextButton?.Dispose();

            _searchButton?.Dispose();
        }

        private void ClearTextBox()
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new ClearTextBoxDelegate(ClearTextBox));
                return;
            }

            _searchTextbox.Text = "";
        }

        private int GetPicturesAndSideAvailableWidth()
        {
            var panelWidth = RunningFeatureConfig.GetInstance().GetConfiguration<int>(FeatureConfigEnum.GalleryPanelWidth);

            return panelWidth > 0 ? panelWidth : Width;
        }

        private int GetPicturesAndSideAvailableHeight(int bottomPanelHeight)
        {
            var panelHeight = RunningFeatureConfig.GetInstance().GetConfiguration<int>(FeatureConfigEnum.GalleryPanelHeight);
            
            return panelHeight > 0 ? panelHeight : Height - bottomPanelHeight;
        }
        
        /// <summary>
        /// Calcule toutes les métriques d'affichage des éléments de la gallerie.
        /// </summary>
        protected void CalculateSpacingInfo()
        {
            if (_spacingData != null) return;

            var config = RunningFeatureConfig.GetInstance();
            var tmp = new GallerySpacingData();

            var showSearch = config.GetConfiguration<bool>(FeatureConfigEnum.IsGalleryShowSearch);
            var ratioPadding = config.GetConfiguration<float>(FeatureConfigEnum.GalleryImagePaddingRatio);
            var sideWidth = config.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth);
            var searchHeight = config.GetConfiguration<int>(FeatureConfigEnum.GallerySearchPanelHeight);
            var panelTopStart = config.GetConfiguration<int>(FeatureConfigEnum.GalleryPanelTopStart);
            var panelLeftStart = config.GetConfiguration<int>(FeatureConfigEnum.GalleryPanelLeftStart);
            var configBottomPanelHeight = config.GetConfiguration<int>(FeatureConfigEnum.GalleryBottomPanelHeight);

            if (_logo?.Height > configBottomPanelHeight)
            {
                configBottomPanelHeight = _logo.Height;
            }

            var availableWidth = GetPicturesAndSideAvailableWidth() - (2 * sideWidth);
            var availableHeight = GetPicturesAndSideAvailableHeight(configBottomPanelHeight);
            var imageRatio = config.GetConfiguration<float>(FeatureConfigEnum.GalleryImageSizeRatio);

            if (showSearch)
                availableHeight -= searchHeight;

            var nbrRows = config.GetConfiguration<int>(FeatureConfigEnum.GalleryRowAmount);
            var nbrPictures = config.GetConfiguration<int>(FeatureConfigEnum.GalleryPicturePerRow);
            
            int candidateWidth;
            int candidateHeight;
            int candidateRowPadding;
            int candidateColumnPadding;
            int candidateRowNumber;
            int candidateColumnNumber;

            // Vérifie que l'on a au moins une valeur valide, et non deux valeurs non identifiées.
            if (nbrRows < 1 && nbrPictures < 1)
            {
                nbrRows = 4;
            }

            var success = MathUtil.CalculateSizeCandidates(ratioPadding, availableWidth, availableHeight, imageRatio, nbrRows, nbrPictures,
                out candidateWidth, out candidateHeight, out candidateRowPadding, out candidateColumnPadding,
                out candidateRowNumber, out candidateColumnNumber);

            if (success)
            {
                // Nombre final de lignes/colonnes
                tmp.RowNumber = candidateRowNumber;
                tmp.ColumnNumber = candidateColumnNumber;

                // Padding
                tmp.RowPadding = candidateRowPadding;
                tmp.ColumnPadding = candidateColumnPadding;

                // La taille des images finale.
                tmp.ImageHeight = candidateHeight;
                tmp.ImageWidth = candidateWidth;
                // Points de départ
                tmp.LeftStart = sideWidth + tmp.RowPadding + panelLeftStart;
                tmp.TopStart = tmp.ColumnPadding + panelTopStart;
                if (showSearch)
                {
                    tmp.TopStart += searchHeight;
                }

                _spacingData = tmp;
            }
            else
            {
                log.Fatal("Could not calculate spacing data.");
            }
        }
        
        /// <summary>
        /// Affiche les Posts donnés en paramètre en remplaçant ceux qui existent déjà.
        /// </summary>
        /// <param name="postsToShow">La liste des Posts à afficher.</param>
        protected void ShowPictures(List<Post> postsToShow)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new ShowPicturesDelegate(ShowPictures), postsToShow);
                return;
            }

            CalculateSpacingInfo();

            Controls.Clear();

            if (_keyboard != null)
            {
                Controls.Add(_keyboard);
            }

            var pictureNumberMax = _spacingData.ColumnNumber;
            var rowNumberMax = _spacingData.RowNumber;
            var pictureNumber = 0;
            var rowNumber = 0;
            foreach (var post in postsToShow)
            {
                if (rowNumber >= rowNumberMax)
                {
                    log.Warn("More posts than necessary were sent to show. Nbr sent = {0}, Maximum was {1}x{2}"
                        , postsToShow.Count, pictureNumberMax, rowNumberMax);
                    break; // On sort de force de la boucle si on à atteint le nombre maximal d'images à afficher.
                }

                try
                {

                    var pictureFileName = post.PictureUrl;

                    if (!File.Exists(pictureFileName))
                    {
                        pictureFileName =
                        DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) +
                        StringUtil.GetFileName(post.PictureUrl);
                    }

                    var profilePictureFileName =
                        DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.user_profile) +
                        StringUtil.GetFileName(post.UserPictureUrl);

                    if (File.Exists(pictureFileName))
                    {
                        var pControl = new PictureControl(_user,
                            new PicturePostInfo(pictureFileName, post.SocialAppUsername, profilePictureFileName, _user.Id, post.ServerId, post.Id, post.RepondantNoFromServer),
                            _spacingData.ImageWidth, _spacingData.ImageHeight);
                        pControl.OnDialogClosed += OnPictureControlDialogClosed;
                        pControl.OnPictureClick += OnPictureControlClick;
                        pControl.Location = new Point(_spacingData.LeftStart + (pictureNumber * (_spacingData.RowPadding + _spacingData.ImageWidth)),
                            _spacingData.TopStart + (rowNumber * (_spacingData.ColumnPadding + _spacingData.ImageHeight)));
                        Controls.Add(pControl);
                    }
                    else
                    {
                        log.Warn("Inexistant file for post. post id : {0}, file : {1}", post.Id, pictureFileName);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Error during loop for Posts.");
                }
                pictureNumber++;

                if (pictureNumber < pictureNumberMax) continue;
                pictureNumber = 0;
                rowNumber++;
            }

            ShowSearchBox();

            Invalidate();
        }

        /// <summary>
        /// Délégate pour appeller la méthode d'affichage sur le fil UI.
        /// </summary>
        /// <param name="postsToShow">La liste de post à afficher.</param>
        internal delegate void ShowPicturesDelegate(List<Post> postsToShow);
    }
}