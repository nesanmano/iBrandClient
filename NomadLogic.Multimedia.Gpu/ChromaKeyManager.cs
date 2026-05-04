using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu.OpenCV;

namespace NomadLogic.Multimedia.Gpu
{
    /// <summary>
    /// Cette classe détermine quelle est le type d'algorithme de clé chromatique à utiliser au niveau du système et exécute ce dernier avec les paramètres
    /// donnés. Sauf quand on est certains du type d'algorithme à utiliser (et donc classe), il faut passer par cette classe. La raison est que certains
    /// algorithmes dépendent de capacités systèmes qui sont peut-être indisponibles sur certains matériels.
    /// </summary>
    public static class ChromaKeyManager
    {

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Applique la transformation de clé chromatique sur le contenu d'un fichier et stocke le résultat dans un autre fichier. Le tout se 
        /// fait de façon synchrone.
        /// Sélectionne le bon algorithme de clé chromatique basé sur les capacité de l'ordinateur et les configurations stockés dans le
        /// service.
        /// Fait une vérification du fichier résultant (il doit exister et être de taille non-vide), et retourne une valeur selon cette vérification.
        /// Si les paramètres sont inutilisables, lance une exception.
        /// </summary>
        /// <param name="inFile">Le fichier intrant.</param>
        /// <param name="outFile">Le fichier sortant. S'il existe, il sera remplacé.</param>
        /// <param name="config">Si on ne veut pas utiliser la version en mémoire de la configuration, on peut passer une référence directement.</param>
        /// <returns>TRUE si le fichier résultant existe et est non-vide à la fin de l'opération.</returns>
        [PublicAPI]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public static bool ChromaKey(string inFile, string outFile, NomadConfiguration config = null)
        {
            if (string.IsNullOrWhiteSpace(inFile) || string.IsNullOrWhiteSpace(outFile))
            {
                throw new System.Exception("Invalid arguments.");
            }

            if (config == null)
            {
                config = ServicesContainer.GetService<NomadConfiguration>();
                if (config == null) throw new System.Exception("Configuration (Nomad) not set as service.");
            }

            var useFfmpeg = config.ForceUsageOfFfmpeg || !config.IsCapabilityReportGroupValid(CapabilityStatusGroupEnum.OpenCv);

            if (useFfmpeg)
            {
                var chroma = GetDefaultChromaConfigFfmpeg();
                FfmpegUtil.ChromakeyImage(config.FfmpegLocation, inFile, outFile, chroma);
            }
            else
            {
                // On utilise la clé chromatique de OpenCv, qui est la seule alternative en ce moment.
                var chroma = GetDefaultChromaConfigOpenCvLuv();

                OpenCvUtil.ChromaKeyOpenCv(inFile, outFile, chroma);
            }

            var info = new FileInfo(outFile);
            return info.Exists && info.Length > 0;
        }

        /// <summary>
        /// Retourne la configuration actuelle de chromacité Ffmpeg. Si la configuration n'existe pas, on créer automatiquement le
        /// fichier et retourne une configuration par défaut.
        /// </summary>
        /// <returns>Une référence vers la configuration de chromacité.</returns>
        [NotNull, PublicAPI]
        [SuppressMessage("ReSharper", "InvertIf")]
        public static FfmpegChromaKeyInfo GetDefaultChromaConfigFfmpeg()
        {
            var fileLocation = new FileInfo(FfmpegChromaKeyInfo.DefaultConfigFileLocation);
            var chroma = JsonUtil.ReadJsonFile<FfmpegChromaKeyInfo>(fileLocation);
            if (chroma != null) return chroma;
            chroma = FfmpegChromaKeyInfo.NewInstanceChromaKeyGreen();
            JsonUtil.WriteJsonFile(fileLocation, chroma);
            log.Warn("No chroma key config file detected. Creating new baseline for green screen: " + fileLocation);
            return chroma;
        }

        /// <summary>
        /// Retourne la configuration actuelle de chromacité Ffmpeg. Si la configuration n'existe pas, on créer automatiquement le
        /// fichier et retourne une configuration par défaut.
        /// </summary>
        /// <returns>Une référence vers la configuration de chromacité.</returns>
        [NotNull, PublicAPI]
        [SuppressMessage("ReSharper", "InvertIf")]
        public static ChromaKeyInfoOpenCvCieLuv GetDefaultChromaConfigOpenCvLuv()
        {
            var fileLocation = new FileInfo(ChromaKeyInfoOpenCvCieLuv.DefaultConfigFileLocation);
            var chroma = JsonUtil.ReadJsonFile<ChromaKeyInfoOpenCvCieLuv>(fileLocation);
            if (chroma != null) return chroma;
            chroma = ChromaKeyInfoOpenCvCieLuv.NewInstanceChromaKeyGreen();
            JsonUtil.WriteJsonFile(fileLocation, chroma);
            log.Warn("No chroma key config file detected. Creating new baseline for green screen: " + fileLocation);
            return chroma;
        }
    }
}