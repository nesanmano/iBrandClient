using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using Logger = NLog.Logger;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload
{
    /// <summary>
    /// Cette classe de service s'occupe de télécharger des fichiers via une liste de priorité, et utilise plusieurs tâches asynchrones en parallèle pour y parvenir.
    /// Une fois chaque demande entrée dans le service, le fichier sera téléchargé, et on peut faire un rappel via Action vers l'instance qui a fait la demande.
    /// Deux possibilités; soit le téléchargement est réussi, ou a échoué. Le système ne fait aussi aucune garantie sur la vitesse du téléchargement: selon la priorité
    /// de chaque éléments, les demandes ne sont pas faites selon une file FIFO, ainsi que les aléas de téléchargements via internet (connexion brisée ou lente, taille du
    /// fichier, etc...)
    /// </summary>
    [PublicAPI]
    public class RespondentEventDataDownloadService : ServiceLongRunningAbstract<RespondentBundleDownloadReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private TaskFactory _factory;
        private RespondentBundleRequest _currentRequest;
        private RespondentBundleFinalData _lastRequestData;
        private CancellationTokenSource _currentDownloadCancelTokenSource;
        private Task _currentExecution;
        private readonly object _lock = new object();
        private ServerConfiguration _currentServerConf;

        /// <summary>
        /// Référence interne vers le logger du service.
        /// </summary>
        internal Logger Logger => log;

        /// <summary>
        /// Créé l'instance avec ses configurations de base.
        /// </summary>
        /// <param name="maxConcurrentDownload">Minimum 1, 5 par défaut.</param>
        public RespondentEventDataDownloadService(uint maxConcurrentDownload = 20)
        {
            if (maxConcurrentDownload < 1)
            {
                throw new ArgumentException(@"Cannot have less than one concurrent download.", nameof(maxConcurrentDownload));
            }

            MaxConcurrentDownloads = maxConcurrentDownload;
        }

        /// <summary>
        /// Nombre maximal d'essais sur un fichier pour le téléchargement. Un code 404 (NOT FOUND) ignore cette valeur.
        /// </summary>
        public uint MaxNumberOfRetries { get; set; } = 3;

        /// <summary>
        /// Le nombre maximal de demandes de téléchargement simultanées.
        /// </summary>
        public uint MaxConcurrentDownloads { get; }

        public override string ServiceName { get; protected set; } = "Main server respondent bundle download service";

        /// <summary>
        /// Event interne pour signifier qu'une nouvelle tâche de Répondant est demandée.
        /// Si une tâche précédente existait déjà, cette tâche doit être stoppée.
        /// </summary>
        private ManualResetEventSlim NewTargetRespondentGiven { get; } = new ManualResetEventSlim(false);
        
        /// <summary>
        /// Si on ne veut pas la configuration de serveur venant du service principal, on peut en fournir une ici. Doit être donnée avant l'initialisation du service.
        /// </summary>
        public ServerConfiguration ForcedServerConfiguration { get; set; }

        protected override bool DoInit()
        {
            if (ForcedServerConfiguration != null)
            {
                _currentServerConf = ForcedServerConfiguration;
            }
            else
            {
                var mainConf = ServicesContainer.GetService<NomadConfiguration>();
                _currentServerConf = mainConf.CurrentServerConfiguration;
            }

            log.Trace("Initialization finished. Server : {0}:{1}", 
                _currentServerConf.Address, _currentServerConf.Port);

            return true;
        }

        protected override void OnDispose()
        {
            lock (_currentExecution)
            {
                _currentRequest?.CallbackError.Invoke(_currentRequest);
                _currentDownloadCancelTokenSource?.Cancel();
                _currentRequest = null;
                _currentDownloadCancelTokenSource = null;
                _lastRequestData = null;
            }
            _currentExecution = null;
            NewTargetRespondentGiven.Dispose();

            _factory = null;
        }
        
        protected override Task<bool> DoMainLoop(CancellationToken ct)
        {
            if (_factory == null)
            {
                _factory = new TaskFactory(ct, TaskCreationOptions.PreferFairness, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            log.Debug("Main loop for Respondent event download service started.");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // On attend un signal de nouveau répondant.
                    NewTargetRespondentGiven.Wait(ct);
                    NewTargetRespondentGiven.Reset();
                    log.Trace("New target event RESET");
                }
                catch (OperationCanceledException e)
                {
                    log.Debug(e, "Wait cancelled. If part of the closing process, this is normal.");
                    continue; // Va sortir du loop.
                }

                if (_currentRequest == null)
                {
                    // Si on a aucune requête de prête (probablement une erreur), on recommence l séquence d'arrêt.
                    continue;
                }

                _currentDownloadCancelTokenSource = new CancellationTokenSource();
                var mainTask = Task.Run(() => DoDownload(_currentDownloadCancelTokenSource.Token, _currentRequest, _lastRequestData), ct);
                _currentExecution = mainTask.ContinueWith(t =>
                {
                    // Nettoyage. Assume que les callback normaux sont appellés dans l'exécution interne.
                    lock (_lock)
                    {
                        _currentDownloadCancelTokenSource = null;
                        _currentExecution = null;
                        _currentRequest = null;
                        log.Trace("Callback nettoyage appellé.");
                    }
                }, ct);
                log.Trace("Main loop end reached. Going for reset wait.");
            }

            log.Debug("Main loop for Respondent event download service stopped.");
            _factory = null;
            return Task.FromResult(true);
        }
        
        public bool SetRespondentToDownload([NotNull] RespondentBundleRequest newRequest)
        {
            // Validation de la requête.
            if (!newRequest.Validate())
            {
                log.Warn("New request failed at self-validation: {0}", newRequest.ToStringPublicProperties());
                return false;
            }

            lock (_lock)
            {
                _lastRequestData?.FlagAsUnusable();
                _lastRequestData = null;

                if (_currentExecution != null)
                {
                    log.Info($"New respondent set when previous one is currently processed. Cancelling old respondent: '{_currentRequest.GetRespondentUniqueId()}'");

                    // Si une exécution est en cours, il faut l'annuler.
                    _currentDownloadCancelTokenSource?.Cancel();
                    _currentDownloadCancelTokenSource = null;
                    _currentExecution = null;
                    _currentRequest?.CallbackCancelled?.Invoke(_currentRequest);
                    _currentRequest = null;
                }

                _currentRequest = newRequest;
                _lastRequestData = new RespondentBundleFinalData();
                log.Debug($"New respondent set: '{_currentRequest.GetRespondentUniqueId()}'");

                NewTargetRespondentGiven.Set();
                log.Trace("New target event SET");
            }

            return true;
        }
        
        private async Task DoDownload(CancellationToken ct, [NotNull] RespondentBundleRequest dr, [NotNull] RespondentBundleFinalData respondentBundleFinalData)
        {
            if (!dr.Validate())
            {
                log.Warn("Invalid bundle request configuration.");
                return;
            }

            await dr.RespondentDataWorkerInstance.DoDownload(ct, _currentServerConf, this, dr, respondentBundleFinalData);
        }
        
        protected override bool CheckIfIdle()
        {
            return _currentExecution == null;
        }

        /// <summary>
        /// Retourne la dernière instance des données traitées par la dernière requête de téléchargement.
        /// Initialement, cette valeur sera nulle, mais dès que au moins une requête a été faite, cette valeur sera non-null.
        /// Si l'instance de service est "disposed", cette valeur peut revenir à nulle.
        /// </summary>
        public RespondentBundleFinalData GetLastRespondentData()
        {
            return _lastRequestData;
        }

        /// <summary>
        /// Quand on doit stopper la recherche d'information de façon publique et haut-niveau, on peut invoquer cette méthode. S'il n'y a pas de téléchargement en cours,
        /// cette opération est un NO-OP.
        /// </summary>
        public void StopDownload()
        {
            lock (_lock)
            {
                if (_currentDownloadCancelTokenSource == null) return;
                log.Info("Download stop request detected. Canceling current async worker.");
                _currentDownloadCancelTokenSource?.Cancel();

                if (_lastRequestData != null)
                {
                    _lastRequestData.DataUnusable = true;
                }
            }
        }
    }

}