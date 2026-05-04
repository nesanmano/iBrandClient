using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.App.EventVideoCapture.Video;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Video;

namespace NomadLogic.App.EventVideoCapture.Capture
{
    /// <summary>
    /// Implémentation de base pour une capture vidéo via CLI avec le logiciel FFMPEG.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(CaptureVideoBaseConverter))]
    public abstract class CaptureVideoBase : IVideoCaptureManager<VideoCaptureFfmpeg>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly List<IProgress<VideoCaptureReport>> _observersList = new List<IProgress<VideoCaptureReport>>();
        protected VideoCaptureReport _lastMessage;
        private volatile bool _captureLaunched;
        private volatile bool _triggerDetected;
        
        [JsonIgnore]
        public string ConfigFileName { get; set; }
        public abstract string DefaultConfigFileName { get; }
        
        /// <summary>
        /// Référence vers le traitement video ultérieur.
        /// Cette référence est nécessaire si on veut faire un vidéo de présentation rapide.
        /// </summary>
        [JsonIgnore]
        public VideoTreatmentAbstract VideoTreatment { get; set; }

        [JsonIgnore]
        public string PreviewDirectory { get; set; }

        public VideoCaptureConfiguration<VideoCaptureFfmpeg> CaptureConfiguration { get; set; }

        [JsonIgnore]
        public bool IsRunning => true;
        
        public TimeSpan DelayAfterTrigger { get; set; }

        public bool Validate(DirectoryInfo dirToCheck)
        {
            if (dirToCheck == null || !dirToCheck.Exists) return false;

            var fileList = dirToCheck.GetFiles();

            var res =  CaptureConfiguration.SourcesConfigurations.All(sc => fileList.Any(f => f.Name.Equals(sc.VideoName)));

            if (!res)
            {
                log.Warn("Validation failed. Not all necessary files were found.");
            }
            return res;
        }

        protected string _execLocation;
        protected string _execProbeLocation;
        protected string _previewDirectory;
        protected string _tmpDirectory;
        private NomadConfiguration _nomadConfig;

        public bool Init()
        {
            if (CaptureConfiguration == null || !CaptureConfiguration.SourcesConfigurations.Any())
            {
                log.Error("No video capture source definition found.");
                return false;
            }

            _nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
            _execLocation = _nomadConfig.FfmpegLocation;
            _execProbeLocation = _nomadConfig.FfprobeLocation;

            if (!File.Exists(_execLocation) || !File.Exists(_execProbeLocation)) return false;

            if (CaptureConfiguration.CreatePreviewVideo && VideoTreatment == null)
            {
                // On ne peut pas créer un vidéo de présentation sans avoir accès à la classe de traitement.
                log.Error("No video treatment instance found. Review configfuration and int setup.");
                return false;
            }

            // Si on n'a pas assigné un répertoire spécifique pour la sauvegarde de déboguage, on le fait ici avec un répertoire par défaut.
            if (CaptureConfiguration.DebugSaveOriginalBasePath == null)
            {
                CaptureConfiguration.DebugSaveOriginalBasePath = new DirectoryInfo(DirectoryUtil.GetLocalApplicationSuspectMediaDir());
            }

            _previewDirectory = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir();
            _tmpDirectory = DirectoryUtil.GetLocalApplicationWorkDir();

            // Vérifie que toutes les configs de script ont un nom de fichier sortant différents.
            if (!CaptureConfiguration.SourcesConfigurations.AllUniqueBy(sc => sc.VideoName))
            {
                log.Warn("Not all final video names to create are unique. They will clash. COnfiguration rejected.");
                return false;
            }

            // Vérifie que chaque config à des initialisation valides (comme DirectShow qui doit faire une "scan" des sources disponibles).
            if (CaptureConfiguration.SourcesConfigurations.Any(sc => !sc.Init(_execLocation)))
            {
                log.Warn("One of the capture configurations failed to initialize.");
                return false;
            }

            // Vérifie qu'aucune forme de délais n'est négative. Par défaut, ce délais est zéro.
            if (CaptureConfiguration.SourcesConfigurations.Any(sc => sc.CaptureDelay < TimeSpan.Zero))
            {
                log.Warn("Negative delay detected in capture configurations.");
                return false;
            }

            return InitExtra();
        }

        public void Dispose()
        {
            _observersList.Clear();
        }

        protected abstract bool InitExtra();

        public void UpdateWatcherList(IList<ExternalStateWatcher> watcherList)
        {
            // No-Op
        }

        public void RemoveObserver(IProgress<VideoCaptureReport> observer)
        {
            lock (_observersList)
            {
                _observersList.Remove(observer);
            }
        }

        public void AddObserver(IProgress<VideoCaptureReport> observer)
        {
            lock (_observersList)
            {
                _observersList.Add(observer);
            }
        }

        public void TriggerReport(VideoCaptureReport report, bool dontCopy = false)
        {
            lock (_observersList)
            {
                _lastMessage = dontCopy ? report : report.Copy();
                foreach (var progress in _observersList)
                {
                    progress.Report(_lastMessage);
                }
            }
        }

        public void SendLastReport()
        {
            TriggerReport(_lastMessage);
        }

        public void TriggerEvent()
        {
            if (_captureLaunched)
            {
                _triggerDetected = true;
            }
        }

        public async Task StartCapture(CancellationToken ct, DirectoryInfo baseDirectory)
        {
            var checkDelay = TimeSpan.FromMilliseconds(20);
            
            // Dès que l'on démarre une capture, il faut attendre le trigger pour un max de temps.
            _triggerDetected = false;
            _captureLaunched = true;
            log.Trace("Waiting for trigger event: {0}", baseDirectory.Name);
            // Le délai maximal pour une détection est déjà géré par le jeton donné par l'appellant.
            try
            {
                while (!ct.IsCancellationRequested && !_triggerDetected)
                {
                    await Task.Delay(checkDelay, ct); // Check à chaque 20 ms
                }
            }
            catch (TaskCanceledException)
            {
                // Normal, on ne fait rien
            }
            catch (Exception e)
            {
                log.Error(e, "Error during trigger wait  time.");
                _captureLaunched = false;
                _triggerDetected = false;
                return;
            }
            log.Trace("Trigger event detected. Continuing: {0}", baseDirectory.Name);

            if (ct.IsCancellationRequested && !_triggerDetected)
            {
                log.Info("Delay reached, no trigger event detected. Cancelling capture task.");
                _captureLaunched = false;
                _triggerDetected = false;
                return;
            }

            if (DelayAfterTrigger > TimeSpan.Zero) // Si le délais n'est pas nul ou négatif.
            {
                if (log.IsTraceEnabled)
                {
                    log.Trace($"Waiting for delay timer to expire ({DelayAfterTrigger.ToStringHourMinSec()})");
                }

                await Task.Delay(DelayAfterTrigger, ct); // Temps mort de timing entre trigger et capture.
            }
            
            await ExecuteCapture(ct, baseDirectory, _tmpDirectory);

            if (CaptureConfiguration.DebugSaveOriginals)
            {
                FileInfo currentFile = null;
                try
                {
                    foreach (var sc in CaptureConfiguration.SourcesConfigurations)
                    {
                        currentFile = new FileInfo(Path.Combine(baseDirectory.FullName, sc.VideoName));
                        await FileUtil.CheckAndWaitForFileLock(currentFile);
                        var saveto = Path.Combine(CaptureConfiguration.DebugSaveOriginalBasePath.FullName, sc.VideoName);
                        File.Copy(currentFile.FullName, saveto);

                        if (log.IsInfoEnabled)
                        {
                            var message = "Original file saved to: " + saveto;
                            log.Info(message);
                            TriggerReport(new VideoCaptureReport(message));
                        }
                    }
                }
                catch (Exception e)
                {
                    var message = "Orginals save sequence had error at file: " + currentFile?.Name;
                    log.Error(e, message);
                    TriggerReport(new VideoCaptureReport(message));
                }
            }

            if (CaptureConfiguration.CreatePreviewVideo)
            {
                var errorMessage = VideoTreatment?.CreateVideoPreview(baseDirectory, _tmpDirectory, _previewDirectory);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    var message = $"Was unable to create preview video from event: {errorMessage}";
                    log.Error(message);
                    TriggerReport(new VideoCaptureReport(message));
                }
            }

            _captureLaunched = false;
            _triggerDetected = false;
        }

        /// <summary>
        /// La méthode à implémenter pour la capture vidéo. Assume que le répertoire de base existe, mais pas encore le répertoire où stocker les vidéos.
        /// </summary>
        /// <param name="token">Le jeton de cancellation pour toute l'opération.</param>
        /// <param name="baseDir">Le répertoire de base, qui contiendra le nouveau répertoire temporaire.</param>
        /// <param name="directoryTempName">Le nom du répertoire temporaire où stocker tous les fichiers de capture.</param>
        protected abstract Task ExecuteCapture(CancellationToken token, DirectoryInfo baseDir, string directoryTempName);
    }
}