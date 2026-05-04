using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;

// ReSharper disable LocalizableElement

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI.JobScheduler
{
    /// <summary>
    /// Cette classe est créée pour tenter de traiter une multitude d'appels asynchrones vers le CLI de ImageMagick via une seule file d'exécution.
    /// Vu que ce concept est proche d'un singleton, nous allons utiliser la structure de Services déjà mis en place.
    /// La raison derrière ceci est que le CLI semble avoir de la difficulté à gérer des accès concurrents à certaines ressources, et que cela provoque des erreurs au niveau de l'exécution.
    /// La solution plus simple à ce problème est de limiter à une seule exécution via une file d'exécution asynchrone, suivant le bon vieux système de producteur/consommateur.
    /// </summary>
    public class ImageMagickJobSchedulerService : ServiceLongRunningAbstract<ImageMagickJobReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        // ReSharper disable once InconsistentNaming
        private readonly ConcurrentQueue<ImageMagickJobDefinition> _mainQueue = new ConcurrentQueue<ImageMagickJobDefinition>();

        private ManualResetEventSlim NewJobAdded { get; } = new ManualResetEventSlim(false);

        private long _nbrFilesDownloaded;

        public void AddNewJob([NotNull] ImageMagickJobDefinition job)
        {
            _mainQueue.Enqueue(job);
            NewJobAdded.Set();
            if (log.IsTraceEnabled)
            {
                log.Trace("Job added. Queue has now {0} elements.", _mainQueue.Count);
            }
        }

        public override string ServiceName { get; protected set; } = "ImageMagick CLI Job Scheduler";
        
        protected override void OnDispose()
        {
            _mainQueue.Clear();
        }

        protected override bool DoInit()
        {
            return true;
        }

        protected override Task<bool> DoMainLoop(CancellationToken ct)
        {
            log.Debug("Main loop started for ImageMagickJobScheduler.");

            while (!ct.IsCancellationRequested)
            {
                // Attends un nouveau événement de téléchargement.
                try
                {
                    NewJobAdded.Wait(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (ct.IsCancellationRequested) break;

                log.Trace("New ImageMagick Job request event triggered.");
                NewJobAdded.Reset();
                
                if (!_mainQueue.Any())
                {
                    continue;
                }

                while (_mainQueue.TryDequeue(out var nextJob))
                {
                    var current = _mainQueue.Count;
                    log.Trace("Launching IM job. Left in Queue: {0}", current);
                    var rez = nextJob.Execute();
                    if (rez) _nbrFilesDownloaded++;
                    
                    var mess = $"Finished job execution. {(rez ? "Job successful." : "Error during job.")}";
                    var messError = rez ? nextJob.ErrorMessage : null;

                    UpdateAndSendReport(mess, messError);
                }
            }

            log.Debug("ImageMagick Scheduler service main cycle stopped.");
            return Task.FromResult(true);
        }

        protected override bool CheckIfIdle()
        {
            return !_mainQueue.Any();
        }

        private void UpdateAndSendReport(string message = null, string errorMessage = null)
        {
            var info = new ImageMagickJobReport
            {
                TotalCliCallsCompleted = _nbrFilesDownloaded,
                MessageError = errorMessage,
                Message = message
            };

            TriggerReport(info, true);
        }
    }

    public class ImageMagickJobReport : IProgressDeepCopy<ImageMagickJobReport>
    {
        public string Message { get; set; }
        public string MessageError { get; set; }
        public long TotalCliCallsCompleted { get; set; }

        public ImageMagickJobReport Copy()
        {
            return new ImageMagickJobReport
            {
                Message = Message,
                MessageError = MessageError,
                TotalCliCallsCompleted = TotalCliCallsCompleted
            };
        }
    }
}