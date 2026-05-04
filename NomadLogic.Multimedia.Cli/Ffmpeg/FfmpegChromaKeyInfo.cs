using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    /// <summary>
    /// Information sur le code de couleur RGB et de ses valeurs transitives pour une clé chromatique via FFMPEG.
    /// </summary>
    [DataContract]
    public class FfmpegChromaKeyInfo : ChromaKeyInfoAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Location par défaut du fichier de configuration local pour ce type de clé chromatique.
        /// </summary>
        public static readonly string DefaultConfigFileLocation = DirectoryUtil.GetLocalApplicationDir() + "FfmpegChromaKeyInfo.config";

        private float _similarity;
        private float _blend;
        /// <summary>
        /// Valeur composite Rouge
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Red { get; set; }
        /// <summary>
        /// Valeur composite Verte
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Green { get; set; }
        /// <summary>
        /// Valeur composite Bleu
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Blue { get; set; }
        
        /// <summary>
        /// Conctructeur par défaut. S'initialise comme écran vert.
        /// </summary>
        public FfmpegChromaKeyInfo()
        {
            Red = 84;
            Green = 167;
            Blue = 104;
            Blend = 0.1f;
            Similarity = 0.05f;
        }

        /// <summary>
        /// Retourne l'information du pixel de chromation sous forme hexadécimal HTML.
        /// Exemple: 0xF0F0F0
        /// </summary>
        /// <returns>Le code RBG sous format HTML</returns>
        public string HexadecimalRbgCode()
        {
            return "0x" + Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
        }

        /// <summary>
        /// Le paramètre de similitude. Habituellement de valeur entre 0.05 et 0.2.
        /// Est constraint à une valeur pourcentage ([0, 1]).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Similarity
        {
            get { return _similarity; }
            set { _similarity = MathUtil.Clamp(0.01f, 1.0f, value); }
        }

        /// <summary>
        /// Le paramètre de fusion de couleur. Habituellement de valeur entre 0.05 et 0.2.
        /// Est constraint à une valeur pourcentage ([0, 1]).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Blend
        {
            get { return _blend; }
            set { _blend = MathUtil.Clamp(0.0f, 1.0f, value); }
        }

        /// <summary>
        /// Donne une nouvelle instance avec des valeur de base pour la chromation sur fond bleu.
        /// </summary>
        /// <returns>Une nouvelle instance.</returns>
        [NotNull, Pure]
        public static FfmpegChromaKeyInfo NewInstanceChromaKeyBlue()
        {
            return new FfmpegChromaKeyInfo
            {
                Red = 96,
                Green = 184,
                Blue = 255,
                Blend = 0.1f,
                Similarity = 0.05f,
                CameraVendor = ChromaKeyInfoCameraVendorEnum.Canon
            };
        }

        /// <summary>
        /// Donne une nouvelle instance avec des valeur de base pour la chromation sur fond vert.
        /// </summary>
        /// <returns>Une nouvelle instance.</returns>
        [NotNull, Pure]
        public static FfmpegChromaKeyInfo NewInstanceChromaKeyGreen()
        {
            return new FfmpegChromaKeyInfo
            {
                //RgbColor = Color.FromArgb(255, 84, 167, 104),
                Red = 84,
                Green = 167,
                Blue = 104,
                Blend = 0.1f,
                Similarity = 0.05f,
                CameraVendor = ChromaKeyInfoCameraVendorEnum.Canon
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("{R=");
            sb.Append(Red);
            sb.Append(", G=");
            sb.Append(Green);
            sb.Append(", B=");
            sb.Append(Blue);
            sb.Append(", Similarity=");
            sb.Append(Similarity);
            sb.Append(", Blend=");
            sb.Append(Blend);
            sb.Append(")}");

            return sb.ToString();
        }
    }
}