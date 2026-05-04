using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Classe d'exécution pour l'envoi vers un PrintStation.
    /// Il n'y pas de classe chaînée définie pour ce Worker.
    /// </summary>
    public class SendFilesToPrintstationWorker : SubmitActionWorkerAbstract<ISubmitActionSendFilesToPrintstation>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="action">L'action qui contient ce travailleur.</param>
        /// <param name="resp">Le répondant qui envoi l'information.</param>
        /// <param name="respId">L'identifiant numérique de ce répondant.</param>
        /// <param name="stepInfo">La définition d'étape.</param>
        public SendFilesToPrintstationWorker(ISubmitActionSendFilesToPrintstation action, Respondent resp, int respId,
            SubmitActionWorkerNextStepInfo stepInfo = null) : base(action, resp, respId, stepInfo) { }

        protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
        {
            var action = _action as ISubmitActionSendFilesToFtp;
            return action != null ? new SendFilesToFtpWorker(action, _respondent, _respId, next) : null;
        }

        /// <inheritdoc />
        protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
        {
            _respondent.Answers = db.GetAnswers(_respondent.Id);
            _respondent.DynamicHeaders = db.GetDynamicHeaders(_respondent.Id);

            var anwersFileName = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".json");
            var fileQuestions = _action.FileQuestions;

            var uniqueId = GenerateUniqueId.GetId();
            var fileTosend = DirectoryUtil.GetLocalApplicationWorkDir() + uniqueId + ".zip";

            var answers = new Answers();

            foreach (var answer in from num in fileQuestions
                                   let response = _respondent.Answers.First(a => a.QuestionNo.Equals(num.ToString()))
                                   select new Answer(response))
            {
                answers.Values.Add(answer);
            }

            foreach (var answer in from num in _action.AnswerQuestions
                                   let response = _respondent.Answers.First(a => a.QuestionNo.Equals(num.ToString()))
                                   select new Answer(response))
            {
                answers.Values.Add(answer);
            }

            JsonUtil.WriteJsonFile(new FileInfo(anwersFileName), answers);

            var zip = new ZipFile { CompressionLevel = CompressionLevel.BestCompression };
            zip.AddFile(anwersFileName, "");

            foreach (var num in fileQuestions)
            {
                var original = _respondent.GetAnswerText(num);

                var renamedFile = DirectoryUtil.CreateLocalTemporaryFile(StringUtil.GetFileName(original));

                if (!File.Exists(renamedFile))
                    File.Copy(original, renamedFile);
                zip.AddFile(renamedFile, "");
            }
            zip.Save(fileTosend);

            if (ct?.IsCancellationRequested ?? false)
            {
                return;
            }

            log.Trace("Identified file to send for following respondent{1}: name={0}", fileTosend, _respId);

            // Étape deux. On envoi les fichiers aux Printstations détectés.

            var result = new SsdpUtil.SsdpPrintstationSendResult();

            var allSuccessSent = db.GetSuccessfulPrintstationFileSending(_respId);

            var inTime = false;
            try
            {
                var task = Task.WhenAny(SsdpUtil.TrySendFilesToPrintstations(_action.MinPrintstationToSend, allSuccessSent, ct, fileTosend)).Unwrap();
                inTime = task.Wait(1000 * 60 * 2);
                result = task.Result;
            }
            catch (AggregateException e)
            {
                log.Debug(e, "Not all tasks completed. Exception was thrown.");
            }
            finally
            {
                allSuccessSent.AddRange(result.SentPrintstations.Select(printSuccess => printSuccess.IpAddress));
                db.UpdateSuccessfulPrintstationFileSending(_respId, allSuccessSent);
                if (!inTime) log.Debug("Not all task completed in the allocated time. (2 minutes)");
            }

            if (log.IsTraceEnabled)
            {
                log.Trace("Sent upload request ({1}) to following printstations: {0}",
                    string.Join(",", result.SentPrintstations.Select(s => s.IpAddress)),
                    result.NbrRequest);
            }

            // On calcule maintenant le nombre d'envois réussi.
            if (allSuccessSent.Count >= _action.MinPrintstationToSend)
            {
                log.Trace("Upload successfuly completed for respondent {0}", _respId);
            }
            else
            {
                throw new System.Exception($"Submit action failed for respondent {_respId} because not enough Printstations received the Data. ({allSuccessSent.Count}/{_action.MinPrintstationToSend})");
            }
        }
    }
}
