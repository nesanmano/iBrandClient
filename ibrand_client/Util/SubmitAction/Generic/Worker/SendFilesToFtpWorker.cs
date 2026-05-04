using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.Ftp;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using WinSCP;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Envoi les fichiers des questions identifiées sur un site FTP.
    /// La configuration texte doit contenir les informations sur l'authentification FTP ainsi que sur l'arborescence que les fichiers FTP doivent avoir.
    /// Il n'y a pas de prochaine étape pour ce travailleur.
    /// </summary>
    public class SendFilesToFtpWorker : SubmitActionWorkerAbstract<ISubmitActionSendFilesToFtp>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
        {
            var listQuestion = _action.FtpQuestions;
            if (listQuestion == null || !listQuestion.Any())
            {
                // No-Op
                return;
            }

            _respondent.Answers = db.GetAnswers(_respondent.Id);
            _respondent.DynamicHeaders = db.GetDynamicHeaders(_respondent.Id);
            
            var existingFiles = new List<Answer>();
            var missingFiles = new List<Answer>();
            
            foreach (var qNo in listQuestion)
            {
                var response = _respondent.GetAnswer(qNo);
                if (File.Exists(response.Response))
                {
                    existingFiles.Add(response);
                }
                else
                {
                    missingFiles.Add(response);
                }
            }
            
            if (missingFiles.Any())
            {
                if (!_action.FtpSynchRespondentWithMissingFiles)
                {
                    throw new System.Exception("Some files were missing during Ftp pre-check. Aborting action: " +
                                               StringUtil.FormatEnumerable(missingFiles.Select(file => file.Response), false, 0, "','"));
                }

                // Si ce n'est pas optionel mais qu'au moins un des fichiers est inexistant, on annule ce répondant.
                log.Warn("Respondent {0} does not have valid files for every question: '{1}'", _respondent.Id,
                    StringUtil.FormatEnumerable(missingFiles.Select(file => file.Response), false, 0, "','"));
                return;
            }

            if (!existingFiles.Any())
            {
                return;
            }

            // Préparation du FTP
            var ftp = new FtpPool(_action.FtpAdress, _action.FtpPort, _action.FtpUser, _action.FtpPassword);
            
            foreach (var fileToSend in existingFiles)
            {
                var fi = new FileInfo(fileToSend.Response);

                var rep = "";
                if (!string.IsNullOrWhiteSpace(_action.FtpDir))
                {
                    var answerMap = db.GetAnswersMap(_respId);
                    rep = StringUtil.CreateStringFromTokenizedPatternRespondent(_action.FtpDir, answerMap, fileToSend.QuestionIdentifier, _respondent);

                    // On s'assure que le répertoire finisse avec un "/"
                    if (!rep.EndsWith("/"))
                    {
                        rep += '/';
                    }
                }

                var ftpPath = ftp.GetUploadPath(rep, fileToSend.Response);
                if (!ftp.CheckFtpDirectoryPath(ftpPath))
                {
                    throw new System.Exception("Invalid file path: " + ftpPath);
                }

                using (var session = new Session())
                {
                    session.Open(ftp.ConnectionOptions);
                    FtpWebUtil.UploadFile(session, ftpPath, fi);
                }

                log.Debug("File uploaded to FTP: {0}", fileToSend);
            }

            if (_action.FtpDestroyFiles)
            {
                foreach (var fileToSend in existingFiles)
                {
                    try
                    {
                        File.Delete(fileToSend.Response);
                    }
                    catch (System.Exception)
                    {
                        log.Warn("Could not destroy file. Continuing: " + fileToSend);
                        // die nicee
                    }
                }
            }
        }

        /// Pas de prochaine étape pour ce travailleur.
        protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo nextSteps)
        {
            throw new NotImplementedException();
        }

        public SendFilesToFtpWorker(ISubmitActionSendFilesToFtp action, Respondent resp, int respId,
            SubmitActionWorkerNextStepInfo stepInfo = null) : base(action, resp, respId, stepInfo) { }
    }
}