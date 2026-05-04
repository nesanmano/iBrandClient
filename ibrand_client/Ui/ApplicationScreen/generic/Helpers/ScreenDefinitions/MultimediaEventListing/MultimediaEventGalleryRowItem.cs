using System;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;
using NomadLogic.Multimedia.Cli.ImageMagickCLI.JobScheduler;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    public class MultimediaEventGalleryRowItem
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private string _fixedMainData;
        private Bitmap _fixedThumbnailImage;
        private Bitmap _loadingThumbnailImage;
        private Bitmap _currentBitmap;
        private readonly object _lock = new object();
        private bool _selected;
        private int _index;
        private bool _cannotBeSelected;
        private DirectoryInfo _workDir;
        private readonly string _creationDate;
        private readonly ImageMagickJobSchedulerService _imageService;
        private IThumbnailOverlaySelector _thumbOverlaySelector;

        public string OriginalDynamicValue { get; set; }
        public string OriginalDynamicGroupDataKey { get; set; }
        public string OriginalDynamicGroupCreationDate { get; set; }
        
        /// <summary>
        /// Indique le type de fichier que représente cette valeur.
        /// </summary>
        public MultimediaEventGalleryRowItemFileType OriginalFileType { get; }

        public MultimediaEventGalleryRowItem(string creationDateString, MultimediaEventGalleryRowItemFileType mediaType = MultimediaEventGalleryRowItemFileType.Image)
        {
            OriginalFileType = mediaType;
            _creationDate = creationDateString;

            _imageService = ServicesContainer.GetService<ImageMagickJobSchedulerService>();
            if (_imageService == null)
            {
                throw new ArgumentException("No ImageMagickJobScheduler is available.");
            }
        }

        private void PropertyUpdate<T>(ref T prop, T newVal)
        {
            if (Equals(prop, newVal))
            {
                return;
            }

            prop = newVal;
            lock (_lock)
            {
                _currentBitmap = null;
                RaiseEvent(ChangeDetected);
            }
        }
        
        /// <summary>
        /// Répertoire de travail. Ce répertoire sera utilisé pour créer les thumbnails transitifs et/ou les divers fichiers requis pour ses transformations.
        /// </summary>
        public DirectoryInfo WorkDirectory
        {
            get => _workDir;
            set => PropertyUpdate(ref _workDir, value);
        }
        
        /// <summary>
        /// L'ordre d'apparition dans l'affichage dans la gallerie.
        /// Également utilisé à l'Interne pour indexer.
        /// </summary>
        public int LayoutIndex
        {
            get => _index;
            set => PropertyUpdate(ref _index, value);
        }

        /// <summary>
        /// Indique que cet élément ne doit pas réagir aux "inputs" pour changer son état ou visionner en présentation unique.
        /// </summary>
        public bool CannotBeSelected
        {
            get => _cannotBeSelected;
            set => PropertyUpdate(ref _cannotBeSelected, value);
        }

        /// <summary>
        /// Indique que l'élément à été choisi pour la prochaine étape.
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set => PropertyUpdate(ref _selected, value);
        }

        /// <summary>
        /// Référence directe à une image pour être un thumbnail de cet élément. Cette image sera copiée (et donc l'image originale sera préservée).
        /// Si cette valeur existe, cette image sera TOUJOURS choisie idépendemment de la valeur dynamique. Utile, par exemple, dans le cas de vidéos.
        /// </summary>
        public Bitmap FixedThumbnailImage
        {
            get => _fixedThumbnailImage;
            set => PropertyUpdate(ref _fixedThumbnailImage, value);
        }

        /// <summary>
        /// Référence directe à une image temporaire utilisée quand on est en train de travailler sur une image.
        /// Si on ne précise pas une image fixe, on crééra un rectangle blanc à la place.
        /// </summary>
        public Bitmap LoadingFixedThumbnail
        {
            get => _loadingThumbnailImage;
            set => PropertyUpdate(ref _loadingThumbnailImage, value);
        }

        /// <summary>
        ///Référence directe à l'élément principal. Si c'est une image, on se fit à cette valeur pour être un chemin absolu vers un fichier d'image.
        /// Sinon, le format est laissé libre d'interprétation (dans le cas d'un vidéo, peut représenter un ID numérique externe, par exemple).
        /// </summary>
        public string FixedMainData
        {
            get => _fixedMainData;
            set => PropertyUpdate(ref _fixedMainData, value);
        }
        /// <summary>
        /// Empêche l'activation des événements. À utiliser quand on doit faire des mises à jour sans notifier tous les événements normaux.
        /// </summary>
        protected internal bool SuspendEvents { get; set; }

        /// <summary>
        /// Dans l'éventualité où on détecte un changement à l'interne, on peut être notifié via cet événement.
        /// Cela veut souvent dire qu'il faut re-créer l'image de thumbnail.
        /// </summary>
        public event EventHandler ChangeDetected;

        protected void RaiseEvent(EventHandler eh)
        {
            if (eh == null)
            {
                log.Trace("No event handler passed.");
                return;
            }
            if (SuspendEvents)
            {
                log.Trace("Event raised, but invocation is suspended.");
                return;
            }
            eh.InvokeFireAndForget(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Créer une image de thumbnail RAPIDEMENT pour représenter cet item. Si ce fichier n'exite pas déjà, il sera créer de façon asynchrone et on notifiera via
        /// un événement. Dans ce cas de figure, la méthode va fournir une image temporaire.
        /// </summary>
        public Bitmap GenerateThumbnailImage(Size wantedSize, [NotNull] IThumbnailOverlaySelector selector)
        {
            _thumbOverlaySelector = selector;

            lock (_lock)
            {
                if (_currentBitmap != null)
                {
                    log.Trace("Bitmap already found. Returning this data.");
                    return _currentBitmap;
                }

                if (_fixedThumbnailImage != null)
                {
                    log.Trace("Fixed thumbnail image found. Assigning.");
                    _currentBitmap = new Bitmap(_fixedThumbnailImage);
                }
                else
                {
                    var filePathTocheck = FixedMainData;

                    // Pas image
                    if (OriginalFileType != MultimediaEventGalleryRowItemFileType.Image)
                    {
                        // Impossible de créer un thumbnail. On utilise l'image par défaut.

                        if (!(_workDir?.Exists ?? false) || string.IsNullOrWhiteSpace(_thumbOverlaySelector?.ThumbnailDefaultFileName))
                        {
                            log.Warn("Invalid work directory and thumbnail file name. Row Item is unable to create a thumbnail. Review configuration.");
                            return null;
                        }

                        filePathTocheck = Path.Combine(_workDir.FullName, _thumbOverlaySelector?.ThumbnailDefaultFileName);
                    }

                    if (string.IsNullOrWhiteSpace(filePathTocheck) || !File.Exists(filePathTocheck))
                    {
                        log.Error("Main data string does not reference a valid file.");
                        return null;
                    }

                    var originalFile = new FileInfo(filePathTocheck);
                    var thbFile = ImageMagickCliUtil.CheckForThumbnailFile(originalFile, WorkDirectory, wantedSize);
                    if (thbFile == null)
                    {
                        log.Trace("Thumbnail file does not exists. Adding job to create it. Original File={0}", originalFile);
                        // Demande de génération de bitmap.
                        _imageService.AddNewJob(new ImageMagickJobCreateThumbnail(
                            job => ThumbnailFinishedCallback((ImageMagickJobCreateThumbnail)job),
                            originalFile, WorkDirectory, wantedSize, true, Color.White));

                        // Assignation d'un bitmap temporaire.
                        if (_loadingThumbnailImage == null)
                        {
                            log.Trace("No loading image found. Using White square.");
                            _currentBitmap = new Bitmap(wantedSize.Width, wantedSize.Height);
                            using (var g = Graphics.FromImage(_currentBitmap))
                            {
                                g.FillRectangle(Brushes.White, 0, 0, wantedSize.Width, wantedSize.Height);
                            }
                        }
                        else
                        {
                            log.Trace("Using placeholder loading image while job is processed.");
                            _currentBitmap = new Bitmap(_loadingThumbnailImage);
                        }
                    }
                    else
                    {
                        log.Trace("Already existing thumbnail image found. Using this as source. File={0}", thbFile);
                        _currentBitmap = FileUtil.LoadBitmap(thbFile.FullName);
                    }
                }

                AddOverlayToBitmap(_currentBitmap);

                return _currentBitmap;
            }
        }

        private void AddOverlayToBitmap(Image currentBitmap)
        {
            if (_thumbOverlaySelector == null) return;

            Image overlay = null;
            
            if (CannotBeSelected)
            {
                overlay = _thumbOverlaySelector.GetThumbnailOverlayImage(ThumbnailOverlaySelectorState.CannotBeSelected);
                if (overlay != null)
                {
                    log.Debug("Date='{0}',Index={1}, Individual selection image state changed to: unselectable", _creationDate, LayoutIndex);
                }
            }
            else if (Selected)
            {
                overlay = _thumbOverlaySelector.GetThumbnailOverlayImage(ThumbnailOverlaySelectorState.Selected);
                if (overlay != null)
                {
                    log.Debug("Date='{0}',Index={1}, Individual selection image state changed to: selected", _creationDate, LayoutIndex);
                }
            }

            if (overlay == null)
            {
                return;
            }

            using (var g = Graphics.FromImage(currentBitmap))
            {
                var rect = new RectangleF(0, 0, currentBitmap.Width, currentBitmap.Height);
                g.DrawImage(overlay, rect);
            }

            log.Trace("Image modified by overlay.");
        }

        private void ThumbnailFinishedCallback(ImageMagickJobDefinition jobRef)
        {
            log.Trace("Job Callback invoked.");
            if (!(jobRef?.Result is Bitmap bmp))
            {
                log.Warn("No valid result in callback. This should be investigated.");
                return;
            }
            lock (_lock)
            {
                log.Trace("Assigning created thumbnail from calbback.");
                _currentBitmap = bmp;
                AddOverlayToBitmap(_currentBitmap);
                RaiseEvent(ChangeDetected);
            }
        }

        /// <summary>
        /// Indique que ce genre d'item contient un image principale, que cette image existe, et donc que l'on peut afficher ce contenu dans un dialogue complet.
        /// </summary>
        /// todo Viméo? Test par type de dialogue?
        public bool CanShowFullDialog()
        {
            switch (OriginalFileType)
            {
                case MultimediaEventGalleryRowItemFileType.Image:
                    return File.Exists(FixedMainData); // Si image, on s'assure qu'ele existe.
                case MultimediaEventGalleryRowItemFileType.Vimeo:
                    return FixedThumbnailImage != null; // Si vidéo, on s'assure d'avoir un thumbnail en mémoire immédiatement.
                default:
                    throw new System.Exception("Unknown Original file type for item. Check code.");
            }
        }

        /// <summary>
        /// Obtient, si possible, l'image principale de cet item, si c'est un type d'image. Retourne null si le type n'est pas une image.
        /// Voir OriginalFileType pour déterminer si le type est une image (ou autre, comme une référence vidéo).
        /// </summary>
        /// <returns></returns>
        public Image GetMainImage()
        {
            if (OriginalFileType == MultimediaEventGalleryRowItemFileType.Image &&
                File.Exists(FixedMainData))
            {
                return FileUtil.LoadImage(FixedMainData);
            }

            log.Warn("Cannot get main image file, as this type of item is not an image one. Or the file does not exists.");
            return null;
        }
    }
}