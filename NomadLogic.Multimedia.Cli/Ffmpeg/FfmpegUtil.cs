using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;

// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    /// <summary>
    /// Cette classe regroupe les principaux appels vers l'exécutable FFMPEG pour la transformation de fichiers.
    /// La quasi-totalité des méthodes, lorsqu'elles doivent créé un fichier de sortie, vont remplacer le fichier de sortie
    /// s'il existe déjà.
    /// Voir ici pour quelques idées sur la ligne de commande pour x264:
    /// https://trac.ffmpeg.org/wiki/x264EncodingGuide
    /// Guide sur FFMEPG général
    /// http://ffmpeg.org/ffmpeg.html
    /// Guide sur les filtres FFMPEG
    /// http://ffmpeg.org/ffmpeg-filters.html#Filtergraph-description
    /// </summary>
    [PublicAPI]
    public abstract class FfmpegUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Convertit un vidéo (qui n'est en fait pas obligé d'être un MP4), vers un format lisible par les cellulaires.
        /// </summary>
        /// <param name="mp4File">Le vidéo intrant.</param>
        /// <param name="threegpFile">Le vidéo de sortie.</param>
        /// <param name="ffmpegLocation"></param>
        [PublicAPI]
        public static void Mp4To3Gp([NotNull] string mp4File, [NotNull] string threegpFile, [NotNull] string ffmpegLocation)
        {
            DeleteOutputFile(threegpFile);
            var args = " -i  \"" + mp4File + "\" -s qvga -b 384k -vcodec libx264 -r 23.976 -acodec libfaac -ac 2 -ar 44100 -ab 64k -vpre baseline -crf 22 -deinterlace -o \"" + threegpFile + "\"";
            LaunchProcessUtil.LaunchProcess(ffmpegLocation, args);
        }

        /// <summary>
        /// Conversion simple, avec paramètres par défaut, pour un conversion vers codec MP4.
        /// </summary>
        /// <param name="aviFile">Vidéo intrant</param>
        /// <param name="threegpFile">Vidéo sortant</param>
        /// <param name="ffmpegLocation">Exécutable FFMPEG</param>
        public static void AviTo3Gp([NotNull] string aviFile, [NotNull] string threegpFile, [NotNull] string ffmpegLocation)
        {
            DeleteOutputFile(threegpFile);
            var args = " -i \"" + aviFile + "\" -vcodec mpeg4 \"" + threegpFile + "\"";

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, args);
        }

        /// <summary>
        /// Conversion vers FLV, avec BitRate par défaut (700 kbps)
        /// </summary>
        /// <param name="aviFile">Vidéo original</param>
        /// <param name="flvFile">Vidéo final</param>
        /// <param name="ffmpegLocation">Location de FFMPEG</param>
        public static void AviToFlv([NotNull] string aviFile, [NotNull] string flvFile, [NotNull] string ffmpegLocation)
        {
            AviToFlv(aviFile, flvFile, ffmpegLocation, 700);
        }

        /// <summary>
        /// Conversion vers FLV, avec option de BitRate. Plus ce chiffre (défaut: 700) est élevé, on augmente la qualité, mais on
        /// ajoute aussi pas mal de données dans le fichier final.
        /// </summary>
        /// <param name="aviFile">Vidéo original.</param>
        /// <param name="flvFile">Vidéo final</param>
        /// <param name="ffmpegLocation">Location de l'original.</param>
        /// <param name="kbps">Le bitRate. (kbps)</param>
        public static void AviToFlv([NotNull] string aviFile, [NotNull] string flvFile, [NotNull] string ffmpegLocation, int kbps)
        {
            DeleteOutputFile(flvFile);
            var args = " -i \"" + aviFile + "\" -vcodec libx264 -acodec libmp3lame -ar 44100 -vb " + kbps + "k -f flv \"" + flvFile + "\"";

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, args);
        }

        /// <summary>
        /// Transformation vers un fichier H264 avec audio MP3 de haute qualité (192 kbps). BitRate (kbps) de 700.
        /// </summary>
        /// <param name="aviFile">Vidéo original</param>
        /// <param name="movFile">Vidéo final</param>
        /// <param name="ffmpegLocation">Location de FFMPEG.</param>
        public static void AviToMov([NotNull] string aviFile, [NotNull] string movFile, [NotNull] string ffmpegLocation)
        {
            AviToMov(aviFile, movFile, ffmpegLocation, 700);
        }

        /// <summary>
        /// Transformation vers un fichier H264 avec audio MP3 de haute qualité (192 kbps).
        /// Plus le BitRate est élevé (base de 700 kbps), plus le fichier sera de haute qualité, et sa taille de fichier sera affectée.
        /// </summary>
        /// <param name="aviFile">Vidéo original</param>
        /// <param name="movFile">Vidéo final</param>
        /// <param name="ffmpegLocation">Location de FFMPEG.</param>
        /// <param name="kbps">Le BitRate</param>
        [PublicAPI]
        public static void AviToMov([NotNull] string aviFile, [NotNull] string movFile, [NotNull] string ffmpegLocation, int kbps)
        {
            DeleteOutputFile(movFile);
            var args = " -i \"" + aviFile + "\" -vcodec libx264 -acodec libmp3lame -ab 192 \"" + movFile + "\"";

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, args);
        }

        /// <summary>
        /// Convertit un vidéo avec Xvid à 640x360 de taille.
        /// </summary>
        /// <param name="movFile">Fichier Intrant</param>
        /// <param name="aviFile">Fichier Sortant</param>
        /// <param name="ffmpegLocation">Location FFMPEG.</param>
        public static void MovToAvi([NotNull] string movFile, [NotNull] string aviFile, [NotNull] string ffmpegLocation)
        {
            MovToAvi(movFile, aviFile, ffmpegLocation, 640, 360, 0);
        }

        /// <summary>
        /// Convertit un vidéo avec Xvid.
        /// </summary>
        /// <param name="movFile">Fichier Intrant</param>
        /// <param name="aviFile">Fichier Sortant</param>
        /// <param name="ffmpegLocation">Location FFMPEG.</param>
        /// <param name="width">La largeur finale</param>
        /// <param name="height">La hauteur finale</param>
        public static void MovToAvi([NotNull] string movFile, [NotNull] string aviFile, [NotNull] string ffmpegLocation, int width, int height)
        {
            MovToAvi(movFile, aviFile, ffmpegLocation, width, height, 0);
        }

        /// <summary>
        /// Convertit un vidéo avec Xvid, avec option vers une taille maximale de fichier.
        /// </summary>
        /// <param name="movFile">Fichier Intrant</param>
        /// <param name="aviFile">Fichier Sortant</param>
        /// <param name="ffmpegLocation">Location FFMPEG.</param>
        /// <param name="width">La largeur finale</param>
        /// <param name="height">La hauteur finale</param>
        /// <param name="maxfileSize">La taille désirée du fichier, en octets. 0 si on veut ne pas spécifier une taille voulue.</param>
        [PublicAPI]
        public static void MovToAvi([NotNull] string movFile, [NotNull] string aviFile, [NotNull] string ffmpegLocation, int width, int height, uint maxfileSize)
        {
            DeleteOutputFile(aviFile);

            string args;
            if (maxfileSize > 0)
            {
                args = " -i \"" + movFile + "\" -s " + width + "x" + height + " -c:v mpeg4 -vtag xvid -b:v " +
                    maxfileSize + "k \"" + aviFile + "\"";
            }
            else
            {
                //args = " -i \"" + mov_file + "\" -s " + width + "x" + height + " -c:v mpeg4 -vtag xvid -qscale:v 1 \"" + avi_file + "\"";
                args = " -i \"" + movFile + "\" -s " + width + "x" + height + " -c:v libxvid -qscale:v 1 \"" + aviFile + "\"";
            }

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, args);
        }

        /// <summary>
        /// Détruit un fichier de sortie, si existant.
        /// </summary>
        /// <param name="outFile">Le chemin vers le fichier.</param>
        private static void DeleteOutputFile([NotNull] string outFile)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
        }

        /// <summary>
        /// Minimize un vidéo avec le codec "libx264". On peut jouer avec la qualité finale avec le paramètre CRF.
        /// <br/>Voir: http://unix.stackexchange.com/questions/28803/how-can-i-reduce-a-videos-size-with-ffmpeg
        /// </summary>
        /// <param name="ffmpegLocation">Location de l'exécutable FFMPEG</param>
        /// <param name="original">Fichier original</param>
        /// <param name="output">Fichier de sortie</param>
        /// <param name="crf">Paramètre de compression. PLus il est bas, moins il y a de compression. Une valeur entre 18 et 24 est recommandée.</param>
        /// <param name="removeAudio">Indique que l'on peut enlever l'audio du vidéo. Enlève encore plus de données.</param>
        public static void CompressVideo([NotNull] string ffmpegLocation, [NotNull] string original, [NotNull] string output, int crf, bool removeAudio = false)
        {
            // ESSENTIEL! Sinon, FFMPEG attend une confirmation, qui ne viendra jamais...
            if (File.Exists(output))
            {
                File.Delete(output);
            }
            var args = "-i \"" + original + "\" -vcodec libx264 -crf " + crf + (removeAudio ? " -an " : "") + " \"" + output + "\"";

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, args);
        }

        /// <summary>
        /// Extrait l'audio d'un vidéo en fichier WAV.
        /// </summary>
        /// <param name="videoFile">Vidéo original</param>
        /// <param name="wavFile">Le fichier de sortie.</param>
        /// <param name="ffmpegLocation">Locatrion de FFMPEG</param>
        /// <param name="isMono">Indique que l'on veut avoir un son mono (un seul canal) au final.</param>
        [PublicAPI]
        public static void ExtractSoundToWav([NotNull] string videoFile, [NotNull] string wavFile, [NotNull] string ffmpegLocation, bool isMono)
        {
            DeleteOutputFile(wavFile);
            var args = " -i \"" + videoFile + "\" \"" + wavFile + "\"";
            if (isMono)
            {
                args = " -i \"" + videoFile + "\" -map_channel 0.1.0 -map_channel 0.1.0 \"" + wavFile + "\"";
            }

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, args);
        }

        /// <summary>
        /// Ralentit un video par un ratio, et optionnellement élague l'image sur le côté (crop).
        /// Enlève l'audio du fichier.
        /// </summary>
        /// <param name="ffmpegLocation">L'exécutable de FFMPEG</param>
        /// <param name="inVideo">vidéo intrant</param>
        /// <param name="outVideo">vidéo résultat. Sera écrasé si existant.</param>
        /// <param name="speedRatio">Le ratio (pourcentage) de la vitesse voulue. La valeur de 1 est neutre.</param>
        /// <param name="finalFrameRate">Le taux de trames par seconde du fichier sortant. ET ce, quel que soit la vitesse du fichier intrant.</param>
        /// <param name="sideCrop">Optionnel, indique le nombre de pixel à retirer de chaque trame, en centrant le résultat.
        /// On retire donc la moitié de cette valeur également de chaque côté.</param>
        public static void SlowCropVideo([NotNull] string ffmpegLocation, [NotNull] string inVideo, [NotNull] string outVideo, double speedRatio, double finalFrameRate, int? sideCrop)
        {
            DeleteOutputFile(outVideo);

            var sb = new StringBuilder();

            sb.Append("-i \"");
            sb.Append(inVideo);
            sb.Append("\" -r ");
            sb.Append(StringUtil.FormatFloat(2, finalFrameRate));

            if (speedRatio != 1 || sideCrop != null)
            {
                sb.Append(" -filter:v ");
                sb.Append("\"");
                if (speedRatio != 1)
                {
                    sb.Append("setpts=");
                    sb.Append(StringUtil.FormatFloat(2, 1 / speedRatio));
                    sb.Append("*PTS");
                }
                if (sideCrop != null)
                {
                    if (speedRatio != 1)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("crop=in_w-");
                    sb.Append(sideCrop.Value);
                    sb.Append(":in_h");
                }
                sb.Append("\"");
            }

            sb.Append(" -an \"");
            sb.Append(outVideo);
            sb.Append("\"");

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, sb.ToString());
        }

        /// <summary>
        /// Ralentit un video par un ratio, et optionnellement élague l'image sur le côté (crop).
        /// Possède aussi l'option de mettre un overlay par dessus le tout.
        /// Enlève l'audio du fichier.
        /// </summary>
        /// <param name="ffmpegLocation">L'exécutable de FFMPEG</param>
        /// <param name="inVideo">vidéo intrant</param>
        /// <param name="outVideo">vidéo résultat. Sera écrasé si existant.</param>
        /// <param name="finalFrameRate">Le taux de trames par seconde du fichier sortant. ET ce, quel que soit la vitesse du fichier intrant.</param>
        /// <param name="overlayFile">On va ajouter cette image comme superposition pour l'ensemble du vidéo.</param>
        /// <param name="speedRatio">Le ratio (pourcentage) de la vitesse voulue. La valeur de 1 est neutre.</param>
        public static void OverlaySlowVideo([NotNull] string ffmpegLocation, [NotNull] string inVideo, [NotNull] string outVideo, double finalFrameRate, [NotNull] string overlayFile, double speedRatio)
        {
            DeleteOutputFile(outVideo);

            /* Code visé */
            // -i "C:\test\result\in.MP4"
            // -loop 1 -i "C:\test\overlay.png"
            // -r 30
            // -filter_complex
            // "[0:v][1:v]overlay=x=0:y=0:shortest=1,setpts=ratio*PTS[over];"
            // -map "[over]" "C:\test\result\out.MP4"

            var sb = new StringBuilder();

            sb.Append("-i \"");
            sb.Append(inVideo);
            sb.Append("\" -loop 1 -i \"");
            sb.Append(overlayFile);
            sb.Append("\" -r ");
            sb.Append(StringUtil.FormatFloat(2, finalFrameRate));

            if (speedRatio != 1)
            {
                sb.Append(" -filter_complex ");
                sb.Append("\"[0:v][1:v]overlay=x=0:y=0:shortest=1");
                if (speedRatio != 1)
                {
                    sb.Append(",setpts=");
                    sb.Append(StringUtil.FormatFloat(2, 1 / speedRatio));
                    sb.Append("*PTS");
                }
                sb.Append("[over]\"");
            }

            sb.Append(" -map \"[over]\" \"");
            sb.Append(outVideo);
            sb.Append("\"");

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, sb.ToString());
        }

        /// <summary>
        /// Obtient les informations de métadonnées propres à un fichier vidéo, qui doit être existant sur disque.
        /// </summary>
        /// <param name="ffprobeLocation">Exécutable ffprobe.</param>
        /// <param name="inVideo">Le vidéo à analyser. Si inexistant, retourne une structure non-initialisée.</param>
        /// <param name="specsFile">Optionel. Sauvegarde le fichier de specs.</param>
        /// <returns>Un référence vers les métadonnées. Null si le fichier video à analyser n'existe pas.</returns>
        [Obsolete("Méthode DANGEUREUSE susceptible de bloquer infiniment! Utilisez une version async avec temps maximal.")]
        public static VideoMetadata GetVideoFileMetadata([NotNull] string ffprobeLocation, FileInfo inVideo, FileInfo specsFile = null)
        {
            if (inVideo == null || !File.Exists(inVideo.FullName)) return null;

            return GetVideoFileMetadata(CancellationToken.None, ffprobeLocation, inVideo.FullName, specsFile).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Obtient les informations de métadonnées propres à cette entrée vidéo.
        /// </summary>
        /// <param name="ffprobeLocation">Exécutable ffprobe.</param>
        /// <param name="inSourceDesc">La source vidéo à analyser.</param>
        /// <param name="specsFile">Optionel. Sauvegarde le fichier de specs.</param>
        /// <returns>Un référence vers les métadonnées. Null si le fichier video à analyser n'existe pas.</returns>
        [Obsolete("Méthode DANGEUREUSE susceptible de bloquer infiniment! Utilisez une version async avec temps maximal.")]
        public static VideoMetadata GetVideoFileMetadata([NotNull] string ffprobeLocation, string inSourceDesc, FileInfo specsFile = null)
        {
            return GetVideoFileMetadata(CancellationToken.None, ffprobeLocation, inSourceDesc, specsFile).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Obtient les informations de métadonnées propres à cette entrée vidéo.
        /// ATTENTION: Il est important de spécifier un jeton valide à cette méthode car cette commande est extrêmement susceptible à bloquer de façon infinie!
        /// Si la source demandée par FFMPEG est valide mais fait un "timeout", cela peut provoquer un deadlock grave!
        /// Comme la fonction de timeout ne fonctionne pas dans la commande FFMPEG (c'est un bogue), il est impératif d'avoir une façon de tuer le processus!
        /// </summary>
        /// <param name="token">Le jeton de cancellation si jamais la source est valide mais de répond pas (timeout).</param>
        /// <param name="ffprobeLocation">Exécutable ffprobe.</param>
        /// <param name="inSourceDesc">La source vidéo à analyser.</param>
        /// <param name="specsFile">Optionel. Sauvegarde le fichier de specs.</param>
        /// <returns>Un référence vers les métadonnées. Null si le fichier video à analyser n'existe pas.</returns>
        public static async Task<VideoMetadata> GetVideoFileMetadata(CancellationToken token, [NotNull] string ffprobeLocation, string inSourceDesc, FileInfo specsFile = null)
        {
            var pc = new ProcessOutputConfig
            {
                ConversionMethod = VideoMetadata.FromTextFile,
                ConversionType = typeof(VideoMetadata),
                OutputFile = specsFile
            };
            var outPutConfig = new Dictionary<ProcessOutputEnum, ProcessOutputConfig>
            {
                { ProcessOutputEnum.StandardOut, pc}

            };

            var result = await LaunchProcessUtil.InvokeCliforTextResult(token, ffprobeLocation, $"-show_streams \"{inSourceDesc}\"", outPutConfig);

            if (!result) return null;
            return (VideoMetadata)pc.ConvertedInstance;
        }
        
        /// <summary>
        /// Obtient les informations de métadonnées propres à cette entrée vidéo.
        /// ATTENTION: Il est important de spécifier un temps approprié à cette méthode car cette commande est extrêmement susceptible à bloquer de façon infinie!
        /// Si la source demandée par FFMPEG est valide mais fait un "timeout", cela peut provoquer un deadlock grave!
        /// Comme la fonction de timeout ne fonctionne pas dans la commande FFMPEG (c'est un bogue), il est impératif d'avoir une façon de tuer le processus!
        /// </summary>
        /// <param name="maxWait">Le temps maximum d'Attente. Est utilisé pour créer un jeton de cancellation.</param>
        /// <param name="ffprobeLocation">Exécutable ffprobe.</param>
        /// <param name="inSourceDesc">La source vidéo à analyser.</param>
        /// <param name="specsFile">Optionel. Sauvegarde le fichier de specs.</param>
        /// <returns>Un référence vers les métadonnées. Null si le fichier video à analyser n'existe pas.</returns>
        public static async Task<VideoMetadata> GetVideoFileMetadata(TimeSpan maxWait, [NotNull] string ffprobeLocation, string inSourceDesc, FileInfo specsFile = null)
        {
            var cts = new CancellationTokenSource(maxWait);
            return await GetVideoFileMetadata(cts.Token, ffprobeLocation, inSourceDesc, specsFile);
        }

        /// <summary>
        /// Coupe un vidéo, d'un temps de début jusqu'à un temps de fin. Le vidéo est ré-encodé pour être exact sur le
        /// temps final du vidéo. Le vidéo sortant est encodé en H264, quel que soit le codec original.
        /// Cette méthode détruit l'audio du vidéo (ce qui sauve un peu d'espace et de temps).
        /// </summary>
        /// <param name="ffmpegLocation">Location de FFMPEG</param>
        /// <param name="inVideo">Vidéo intrant</param>
        /// <param name="outVideo">Vidéo coupé</param>
        /// <param name="start">Temps de départ, en secondes. Doit être strictement inférieur au temps de fin.</param>
        /// <param name="end">Temps de fin, en secondes.</param>
        public static void CutVideo([NotNull] string ffmpegLocation, [NotNull] string inVideo, [NotNull] string outVideo, double start, double end)
        {
            // http://superuser.com/questions/138331/using-ffmpeg-to-cut-up-video

            DeleteOutputFile(outVideo);

            if (start >= end) return;

            var sb = new StringBuilder();

            sb.Append("-i \"");
            sb.Append(inVideo);
            sb.Append("\" -ss ");
            sb.Append(StringUtil.FormatTime(start));
            sb.Append(" -t ");
            sb.Append(StringUtil.FormatTime(end - start));
            sb.Append(" -vcodec libx264 -an \"");
            sb.Append(outVideo);
            sb.Append("\"");

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, sb.ToString());
        }

        private static readonly char[] charsToEscape = { ':', '\\', '\'' };
        
        /// <summary>
        /// Ajoute des caractères pour "escaper" correctement les caractères réservés du lecteur de paramètres "filtergraph" de FFMPEG.
        /// Voir ce lien pour plus d'informations: http://ffmpeg.org/ffmpeg-filters.html#Notes-on-filtergraph-escaping
        /// Par example, la ligne suivante
        /// C:\test\result\bkg_mod.mov
        /// devient
        /// C\\\:\\\\test\\\\result\\\\bkg_mod.mov
        /// </summary>
        /// <param name="fileName">Le nom de fichier (ou tout autre type de paramètre en fait) à 'escaper'.</param>
        /// <returns>La chaîne modifiée et utilisable dans FFMPEG.</returns>
        public static string EscapePathForFilterParameter([NotNull] string fileName)
        {
            var sb = new StringBuilder();

            foreach (var c in fileName.ToCharArray())
            {
                if (charsToEscape.Contains(c))
                {
                    sb.Append(@"\\\");
                }
                sb.Append(c);
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// Fait une transformation chromatique sur une image, et met le résultat dans le fichier extrant, en l'écrasant au besoin.
        /// </summary>
        /// <param name="ffmpegLocation">Location de l'exécutable Ffmpeg.</param>
        /// <param name="inImage">L'image d'entrée</param>
        /// <param name="outImage">L'image de sortie.</param>
        /// <param name="chroma">L'information de transformation chromatique.</param>
        public static void ChromakeyImage([NotNull] string ffmpegLocation, [NotNull] string inImage, [NotNull] string outImage, [NotNull] FfmpegChromaKeyInfo chroma)
        {
            // https://ffmpeg.org/ffmpeg-filters.html#chromakey

            DeleteOutputFile(outImage);

            var str = $"-i \"{inImage}\" -vf chromakey={chroma.HexadecimalRbgCode()}:{StringUtil.FormatFloat(2, chroma.Similarity)}:{StringUtil.FormatFloat(2, chroma.Blend)} \"{outImage}\"";

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, str);
        }

        /// <summary>
        /// Fait une transformation chromatique sur une image, et met le résultat dans le fichier extrant, en l'écrasant au besoin.
        /// </summary>
        /// <param name="ffmpegLocation">Location de l'exécutable Ffmpeg.</param>
        /// <param name="inImage">L'image d'entrée</param>
        /// <param name="outImage">L'image de sortie.</param>
        /// <param name="red">Valeur entière rouge</param>
        /// <param name="green">Valeur entière verte</param>
        /// <param name="blue">Valeur entière bleu</param>
        /// <param name="similarity">Similarité entre les couleurs</param>
        /// <param name="blend">Niveau de transparence des pixels affectés.</param>
        public static void ChromakeyImage([NotNull] string ffmpegLocation, [NotNull] string inImage, [NotNull] string outImage, byte red, byte green, byte blue, float similarity, float blend)
        {
            ChromakeyImage(ffmpegLocation, inImage, outImage, new FfmpegChromaKeyInfo
            {
                Red = red,
                Green = green,
                Blue = blue,
                Similarity = similarity,
                Blend = blend
            });
        }

        /// <summary>
        /// Fait une transformation chromatique sur une image, et met le résultat dans le fichier extrant, en l'écrasant au besoin.
        /// </summary>
        /// <param name="ffmpegLocation">Location de l'exécutable Ffmpeg.</param>
        /// <param name="inImage">L'image d'entrée</param>
        /// <param name="outImage">L'image de sortie.</param>
        /// <param name="color">Couleur à modifier. La valeur alpha n'est pas prise en compte.</param>
        /// <param name="similarity">Similarité entre les couleurs</param>
        /// <param name="blend">Niveau de transparence des pixels affectés.</param>
        [PublicAPI]
        public static void ChromakeyImage([NotNull] string ffmpegLocation, [NotNull] string inImage, [NotNull] string outImage, Color color, float similarity, float blend)
        {
            ChromakeyImage(ffmpegLocation, inImage, outImage, new FfmpegChromaKeyInfo
            {
                Red = color.R,
                Green = color.G,
                Blue = color.B,
                Similarity = similarity,
                Blend = blend
            });
        }

        /// <summary>
        /// Extrait une image du vidéo, à un temps donné. Le temps n'est pas exact, il va prendre le "key frame" le plus proche pour rendre l'opération rapide
        /// et s'assurer que l'image donnée soit de bonne qualité (les key frames sont fait pour ça).
        /// Extrait le résultat dans le nom de fichier donné. Si ce fichier est vide une fois l'exécution terminée, c'est qu'il y a eu un problème.
        /// </summary>
        /// <param name="ffmpegLocation">L'exécutable de FFMPEG</param>
        /// <param name="videoFileName">Le fichier vidéo à faire l'extraction de.</param>
        /// <param name="frameTime">Le temps, en secondes, d'où prendre la trame. Approximatif.</param>
        /// <param name="frameFileName">Le nom du fichier d'image extrant.</param>
        /// <param name="newSize">Nouvelle dimension du fichier, au besoin.</param>
        [Obsolete("Utilisez une version avec cancellation. Ces versions offrent plus de fonctionalité aussi.")]
        public static void ExtractSingleFrame([NotNull] string ffmpegLocation, [NotNull] string videoFileName, float frameTime, [NotNull] string frameFileName, Size? newSize = null)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            ExtractSingleFrameAsync(cts.Token, ffmpegLocation, videoFileName, frameTime, frameFileName, newSize).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Extrait une image du vidéo, à un temps donné. Le temps n'est pas exact, il va prendre le "key frame" le plus proche pour rendre l'opération rapide
        /// et s'assurer que l'image donnée soit de bonne qualité (les key frames sont fait pour ça).
        /// Extrait le résultat dans le nom de fichier donné. Si ce fichier est vide une fois l'exécution terminée, c'est qu'il y a eu un problème.
        /// On peut donner l'option de spécifier une taille pour le fichier résultant. FFMPEG ne fera pas de vérification sur le ratio entre l'original et le fichier
        /// de sortie.
        /// </summary>
        /// <param name="ct">Jeton de cancellation</param>
        /// <param name="ffmpegLocation">L'exécutable de FFMPEG</param>
        /// <param name="videoFileName">Le fichier vidéo à faire l'extraction de.</param>
        /// <param name="frameTime">Le temps, en secondes, d'où prendre la trame. Approximatif.</param>
        /// <param name="frameFileName">Le nom du fichier d'image extrant.</param>
        /// <param name="newSize">Optionnel. Si on veut que l'image soit d'une dimension différente que le vidéo original.</param>
        public static async Task ExtractSingleFrameAsync(CancellationToken ct, [NotNull] string ffmpegLocation, [NotNull] string videoFileName, float frameTime, [NotNull] string frameFileName, Size? newSize = null)
        {
            // http://blog.roberthallam.org/2010/06/extract-a-single-image-from-a-video-using-ffmpeg/

            DeleteOutputFile(frameFileName);

            if (frameTime < 0) return;
            var sizeCommand = newSize == null ? "" : $"-s {StringUtil.FormatSize(newSize.Value)} "; // Laissez un espace après la commande pour espacer correctement.
            var str = $"-ss {StringUtil.FormatFloat(2, frameTime)} -i \"{videoFileName}\" -vframes 1 {sizeCommand}-f image2 \"{frameFileName}\"";

            await LaunchProcessUtil.LaunchProcessAsync(ct,  ffmpegLocation, str);
        }

        /// <summary>
        /// Cette méthode va appliquer sur un vidéo de base un nombre arbitraire d'image en superposition. En clair, le viédo est la
        /// couche de base, sur laquelle on va appliquer la première image, la deuxième sera appliquée sur le résultat, et ainsi de suite
        /// pour tous les fichiers restants.
        /// <br/>Toutes les images durent pour la totalité du vidéo.
        /// </summary>
        /// <param name="ffmpegLocation">L'exécutable FFMPEG</param>
        /// <param name="originalVideo">Le vidéo de base.</param>
        /// <param name="resultVideo">Le vidéo résultant.</param>
        /// <param name="overlayFiles">Un tableau de chemins d'images. Si une seule de ces images n'existe pas, la méthode ne fait rien.
        /// Ce tableau doit contenir au moins un élément.</param>
        public static void OverlayOnVideo([NotNull] string ffmpegLocation, [NotNull] string originalVideo, [NotNull] string resultVideo, [NotNull] string[] overlayFiles)
        {
            var commandStr = new StringBuilder();

            DeleteOutputFile(resultVideo);

            if (overlayFiles.Length < 1) return;

            commandStr.Append($"-i \"{originalVideo}\"");
            foreach (var str in overlayFiles)
            {
                if (!File.Exists(str))
                {
                    return; // Si ce n'est pas un fichier valide, la commande ne fera rien de toute façon. Alors on sort tout de suite.
                }
                commandStr.Append($" -i \"{str}\"");
            }
            commandStr.Append(" -filter_complex \"");

            var outChain = "";
            for (var i = 0; i < overlayFiles.Length; i++)
            {
                string baseOverlay;
                var newOverlay = $"{i + 1}:v";
                outChain = $"v{i + 1}";
                if (i > 0)
                {
                    commandStr.Append(';');
                    baseOverlay = $"v{i}";
                }
                else
                {
                    baseOverlay = $"{i}:v";
                }
                commandStr.Append($"[{baseOverlay}][{newOverlay}]overlay[{outChain}]");
            }
            commandStr.Append($"\" -map \"[{outChain}]\" \"{resultVideo}\"");

            LaunchProcessUtil.LaunchProcess(ffmpegLocation, commandStr.ToString());
        }

        /// <summary>
        /// Ajoute un flux audio par dessus un video.
        /// Cette méthode va s'assurer que la durée du son soit ajusté en fonction de la taille du vidéo. Si le vidéo est plus petit, le son sera coupé pour
        /// être égal à la durée du vidéo. Si le vidéo est plus long, le son sera "paddé" avec du silence jusqu'à la fin du vidéo.
        /// Si le vidéo avait déjà une trame sonore, elle sera remplacée par le fichier de trame sonore.
        /// Voir: https://www.ffmpeg.org/ffmpeg-filters.html#apad
        /// </summary>
        /// <param name="stringffmpegExecutable">L'exécutable FFMPEG</param>
        /// <param name="videoOut">Le vidéo final</param>
        /// <param name="videoIn">Le vidéo intrant à mettre le son dedans.</param>
        /// <param name="soundtrack">Le fichier de trame sonore.</param>
        public static void AddPaddedSoundtrack([NotNull] string stringffmpegExecutable, [NotNull] string videoOut, [NotNull] string videoIn, [NotNull] string soundtrack)
        {
            DeleteOutputFile(videoOut);

            var str = $"-i \"{videoIn}\" -i \"{soundtrack}\" -filter_complex \"[1:0]apad\" -shortest \"{videoOut}\"";

            LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, str);
        }

        /// <summary>
        /// Créer un vidéo à partir d'une énumération de fichiers.
        /// Un masque de fichiers valide prend la forme suivante: %03d.jpg
        /// ATTENTION: Dû à un bogue dans la librairie FFMPEG, on ne peut utiliser un masque avec un répertoire qui contient des caractères
        /// accentués. Si Cela pose problème, il faut que la machine qui lance l'application modifie sa variable d'environnement TMP et TMP
        /// pour un réperoire qui est lisible ASCII.
        /// ATTENTION 2: La séquence de fichier DOIT être séquentielle sans aucun trou ET DOIT commencer par la valeur de "startNumber". Cela implique une vérification
        /// préalable des fichiers, ce que ne fait pas cette méthode.
        /// </summary>
        /// <param name="fileNameMask">Le masque pour les noms de fichiers. Si n'est pas un masque, et va vers un fichier spécifique, ce seul fichier sera utilisé.</param>
        /// <param name="videoFileName">Le nom du vidéo résultant.</param>
        /// <param name="stringffmpegExecutable">L'endroit de l'exécutable de FFMPEG</param>
        /// <param name="directoryToFindFiles">Le répertoire où trouver les fichiers.</param>
        /// <param name="filePerSecond">Le nombre de fichier à faire entrer par secondes dans le vidéo.</param>
        /// <param name="framerate">La vitesse de trame. Comme il s'agit d'images fixe, on peut le fixer assez bas pour sauver de l'espace disque. Devrait être au moins le
        /// double de la variable de fichier par seconde pour éviter les problèmes d'affichage.</param>
        /// <param name="startNumber">Le numéro de la séquence pour débuter.</param>
        public static void CreateVideoFromPictureFiles([NotNull] string stringffmpegExecutable, [NotNull] DirectoryInfo directoryToFindFiles,
                                                       [NotNull] string fileNameMask, [NotNull] string videoFileName, double filePerSecond = 6.0, uint framerate = 15, uint startNumber = 1)
        {
            DeleteOutputFile(videoFileName);

            if (StringUtil.ContainsNonAscii(directoryToFindFiles.FullName)) {
                log.Warn("Path contains non-ascii caracters. This is not supported by FFMPEG masks and therefore video will not be created. Change temporary path for something compatible. Path=" + directoryToFindFiles.FullName);
                return;
            }

            var str = $"-framerate {StringUtil.FormatFloat(2, filePerSecond)} -start_number {startNumber} -i \"{directoryToFindFiles.FullName}{fileNameMask}\" -c:v libx264 -r {framerate} -pix_fmt yuv420p \"{videoFileName}\"";

            LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, str);
        }

        /// <summary>
        /// Cette méthode va préparer une liste de fichiers vers un répertoire de travail (qui sera vidé au moment de l'invocation) pour
        /// l'utilisation de la méthode "FfmpegUtil.CreateVideoFromPictureFiles".
        /// Cette méthode va créer une série de fichiers à partir de la liste original en les nommant de façon légale pour FFMPEG (c'est à dire, de "000.jpg" à
        /// "XXX.jpg", ou le nombre de chiffres est ici 3). L'extension utilisée sera la même que celle du premier fichier. De toute
        /// façon, pour que la commande FFMPEG fonctionne, l'extension doit être la même pour tous les fichiers. Le masque de fichier commence toujours avec zéro.
        /// Les fichiers originaux peuvent avoir le noms qu'ils veulent, mais l'ordre dans lequel ils sont dans la liste est primordial, car ils seront copiés
        /// en raison de leur index.
        /// Le nombre d'itération est le nombre de répétition totale voulue des trames. Le paramètre "loopBack" indique que l'on revient dans l'ordre des trames à l'envers
        /// quand on a atteint le dernier fichier. Une boucle de ce type équivaut à une seule répétition.
        /// Chaque boucle ne copie pas la première trame à la fin, de façon à ne pas afficher la première trame plus d'une fois quand on met chaque répétition bout-à-bout.
        /// Toutefois, pour finaliser, on ajoute la première trame à la toute fin du processus pour compléter la dernière boucle de la dernière répétition
        /// (seulement si on utilise un boucle).
        /// Le nombre de fichiers en sortie est égal à:
        /// Sans bouclage: (Répétitions) X (nombre d'images originales)
        /// Avec bouclage: ((Répétitions) X ((nombre d'images originales X 2) - 2)) + 1
        /// Ce nombre est utilisé pour calculer le nombre d'espace requis pour le nom de fichiers. "0000" implique 4 espaces. Cette valeur est retournée par la méthode.
        /// Si le nombre retourné est inférieur à un, une erreur s'est produite ou l'opération ne peut être complétée à cause d'arguments invalides.
        /// Exemples, en omettant les extensions de fichiers, et en mettant chaque répétition entre "[]" pour les illustrer:
        /// 10 fichiers, 1 répétition, pas de boucle, 2 chiffres =>
        /// [00-01-02-03-04-05-06-07-08-09]
        /// 4 fichiers, 2 répétitions, bouclage, 2 chiffres (notez la trame d'extra à la fin).
        /// [00-01-02-03-04-05]-[06-07-08-09-10-11]-12
        /// </summary>
        /// <param name="pics">La liste d'images originales.</param>
        /// <param name="workDir">La répertoire qui recevra les images qui serviront de trames pour le futur vidéo.</param>
        /// <param name="nbrIteration">Le nombre d'itération de chaque séquence de fichiers originaux.</param>
        /// <param name="loopBack">Indique si chaque séquence inclu une boucle de retour vers la trame originale.</param>
        /// <returns>Le nombre de caractères requis pour contenir les noms de fichiers (qui représentent des nombres.)</returns>
        public static int PrepareDirectoryForImageListVideoCreation(IList<FileInfo> pics, DirectoryInfo workDir, int nbrIteration = 1, bool loopBack = true)
        {

            // Validation des arguments.
            if (pics == null || !pics.Any()) return -1;
            if (nbrIteration < 1) return -1;
            if (workDir == null || !workDir.Exists) return -1;

            var ext = '.' + StringUtil.GetFileExtension(pics.First().Name);

            // 0- Vidage du répertoire, pour s'assurer de ne pas avoir de fichiers déjà existants qui pourrait créer des problèmes.
            foreach (var file in workDir.GetFiles())
            {
                file.Delete();
            }
            if (workDir.GetFiles().Any())
            {
                log.Error("Could not empty working directory: " + workDir.FullName);
                return -1; // N'a pas pu vider le répertoire
            }

            // 1- Calcul du nombre d'images final
            var sequenceLength = !loopBack || pics.Count == 1 ? pics.Count : (pics.Count * 2) - 2;
            var totalFiles = nbrIteration * sequenceLength + (loopBack ? 1 : 0);

            var nbrDigits = totalFiles.ToString().Length;
            var formatKey = "D" + nbrDigits;

            // Calcul d'une séquence d'index
            var indexSequence = Enumerable.Range(0, pics.Count).ToList();
            if (loopBack && sequenceLength > 2)
            {
                for (var i = pics.Count - 2; i > 0; i--) { indexSequence.Add(i); }
            }

            // Boucles
            var currentNbr = 0;
            for (var ite = 0; ite < nbrIteration; ite++)
            {
                foreach (var index in indexSequence)
                {
                    var pic = pics[index];
                    var copyFilePath = Path.Combine(workDir.FullName, currentNbr.ToString(formatKey) + ext);

                    if (File.Exists(copyFilePath)) File.Delete(copyFilePath);
                    File.Copy(pic.FullName, copyFilePath);
                    if (!File.Exists(copyFilePath))
                    {
                        log.Error("Could not copy file into working directory: ({0}) => ({1})", pic.FullName, copyFilePath);
                        return -1; // Vérification d'erreur.
                    }

                    currentNbr++;
                }
            }

            // Fichier final, si loopBack
            if (loopBack)
            {
                var pic = pics.First();
                var copyFilePath = Path.Combine(workDir.FullName, currentNbr.ToString(formatKey) + ext);

                if (File.Exists(copyFilePath)) File.Delete(copyFilePath);
                File.Copy(pic.FullName, copyFilePath);
                if (!File.Exists(copyFilePath))
                {
                    log.Error("Could copy final loopBack file into working directory: ({0}) => ({1})", pic.FullName, copyFilePath);
                    return -1; // Vérification d'erreur.
                }
                currentNbr++;
            }

            // Dernière vérification
            if (currentNbr == totalFiles) return nbrDigits;

            log.Error("Total number of files copied does not match expected number: {0}/{1}", currentNbr, totalFiles);
            return -1; // Vérification d'erreur.
        }

        /// <summary>
        /// Cette méthode prend un video en entrée en va le modifier pour que sa taille respecte celle passée en paramètre.
        /// Cela implique une re-compression, et si l'extension de fichier change, cette méthode peut aussi être utilisée comme
        /// transformation d'un format à un autre.
        /// </summary>
        /// <param name="stringffmpegExecutable">L'exécutable FFMPEG.</param>
        /// <param name="inVideo">Vidéo de base.</param>
        /// <param name="outVideo">Vidéo résultt.</param>
        /// <param name="width">La nouvelle largeur. Doit être un multiple de deux et positif non-nul.</param>
        /// <param name="height">La nouvelle hauteur. Doit être un multiple de deux et positif non-nul.</param>
        public static void ResizeVideo([NotNull] string stringffmpegExecutable, [NotNull] string inVideo,  [NotNull] string outVideo, int width, int height) {
            if (width < 2 || width % 2 == 1 || height < 2 || height % 2 == 1) return;

            DeleteOutputFile(outVideo);

            var str = $"-i \"{inVideo}\" -vf scale={width}:{height} -c:a copy  \"{outVideo}\"";

            LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, str);
        }
        
        /// <summary>
        /// Obtient les sources DirectShow (A.K.A.: les webcams) présente sur le système.
        /// </summary>
        /// <param name="stringFfmpegExecutable">L'exécutable FFMPEG.</param>
        /// <param name="specsFile">Optionel. Un fichier où stocker le résultat brut de la commande.</param>
        /// <returns>Un objet avec toutes les sources DirectShow trouvées par FFMPEG.</returns>
        public static VideoDirectShowSources FindDirectShowSources([NotNull] string stringFfmpegExecutable, FileInfo specsFile = null)
        {
            var pc = new ProcessOutputConfig
            {
                ConversionMethod =  VideoDirectShowSources.FromTextFile,
                ConversionType = typeof(VideoDirectShowSources),
                OutputFile = specsFile
            };
            var outPutConfig = new Dictionary<ProcessOutputEnum, ProcessOutputConfig>
            {
                { ProcessOutputEnum.StandardError, pc}

            };

            var result = LaunchProcessUtil.InvokeCliforTextResult(CancellationToken.None, stringFfmpegExecutable, "-list_devices true -f dshow -i dummy", outPutConfig)
                .GetAwaiter().GetResult();

            if (!result) return null;
            return (VideoDirectShowSources)pc.ConvertedInstance;
        }

        /// <summary>
        /// Obtient les informations d'options d'une source DirectShow. Cette invocation va remplir les options de l'instance passée en paramètre, tout en retournant le référence de ces
        /// options de source.
        /// </summary>
        /// <param name="stringFfmpegExecutable">L'exécutable de FFMPEG.</param>
        /// <param name="device">L'instance descriptive de la source DirectShow à analyser.</param>
        /// <param name="specsFile">Optionel. Un fichier qui contient la valeur brute de l'exécution de la commande.</param>
        /// <returns>Une référence vers les options détectées.</returns>
        public static FfmpegDirectShowDeviceSourceOptions UpdateDirectShowSourceData([NotNull] string stringFfmpegExecutable,
            [NotNull] FfmpegDirectShowDeviceSource device, FileInfo specsFile = null)
        {
            if (device.DeviceType == FfmpegDirectShowDeviceSourceType.Unknown) return null;

            var comm = $" -f dshow -list_options true -i {device.DeviceType.ToString().ToLowerInvariant()}=\"{device.Name}\"";

            var pc = new ProcessOutputConfig
            {
                ConversionMethod =  FfmpegDirectShowDeviceSourceOptions.FromTextFile,
                ConversionType = typeof(FfmpegDirectShowDeviceSourceOptions),
                OutputFile = specsFile
            };
            var outPutConfig = new Dictionary<ProcessOutputEnum, ProcessOutputConfig>
            {
                { ProcessOutputEnum.StandardError, pc}

            };

            var result = LaunchProcessUtil.InvokeCliforTextResult(CancellationToken.None, stringFfmpegExecutable, comm, outPutConfig)
                .GetAwaiter().GetResult();
            
            if (!result) return null;
            var opt = (FfmpegDirectShowDeviceSourceOptions)pc.ConvertedInstance;
            device.Options = opt;
            
            return opt;
        }

        /// <summary>
        /// Créer un vidéo en appellant FFMPEG sur une source DirectShow.
        /// </summary>
        /// <param name="token">Jeton de cancellation de l'opération.</param>
        /// <param name="execLocationFfmpeg">L'exécutable FFMPEG.</param>
        /// <param name="videoSource">L'instance descriptive de la source à utiliser. Doit être une source vidéo.</param>
        /// <param name="videoCanal">L'option de capture vidéo à utiliser.</param>
        /// <param name="time">Le temps de capture, en secondes.</param>
        /// <param name="outPutFile">Le fichier résultant.</param>
        /// <param name="verticalFlip">Indique s'il faut faire un flip du flux vidéo (certaines webcam envoient un flux inversé par défaut).</param>
        /// <param name="bufferSize">La taille mémoire du buffer de capture. Par défaut, 100 MB. Utiliser un format minimal (EX: 1024 MB, 9000k) d'octets.</param>
        /// <param name="bitrate">La vitesse de lecture du fichier vidéo final. Influe sur la vitesse d'encodage et la qualité finale. Par défaut, 1800k</param>
        /// <param name="stdErrOutput">Au besoin, sauvegarde les messages de FFMPEG dans un fichier externe.</param>
        /// <returns>Indique si les configurations était correcte et que la commande est lancée. Il faut plutôt vérifier l'état du fichier résultant pour
        /// savoir si tout s'est bien passé.</returns>
        public static async Task<bool> CaptureVideoFromDirectShowSource(CancellationToken token, [NotNull] string execLocationFfmpeg,
            [NotNull] FfmpegDirectShowDeviceSource videoSource,
            [NotNull] FfmpegDirectShowDeviceSourceVideoOptionData videoCanal, 
            double time,
            [NotNull] FileInfo outPutFile,
            bool verticalFlip = false,
            string bufferSize = null,
            string bitrate = null,
            FileInfo stdErrOutput = null)
        {
            if (videoSource.DeviceType != FfmpegDirectShowDeviceSourceType.Video)
            {
                log.Warn("Source is not a video type.");
                return false;
            }

            if (time < 0.05)
            {
                log.Warn("Time too short.");
                return false;
            }

            DeleteOutputFile(outPutFile.FullName);

            string bRate;

            if (videoCanal.VideoCodec == null)
            {
                if (bitrate != null)
                {
                    bRate = "-b:v " + bitrate;
                }
                else
                {
                    bRate = "-c:v libx264 -crf 23";
                }

                if (videoCanal.PixelFormat != null)
                {
                    bRate += " -pix_fmt " + videoCanal.PixelFormat;
                }
            }
            else
            {
                bRate = "-c:v copy";
            }

            var bSize = bufferSize ?? "100M";
            var vFlip = verticalFlip ? "-vf vflip " : string.Empty;
            var duration = StringUtil.FormatTime(time);
            
            var str = $"-f dshow -rtbufsize {bSize} -video_size {videoCanal.SizeDesc} -framerate {videoCanal.FpsRange[1].ToString("0.00", CultureInfo.InvariantCulture)} -i video=\"{videoSource.Name}\" {vFlip}-threads 0 {bRate} -t {duration} \"{outPutFile}\"";

            await LaunchProcessUtil.LaunchProcessAsync(token, execLocationFfmpeg, str, null, stdErrOutput);

            return true;
        }

        /// <summary>
        /// Obtient un string, séparé par ligne, de chaque source DIrect Show détectée et de ses capacités.
        /// Ceci est un outil de débogage.
        /// </summary>
        /// <param name="execLocationFfmpeg">Exécutable FFMPEG</param>
        /// <param name="sources">Les sources à analyser.</param>
        /// <param name="tabsPerLine">Le nombre de tabultaions par ligne.</param>
        /// <returns>Un string avec toute l'information.</returns>
        public static string GetDirectShowSourceDebugData([NotNull] string execLocationFfmpeg, VideoDirectShowSources sources, int tabsPerLine = 0)
        {
            var sb = new StringBuilder();
            var tabsString = tabsPerLine != 0 ? new string('\t', tabsPerLine) : null;

            if (sources == null)
            {
                if (tabsString != null)
                {
                    sb.Append(tabsString);
                }
                sb.Append("Sources object is null.");
                return sb.ToString();
            }

            if (sources.VideoDevices.Any())
            {
                if (tabsString != null)
                {
                    sb.Append(tabsString);
                }
                sb.AppendLine("Direct show video sources detected by FFMPEG:");
                foreach (var vd in sources.VideoDevices)
                {
                    if (vd.Options == null)
                    {
                        UpdateDirectShowSourceData(execLocationFfmpeg, vd); // Mise à jour
                    }

                    if (!(vd.Options is FfmpegDirectShowDeviceSourceVideoOptions vOpt)) continue;

                    if (tabsString != null)
                    {
                        sb.Append(tabsString);
                    }
                    sb.AppendLine($"{vd.Name}, alt='{vd.AlternativeName}'");
                    
                    foreach (var vo in vOpt.VideoOptions)
                    {
                        if (tabsString != null)
                        {
                            sb.Append(tabsString);
                        }
                        sb.AppendLine($"- Size: {vo.SizeDesc}, Fps:[{vo.FpsMaxDesc}, pfmt:{vo.PixelFormat}, codec:{vo.VideoCodec}]");
                    }
                }
            }

            // TODO Audio sources

            return sb.ToString();
        }
    }
}
