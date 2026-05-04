using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    public class MultimediaEventItemPresentation : IThumbnailOverlaySelector
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private string _assetsDirPath;

        [JsonRequired]
        public IMultimediaEventItemPresentationDetail FullScreenViewPresentation { get; set; }
        
        public MultimediaEventVimeoPresentation FullScreenVimeoPresentation { get; set; }

        public MultimediaEventItemPresentationQuickSelection QuickSelectionConfig { get; set; }

        [JsonRequired]
        public Rectangle LayoutRectangle { get; set; }
        [JsonRequired] public Size ThmbSize { get; set; } = new Size(160, 90);
        
        /// <summary>
        /// Format d'affichage de la date de création.
        /// </summary>
        public string CustomTimeFormat { get; set; } = DateUtil.UtcTimezoneFormatString;
        
        /// <summary>
        /// Couleur de fond. Si null, sera mis à blanc.
        /// </summary>
        public Color? MainLayoutBackgroundColor { get; set; }

        public FontDefinitionInfo DateFontDefinition { get; set; } = new FontDefinitionInfo
        {
            Color = Color.Black,
            Size = 20,
            GraphicsUnit = GraphicsUnit.Pixel,
            Style = "Bold",
            FamilyName = FontFamily.GenericMonospace.Name
        };

        /// <summary>
        /// Le répertoire de base d'où chercher les fichiers des visuels. Les configurations de cet objet ne contiennent que les
        /// noms (sans chemin) des fichiers requis.
        /// Assigne automatiquement aux enfants qui en ont besoin la même référence.
        /// </summary>
        [JsonIgnore]
        public string AssetsDirectory
        {
            set
            {
                _assetsDirPath = value;
                if (FullScreenViewPresentation != null)
                    FullScreenViewPresentation.AssetsDirectory = _assetsDirPath;
                if (QuickSelectionConfig != null)
                    QuickSelectionConfig.AssetsDirectory = _assetsDirPath;
                if (FullScreenVimeoPresentation != null)
                    FullScreenVimeoPresentation.AssetsDirectory = _assetsDirPath;
            }

            get => _assetsDirPath;
        }

        /// <summary>
        /// Si on veut un fichier spécifique comme présentation quand il n'y a pas de "thumbnail" pré-établit, on le met ici.
        /// Cette image peut-être modifiée par les overlays de type et de sélection, au besoin.
        /// </summary>
        public string ThumbnailDefaultFileName { get; set; }

        /// <summary>
        /// Définitions d'images à utiliser comme superpositions pour les thumbnails, pour certains états.
        /// Les informations de fichiers sont relatif au répertoire de défaut de la classe de présentation.
        /// </summary>
        public IDictionary<ThumbnailOverlaySelectorState, ThumbnailOverlaySelectorImageInfo> ThumbnailOverlayImagesDefinitions { get; set; }
        
        /// <summary>
        /// Tente de placer les éléments dans chaque rangée de façon centrée plutôt que justifié à gauche.
        /// </summary>
        public bool CenterRow { get; set; }

        public void Dispose()
        {

            FullScreenViewPresentation?.Dispose();
        }

        public bool Init()
        {
            if (ThumbnailOverlayImagesDefinitions != null)
            {
                foreach (var tid in ThumbnailOverlayImagesDefinitions.Values)
                {
                    if (tid.InitImage(AssetsDirectory)) continue;
                    log.Error("Could not initialize shared overlay image: {0}", tid.ImageFileName);
                    return false;
                }

                // Vérification que l'on a assez d'éléments pour fonctionner.
                if (ThumbnailOverlayImagesDefinitions.Count < 3)
                {
                    log.Error("Do not have enough thumbnail image definitions to go on. Enum defs list: {0}", StringUtil.FormatEnumerable(ThumbnailOverlayImagesDefinitions.Keys, false));
                }
            }

            return FullScreenViewPresentation?.Init() ?? false;
        }

        public Image GetThumbnailOverlayImage(ThumbnailOverlaySelectorState key)
        {
            if (ThumbnailOverlayImagesDefinitions == null || !ThumbnailOverlayImagesDefinitions.TryGetValue(key, out var definition))
            {
                return null;
            }

            return definition.GetImageInstance();
        }
    }
}