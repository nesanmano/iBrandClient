using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using NLog;
using NomadLogic.PrintStation.Json;

namespace NomadLogic.PrintStation
{
    /// <summary>
    /// Liste exhaustive des configurations d'un PrintStation
    /// </summary>
    public enum FeatureConfigEnum
    {
        /// <summary>
        /// Valeur Bool (false). Indique si le projet doit afficher un questionnaire quand on choisit une photo.
        /// </summary>
        HasQuestionnaire,
        /// <summary>
        /// Valeur Bool (false). Indique si le projet doit creer un repondant a partir d'une photo.
        /// </summary>
        CreateRespondentFromPicture,
        /// <summary>
        /// Valeur String (FFFFFF). La couleur unie du fond d'écran. Doit être un code de couleur hexadécimal RGB.
        /// </summary>
        BackgroundColor,
        /// <summary>
        /// Valeur Bool (false). Indique si un concour est associé au projet.
        /// </summary>
        HasContest,
        /// <summary>
        /// Valeur Bool (true). Indique si on imprime l'image. Certains projets ne permettent pas l'impression,
        /// car ils envoie un courriel ou par partage social par exemple.
        /// </summary>
        HasPrint,
        /// <summary>
        /// Valeur Bool (true). Indique si on utilise une image de fond pour mettre l'image choisie par dessus.
        /// Si faux, uniquement l'image est imprimée ou utilisée.
        /// </summary>
        HasFrame,
        /// <summary>
        /// Valeur Bool (false). Si vrai, on imprime une image immédiatement après l'avoir reçu. À utiliser avec
        /// des projets qui reçoivent peu d'image (comme par exemple d'une caméra en envoi direct).
        /// </summary>
        IsAutomaticPrint,
        /// <summary>
        /// Valeur String (null). Indique quelle classe spécifique à utiliser pour la création
        /// de Post via le port de communication direct. La seule présence de cette valeur indique qu'il faut l'utiliser.
        /// </summary>
        CreatePostFromIbrandCustomAction,
        /// <summary>
        /// Indique que les images produites sont de format 2x6, au lieu de 4x6.
        /// </summary>
        IsPictureSizeTwoBySix,
        /// <summary>
        /// Valeur String (null). Indique une page web (relative à notre application Le Nomad), pour l'affichage des images.
        /// Représente une page web de présentation personnalisée. Prend précédence sur Animation.
        /// </summary>
        AnimationCustomUrl,
        /// <summary>
        /// Valeur String (null). Le type d'animation web pour la présentation d'images. Chaque valeur est associée à une
        /// page web de présentation existante. Ne représente pas un Url de façon directe.
        /// <p>Valeurs possibles: one_by_one_transition, mosaic</p>
        /// </summary>
        Animation,
        /// <summary>
        /// Valeur String (null). Indique qu'il faut utiliser un fichier CSS spécialisé pour l'affichage des images dans une
        /// page web. Le fichier est relatif à notre application Le Nomad. S'utilise normalement avec la configuration
        /// "Animation" ou "AnimationCustomUrl".
        /// </summary>
        Css,
        /// <summary>
        /// Valeur Bool (false). Indique qu'il faut uniquement regarder le plus récent Post sur le server. Empêche le téléchargement de tonnes d'images
        /// en cas de ralentissement.
        /// </summary>
        IsLookForMostRecentPostOnServer,
        /// <summary>
        /// Valeur string (NULL). Si non-nul, on tente de rechercher des images à partir d'un répertoire local (un peu comme Listen to Ibrand).
        /// La valeur de cette configuration est soit un répertoire absolu, ou un répertoire relatif au répertoire de travail du Printstation.
        /// </summary>
        CheckForLocalImagesDir,
        /// <summary>
        /// Valeur string (bool). Indique si on utilise une recherche résursive (en gros, tous les fichiers dans des sous-répertoire) pour l'analyse du
        /// répertoire local. Faux par défaut.
        /// </summary>
        CheckForLocalImagesUseSubDir,
        /// <summary>
        /// Valeur string (NULL). Si non-null, indique deux dates possibles, minimum une, sinon deux séparée par le symbole '|' (Pipe), représentant la date minimale,
        /// et la date maximale. Si omise, la date maximale est aujourd'hui.
        /// </summary>
        CheckForLocalImagesDateSpan,
        /// <summary>
        /// Valeur string (NULL). Si non-nul, Représente un temps (TimeSpan) d'age maximal pour un fichier, à partir de NOW.
        /// Voir ici pour représentation String d'un TimeSpan: https://docs.microsoft.com/en-us/dotnet/api/system.timespan.parseexact?view=netframework-4.8
        /// </summary>
        CheckForLocalImagesMaxAge,

        /***** Ancienne valeur du fichier de configuration *****/

        /// <summary>
        /// Valeur int (100). Taille en hauteur en pixel du panneau de contrôle du dialogue de présentation d'image.
        /// </summary>
        PictureDialogPanelPrintHeight,
        /// <summary>
        /// Valeur int (10). Padding en pixels du bouton d'impression du dialogue de présentation d'image.
        /// </summary>
        PictureDialogPrintButtonPadding,
        /// <summary>
        /// Valeur int (416). Hauteur en pixels du dialogue de présentation d'image.
        /// </summary>
        PictureDialogWidth,
        /// <summary>
        /// Valeur int (416). Largeur en pixels du dialogue de présentation d'image.
        /// </summary>
        PictureDialogHeight,
        /// <summary>
        /// Valeur int (2). Bordure minimale du dialogue de présentation d'image.
        /// </summary>
        PictureDialogBorder,
        /// <summary>
        /// Valeur int (0). Écart supplémentaire (en plus de la valeur calculée de PicturePadding) pour l'image sur le Frame 
        /// à partir du haut. En pixels.
        /// </summary>
        PictureOffsetTop,
        /// <summary>
        /// Valeur int (0). Écart supplémentaire (en plus de la valeur calculée de PicturePadding) pour l'image sur le Frame 
        /// à partir de la gauche. En pixels.
        /// </summary>
        PictureOffsetSide,
        /// <summary>
        /// Valeur float (0.04). Distance entre les côtés et le dessus du Frame pour placer l'image du répondant. Est calculé en
        /// pourcentage de la taille du Frame.
        /// </summary>
        PicturePadding,
        /// <summary>
        /// Valeur bool (false). Indique que le Frame doit être dessiné par dessus le résultat de la composition de l'image.
        /// Souvent accompagné d'une couleur de fond.
        /// </summary>
        PictureFrameOver,
        /// <summary>
        /// Valeur bool (true). Indique que les images sur le frame doivent être de format carré (cropMaximized).
        /// </summary>
        PictureCropSquare,
        /// <summary>
        /// Valeur string (null). Indique que l'on doit peindre une couleur de base sur le canevas d'une image d'impression avant tout.
        /// Cela peut être nécessaire si on utilise beaucoup de transparence dans les éléments à dessiner.
        /// Les valeurs (3 ou 4) doivent être séparées par des caractères de séparation tel que vu ici: StringUtil.STRING_DELIMITER_CHARS
        /// </summary>
        PictureBackgroundColor,
        /// <summary>
        /// Valeur string (null). Cette valeur indique une position absolue et une taille absolue lors de la création d'une image d'impression.
        /// Ces valeurs sont similaires à un Rectangle GDI.
        /// Si cette valeur est non-nulle, elle écrase les configurations lues de: PictureOffsetSide, PictureOffsetTop, PicturePadding, PictureCropSquare
        /// Les valeurs (4) doivent être séparées par des caractères de séparation tel que vu ici: StringUtil.STRING_DELIMITER_CHARS
        /// </summary>
        PictureAbsolutePosition,
        /// <summary>
        /// Valeur int (0). Rotation horaire en degré de l'image sur le Frame.
        /// </summary>
        PictureRotation,
        /// <summary>
        /// Valeur string (null). Indique que l'on doit chercher un fichier d'image dans les fichier UI de l'usager, et l'appliquer comme
        /// overlay sur l'image avant de la mettre dans le frame.
        /// </summary>
        PictureOverlay,
        /// <summary>
        /// Valeur int (0). Rotation horaire en degrés de l'image à imprimer dans la dialogue de présentation.
        /// </summary>
        PictureDialogRotation,
        /// <summary>
        /// Valeur string (null). Représente un rectangle d'affichage à effectuer sur l'image de présentation, dans la gallerie et dans le dialogue d'impression.
        /// Lors de l'impression comme telle ou d'une manipulation dans le questionnaire, l'image sans modification est utilisée.
        /// Cette valeur n'est utilisée que si aucun autre paramètre de transformation n'est utilisé, comme l'ajout sur cadre.
        /// </summary>
        PicturePresentationCrop,
        /// <summary>
        /// Valeur bool (false). Indique que lors de l'impression via Printstation, on utilise également les informations de cropping utilisée par la présentation de la gallerie.
        /// Ne sert à rien si on n'utilise pas la propriété "PicturePresentationCrop".
        /// </summary>
        UsePicturePresentationCropInPrint,
        /// <summary>
        /// Valeur string (null). Cette valeur indique une position absolue et une taille absolue lors de la création d'une image d'impression.
        /// Ces valeurs sont similaires à un Rectangle GDI. Ce positionnement est utilisé pour afficher et positionner l'image de profil de l'usager
        /// du service social d'où est issu le Post.
        /// Les valeurs (4) doivent être séparées par des caractères de séparation tel que vu ici: StringUtil.STRING_DELIMITER_CHARS
        /// </summary>
        PictureSocialUserImageAbsolutePosition,
        /// <summary>
        /// Valeur string (null). Cette valeur indique une position absolue lors de la création d'une image d'impression.
        /// Ces valeurs sont similaires à un Point GDI. Ce positionnement est utilisé pour afficher et positionner l'image texte créée à
        /// partir du nom de l'usager du service social d'où est issu le Post.
        /// Les valeurs (2) doivent être séparées par des caractères de séparation tel que vu ici: StringUtil.STRING_DELIMITER_CHARS
        /// </summary>
        PictureSocialUserNameAbsolutePoint,
        /// <summary>
        /// Valeur string (null). Cette configuration est une définition d'un objet "FontDefinitionInfo" sous format JSON texte.
        /// Utiliser NULL implique l'on utilise les valeurs par défaut de la classe.
        /// La valeur de cette propriété sera lue en tant que format JSON et directement transformée en instance de "FontDefinitionInfo".
        /// </summary>
        PictureSocialUserNameFontJson,
        /// <summary>
        /// Absolument aucune modification n'est faite sur l'image. On imprime le fichier comme tel. Ce choix supercède toute autre configuration.
        /// </summary>
        PictureNoModification,


        /// <summary>
        /// Valeur int (80). Largeur en pixel du panneau sur les côtés, qui contiennent les flèches de navigation.
        /// </summary>
        GallerySidePanelWidth,
        /// <summary>
        /// Valeur int (-1). Si on ne veut pas une zone d'affichage aussi large que le maximum, on met cette valeur dans cette configuration.
        /// </summary>
        GalleryPanelWidth,
        /// <summary>
        /// Valeur int (-1). Si on ne veut pas une zone d'affichage aussi haute que le maximum, on met cette valeur dans cette configuration.
        /// </summary>
        GalleryPanelHeight,
        /// <summary>
        /// Valeur int (0). Ajustement de départ pour la zone d'affichage à partir du haut.
        /// </summary>
        GalleryPanelTopStart,
        /// <summary>
        /// Valeur int (0). Ajustement de départ pour la zone d'affichage à partir de la gauche.
        /// </summary>
        GalleryPanelLeftStart,
        /// <summary>
        /// Valeur int (140). Taille en pixels pour la hauteur du panneau du dessous, qui contient le logo.
        /// </summary>
        GalleryBottomPanelHeight,
        /// <summary>
        /// Valeur int (70). Taille en pixels pour la hauteur du panneau du dessous, qui contient le logo.
        /// </summary>
        GallerySearchPanelHeight,
        /// <summary>
        /// Valeur bool (true). Indique si on doit afficher la boîte de recherche.
        /// </summary>
        IsGalleryShowSearch,
        /// <summary>
        /// Valeur float (1.0f). Indique le ratio largeur/hauteur que doivent prendre la boîte de présentation des images. Un ratio de 1 (par défaut)
        /// indique que le bloc de présentation est carré. Un ration inférieur à un indique un image en portrait (plus haute que large) et un ratio
        /// supérieur à 1 indique un forme paysage (plus large que haut).
        /// <br/>À titre d'exemples, voici quelques ratios commun pour nos image:
        /// Image paysage d'une caméra: 3:2 => 1.5f
        /// Image avec Frame pré-fait (4x6 pouces ou 1065x1598 pixels): 0.666f
        /// Image avec Frame pré-fait de (2x6 pouces): 0.333f
        /// </summary>
        GalleryImageSizeRatio,
        /// <summary>
        /// Valeur float (0.05f). Indique le pourcentage de padding minimal que l'on voudrait entre les images. Est basé sur la taille des images
        /// lorsque calculé pour l'affichage plutôt qu'une valeur fixe. Valeur par défaut de 5%. Par exemple, une image de 200x150 de large aurait
        /// 10 pixels de padding sur l'horizontale, et 7 pixels sur la verticale.
        /// Il faut prendre en considération que c'est une valeur candidate: elle sera probablement modifiée et recalculée pour espacer les blocks
        /// d'affichage en fonction de l'espace disponible.
        /// </summary>
        GalleryImagePaddingRatio,
        /// <summary>
        /// Valeur int (2). Série de nombre qui représentent une façon d'afficher l'image dans le bloc de présentation. Les différentes
        /// valeurs sont:
        /// 0 => Image étirée pour se conformer au bloc.
        /// 1 => Conserver le ratio de l'image. On centre l'image dans le bloc au mieux pour afficher l'image au complet. Laisse des trous inégaux dans l'affichage.
        /// 2 => Élaguer (crop) l'image pour qu'elle conserve son ratio de pixel, mais en perdant une partie de l'affichage pour remplir le bloc.
        /// </summary>
        GalleryImageViewMode,
        /// <summary>
        /// Valeur int (2). Nombre de rangées d'images à présenter. Si inférieur à un, maximise le nombre selon les colonnes.
        /// </summary>
        GalleryRowAmount,
        /// <summary>
        /// Valeur int (-1). Nombre d'images par rangée à présenter. Si inférieur à un, maximise le nombre selon les rangées.
        /// </summary>
        GalleryPicturePerRow,
        /// <summary>
        /// Valeur int (20). Temps de rafraîchissement du contrôle, en seconde.
        /// </summary>
        GalleryRefreshTimer,
        /// <summary>
        /// Valeur int (20). Espace, en pixel, à partir du dessus de l'écran et le panneau de recherche (si visible).
        /// </summary>
        GallerySearchOffset,
        /// <summary>
        /// Valeur int (400). Largeur de la boîte TextBox de recherche.
        /// </summary>
        GallerySearchBoxWidth,
        /// <summary>
        /// Valeur int (20). Taille du Font dans la boîte de recherche. Affecte la hauteur de ce dernier.
        /// </summary>
        GallerySearchBoxTextSize,
        /// <summary>
        /// Valeur int (1240). Largeur du clavier, en pixels.
        /// </summary>
        GalleryKeyboardWidth,
        /// <summary>
        /// Valeur int (435). Hauteur du clavier, en pixels.
        /// </summary>
        GalleryKeyboardHeight,
        /// <summary>
        /// Valeur bool (false). Cache un post une fois que l'on a fait un questionnaire ou une impression.
        /// </summary>
        HidePostOnceClicked
    }

    #region Helper classes

    /// <summary>
    /// Élément essentiels pour traiter une configuration.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal struct FeatureConfigEnumData
    {
        /// <summary>
        /// La configuration à définir.
        /// </summary>
        internal FeatureConfigEnum _config;
        /// <summary>
        /// Son type (bool, String, autre...)
        /// </summary>
        internal Type _data_type;
        /// <summary>
        /// Sa valeur numérique minimale, si applicable.
        /// </summary>
        internal float? _max_value;
        /// <summary>
        /// Sa valeur numérique maximale, si applicable.
        /// </summary>
        internal float? _min_value;
        /// <summary>
        /// Valeur par défaut, de façon textuelle. Si cette valeur est nulle, alors il s'agit de sa valeur par défaut.
        /// </summary>
        internal string _default_value;
        /// <summary>
        /// Le nom de la configuration dans la base de données.
        /// </summary>
        internal string _database_tag;
    }

    /// <summary>
    /// Classe de comparaison pour la structure.
    /// </summary>
    internal class FeatureConfigEnumDataComparer : IComparer<FeatureConfigEnumData>
    {
        int IComparer<FeatureConfigEnumData>.Compare(FeatureConfigEnumData x, FeatureConfigEnumData y)
        {
            return x._config.CompareTo(y._config);
        }
    }

    #endregion

    #region DataInit

    /// <summary>
    /// Comme les énumérations de C# ne permettent pas d'associer des valeurs ou des méthodes à des énumérations (ce ne sont pas des classes comme dans Java),
    /// cette classe statique à pour but de contenir la logique de valeurs par défaut, minimale, maximale et de validation des différents paramètres de configuration
    /// du PrintStation.
    /// </summary>
    internal static class FeatureConfigManager
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly SortedSet<FeatureConfigEnumData> configData = new SortedSet<FeatureConfigEnumData>(new FeatureConfigEnumDataComparer());
        private static readonly Dictionary<FeatureConfigEnum, string> currentConfig = new Dictionary<FeatureConfigEnum, string>();
        public static readonly CultureInfo DefaultCultureNumberFormat = CultureInfo.InvariantCulture;
        public static readonly NumberStyles DefaultNumberStyle = NumberStyles.Number;

        /// <summary>
        /// Constructeur statique
        /// </summary>
        static FeatureConfigManager()
        {
            // On n'initialise qu'une fois.
            if (configData.Count != 0) return;

            // On initialise chaque Énumération pour son usage
            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.HasQuestionnaire,
                _data_type = typeof(bool),
                _database_tag = "QUESTIONNAIRE",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.CreateRespondentFromPicture,
                _data_type = typeof(bool),
                _database_tag = "CREATE_RESPONDENT_FROM_PICTURE",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.CreatePostFromIbrandCustomAction,
                _data_type = typeof(string),
                _database_tag = "CREATE_POST_FROM_IBRAND_CUSTOM_ACTION",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.HidePostOnceClicked,
                _data_type = typeof(bool),
                _database_tag = "HIDE_POST_ONCE_CLICKED",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.HasContest,
                _data_type = typeof(bool),
                _database_tag = "CONTEST",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.Animation,
                _data_type = typeof(string),
                _database_tag = "ANIMATION",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.AnimationCustomUrl,
                _data_type = typeof(string),
                _database_tag = "URL",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.BackgroundColor,
                _data_type = typeof(string),
                _database_tag = "BACKGROUND_COLOR",
                _default_value = "FFFFFF",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.Css,
                _data_type = typeof(string),
                _database_tag = "CSS",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.HasFrame,
                _data_type = typeof(bool),
                _database_tag = "FRAME",
                _default_value = "true",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.HasPrint,
                _data_type = typeof(bool),
                _database_tag = "PRINT",
                _default_value = "true",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.IsPictureSizeTwoBySix,
                _data_type = typeof(bool),
                _database_tag = "TWO_BY_SIX",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.IsAutomaticPrint,
                _data_type = typeof(bool),
                _database_tag = "PRINT_AUTO",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.IsLookForMostRecentPostOnServer,
                _data_type = typeof(bool),
                _database_tag = "LOOK_FOR_MOST_RECENT_POST",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.CheckForLocalImagesDir,
                _data_type = typeof(string),
                _database_tag = "LOCAL_IMAGE_DIR_SOURCE",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.CheckForLocalImagesUseSubDir,
                _data_type = typeof(bool),
                _database_tag = "LOCAL_IMAGE_DIR_SOURCE_RECURSIVE",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.CheckForLocalImagesDateSpan,
                _data_type = typeof(string),
                _database_tag = "LOCAL_IMAGE_DIR_SOURCE_DATES",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.CheckForLocalImagesMaxAge,
                _data_type = typeof(string),
                _database_tag = "LOCAL_IMAGE_DIR_SOURCE_MAX_AGE",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureDialogPanelPrintHeight,
                _data_type = typeof(int),
                _database_tag = "PICTURE_DIALOG_PANEL_PRINT_HEIGHT",
                _default_value = "100",
                _min_value = 30,
                _max_value = 1000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureDialogPrintButtonPadding,
                _data_type = typeof(int),
                _database_tag = "PICTURE_DIALOG_PANEL_PRINT_BUTTON_PADDING",
                _default_value = "10",
                _min_value = 0,
                _max_value = 1000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureDialogWidth,
                _data_type = typeof(int),
                _database_tag = "PICTURE_DIALOG_WIDTH",
                _default_value = "416",
                _min_value = 100,
                _max_value = 5000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureDialogHeight,
                _data_type = typeof(int),
                _database_tag = "PICTURE_DIALOG_HEIGHT",
                _default_value = "416",
                _min_value = 100,
                _max_value = 5000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureDialogBorder,
                _data_type = typeof(int),
                _database_tag = "PICTURE_DIALOG_BORDER",
                _default_value = "2",
                _min_value = 0,
                _max_value = 2000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureOffsetTop,
                _data_type = typeof(int),
                _database_tag = "PICTURE_OFFSET_TOP",
                _default_value = "0",
                _min_value = -100,
                _max_value = 100
            });


            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureOffsetSide,
                _data_type = typeof(int),
                _database_tag = "PICTURE_OFFSET_SIDE",
                _default_value = "0",
                _min_value = -100,
                _max_value = 100
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PicturePadding,
                _data_type = typeof(float),
                _database_tag = "PICTURE_PADDING",
                _default_value = "0.04",
                _min_value = 0,
                _max_value = 99
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureRotation,
                _data_type = typeof(int),
                _database_tag = "PICTURE_ROTATION",
                _default_value = "0",
                _min_value = -360,
                _max_value = 360
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureOverlay,
                _data_type = typeof(string),
                _database_tag = "PICTURE_OVERLAY",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureFrameOver,
                _data_type = typeof(bool),
                _database_tag = "PICTURE_FRAME_OVER",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureCropSquare,
                _data_type = typeof(bool),
                _database_tag = "PICTURE_CROP_SQUARE",
                _default_value = "true",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureBackgroundColor,
                _data_type = typeof(string),
                _database_tag = "PICTURE_BKG_COLOR",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureAbsolutePosition,
                _data_type = typeof(string),
                _database_tag = "PICTURE_ABSOLUTE_POSITION",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureDialogRotation,
                _data_type = typeof(int),
                _database_tag = "PICTURE_DIALOG_ROTATION",
                _default_value = "0",
                _min_value = -360,
                _max_value = 360
            });
            
            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PicturePresentationCrop,
                _data_type = typeof(string),
                _database_tag = "PICTURE_PRESENTATION_CROP",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });
            
            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.UsePicturePresentationCropInPrint,
                _data_type = typeof(bool),
                _database_tag = "PICTURE_PRESENTATION_CROP_IN_PRINT",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureSocialUserImageAbsolutePosition,
                _data_type = typeof(string),
                _database_tag = "PICTURE_SOCIAL_IMAGE_ABSOLUTE_POSITION",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureSocialUserNameAbsolutePoint,
                _data_type = typeof(string),
                _database_tag = "PICTURE_SOCIAL_NAME_ABSOLUTE_POSITION",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureSocialUserNameFontJson,
                _data_type = typeof(string),
                _database_tag = "PICTURE_SOCIAL_NAME_FONT_JSON",
                _default_value = null,
                _min_value = null,
                _max_value = null
            });

            // PictureNoModification
            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.PictureNoModification,
                _data_type = typeof(bool),
                _database_tag = "PICTURE_NO_MODIFICATION",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GallerySidePanelWidth,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PANEL_SIDE_WIDTH",
                _default_value = "80",
                _min_value = 0,
                _max_value = 500
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryPanelWidth,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PANEL_WIDTH",
                _default_value = "-1",
                _min_value = 0,
                _max_value = 4000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryPanelHeight,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PANEL_HEIGHT",
                _default_value = "-1",
                _min_value = 0,
                _max_value = 800
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryPanelTopStart,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PANEL_TOP_START",
                _default_value = "0",
                _min_value = 0,
                _max_value = 800
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryPanelLeftStart,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PANEL_LEFT_START",
                _default_value = "0",
                _min_value = 0,
                _max_value = 800
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryBottomPanelHeight,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PANEL_BOTTOM_HEIGHT",
                _default_value = "140",
                _min_value = 0,
                _max_value = 600
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GallerySearchPanelHeight,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PANEL_SEARCH_HEIGHT",
                _default_value = "70",
                _min_value = 0,
                _max_value = 600
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.IsGalleryShowSearch,
                _data_type = typeof(bool),
                _database_tag = "GALLERY_SHOW_SEARCH",
                _default_value = "false",
                _min_value = null,
                _max_value = null
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryRowAmount,
                _data_type = typeof(int),
                _database_tag = "GALLERY_ROW_AMOUNT",
                _default_value = "2",
                _min_value = int.MinValue,
                _max_value = 8
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryPicturePerRow,
                _data_type = typeof(int),
                _database_tag = "GALLERY_PICTURE_PER_ROW",
                _default_value = "-1",
                _min_value = int.MinValue,
                _max_value = 20
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryImageSizeRatio,
                _data_type = typeof(float),
                _database_tag = "GALLERY_IMAGE_SIZE_RATIO",
                _default_value = "1.0",
                _min_value = 0.1f,
                _max_value = 10f
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryImagePaddingRatio,
                _data_type = typeof(float),
                _database_tag = "GALLERY_IMAGE_PADDING_RATIO",
                _default_value = "0.05",
                _min_value = 0,
                _max_value = 1
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryImageViewMode,
                _data_type = typeof(int),
                _database_tag = "GALLERY_IMAGE_VIEW_MODE",
                _default_value = "2",
                _min_value = 0,
                _max_value = 2
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryRefreshTimer,
                _data_type = typeof(int),
                _database_tag = "GALLERY_REFRESH_TIMER",
                _default_value = "20",
                _min_value = 5,
                _max_value = 120
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GallerySearchOffset,
                _data_type = typeof(int),
                _database_tag = "GALLERY_SEARCH_OFFSET",
                _default_value = "20",
                _min_value = 0,
                _max_value = 1000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GallerySearchBoxWidth,
                _data_type = typeof(int),
                _database_tag = "GALLERY_SEARCH_BOX_WIDTH",
                _default_value = "400",
                _min_value = 100,
                _max_value = 2000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GallerySearchBoxTextSize,
                _data_type = typeof(int),
                _database_tag = "GALLERY_SEARCH_BOX_TEXT_SIZE",
                _default_value = "20",
                _min_value = 5,
                _max_value = 100
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryKeyboardWidth,
                _data_type = typeof(int),
                _database_tag = "GALLERY_KEYBOARD_WIDTH",
                _default_value = "1240",
                _min_value = 600,
                _max_value = 5000
            });

            configData.Add(new FeatureConfigEnumData
            {
                _config = FeatureConfigEnum.GalleryKeyboardHeight,
                _data_type = typeof(int),
                _database_tag = "GALLERY_KEYBOARD_HEIGHT",
                _default_value = "435",
                _min_value = 300,
                _max_value = 5000
            });

            // On teste maintenant si TOUTES les énumérations font partie de la mémoire du manager (au cas où quelqu'un oublie de configurer une
            // nouvelle énumération ici.
            var totalEnums = Enum.GetValues(typeof(FeatureConfigEnum)).Length;
            if (totalEnums == configData.Count) return;

            log.Fatal(
                "Initialization Fatal Error. The number of initialized running features does not match the total number that exists in the enumeration. {0}/{1}",
                configData.Count, totalEnums);

            IList<FeatureConfigEnum> allENum =
                Enum.GetValues(typeof(FeatureConfigEnum)).OfType<FeatureConfigEnum>().ToList();

            IList<FeatureConfigEnum> configENum = configData.Select(data => data._config).ToList();

            var result = allENum.Except(configENum);

            // Construction du message d'erreur
            var sb = new StringBuilder();
            sb.Append(
                "Incomplete initialization in Feature config manager. The following ENUMs are not configured.");
            foreach (var e in result)
            {
                sb.Append("\n\t\t- ");
                sb.Append(e.ToString());
            }

            throw new Exception(sb.ToString());
        }

#endregion

    #region Generic code

        /// <summary>
        /// Enlève une valeur des configurations actuelle. Si on demande sa valeur dans le futur, ce sera sa valeur par défaut, si existante.
        /// </summary>
        /// <param name="confEnum">La configuration à détruire.</param>
        /// <returns>TRUE si l'opération à détruit une valeur.</returns>
        internal static bool RemoveConfigurationValue(FeatureConfigEnum confEnum)
        {
            return currentConfig.Remove(confEnum);
        }

        /// <summary>
        /// Met à jour une valeur de configuration. Si la valeur est nulle, elle est refusée.
        /// Si une valeur numérique possède un maximum et minimum (valeur étalon entière), alors toute
        /// tentative d'aller en dessous ou au dessus se verra affecter la valeur min/max, selon circonstance.
        /// Ne provoquera pas d'erreur, mais un WARN sera mis mis dans les journaux.
        /// </summary>
        /// <param name="confEnum">La configuratiòn à changer.</param>
        /// <param name="v">LA nouvelle valeur.</param>
        /// <returns>S'il y a eu modification.</returns>
        internal static bool SetConfigurationValue(FeatureConfigEnum confEnum, string v)
        {
            if (string.IsNullOrWhiteSpace(v))
            {
                return false;
            }

            // Test d'assignation
            // http://stackoverflow.com/questions/3957817/calling-generic-method-with-type-variable
            var tmp = configData.First(confType => confType._config == confEnum);
            var methodType = typeof(FeatureConfigManager).GetMethod("ConvertFromStringToType");
            var methodGenericRef = methodType.MakeGenericMethod(tmp._data_type);
            object[] args = { v };
            methodGenericRef.Invoke(null, args);

            var modV = v;

            // Vérification valeur min/max
            try
            {
                if (tmp._min_value.HasValue)
                {
                    var minVal = tmp._min_value.Value;
                    var val = double.Parse(v, CultureInfo.InvariantCulture);
                    if (val < minVal)
                    {
                        modV = minVal.ToString(CultureInfo.InvariantCulture);
                        log.Warn("Tried to Set lower than minimal value for config {0}. Illegal value = {1}", confEnum, v);
                    }
                }

                if (tmp._max_value.HasValue)
                {
                    var maxVal = tmp._max_value.Value;
                    var val = double.Parse(v, CultureInfo.InvariantCulture);
                    if (val > maxVal)
                    {
                        modV = maxVal.ToString(CultureInfo.InvariantCulture);
                        log.Warn("Tried to Set higher than maximal value for config {0}. Illegal value = {1}", confEnum, v);
                    }
                }
            }
            catch (Exception e)
            {
                var minString = tmp._min_value?.ToString("0.000", CultureInfo.InvariantCulture);
                var maxString = tmp._max_value?.ToString("0.000", CultureInfo.InvariantCulture);
                log.Error(e, "Error during Min/Max/Value conversion: min={0}, max={1}, enum={2}, value={3}", minString, maxString, confEnum, v);
            }

            if (currentConfig.ContainsKey(confEnum))
            {
                var previousValue = currentConfig[confEnum];
                if (previousValue.Equals(modV)) return false;
            }

            currentConfig[confEnum] = modV;
            return true;
        }

        // Enlève toutes les configurations du manager.
        internal static void ClearConfigurations()
        {
            currentConfig.Clear();
        }

        internal static bool SetConfigurationValue(FeatureConfig f)
        {
            // N'accepte pas les valeur nulle ou une configuration nulle.
            if (string.IsNullOrWhiteSpace(f?.Value))
            {
                return false;
            }

            // http://stackoverflow.com/questions/15407065/firstordefault-result-of-a-struct-collection
            var fce = configData.Where(item => item._database_tag.Equals(f.Name, StringComparison.Ordinal))
                .Cast<FeatureConfigEnumData?>()
                .FirstOrDefault();

            // Ne contient pas le TAG de la BD pour associer avec le nom d'énumération.
            if (!fce.HasValue)
            {
                return false;
            }

            // Assigne la valeur dans le dictionnaire.
            SetConfigurationValue(fce.Value._config, f.Value);
            return true;
        }

        /// <summary>
        /// Indique si une configuration existe ou possède au moins une valeur par défaut.
        /// </summary>
        /// <param name="confEnum">La configuration à tester.</param>
        /// <returns>TRUE si la configuration est assignée ou si elle possède une valeur par défaut.</returns>
        internal static bool HasConfig(FeatureConfigEnum confEnum)
        {
            if (currentConfig.ContainsKey(confEnum))
            {
                return true;
            }
            else
            {
                var tmp = configData.First(conf => conf._config == confEnum);
                return !string.IsNullOrEmpty(tmp._default_value);
            }
        }

        /// <summary>
        /// Retourne la valeur voulue de la configuration dans le type spécifié. Si est impossible de convertir la valeur, ou si
        /// la valeur par défaut est nulle et que le type demandé est non nullable, une exception sera lancée.
        /// Voir la méthode HasConfig() pour voir si la configuration existe bel et bien ou possède une valeur par défaut valide.
        /// </summary>
        /// <typeparam name="T">La type voulu.</typeparam>
        /// <param name="confEnum">La configuration voulue.</param>
        /// <returns>La valeur (donnée ou par défaut) de la configuration demandée.</returns>
        internal static T GetConfigValue<T>(FeatureConfigEnum confEnum) where T : IConvertible
        {
            var tmp = configData.First(conf => conf._config == confEnum);

            // AbstractConfiguration existante
            if (currentConfig.ContainsKey(confEnum))
            {
                var val = currentConfig[confEnum];
                return ConvertFromStringToType<T>(val);
            }

            // S'il n'y a pas de valeur par défaut et qu'il est impossible d'Assigner null comme défaut
            // Par exemple, T est un "int"
            if (tmp._default_value == null)
            {
                if (default(T) == null) return default(T);

                string res = $"Cannot use null as value for inexisting, non-defaulted and non-nullable type. WebFacadeConfig={confEnum} Type={typeof(T)}";
                throw new InvalidOperationException(res);
            }

            try
            {
                return ConvertFromStringToType<T>(tmp._default_value);
            }
            catch (Exception e)
            {
                log.Warn(e, "Error with following enum: '{3}'. Could not convert value ({0}) to type: {1}. Returning default value: {2}",
                    tmp._default_value, typeof(T), default(T), confEnum);
                return default(T);
            }
        }

        /// <summary>
        /// Conversion d'une valeur String en le type convertible voulu.
        /// </summary>
        /// <typeparam name="T">Type convertible.</typeparam>
        /// <param name="val">Valeur dans une chaîne de caractère.</param>
        /// <returns>La valeur sous son type voulue.</returns>
        public static T ConvertFromStringToType<T>(string val) where T : IConvertible
        {
            var type = typeof(T);
            object conversion = null;

            if (type == typeof(int))
            {
                conversion = int.Parse(val, DefaultNumberStyle, DefaultCultureNumberFormat);
            }
            else if (type == typeof(float))
            {
                conversion = float.Parse(val, DefaultNumberStyle, DefaultCultureNumberFormat);
            }
            else if (type == typeof(double))
            {
                conversion = double.Parse(val, DefaultNumberStyle, DefaultCultureNumberFormat);
            }
            else if (type == typeof(bool))
            {
                conversion = bool.Parse(val);
            }
            if (type == typeof(string))
            {
                conversion = val;
            }

            return (T)Convert.ChangeType(conversion, typeof(T));
        }

        #endregion
    }
}
