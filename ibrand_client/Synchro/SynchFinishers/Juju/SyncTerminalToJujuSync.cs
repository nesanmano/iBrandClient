using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Synchro.SynchFinishers.Juju
{
    [PublicAPI]
    public class SyncTerminalToJujuSync : SyncTerminalToJujuBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Constructeur vide pour permettre instanciation via propriétés.
        /// </summary>
        public SyncTerminalToJujuSync(){}

        /// <summary>
        /// Constructeur complet pour exécution.
        /// </summary>
        public SyncTerminalToJujuSync([NotNull] ISyncSendReport parentReporter, [NotNull] TerminalInfo currTerminalInfo, [NotNull] Questionnaire currQuestionnaire, 
            [NotNull] List<int> listRespId, [NotNull] Synchronization currentSynchro) :
            base(parentReporter, currTerminalInfo, currQuestionnaire, listRespId, currentSynchro)
        {
            ParentReporter = parentReporter;
            CurrentTerminal = currTerminalInfo;
            CurrentQuestionnaire = currQuestionnaire;
            ListRespondentId = listRespId;
            CurrentSynchronization = currentSynchro;
        }

        protected override async Task<bool> ExecuteSend(CancellationToken ct, string authToken, Respondent respondent)
        {
            var resp = new RequestSyncRespondent
            {
                CreationDate =  respondent.DateLocal,
                Answers = respondent.Answers,
                Language = respondent.Language,
                RespondentId = respondent.Id
            };

            var dataSent = resp.SizeInBytes();
            var sync = await ServerRestRequestsUtil.Sync(ct, _servConfig, authToken, resp, CurrentTerminal);
            dataSent +=  sync.Data?.SizeInBytes() ?? 0;

            if (sync.Result == ServerRestRequestResultType.Success)
            {
                log.Trace($"Respondent ({resp.RespondentId}) sync has finished succesfully (no files).");
                ParentReporter.SendReport($"Respondent ({resp.RespondentId}) uploaded. No files sent.");
                ParentReporter.ReportAddDataSent((ulong)dataSent);
                return true;
            }

            if (sync.Result != ServerRestRequestResultType.SuccessPartial)
            {
                log.Trace($"Respondent ({resp.RespondentId}) sync has failed with the result: {0}", sync.Result.ToString());
                ParentReporter.SendErrorReport($"Respondent ({resp.RespondentId}) failed.");
                ParentReporter.ReportAddDataSent((ulong)dataSent);
                return false;
            }

            // À cet endroit, data, ne peut pas être nul.
            if (sync.Data == null)
            {
                log.Trace("Data cannot be null with a partial response. Cannot continue.");
                ParentReporter.SendErrorReport("Data cannot be null with a partial response. Cannot continue.");
                ParentReporter.ReportAddDataSent((ulong)dataSent);
                return false;
            }

            // On envoie maintenant les fichiers.

            // Requête d'envoi de fichier vers le serveur.
            var sessionId = sync.Data.SyncSessionId;
            var filesToSend = sync.Data.FilesToUpload;
            var totalTries = 0;

            do
            {
                if (filesToSend != null && filesToSend.Any())
                {
                    if (log.IsTraceEnabled)
                    {
                        var sb = new StringBuilder();

                        sb.Append("Sending following files as part of respondent sync:");
                        foreach (var f in filesToSend)
                        {
                            sb.Append($"\n\t- {f}");
                        }

                        log.Trace(sb.ToString);
                    }

                    // Faire une variante parallèle plus tard.
                    foreach (var f in filesToSend)
                    {
                        var localFile = new FileInfo(f.LocalFilePath);

                        // TODO Idéalement, il faut faire quelque chose de bcp plus robuste que juste sauter cette étape!
                        if (!localFile.Exists)
                        {
                            log.Fatal("Local file does not exists! Skipping.");
                            continue;
                        }

                        dataSent += new FileInfo(f.LocalFilePath).Length;
                        var up = ServerRestRequestsUtil.SyncUpload(_servConfig, authToken, sessionId, f.LocalFilePath, f.AnswerIdentifier);
                        if (up.Result == ServerRestRequestResultType.Success)
                        {
                            log.Trace("File uploaded. Resp=({0}), session=({1}), qId=({2}), file=({3})", 
                                resp.RespondentId, sessionId, f.AnswerIdentifier, f.LocalFilePath);
                        }
                        else
                        {
                            log.Warn("File could not be uploaded. Will retry (errors={4}). Resp=({0}), session=({1}), qId=({2}), file=({3})", 
                                resp.RespondentId, sessionId, f.AnswerIdentifier, f.LocalFilePath, totalTries);
                        }
                    }
                }

                var fin = ServerRestRequestsUtil.SyncFinalize(_servConfig, authToken, sessionId);

                if (fin.Result == ServerRestRequestResultType.Success)
                {
                    log.Trace("Respondent sync has finished with the result: {0}", fin.Result);
                    ParentReporter.ReportAddDataSent((ulong)dataSent);
                    return true;
                }
                
                if (fin.Result == ServerRestRequestResultType.SuccessPartial)
                {
                    // Il reste des fichiers
                    filesToSend = fin.Data.FilesToUpload;
                    totalTries++;
                }
                else
                {
                    log.Warn("Respondent finalized failed for unknown reasons. Cancelling respondent.");
                    ParentReporter.SendErrorReport("Respondent finalized failed for unknown reasons. Cancelling respondent.");
                    CancelRespondent(authToken, _servConfig, sessionId, resp);
                    ParentReporter.ReportAddDataSent((ulong)dataSent);
                    return false;
                }
                
            } while (totalTries > 5);

            return false;
        }

        private void CancelRespondent(string authToken, ServerConfiguration conf, string sessionId, RequestSyncRespondent resp)
        {
            var can = ServerRestRequestsUtil.SyncCancel(conf, authToken, sessionId);
            if (can.Result == ServerRestRequestResultType.Success)
            {
                log.Trace("Respondent upload cancelled with success.");
                ParentReporter.SendErrorReport($"Cancel request detected. Respondent ({resp.RespondentId}) cancelled successfully.");
            }
            else
            {
                log.Trace("Respondent upload cancelled with error: " + can.MessageUi);
                ParentReporter.SendErrorReport($"Cancel request detected. Respondent ({resp.RespondentId}) cancel request failed: " + can.MessageUi);
            }
        }
    }
}
