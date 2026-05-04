using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Classe pour envoyer un fichier video sur vers un endroit sur le système local en tant que copie de sauvegarde.
    /// La prochaine étape de cette classe est l'envoi vers le un printstation, via communication directe (Socket).
    /// </summary>
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public class CopyToBackUpSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly ISubmitActionCopyToBackUp _action;
        private readonly int _respId;
        private readonly SubmitActionWorkerNextStepInfo _nextStep;

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public CopyToBackUpSubmitActionWorker(ISubmitActionCopyToBackUp action, Respondent resp, int respId, SubmitActionWorkerNextStepInfo stepInfo = null)
        {
            _respondent = resp;
            _action = action;
            _respId = respId;
            _nextStep = stepInfo;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                if (_action.IsStopped()) return;

                var db = ServicesContainer.GetService<IbrandDatabase>();

                if (_nextStep?.CurrentConfig.ExecuteStep ?? true) // Check no-op
                {
                    var filesQuestionNo = _action.QuestionsFileToBackUp;

                    if (filesQuestionNo == null)
                    {
                        throw new System.Exception("Invalid question list"); // Ne devrait en principe ne pas arriver si la validation est correcte.
                    }

                    if ((filesQuestionNo.Length == 0) && _action.OptionalBackup)
                    {
                        // Aucune question de fichier, mais c'est optionnel.
                        db.SetRespondentReadyToSync(_respId);
                        return;
                    }

                    var conf = ServicesContainer.GetService<NomadConfiguration>();
                    var dir = new DirectoryInfo(DirectoryUtil.GetBackupDir(conf));

                    foreach (var qNo in filesQuestionNo)
                    {
                        var qi = new QuestionIdentifier(qNo);

                        var answersDic = db.GetAnswersMap(_respId);
                        
                        var file = _respondent.GetAnswerText(qNo);
                        string fileNameBkp;
                        if (_action.FileNameToken == null)
                        {
                            fileNameBkp = StringUtil.GetFileName(file);
                        }
                        else
                        {
                            fileNameBkp = StringUtil.CreateStringFromTokenizedPatternRespondent(_action.FileNameToken,
                                answersDic, qi, _respondent);
                        }

                        string bkpFileDir;
                        if (!_action.UseDirectoryTree)
                        {
                            bkpFileDir = dir.FullName + Path.DirectorySeparatorChar + fileNameBkp;
                        }
                        else
                        {
                            var dirCandidate = dir + StringUtil.CreateStringFromTokenizedPatternRespondent(_action.DirectoryTreeToken,
                                    answersDic, qi, _respondent);
                            var dirInfo = Directory.CreateDirectory(dirCandidate);
                            bkpFileDir = dirInfo.FullName + Path.DirectorySeparatorChar + fileNameBkp;
                        }

                        if (file == null || !File.Exists(file))
                        {
                            if (!_action.OptionalBackup)
                            {
                                throw new FileNotFoundException($"Could not find file to upload for mandatory question: {file}");
                            }
                        }
                        else
                        {
                            try
                            {
                                if (File.Exists(bkpFileDir))
                                {
                                    File.Delete(bkpFileDir);
                                }

                                File.Copy(file, bkpFileDir);
                            }
                            catch (System.Exception ex)
                            {
                                if (!_action.OptionalBackup) throw;
                                // En cas d'erreur, mais que c'est optionnel, on met quand même le répondant prêt à être envoyé.
                                log.Error(ex, "Error while copying optional file for question: {0}", qNo);
                                //_action.UiSynchro?.AddUiMessage("Error while copying file.", 4);
                                continue;
                            }

                            if (File.Exists(bkpFileDir)) continue;
                            if (!_action.OptionalBackup)
                            {
                                throw new System.Exception($"Backup file does not exists after copy: {bkpFileDir}");
                            }
                        }
                    }
                }

                if (ct?.IsCancellationRequested ?? false) return;

                // Chaînage
                var nextStepInfo = _nextStep?.GetNextWorkerStepInfo();
                if (nextStepInfo != null)
                {
                    // S'il y a une prochaine étape, on instancie la classe.
                    if (_action is ISubmitActionSendFilesToPrintstation action)
                    {
                        NextWorkerStep = new SendFilesToPrintstationWorker(action, _respondent, _respId, nextStepInfo);

                        // On chaîne maintenant le backup dans le bassin donnée en paramètre, s'il existe.
                        nextStepInfo.CurrentConfig.Pool?.AddWorker(NextWorkerStep);
                    }
                }

                if (NextWorkerStep != null) return;
                // Si pas de chaînâge, on ferme le répondant actuel.
                db.SetRespondentReadyToSync(_respId);
                _action.NewRespondentEventHandle?.Set();
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while executing worker.");
                //_action.UiSynchro?.AddUiMessage($"Error while executing worker: {ex.Message}", 1);
            }
        }

        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        public int GetRespId()
        {
            return _respId;
        }
    }
}
