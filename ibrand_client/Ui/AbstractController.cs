using System;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Ui
{
    /// <summary>
    /// Contient la structure de base pour un contrôleur d'écran.
    /// </summary>
    public abstract class AbstractController : ICurrentRespondentSource
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected SynchronizationService _syncService;
        private readonly ActivityLoggerService _activityLogger;

        /// <summary>
        /// Indique que le questionnaire a été détruit. Ce qui équivaut à détruire le controlleur lui-même.
        /// </summary>
        public event EventHandler QuestionnaireClosed;

        public Questionnaire Questionnaire { get; protected set; }
        public Project Project { get; protected set; }
        public IbrandEventInfo IbrandEventInfo { get; protected set; }
        public string UserName { get; protected set; }



        /// <summary>
        /// Indique que l'on tente présentement de fermer le controleur. Ne peut être mis à vrai que si on démarre la séquence de fermeture
        /// en appellant la méthode "StartExitSequence".
        /// </summary>
        public bool WaitingForExit { get; private set; }

        protected AbstractController(Project project, IbrandEventInfo eventt, string userName, Questionnaire questionnaire)
        {
            Project = project;
            Questionnaire = questionnaire;
            IbrandEventInfo = eventt;
            UserName = userName;

            StartLiveSynchro();

            _activityLogger = ServicesContainer.GetService<ActivityLoggerService>();
        }

        protected void LogActivity(int terminalId, ActivityLoggerActionType actionType, string details = null)
        {
            _activityLogger?.LogActivity(terminalId, actionType, details);
        }

        /// <summary>
        /// Démarre la synchronisation en temps réel, si la configuration le permet.
        /// </summary>
        protected void StartLiveSynchro()
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            if (!configuration.LiveSynchro) return;

            var db = ServicesContainer.GetService<IbrandDatabase>();
            if (db == null || !db.Init()) return;

            _syncService = ServicesContainer.GetService<SynchronizationService>();

            if (_syncService == null) return;

            if (_syncService.Status == LongRunningStatus.ServiceCreated ||
                  _syncService.Status == LongRunningStatus.Paused)
            {
                _syncService.StartLongRunning();
            }
        }

        protected void SetNewRespondentEvent()
        {
            _syncService?.SignalNewRespondent();
        }

        protected void OnQuestionnaireClosed()
        {
            log.Trace("OnQuestionnaireClosed Invoked");
            QuestionnaireClosed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Démarrage de la séquence de traitement pour la sortie du controller. On commence par stopper le SubmitAction du questionnaire,
        /// s'il y en a une.
        /// Étape 1 sur 3 de la séquence de fermeture.
        /// </summary>
        protected void StartExitSequence()
        {
            log.Trace("Exit Sequence Called.");
            
            if (WaitingForExit)
            {
                log.Trace("We are already waiting to exit: will not launch another exit sequence.");
                return;
            }
            
            if (Questionnaire.SubmitAction != null)
            {
                log.Trace("End Sequence for submitaction intiated.");
                
                Questionnaire.SubmitAction.OnCompleteStopReached += CancelUploadLive;
                
                Questionnaire.SubmitAction.StopProcessing();
                
                // Si l'arrêt de la submit action n'est pas terminé.
                WaitingForExit = true;
            }
            else
            {
                CancelUploadLive();
            }
        }

        /// <summary>
        /// Cette méthode tente de stopper le live upload, s'il est partit. Une fois complété, appelle la méthode "CloseController".
        /// Étape 2 sur 3 de la séquence de fermeture.
        /// </summary>
        protected void CancelUploadLive()
        {
            log.Trace("CancelUploadLive called.");
            var syncServ = ServicesContainer.GetService<SynchronizationService>();
            log.Trace("End Sequence for Sync Service Live intiated.");
            WaitingForExit = true;
            syncServ?.StopLongRunning().ContinueWith(task => CloseController());
        }

        /// <summary>
        /// Cette méthode fait le nettoyage du controller d'écran. Est invoqué lors de la séquence de fermeture. Ne pas appeller directement.
        /// Étape 3 sur 3 de la séquence de fermeture.
        /// </summary>
        protected abstract void CloseController();

        /// <summary>
        /// Affiche un boîte à message pour l'usager.
        /// </summary>
        /// <param name="message">Le message</param>
        /// <param name="caption">Le titre principal</param>
        public abstract void ShowMessageBox(string message, string caption);

        /// <summary>
        /// Affiche une boîte de message à l'usager.
        /// </summary>
        /// <param name="message">Le message à afficher.</param>
        public abstract void ShowMessageBox(string message);

        public abstract IRespondent GetValidatedCurrentRespondent();
    }
}