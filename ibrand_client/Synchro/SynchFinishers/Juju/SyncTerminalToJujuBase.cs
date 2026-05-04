using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Exception;
using static NomadLogic.Common.QuestionTypeEnum;

namespace NomadLogic.iBrandDesktop.Synchro.SynchFinishers.Juju
{
    [PublicAPI]
    public abstract class SyncTerminalToJujuBase : SyncFinisherBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected ServerConfiguration _servConfig;
        private readonly ActivityLoggerService _actiLogger;

        /// <summary>
        /// Constructeur vide pour permettre instanciation via propriétés.
        /// </summary>
        protected SyncTerminalToJujuBase() {}

        /// <summary>
        /// Constructeur complet pour exécution.
        /// </summary>
        protected SyncTerminalToJujuBase([NotNull] ISyncSendReport parentReporter, [NotNull] TerminalInfo currTerminalInfo, [NotNull] Questionnaire currQuestionnaire, 
            [NotNull] List<int> listRespId, [NotNull] Synchronization currentSynchro)
        {
            ParentReporter = parentReporter;
            CurrentTerminal = currTerminalInfo;
            CurrentQuestionnaire = currQuestionnaire;
            ListRespondentId = listRespId;
            CurrentSynchronization = currentSynchro;
            _actiLogger = ServicesContainer.GetService<ActivityLoggerService>();
        }

        /// <summary>
        /// Lance le processus d'envoi de répondants vers le serveur.
        /// </summary>
        public override async Task Execute(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            _servConfig = ParentReporter.NomadConfig.CurrentServerConfiguration;

            log.Trace($"Requesting Juju authentification to '{_servConfig.Address}'");
            
            // WARN: J'utilise ici un appel synchrone à cause d'un "hang-up" extrêmement difficile à identifier avec un call ASYNC.
            var login = ServerRestRequestsUtil.LoginSync(_servConfig);
            if (login.Result != ServerRestRequestResultType.Success)
            {
                log.Warn("Could not authenticate to main server: " + _servConfig.Address);
                ParentReporter.SendErrorReport("Could not authenticate to main server.");
                return;
            }

            try
            {
                ParentReporter.ReportMessage(Resources.iBrand.ConnectServer);
                
                ParentReporter.ReportMessage(Resources.iBrand.SynchroSynchingProject + CurrentTerminal.ProjectName);
                await SyncJujuRespondent(login.Data, ct).ConfigureAwait(false);
                
            }
            catch (IOException ex)
            {
                throw new SynchroException(Resources.iBrand.ErrorConnectionClosed, ex);
            }
        }
        
        private async Task SyncJujuRespondent(string authToken, CancellationToken token)
        {
            log.Trace("SyncProject: Terminal {0} Synchronisation started.", CurrentTerminal.TerminalId);
            ParentReporter.ReportSetNbrRespondentToTreat(0);
            
            var db = ParentReporter.Database;
            
            ParentReporter.SendReport();
            
            foreach (var repId in ListRespondentId)
            {
                if (token.IsCancellationRequested)
                {
                    // On saute les répondants restants si on se fait lancer la commande de canceller l'envoi.
                    log.Trace("SyncTerminal: Cancellation command detected. Cancelling remaining respondents to upload. projectId={0}", CurrentTerminal.ProjectId);
                    break;
                }
                
                var respondent = db.GetRespondent(repId);
                
                var autoCleanAnswers = CurrentQuestionnaire.AutoCleanAnswers;
                
                // Information venant de source différentes
                respondent.DeviceName = CurrentSynchronization.Ppc;
                respondent.TerminalId = CurrentTerminal.TerminalId;
                
                var answerFromDb = db.GetAnswers(repId);

                var finalAnswerList = new List<Answer>();
                foreach (var ans in answerFromDb)
                {
                    // On enlève les questions qui ne doivent pas être sauvegardées.
                    var q = CurrentQuestionnaire.GetQuestion(ans.QuestionNo);
                    if (q.ToSync)
                    {
                        finalAnswerList.Add(ans);
                    }

                    // Si la question est un type fichier mais qu'il ne faut pas envoyer vers le serveur, on force le type TEXT
                    // au lieu du type original pour n'envoyer que le "path" local vers le serveur.
                    if (!q.ToSave && q.QuestionType.IsFileType())
                    {
                        ans.QuestionType = Text;
                    }

                    // Si la réponse est de type EUI, on assigne sa valeur au répondant.
                    if (ans.QuestionType == ExternalUniqueIdentifier)
                    {
                        respondent.ExternalUniqueIdentifier = ans.Response;
                    }
                }

                respondent.Answers = finalAnswerList;
                respondent.DynamicHeaders = db.GetDynamicHeaders(repId);

                if (CurrentQuestionnaire.DynamicHeaderSyncExclusionList?.Any() ?? false)
                {
                    var removed = respondent.DynamicHeaders.RemoveAll(dh => CurrentQuestionnaire.DynamicHeaderSyncExclusionList.Contains(dh.DataKey));
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("{0} elements removed from existing dynamic headers from exclusion list: {1}",
                            removed, StringUtil.FormatEnumerable(CurrentQuestionnaire.DynamicHeaderSyncExclusionList));
                    }
                }

                if (log.IsTraceEnabled)
                {
                    var answerNbr = respondent.Answers.Count;
                    var dhNbr = respondent.DynamicHeaders.Count;
                    log.Trace($"Respondent ready for upload: LocalId={respondent.Id}, Questionnaire={respondent.QuestionnaireId}, Terminal={respondent.TerminalId}, Total Answers={answerNbr}, Total DynamicHeaders={dhNbr}");
                }

                var finalizeRespondent = false;

                if (CheckIfRespondentValid(respondent))
                {
                    finalizeRespondent = await ExecuteSend(token, authToken, respondent);
                    ParentReporter.SendReport($"Respondent uploaded ({repId})");
                }

                if (finalizeRespondent)
                {
                    ParentReporter.ReportRespondentTreated();
                    db.SetRespondentSynchronized(repId, CurrentSynchronization.No);
                    if (autoCleanAnswers) db.DeleteAllAnswers(repId);
                    log.Trace("Finished sequence for respondent {0}. Finalized.", repId);
                    _actiLogger?.LogActivity(this.CurrentTerminal.TerminalId, ActivityLoggerActionType.RespondentFinalized, repId.ToString());
                }
            }

            ParentReporter.SendReport(null, null, true);
        }
        
        protected virtual bool CheckIfRespondentValid(Respondent resp)
        {
            return true;
        }

        protected abstract Task<bool> ExecuteSend(CancellationToken ct, string authToken, Respondent respondent);
        
    }
}
