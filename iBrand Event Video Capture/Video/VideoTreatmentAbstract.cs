using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.App.EventVideoCapture.config;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.ThreadPool;

namespace NomadLogic.App.EventVideoCapture.Video
{
    public abstract class VideoTreatmentAbstract : 
        CustomWorker<VideoTreatmentParameter, VideoTreatmentResult, CustomWorkerPool<VideoTreatmentParameter, VideoTreatmentResult>>
    {
        // noms de métadonnées universellement utiles.
        public const string RESPONDENT_ID_KEY = "respondent_id";
        public const string DATE_TIME = "date_time";

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private Stopwatch _intermediateSw;
        private string _intermediateSwName;
        
        public string WorkerName { get; set; }

        public ServerConfiguration ServerConf { get; set; }

        public VideoCaptureConfig ProjConfig { get; set; }

        public override void DoWork(CancellationToken token)
        {
            Stopwatch fullStopwatch = null;

            if (log.IsTraceEnabled)
            {
                fullStopwatch = new Stopwatch();
                fullStopwatch.Start();
            }

            try
            {
                Success = Launch(token).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                LoggerReference.LogEvent("Exception caught in worker. Id = " + WorkerName);
                log.Error(e);
            }
            finally
            {
                StopIntermediateStopwatch();
                if (fullStopwatch != null)
                {
                    fullStopwatch.Stop();
                    log.Trace($"Treatment finished. Worker name = {WorkerName}. Time Elapsed = {fullStopwatch.Elapsed.ToMinimumReadableTime()}");
                }
            }
        }

        public override void DoPreparationWork()
        {
            if (WorkerName == null)
            {
                throw new Exception("VideoTreatmentWorker: No worker name assigned.");
            }
            if (ServerConf == null)
            {
                throw new Exception("VideoTreatmentWorker: No server configuration assigned.");
            }
            if (ProjConfig == null)
            {
                throw new Exception("VideoTreatmentWorker: No project definition configuration assigned.");
            }
        }

        /// <summary>
        /// Démarre un chronomètre pour une étape temporelle. Une seule étape intermédiaire peut être partie par agent.
        /// Utiliser cette méthode assure que le chronomètre intermédiaire est fermée à la fin de l'exécution de l'agent, en cas de problème et dans la situation où
        /// il termine normalement.
        /// Appeller à nouveau cette méthode va stopper et journaliser le temps de la dernière étape intermédiaire.
        /// </summary>
        /// <param name="intermediateStepName">Le nom de cette étape. Optionnel.</param>
        protected void StartIntermediateStopwatch(string intermediateStepName = null)
        {
            if (!log.IsTraceEnabled) return;

            if (_intermediateSw != null)
            {
                StopIntermediateStopwatch();
            }
            _intermediateSw = new Stopwatch();
            _intermediateSwName = intermediateStepName;
        }

        private void StopIntermediateStopwatch()
        {
            if (!log.IsTraceEnabled || _intermediateSw == null) return;

            var sb = new StringBuilder();
            sb.Append("Intermediate step finished");
            if (!string.IsNullOrWhiteSpace(_intermediateSwName))
            {
                sb.Append(". Step name = ");
                sb.Append(_intermediateSwName);
            }

            sb.Append(". Time elapsed = ");
            sb.Append(_intermediateSw.Elapsed.ToMinimumReadableTime());

            log.Trace(sb.ToString);

            _intermediateSw = null;
            _intermediateSwName = null;
        }

        /// <summary>
        /// Méthode à implémenter pour lancer le worker.
        /// </summary>
        protected abstract Task<bool> Launch(CancellationToken token);
        
        /// <summary>
        /// Cette méthode remet l'instance dans la queue de traitement.
        /// Toutefois, cette opération détruit la priorisation de ce traitement et revient à la fin de la file.
        /// De plus, pour éviter une refont immédiate, il est possible que le traitement soit bloqué pour une période fixe,
        /// comme en cas de problème internet.
        /// </summary>
        protected void RequeueWorker()
        {
            var message = "Worker re-queued: " + WorkerName;
            TimeDelay = DateTime.Now + ProjConfig.RequeueDelay;
            log.Info(message);
            if (log.IsTraceEnabled)
            {
                LoggerReference.LogEvent(message);
            }
            Pool?.AddWorker(this);
        }
        
        /// <summary>
        /// Initialisation supplémentaire avec les configurations principales de l'Application.
        /// </summary>
        public virtual bool CustomInit(VideoCaptureConfig config, NomadConfiguration nomadConfig)
        {
            if (config == null || nomadConfig?.CurrentServerConfiguration == null) return false;

            ProjConfig = config;
            ServerConf = nomadConfig.CurrentServerConfiguration;

            return true;
        }

        /// <summary>
        /// Selon la classe, on peut devoir créer un vidéo rapide pour visualisation rapide.
        /// Comme le traitement est hautement dépendent des vidéos capturés (et de la volonté des clients),
        /// c'est la classe de traitement qui doit implémentercette fonctionalité.
        /// Il est recommandé que cette classe termine cette méthode le plus vite possible; possiblement en créant une tâche
        /// future plutôt qu'un traitement synchrone.
        /// Si une erreur se produit on retourne un "string" non-nul qui doit être envoyé au UI.
        /// Le retour est null si tout s'est bien passé.
        /// </summary>
        /// <param name="baseDirectory">Répertoire de base des événements.</param>
        /// <param name="tempName">Le nom de répertoire où sont stockés les fichiers.</param>
        /// <param name="previewDirectory">Le répertoire distant où devra être transféré le vidéo de présentation.</param>
        public abstract string CreateVideoPreview(DirectoryInfo baseDirectory, string tempName, string previewDirectory);

        /// <summary>
        /// Enlève les fichiers temporaires et les fichiers sources du disque dur, pour nettoyer.
        /// Si on donne un répertoire d'exil, on déplace le tout au lieu de simplement détruire.
        /// </summary>
        /// <param name="dirToDestroy">Le répertoire temporaire à détruire.</param>
        /// <param name="exileDirectory">Optionnel. Le répertoire pour contenir le répertoire problématique</param>
        protected async Task InvalidateVideoEvent(DirectoryInfo dirToDestroy, DirectoryInfo exileDirectory = null)
        {
            try
            {
                if (exileDirectory != null && exileDirectory.Exists)
                {
                    await Task.Delay(100); // Tente d'éviter les "locks" de fichiers.
                    Directory.Move(dirToDestroy.FullName, exileDirectory.FullName);
                    return;
                }

                FileUtil.DeleteFolderContent(dirToDestroy.FullName);
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            dirToDestroy.Refresh();
            if (!dirToDestroy.Exists) return;

            log.Fatal("Could not clean directory! Errors will compound. Stopping execution. File: " + dirToDestroy.FullName);
            throw new Exception("Critical directory could not be destroyed: " + dirToDestroy.FullName);
        }
    }
}