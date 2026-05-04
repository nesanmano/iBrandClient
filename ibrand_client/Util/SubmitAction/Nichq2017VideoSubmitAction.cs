using System;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe de SubmitAction pour créer et envoyer un vidéo vers vimeo.
    /// </summary>
    public class Nichq2017VideoSubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("CreateVideo", 1);
            AddSubmitActionWorkerPool("SendVideo", 3);
        }

        /// <inheritdoc />
        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var respId in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var respondent = db.GetRespondent(respId);

                respondent.Answers = db.GetAnswers(respId);

                var temp = CreateWorker(respondent, false);
                temp.DoWork(ct);

                if (ct != null && !ct.Value.IsCancellationRequested) break;
                ((CreateVideoWorkerNichq)temp).SecondWorkerStep?.DoWork(ct);

                UiSynchro?.AddProgressStep(1);
            }
        }

        /// <inheritdoc />
        protected override void ValidateConfigData() { }

        /// <inheritdoc />
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new CreateVideoWorkerNichq(this, resp, resp.Id, usePool ? _submitActionWorkerPools[1] : null);
        }

        private class CreateVideoWorkerNichq : SubmitActionWorkerAbstract<Nichq2017VideoSubmitAction>
        {
            private readonly SubmitActionWorkerPool _pool;

            internal NichqSendVideoToVimeoWorker SecondWorkerStep { get; private set; }

            public CreateVideoWorkerNichq(Nichq2017VideoSubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
            {
                // Rien de plus
                _pool = secondPool;
            }

            public override void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                if (_action._stopped) return;

                try
                {
                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    // Vé4rification du répertoire de back-up.
                    var bkpDirPath = config.BackupDrive + Path.DirectorySeparatorChar + "nichq2017_video_bkp" + Path.DirectorySeparatorChar;
                    var bkpDir = Directory.CreateDirectory(bkpDirPath);

                    if (!bkpDir.Exists) throw new System.Exception("Backup directory could not be created. Verify that this path is legit: " + bkpDirPath);

                    var db = ServicesContainer.GetService<IbrandDatabase>();
                    var answerMap = db.GetAnswersMap(_respId);
                    var questionsVideo = new[] { 6, 8 };

                    foreach (var qNo in questionsVideo)
                    {
                        var qi = new QuestionIdentifier(qNo);

                        var response = answerMap.ContainsKey(qi) ? answerMap[qi] : null;

                        if (response == null || response.Response.EndsWith("mp4", StringComparison.InvariantCultureIgnoreCase)) continue;

                        var videoFileName = VideoCreation(response.Response, config);

                        if (!File.Exists(videoFileName)) { throw new System.Exception($"FFMPEG could not create video file for respondant {_respId}"); }

                        /* On fait le back-up du fichier */
                        var bkpFile = Path.Combine(bkpDir.FullName, StringUtil.GetFileName(videoFileName));
                        File.Copy(videoFileName, bkpFile);
                        if (!File.Exists(bkpFile)) throw new System.Exception("Could not create backup file: " + bkpFile);

                        /* Les fichiers sont ok, on met les résultats dans la BD. */
                        var videoAnswer = new Answer{QuestionType = QuestionTypeEnum.Video, QuestionNo = qNo, Response = videoFileName};
                        db.DeleteAnswer(_respId, videoAnswer);
                        db.InsertAnswer(_respId, videoAnswer);

                        _respondent.Answers.RemoveAll(a => a.QuestionNo == qNo);
                        _respondent.Answers.Add(videoAnswer);

                        File.Delete(response.Response); // Destruction du MOV, en tout dernier.
                    }

                    SecondWorkerStep = new NichqSendVideoToVimeoWorker(_action, _respondent, _respId);

                    // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                    _pool?.AddWorker(SecondWorkerStep);
                }
                catch (System.Exception ex)
                {
                    var str = "Error while doing CreateVideoAndSend submit action for respondent: " + _respId;
                    log.Fatal(ex, str);
                    _action.UiSynchro?.AddUiMessage(str, 4);
                }
                finally
                {
                    _action.OnWorkFinished(_respId);
                }
            }

            protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
            {
                throw new NotImplementedException();
            }

            protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
            {
                throw new NotImplementedException();
            }

            private string VideoCreation(string baseFileMov, NomadConfiguration config) {
                var output = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo, DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                                                     DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + ".mp4";

                FfmpegUtil.CompressVideo(config.FfmpegLocation, baseFileMov, output, 20);

                var info = new FileInfo(output);
                if (info.Exists && info.Length >= 100) return output;

                if (info.Exists) { info.Delete(); }
                throw new System.Exception("Could not compress video file: " + baseFileMov);
            }
        }

        internal class NichqSendVideoToVimeoWorker : SubmitActionWorkerAbstract<Nichq2017VideoSubmitAction>
        {
            /// <inheritdoc />
            public NichqSendVideoToVimeoWorker(Nichq2017VideoSubmitAction action, Respondent resp, int respId) : base(action, resp, respId) { }

            #region Overrides of SubmitActionWorkerAbstract<Nichq2017VideoSubmitAction>

            /// <inheritdoc />
            public override void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;
                if (_action._stopped) return;

                var db = ServicesContainer.GetService<IbrandDatabase>();
                var answerMap = db.GetAnswersMap(_respId);

                try
                {
                    var questionsVideo = new[] { 6, 8 };
                    var questionsVimeo = new[] { 7, 9 };

                    for (var i = 0; i < 2; i++) {

                        var vId = new QuestionIdentifier(questionsVimeo[i]);
                        var responseVimeo = answerMap.ContainsKey(vId) ? answerMap[vId] : null;
                        if (responseVimeo != null) continue; // Si déjà envoyé, on passe au prochain.

                        var qId = new QuestionIdentifier(questionsVideo[i]);
                        var fileName = answerMap[qId].Response;

                        /*
                        using (var file = new BinaryContent(fileName))
                        {
                            var client = new VimeoClient("5e8984e458692a57658d11e3626a4eda");
                            
                            var completedRequest = client.UploadEntireFile(file);

                            if (completedRequest.IsVerifiedComplete)
                            {
                                var videoClip = completedRequest.ClipUri;

                                var startIndex = videoClip.LastIndexOf('/') + 1;
                                var lenght = videoClip.LastIndexOf(':') - startIndex;

                                var vimeoId = videoClip.Substring(startIndex, lenght);
                                // Enregistrement de la question Viméo
                                var vimeoA = new Answer(QuestionText.Type, vId, 1, vimeoId);
                                db.DeleteAnswer(_respId, vimeoA);
                                db.InsertAnswer(_respId, vimeoA);
                                
                                _action.UiSynchro?.AddUiMessage("Video transformed, thumbnail created, VIMEO sent.", 0);
                            }
                            else
                            {
                                throw new System.Exception("Could not upload to vimeo: " + file);
                            }
                        }
                        */
                    }
                    
                    db.SetRespondentReadyToSync(_respId);
                    _action.NewRespondentEventHandle?.Set();
                }
                catch (System.Exception ex)
                {
                    log.Fatal("Error while NichqSendVideoToVimeoWorker#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while NichqSendVideoToVimeoWorker#DoWork()", 1);
                }
            }

            protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
            {
                throw new NotImplementedException();
            }

            protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}