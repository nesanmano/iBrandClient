using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using Vlc.DotNet.Core;
using Vlc.DotNet.Forms;

namespace NomadLogic.ShowPicturesOnTV.DisplayControllers
{
    /// <summary>
    /// Interface commune pour chaque type de contrôleur.
    /// </summary>
    abstract class DisplayControllerAbstract : IDisplayController, IDisposable
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        // ReSharper disable once StaticMemberInGenericType
        private static readonly TimeSpan BASE_TIME_WAIT = new TimeSpan(0, 0, 0, 0, 100);
        protected static readonly string[] MOVIES_EXTENSIONS = { ".mp4", ".mov", ".flv" };
        protected static readonly string[] IMAGES_EXTENSIONS = { ".jpg", ".png", ".JPG", ".PNG" };

        protected readonly Random _random;
        /// <summary>
        /// Indique combien d'éléments existaient quand on a remplit la liste d'Affichage la dernière fois.
        /// </summary>
        protected int _nbFilesToShowWhenFull;
        protected int _itemShownIndex;
        protected List<FileInfo> _filesToShow = new List<FileInfo>();
        protected int _intermissionTimer;

        protected Size _elementSize;
        protected Point _elementLocation;

        protected Size _intermissionSize;
        protected readonly Point _intermissionLocation = new Point(0, 0);

        private bool _isIntermissionFileSelected;
        private string _lastNewFile;
        private VlcControl _vlcControl;
        private volatile bool _isPlaying;
        private Bitmap _imageDisplayed;
        private PictureBox _pictureBox;
        private volatile bool _stopRequested;

        protected DisplayControllerAbstract()
        {
            _random = new Random();
        }

        public ConfigurationShowPictureOnTv DisplayConfiguration { get; set; }

        /// <summary>
        /// Le 'Panel' dans lequel le contrôleur peut insérer des éléments au besoin.
        /// </summary>
        public Panel DisplayPanel { get; set; }

        /// <summary>
        /// Le répertoire où chercher les nouveaux items d'affichages.
        /// </summary>
        public string MainFolder { get; set; }

        /// <summary>
        /// Le répertoire ou mettre les items affichés au moins une fois.
        /// </summary>
        public string ShownFolder { get; set; }


        public MouseEventHandler OnMouseEvent { get; set; }

        /// <summary>
        /// Action à exécuter quand la boucle principale est arrêter. C'est le "callback" d'arrêt.
        /// </summary>
        public event Action OnCompleteStop;

        /// <summary>
        /// Obtient les extensions de fichiers visées par l'instance du contrôleur.
        /// </summary>
        /// <returns></returns>
        protected abstract string[] GetFileExtensions();

        /// <summary>
        /// Construit la liste des prochains éléments de façon ordonnée, si elle était vide. Puis retourne le premier élément comme
        /// élément à afficher. Cet élément est ensuite retiré de la liste.
        /// </summary>
        /// <returns>Le prochain élément à afficher. Null, s'il n'y a aucun élément à afficher (la liste construite est vide).</returns>
        private string GetNextOrderedItem()
        {
            var showDir = new DirectoryInfo(ShownFolder);
            if (!_filesToShow.Any())
            {
                switch (DisplayConfiguration.OrderParam)
                {
                    case ConfigurationShowPictureOnTvOrderParamEnum.Date:
                        switch (DisplayConfiguration.DateRestriction)
                        {
                            case ConfigurationShowPictureOnTvDateRestrictionEnum.All:
                                _filesToShow = new List<FileInfo>(FileUtil.GetFilesByExtensionsOrderByDate(showDir, 0, GetFileExtensions()));
                                break;
                            case ConfigurationShowPictureOnTvDateRestrictionEnum.Today:
                                _filesToShow = new List<FileInfo>(FileUtil.GetFilesByExtensionsTodayOrderByDate(showDir, GetFileExtensions()));
                                break;
                        }
                        break;
                    case ConfigurationShowPictureOnTvOrderParamEnum.Name:
                        switch (DisplayConfiguration.DateRestriction)
                        {
                            case ConfigurationShowPictureOnTvDateRestrictionEnum.All:
                                _filesToShow = new List<FileInfo>(FileUtil.GetFilesByExtensionsOrderByName(showDir, GetFileExtensions()));
                                break;
                            case ConfigurationShowPictureOnTvDateRestrictionEnum.Today:
                                _filesToShow = new List<FileInfo>(FileUtil.GetFilesByExtensionsTodayOrderByName(showDir, GetFileExtensions()));
                                break;
                        }
                        break;
                }
            }

            if (!_filesToShow.Any()) return null;

            var fileToDisplay = ShownFolder + Path.DirectorySeparatorChar + _filesToShow.ElementAt(0);

            _filesToShow.RemoveAt(0);

            return fileToDisplay;
        }

        /// <summary>
        /// Construit la liste des prochains éléments si elle était vide. Puis retourne un élément au hasard dans cette liste comme
        /// élément à afficher. Cet élément est ensuite retiré de la liste.
        /// Si l'option de recréer la liste à sa moitié est active, on va détruire la liste une fois la moitié consommée par l'affichage.
        /// </summary>
        /// <returns>Le prochain élément à afficher. Null, s'il n'y a aucun élément à afficher (la liste construite est vide).</returns>
        private string GetNextRandomItem()
        {
            var showDir = new DirectoryInfo(ShownFolder);

            if (!_filesToShow.Any() ||
                (DisplayConfiguration.ResetList == ConfigurationShowPictureOnTvResetEnum.HalfShown && _filesToShow.Count <= _nbFilesToShowWhenFull / 2))
            {
                if (DisplayConfiguration.DateRestriction == ConfigurationShowPictureOnTvDateRestrictionEnum.All)
                    _filesToShow = new List<FileInfo>(FileUtil.GetFilesByExtensionsOrderByDate(showDir, 0, GetFileExtensions()));
                else if (DisplayConfiguration.DateRestriction == ConfigurationShowPictureOnTvDateRestrictionEnum.Today)
                    _filesToShow = new List<FileInfo>(FileUtil.GetFilesByExtensionsTodayOrderByDate(showDir, GetFileExtensions()));

                if (_filesToShow.Any())
                {
                    _nbFilesToShowWhenFull = _filesToShow.Count;
                }
                else
                {
                    _nbFilesToShowWhenFull = 0;
                    _filesToShow.Clear();
                }
            }

            if (!_filesToShow.Any()) return null;

            var index = _random.Next(_filesToShow.Count);
            var fileToDisplay = ShownFolder + Path.DirectorySeparatorChar + _filesToShow.ElementAt(index);

            _filesToShow.RemoveAt(index);

            return fileToDisplay;
        }

        /// <summary>
        /// Obtient l'objet d'affichage. On fait une recherche selon s'il existe de nouveaux items dans le répertoire par défaut.
        /// Si c'est le cas, on affiche ces derniers en priorité sur la liste d'affichage déjà en cours.
        /// Sinon, on fait une recherche dans la liste d'Affichage en cours. Si elle est vide, ou si certaines circonstances se présentent,
        /// on en refait une nouvelle liste.
        /// </summary>
        /// <returns>Une instance de l'objet à afficher. NULL sinon.</returns>
        [SuppressMessage("ReSharper", "RedundantCaseLabel")]
        [SuppressMessage("ReSharper", "RedundantEmptyDefaultSwitchBranch")]
        protected virtual DisplayItemInfo GetItemToDisplay()
        {
            var dir = new DirectoryInfo(MainFolder);
            var files = FileUtil.GetFilesByExtensionsOrderByDate(dir, DisplayConfiguration.Delay, GetFileExtensions()) ?? new FileInfo[] { };

            DisplayItemInfo itemToDisplay = null;
            _isIntermissionFileSelected = false;

            if (DisplayConfiguration.IntermissionFile != null && DisplayConfiguration.StandbyMethod != ConfigurationShowPictureOnTvStandbyEnum.None)
            {
                try
                {
                    if (DisplayConfiguration.IntermissionNumber < 1)
                    {
                        log.Warn("Intermission number was invalid. Changed to 5 from {0}", DisplayConfiguration.IntermissionNumber);
                        DisplayConfiguration.IntermissionNumber = 5;
                    }

                    if (_intermissionTimer >= DisplayConfiguration.IntermissionNumber)
                    {
                        itemToDisplay = CreateDisplayItemInfo(DisplayConfiguration.IntermissionFile, _intermissionSize, _intermissionLocation, true);
                        _isIntermissionFileSelected = true;
                        _intermissionTimer = 0;
                        return itemToDisplay;
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not instanciate intermission file: {0}", DisplayConfiguration.IntermissionFile);
                    DisplayConfiguration.IntermissionFile = null;
                    return itemToDisplay;
                }
            }
            var fileInfos = files.ToArray();
            string fileToDisplay = null;
            if (true)
            {
                if (fileInfos.Any())
                {
                    // On cherche le premier fichier valide le plus récent.
                    foreach (var f in fileInfos)
                    {
                        fileToDisplay = ShownFolder + Path.DirectorySeparatorChar + StringUtil.GetFileName(f.FullName);
                        try
                        {
                            var error = false;
                            if (File.Exists(fileToDisplay))
                            {
                                try
                                {
                                    File.Delete(fileToDisplay);
                                }
                                catch (Exception)
                                {
                                    // On essaie de détruire le fichier plus tard.
                                    error = true;
                                }
                            }
                            if (!error)
                            {
                                File.Move(f.FullName, fileToDisplay);
                            }
                        }
                        finally
                        {
                            try
                            {
                                File.Delete(f.FullName);
                            }
                            catch (Exception)
                            {
                                /* Die nice */
                            }
                        }
                        log.Debug("Item moved: {0}", fileToDisplay);

                    if (DisplayConfiguration.StandbyMethod == ConfigurationShowPictureOnTvStandbyEnum.Randomize &&
                        DisplayConfiguration.ResetList == ConfigurationShowPictureOnTvResetEnum.NewAdded)
                        {
                            log.Debug("Next file to display: {0}" + fileToDisplay);
                            _filesToShow.Add(new FileInfo(StringUtil.GetFileName(fileToDisplay)));
                        }

                        break;
                    }
                }
            }

            // Si null, on arrive en standby
            if (fileToDisplay == null)
            {
                switch (DisplayConfiguration.StandbyMethod)
                {
                    case ConfigurationShowPictureOnTvStandbyEnum.Orderedlist:
                        log.Debug("Showing next Ordered already shown image.");
                        fileToDisplay = GetNextOrderedItem();
                        break;
                    case ConfigurationShowPictureOnTvStandbyEnum.Randomize:
                        log.Debug("Showing next Randomize already shown image.");
                        fileToDisplay = GetNextRandomItem();
                        break;
                    case ConfigurationShowPictureOnTvStandbyEnum.AlwaysLast:
                        fileToDisplay = _lastNewFile;
                        break;
                    case ConfigurationShowPictureOnTvStandbyEnum.None:
                        fileToDisplay = DisplayConfiguration.IntermissionFile;
                        _isIntermissionFileSelected = true;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                _lastNewFile = fileToDisplay;
            }

            if (string.IsNullOrEmpty(fileToDisplay))
            {
                return null;
            }

            log.Trace("Displaying following image: {0}", fileToDisplay);

            // Test de l'image
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                itemToDisplay = CreateDisplayItemInfo(fileToDisplay, _elementSize, _elementLocation, false);
            }
            catch (Exception e)
            {
                log.Error(e, "Could not make the following file into an image. Discarding it: {0}", fileToDisplay);
                try
                {
                    var tmp = DirectoryUtil.GetLocalApplicationDir() + "error" + Path.DirectorySeparatorChar + StringUtil.GetFileName(fileToDisplay);
                    File.Move(fileToDisplay, tmp);
                }
                catch (Exception)
                {
                    // die nice
                }
            }
            if (DisplayConfiguration.IntermissionFile != null && DisplayConfiguration.StandbyMethod != ConfigurationShowPictureOnTvStandbyEnum.None) _intermissionTimer++;

            return itemToDisplay;
        }

        /// <summary>
        /// Créer l'instance de définition d'item à définir. Peut sortir null si inadéquat.
        /// </summary>
        /// <param name="path">Le chemin à analyser.</param>
        /// <param name="location">La location à placer l'élément.</param>
        /// <param name="checkRelativePath">Indique si on teste le chemin pour trouver le fichier dans le répertoire usager de l'application, si on ne peut
        /// trouver l'élément via chemin absolu.</param>
        /// <param name="newSize">La taille de l'élément.</param>
        /// <returns>Une instance de définition de l'item à afficher, si null, l'élément était inadéquat.</returns>
        protected DisplayItemInfo CreateDisplayItemInfo(string path, Size newSize, Point location, bool checkRelativePath)
        {
            var tmp = new DisplayItemInfo
            {
                Size = newSize,
                Location = location
            };
            
            if (string.IsNullOrWhiteSpace(path)) return null;

            // On regarde si le fichier existe
            if (!File.Exists(path))
            {
                var relativePath = DirectoryUtil.GetLocalApplicationDir() + path;
                if (!File.Exists(relativePath))
                {
                    log.Warn("File does not exists: " + path);
                    return null;
                }

                tmp.Path = relativePath;
            }
            else
            {
                tmp.Path = path;
            }

            // Si image
            if (IMAGES_EXTENSIONS.Any(ext => path.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)))
            {
                tmp.DisplayType = DisplayItemTypeEnum.Image;
            } 
            // Si video
            else if (MOVIES_EXTENSIONS.Any(ext => path.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)))
            {
                tmp.DisplayType = DisplayItemTypeEnum.Video;
            }
            else
            {
                tmp.DisplayType = DisplayItemTypeEnum.Unknown; // Probablement un bundle.
            }

            return tmp;
        }

        public virtual void Init()
        {
            _intermissionSize = DisplayPanel.Size;

            if (DisplayConfiguration.ItemRectangle == null)
            {
                // Par défaut, plein écran
                _elementSize = DisplayPanel.Size;
                _elementLocation = new Point(0,0);
            }
            else
            {
                _elementSize = DisplayConfiguration.ItemRectangle.Value.Size;
                _elementLocation = DisplayConfiguration.ItemRectangle.Value.Location;
            }
            
            if (_pictureBox == null)
            {
                _pictureBox = new PictureBox
                {
                    SizeMode = DisplayConfiguration.ItemLayout,
                    Location = _elementLocation,
                    Size = _elementSize,
                    BackColor = Color.Transparent
                };

                DisplayPanel.Controls.Add(_pictureBox);
                _pictureBox.MouseUp += OnMouseEvent;
                _pictureBox.Hide();
            }

            if (_vlcControl != null) return;

            _vlcControl = new VlcControl
            {
                Location = _elementLocation,
                Size = _elementSize,
                VlcLibDirectory = null,
                BackColor = Color.Black, // Impossible de mettre transparent pour des raisons d'accélération graphique et de décodage vidéo.
                Name = "myVlcControl"
            };

            _vlcControl.VlcLibDirectoryNeeded += GetLibDirectory;
            DisplayPanel.Controls.Add(_vlcControl);

            var panelMouseEvents = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            panelMouseEvents.MouseUp += OnMouseEvent;
            _vlcControl.Controls.Add(panelMouseEvents);
            _vlcControl.Controls.Add(panelMouseEvents);

            _vlcControl.EndInit();
            _vlcControl.BringToFront();
            _vlcControl.EndReached += OnVideoFinished;
            _vlcControl.Hide();

            DisplayPanel.MouseUp += OnMouseEvent;
        }

        /// <summary>
        /// Cette effectue l'opéation sur la Forme pour afficher l'item choisit.
        /// </summary>
        private void DisplayItem(DisplayItemInfo itemToDisplay)
        {
            if (itemToDisplay == null) return;
            switch (itemToDisplay.DisplayType)
            {
                case DisplayItemTypeEnum.Image:

                    FormUtil.SafeCall(DisplayPanel, () => { 
                        var bmp = FileUtil.LoadBitmap(itemToDisplay.Path);
                        
                        Bitmap toDispose = null;
    
                        try
                        {
                            if (_imageDisplayed != null)
                            {
                                toDispose = _imageDisplayed;
                            }
    
                            _vlcControl.Hide();

                            if (_pictureBox.Size != itemToDisplay.Size ||
                                _pictureBox.Location != itemToDisplay.Location)
                            {
                                _pictureBox.Size = itemToDisplay.Size;
                                _pictureBox.Location = itemToDisplay.Location;
                            }

                            _pictureBox.Show();
    
                            _pictureBox.Image = bmp;
                            _imageDisplayed = bmp;
    
    
                            DisplayPanel.Invoke(new Action(() => { DisplayPanel.Refresh(); }));
                        }
                        catch (Exception e)
                        {
                            log.Error(e, "Error during image display initialization.");
                        }
                        finally
                        {
                            toDispose?.Dispose();
                        }
                    });
                    break;
                case DisplayItemTypeEnum.Video:
                    FormUtil.SafeCall(DisplayPanel, () =>
                    {
                        var fi = new FileInfo(itemToDisplay.Path);

                        if (!fi.Exists) return;

                        try
                        {
                            _pictureBox.Hide();

                            if (_vlcControl.Size != itemToDisplay.Size ||
                                _vlcControl.Location != itemToDisplay.Location)
                            {
                                _vlcControl.Size = itemToDisplay.Size;
                                _vlcControl.Location = itemToDisplay.Location;
                            }

                            _vlcControl.Show();

                            _vlcControl.Stop();
                            _isPlaying = true;
                            _vlcControl.Play(fi);
                            log.Trace("Playing video file: " + fi.Name);
                        }
                        catch (Exception e)
                        {
                            log.Error(e, "Error during video display initialization.");
                        }
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Boucle principale pour la vérification des fichiers. Si le jeton d'arrêt est invalide, cette méthode sort
        /// immédiatement.
        /// </summary>
        /// <param name="ct">Le jeton de cancellation.</param>
        public void LookForDisplayItems(CancellationToken ct)
        {
            if (!ct.CanBeCanceled) return;

            log.Debug("Main loop initialized.");
            while (!ct.IsCancellationRequested && !DisplayPanel.IsDisposed && !_stopRequested)
            {
                try
                {
                    var item = GetItemToDisplay();
                    DisplayItem(item);
                }
                catch (Exception e)
                {
                    log.Error(e);
                    break;
                }

                if (DisplayConfiguration.CheckTimer < BASE_TIME_WAIT.Milliseconds)
                {
                    DisplayConfiguration.CheckTimer = BASE_TIME_WAIT.Milliseconds;
                }
                var nextTimeInc = new TimeSpan(DisplayConfiguration.CheckTimer);

                // Situation particulière: Avec Aucun standby et Fichier d'intermission.
                if (!(DisplayConfiguration.IntermissionFile != null && DisplayConfiguration.StandbyMethod == ConfigurationShowPictureOnTvStandbyEnum.None) ||
                    !_isIntermissionFileSelected)
                {
                    nextTimeInc = TimeSpan.FromMilliseconds(DisplayConfiguration.DisplayTime);
                }

                var nextLoop = DateTime.Now + nextTimeInc;
                while (!_stopRequested && !ct.IsCancellationRequested && (DateTime.Now < nextLoop || !CanShowNextElement()))
                {
                    try
                    {
                        Thread.Sleep(500);
                    }
                    catch (Exception)
                    {
                        // Die nice
                    }
                }
            }
            log.Debug("Main loop Stopped.");

            Dispose();

            OnCompleteStop?.Invoke();
        }

        public void Stop()
        {
            _stopRequested = true;
        }

        /// <summary>
        /// Cette méthode indique s'il est possible de passer au prochain élément une fois que le temps minimal est terminé.
        /// Dans certaines situations (comme un traitement ou un vidéo par exemple), cette méthode peut retourner faux, selon
        /// l'implémentation.
        /// </summary>
        /// <returns>L'item à terminer sa présentation.</returns>
        private bool CanShowNextElement()
        {
            return !_isPlaying;
        }

        /// <summary>
        /// Détruit l'instance, pour être prêt à être "Garbage Collected".
        /// </summary>
        public virtual void Dispose()
        {
            _vlcControl?.Stop();
            _pictureBox?.Image?.Dispose();
        }
        
       /// Mettre dans une méthode statique dans un utilitaire commun. 
        private static void GetLibDirectory(object sender, VlcLibDirectoryNeededEventArgs e)
       {
           var result =
               AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).ProcessorArchitecture ==
               ProcessorArchitecture.X86
                   ? @"x86\"
                   : @"x64\";

            var tmp = new DirectoryInfo(DirectoryUtil.GetLocalNomadLogicDir()+ @"Nomad\lib\vlc_dot_net\" + result) ;
            
            if (!tmp.Exists)
            {
                throw new SystemException("Could not find VLC libs directory: " + tmp.FullName);
            }

            if (e != null) e.VlcLibDirectory = tmp;
        }

        private void OnVideoFinished(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            _isPlaying = false;
        }
    }
}
