using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    /// <summary>
    /// Variation d'une capture FFMPEG via directshow.
    /// Pour cette façon, il faut avant tout valider l'existence de sources DirectShow, puis sélectionner la meilleure selon les configs, puis
    /// exécuter une méthode directe via la classe utilitaire.
    /// </summary>
    [PublicAPI]
    public class VideoCaptureFfmpegDirectShow : VideoCaptureFfmpeg
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static VideoDirectShowSources _sourcesMaster;
        private bool _masterSourceAlreadyChecked;

        private FfmpegDirectShowDeviceSource _sourceCandidate;
        private FfmpegDirectShowDeviceSourceVideoOptionData _sourceVideoCandidate;
        

        /// <summary>
        /// Indique la vitesse de trame désirée.
        /// </summary>
        public double DesiredMaxFps { get; set; } = 15;

        /// <summary>
        /// Indique la taille de vidéo désiré.
        /// </summary>
        public Size DesiredVideoSize { get; set; } = new Size(1920, 1080);

        public override bool Init(string ffmpegLocation)
        {
            if (_sourcesMaster == null)
            {
                // On ne fait que la vérification une seule fois des sources DirectShow, pour cette classe.
                if (_masterSourceAlreadyChecked)
                {
                    _sourcesMaster = FfmpegUtil.FindDirectShowSources(ffmpegLocation);
                    _masterSourceAlreadyChecked = true;
                }
                
                if (_sourcesMaster == null)
                {
                    return false;
                }
            }

            if (!_sourcesMaster.VideoDevices.Any())
            {
                return false;
            }

            if (_sourcesMaster.GetBestVideoFeed(DesiredVideoSize, DesiredMaxFps, out _sourceCandidate, out _sourceVideoCandidate, ffmpegLocation)) return true;

            var message = FfmpegUtil.GetDirectShowSourceDebugData(ffmpegLocation, _sourcesMaster, 1);
            log.Error($"Could not identifiy a candidate for direct show source with size '{DesiredVideoSize.Width}x{DesiredVideoSize.Height}' and with max FPS at {DesiredMaxFps:0.00}. Here is a list of detecte devices:\r\n{message}");
            return false;
        }

        protected override async Task<bool> ExecuteFfmpegCommand(CancellationToken token, string ffmpegExec, string ffmpegProbe, string directoryTemp, string resultPath)
        {
            log.Trace("Capture from webcam launched.");
            return await FfmpegUtil.CaptureVideoFromDirectShowSource(token, ffmpegExec, _sourceCandidate, _sourceVideoCandidate, VideoLength, new FileInfo(resultPath));
        }
    }
}