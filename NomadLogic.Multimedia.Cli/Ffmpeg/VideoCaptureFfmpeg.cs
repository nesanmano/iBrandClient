using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia.Video;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    /// <summary>
    /// Variation d'un script de capture d'un flux vidéo via FFMPEG.
    /// On fonctionne sous base de script avec deux variables:
    /// - Q2: qui est le UTR complet (directshow, RTSP au autre) d'où saisir un flux vidéo.
    /// - Q1: qui est le temps en secondes fractionnaires.
    /// La variable "VideoLength" du parent de cette classe est utilisé pour la variable de temps.
    /// </summary>
    /// 
    [JsonConverter(typeof(VideoCaptureFfmpegConverter))]
    public class VideoCaptureFfmpeg : VideoCaptureSourceConfigurationBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Nom du fichier de scriptage pour cette instance de configuration.
        /// </summary>
        public string ScriptFileName { get; set; }

        /// <summary>
        /// Url de capture complet pour obtenir un flux d'image.
        /// Ce URL fait partie du script comme étant la variable "Q2"
        /// </summary>
        public string CaptureUrl { get; set; }

        /// <summary>
        /// Exécution des bases de la commande FFMPEG.
        /// </summary>
        public virtual async Task<bool> LaunchFfmpegExecution(CancellationToken token, string ffmpegExec, string ffmpegProbe, string directoryTemp, Action<string> triggerReport = null)
        {
            var resultPath = Path.Combine(directoryTemp, VideoName);

            if (!await ExecuteFfmpegCommand(token, ffmpegExec, ffmpegProbe, directoryTemp, resultPath)) return false;

            // On teste maintenant que le fichier résultat est valide.
            var fileTest = new FileInfo(resultPath);

            await FileUtil.CheckAndWaitForFileLock(fileTest);
            
            if (!fileTest.Exists)
            {
                log.Error("Ffmpeg capture failed.");
                return false;
            }
            
            if (log.IsDebugEnabled && triggerReport != null)
            {
                var vmd = await FfmpegUtil.GetVideoFileMetadata(token, ffmpegProbe, resultPath);
                var mvsd = vmd.GetMainVideoStreamData();
                var message = $"New video data: Disk={MathUtil.ByteNumberReadable(fileTest.Length)}, Nbr.Frames={vmd.GetVideoFrameCount()}, Fps={mvsd.FrameRate}, bitrate={MathUtil.ByteNumberReadable(mvsd.BitRate)}";
                triggerReport(message);
            }

            if (fileTest.Exists && fileTest.Length >= 100)
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Exécution d'un script, qui est la base d'une exécution de FFMPEG.
        /// Si on veut utiliser une autre méthode, ou simplement appeller une fonction d'une librarie interne de FfmpegUtil, il faut
        /// surcharger cette méthode.
        /// </summary>
        protected virtual async Task<bool> ExecuteFfmpegCommand(CancellationToken token, string ffmpegExec, string ffmpegProbe, string directoryTemp, string resultPath)
        {
            log.Trace("Capture from FFMPEG script launched.");

            var dico = new Dictionary<QuestionIdentifier, Answer>
            {
                {new QuestionIdentifier(1), new Answer {Response = StringUtil.FormatTime(VideoLength)}},
                {new QuestionIdentifier(2), new Answer {Response = CaptureUrl}}
            };

            var scriptPath = DirectoryUtil.GetLocalApplicationDir() + ScriptFileName;
            

            var tokenCommand = FileUtil.ReadFileToStringSingleLine(scriptPath);
            var modifiedCommandLine = StringUtil.CreateStringFromTokenizedPatternRespondent(tokenCommand, dico,
                null, null, null, resultPath);

            if (string.IsNullOrWhiteSpace(modifiedCommandLine))
            {
                log.Error("Modified arguments for commands line are invalid.");
                return false;
            }

            await LaunchProcessUtil.LaunchProcessAsync(token, ffmpegExec, modifiedCommandLine);
            return true;
        }
    }
}