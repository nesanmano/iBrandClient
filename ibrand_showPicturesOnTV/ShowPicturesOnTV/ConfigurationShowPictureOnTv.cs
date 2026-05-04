using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia.XmlTransformation.image;

namespace NomadLogic.ShowPicturesOnTV
{
    #region enums

    /// <summary>
    /// Le type de controller. C'est qui détermine le type dl'agorithme principal à utiliser ainsi que sur quels type de fichiers il faut afficher.
    /// </summary>
    public enum ConfigurationShowPictureOnTvControllerType
    {
        /// <summary>
        /// Une image à la fois. Ne considère que les fichiers d'images.
        /// </summary>
        SingleImage,
        /// <summary>
        /// Un dossier d'images à la fois. Joue avec les dossiers autres que "shown", et leur contenu en terme d'images.
        /// </summary>
        PictureBundle,
        /// <summary>
        /// Affiche un vidéo à la fois. Ne considère que les fichiers vidéos.
        /// </summary>
        Video
    }

    /// <summary>
    /// Action à prendre quand on doit créer la liste des éléments anciens à afficher, indiquand qu'il n'y a plus de fichiers qui n'ont pas été affichés au moins une fois.
    /// </summary>
    public enum ConfigurationShowPictureOnTvStandbyEnum
    {
        /// <summary>
        /// On prend les éléments passés ordonnés au hasard.
        /// </summary>
        Randomize,
        /// <summary>
        /// On prend les éléments passé, en ordre donnée par l'énum "ConfigurationShowPictureOnTvOrderParamEnum"
        /// </summary>
        Orderedlist,
        /// <summary>
        /// N'affiche aucun élément passé un fois les nouveaux éléments affichés. Si on utilise un fichier d'intermission,
        /// ce fichier sera utilisé en lieu et place de ne rien affiché; il ne sera pas affiché avec X événements.
        /// </summary>
        None,
        /// <summary>
        /// Affiche continuellement le plus récent élément. Bloque "ConfigurationShowPictureOnTvResetEnum"
        /// </summary>
        AlwaysLast
    }

    /// <summary>
    /// Le type de restriction de date à utiliser comme filtre pour chercher de nouveaux et anciens fichiers.
    /// </summary>
    public enum ConfigurationShowPictureOnTvDateRestrictionEnum
    {
        /// <summary>
        /// Prend en considération tous les fichiers existants dans les recherches d'éléments
        /// </summary>
        All,
        /// <summary>
        /// Prend uniquement les fichier d'aujourd'hui (jour) dans les recherches d'éléments
        /// </summary>
        Today
    }

    /// <summary>
    /// Détermine quand on doit recréer la liste des anciens fichiers à afficher.
    /// </summary>
    public enum ConfigurationShowPictureOnTvResetEnum
    {
        /// <summary>
        /// Recréer la liste une fois totalement vide.
        /// </summary>
        AllShown,
        /// <summary>
        /// Recréer la liste une fois à moitié vide (basé sur nombre d'éléments lors de la construction).
        /// </summary>
        HalfShown,
        /// <summary>
        /// Recréer la liste à chaque fois qu'un nouvel élément est détecté.
        /// </summary>
        NewAdded
    }

    /// <summary>
    /// Indique sur quel critère on doit ordonner les fichiers.
    /// </summary>
    public enum ConfigurationShowPictureOnTvOrderParamEnum
    {
        /// <summary>
        /// Ordonne les fichier par date de création.
        /// </summary>
        Date,
        /// <summary>
        /// Ordonne les fichiers par nom.
        /// </summary>
        Name
    }

    #endregion

    /// <summary>
    /// Configuration pour le fonctionnement de l'application de ShowPictureOnTV.
    /// </summary>
    [PublicAPI]
    public class ConfigurationShowPictureOnTv
    {
        public static readonly string ConfigFileName = "show_config.json";
        public static readonly string PictureDirDefault = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Créer une nouvelle instance. Vérifie aussi si le fichier JSON de base existe. Sinon, il le créer avec la configuration par défaut.
        /// </summary>
        public ConfigurationShowPictureOnTv()
        {
            var file = DirectoryUtil.GetLocalApplicationDir() + ConfigFileName;
            if (!File.Exists(file))
            {
                JsonUtil.WriteJsonFile(new FileInfo(file), this);
            }
        }
        
        public string PictureDir { get; set; }
        
        public string TransformationsInitFile { get; set; }

        public TransformationsController GetTransformationsController()
        {
            if (TransformationsInitFile == null) return null;
            try
            {
                var controller = new TransformationsController();
                controller.Initialize(TransformationsInitFile);
                return controller;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not instanciate transformations controller.");
            }
            return null;
        }
        
        public int DisplayTime { get; set; } = 8500;

        public int Delay { get; set; } = 1000;

        [JsonConverter(typeof(StringEnumConverter))]
        public ConfigurationShowPictureOnTvControllerType ControllerType { get; set; } = ConfigurationShowPictureOnTvControllerType.SingleImage;

        [JsonConverter(typeof(StringEnumConverter))]
        public ConfigurationShowPictureOnTvStandbyEnum StandbyMethod { get; set; } = ConfigurationShowPictureOnTvStandbyEnum.Randomize;

        [JsonConverter(typeof(StringEnumConverter))]
        public ConfigurationShowPictureOnTvDateRestrictionEnum DateRestriction { get; set; } = ConfigurationShowPictureOnTvDateRestrictionEnum.All;

        [JsonConverter(typeof(StringEnumConverter))]
        public ConfigurationShowPictureOnTvResetEnum ResetList { get; set; } = ConfigurationShowPictureOnTvResetEnum.AllShown;

        [JsonConverter(typeof(StringEnumConverter))]
        public ConfigurationShowPictureOnTvOrderParamEnum OrderParam { get; set; } = ConfigurationShowPictureOnTvOrderParamEnum.Name;

        public string IntermissionFile { get; set; }

        public int IntermissionNumber { get; set; } = 5;

        /// <summary>
        /// Nombre d'itération sur le même groupe d'images.
        /// </summary>
        public int BundleLoop { get; set; } = 1;

        /// <summary>
        /// Si on parcoure les images en sens inverse une fois avoir affiché ces dernières dans le bon. Compte pour une seule et même itération si à vrai et à faux.
        /// </summary>
        public bool RewindLoop { get; set; }

        public int ScreenIdToDisplay { get; set; } = 1;

        public int CheckTimer { get; set; } = 100;

        public string BackgroundFile { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ImageLayout BackgroundFileLayout { get; set; } = ImageLayout.None;

        public Rectangle? ItemRectangle { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PictureBoxSizeMode ItemLayout { get; set; } = PictureBoxSizeMode.Zoom;
    }
}
