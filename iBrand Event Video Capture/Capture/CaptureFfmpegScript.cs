using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Video;

namespace NomadLogic.App.EventVideoCapture.Capture
{
    public class CaptureFfmpegScript : CaptureVideoBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override string DefaultConfigFileName => "capture_script_ffmpeg.json";

        protected override bool InitExtra()
        {
            foreach (var sc in CaptureConfiguration.SourcesConfigurations)
            {
                var path = DirectoryUtil.GetLocalApplicationDir() + sc.ScriptFileName;

                // Test si on a un fichier de script et un URL de capture valide
                if (!File.Exists(path))
                {
                    log.Warn("No script file found in FFMPEG video capture configuration.");
                    return false;
                }
                
                if (sc.CaptureUrl == null)
                {
                    log.Warn("No capture URL found in FFMPEG video capture configuration.");
                    return false;
                }

                // TODO Incapable de faire fonctionner un test avec FFMPEG qui "hang", même avec une sécurité de Token. Quelque chose ne fonctionne pas...
                return true;

                // On teste maintenant avec FFPROBE que le URL pointe sur quelque chose de valide.
                var metaData = FfmpegUtil.GetVideoFileMetadata(TimeSpan.FromSeconds(3),  _execProbeLocation, sc.CaptureUrl).GetAwaiter().GetResult();
                if (metaData == null)
                {
                    return false;
                }
            }

            return true;
        }

        protected override async Task ExecuteCapture(CancellationToken token, DirectoryInfo baseDir, string directoryTempName)
        {
            var captCount = CaptureConfiguration.SourcesConfigurations.Count;
            var taskList = new Task[CaptureConfiguration.SourcesConfigurations.Count];

            var actionReport = new Action<string>(m =>
            {
                TriggerReport(new VideoCaptureReport(m), true);
            });

            for (var i = 0; i < captCount; i++)
            {
                var sc = CaptureConfiguration.SourcesConfigurations[i];
                taskList[i] = Task.Run(async () =>
                {
                    log.Debug("Starting capture script: {0}", sc.SourceName);
                    // Calcul d'un délai supplémentaire pour chaque instance de demande de capture.
                    if (sc.CaptureDelay > TimeSpan.Zero)
                    {
                        log.Trace("Delaying capture script: {0}", sc.SourceName);
                        await Task.Delay(sc.CaptureDelay, token);
                        if (token.IsCancellationRequested) return;
                    }

                    if (token.IsCancellationRequested)
                    {
                        log.Info("Script execution cancelled by token: {0}", sc.SourceName);
                        return;
                    }

                    log.Trace("Launching FFMPEG CLI command: {0}", sc.SourceName);
                    await sc.LaunchFfmpegExecution(token, _execLocation, _execProbeLocation, baseDir.FullName, actionReport);
                    log.Trace("Ending capture script: {0}", sc.SourceName);
                }, token);
            }
            
            await Task.WhenAll(taskList);
            log.Debug("All scripts ({0}) finished.", captCount);
        }
    }
}