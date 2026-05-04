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
    public class G7SelenaSubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public string _vimeoConsumerKey;
        public string _vimeoConsumerSecret;
        public string _vimeoToken;
        public string _vimeoTokenSecret;

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_t1", 1);
            AddSubmitActionWorkerPool("upload_t1", 1);
        }


        public override bool SubmitRespondent(Respondent respondent)
        {
            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();
                var respId = db.InsertRespondent(respondent, false);
                respondent.Id = respId;

                var temp = CreateWorker(respondent);

                _submitActionWorkerPools[0].AddWorker(temp);

                return true;
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex.Message);
                return false;
            }
        }

        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var respId in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var temp = new SelenaSubmitActionWorker(this, null, respId, null);
                temp.DoWork(ct);

                temp.SecondWorkerStep?.DoWork(ct);

                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new SelenaSubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }

        private class SelenaSubmitActionWorker : SubmitActionWorkerAbstract<G7SelenaSubmitAction>
        {
            private const int QUESTION_VIDEO = 2;
            private const int QUESTION_CUT = 3;
            private const int QUESTION_CUT_VIDEO = 5;

            private readonly SubmitActionWorkerPool _pool;

            public SelenaSubmitActionWorkerStep2 SecondWorkerStep { get; private set; }

            public SelenaSubmitActionWorker(G7SelenaSubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
            {
                // Rien de plus
                _pool = secondPool;
            }

            private void CreateVideoFile(string originalVideoFileName, string resultFileName)
            {

            }

            public override void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                if (_action._stopped) return;

                try
                {
                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    var respondent = db.GetRespondent(_respId);
                    respondent.Answers = db.GetAnswers(_respId);

                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    var videoFileName = respondent.GetAnswerText(QUESTION_VIDEO, 1);

                    string slowVideoFileName = null;

                    if (StringUtil.GetFileName(videoFileName).IndexOf("_slow") == -1 || !File.Exists(videoFileName))
                    {
                        var start = double.Parse(respondent.GetAnswerText(QUESTION_CUT,1));

                        var cutVideoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + "_cut.mp4";

                        throw new System.Exception("Obsolete.");
                        
                        var videoMetadata = FfmpegUtil.GetVideoFileMetadata(null, new FileInfo(videoFileName));

                        var endTime = start + 1.5;
                        var totalSec = 0.0;
                        if (totalSec < endTime)
                            endTime = start + (totalSec - start);

                        //Tranformation du video
                        FfmpegUtil.CutVideo(config.FfmpegLocation, videoFileName, cutVideoFileName, start, endTime);

                        var cutVideoAnswer = new Answer{QuestionType = QuestionTypeEnum.Text, QuestionNo = QUESTION_CUT_VIDEO, Response = StringUtil.GetFileName(cutVideoFileName)};
                        db.DeleteAnswer(_respId, cutVideoAnswer);
                        db.InsertAnswer(_respId, cutVideoAnswer);

                        slowVideoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + "_slow.mp4";

                        var overlay = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                        DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + "overlay.png";

                        const double SCALE = 0.2;
                        //FfmpegUtil.SlowCropVideo(config.FfmpegLocation, cutVideoFileName, slowVideoFileName, SCALE, videoMetadata.VideoFps * SCALE, null);
                        FfmpegUtil.OverlaySlowVideo(config.FfmpegLocation, cutVideoFileName, slowVideoFileName, 0.0 * SCALE, overlay, SCALE);

                        var videoAnswer = new Answer{QuestionType = QuestionTypeEnum.Video, QuestionNo = QUESTION_VIDEO, Response = slowVideoFileName};
                        db.DeleteAnswer(_respId, videoAnswer);
                        db.InsertAnswer(_respId, videoAnswer);
                    }
                    else
                    {
                        slowVideoFileName = videoFileName;
                    }

                    if (File.Exists(slowVideoFileName))
                    {
                        SecondWorkerStep = new SelenaSubmitActionWorkerStep2(_action, respondent, _respId, slowVideoFileName);

                        // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                        _pool?.AddWorker(SecondWorkerStep);
                    }
                }
                catch (System.Exception ex)
                {
                    log.Fatal(ex, "Error while doing T1 DFC submit action.");

                    _action.UiSynchro?.AddUiMessage(
                        "Error while Treating camera ring pictures for respondent id=" + _respId, 4);
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
        }

        class SelenaSubmitActionWorkerStep2 : ISubmitActionWorker
        {
            private static readonly Logger log = LogManager.GetCurrentClassLogger();
            private static readonly QuestionIdentifier QUESTION_VIMEO_ID = new QuestionIdentifier(4);

            private readonly G7SelenaSubmitAction _action;
            private readonly Respondent _respondent;
            private readonly int _respId;
           

            private readonly string _fileName;


            /// <summary>
            /// Constructeur principal
            /// </summary>
            /// <param name="action">L'initiateur</param>
            /// <param name="resp">Le répondant à créer</param>
            /// <param name="respId">L'identifiant.</param>
            /// <param name="fileName"></param>
            public SelenaSubmitActionWorkerStep2(G7SelenaSubmitAction action, Respondent resp, int respId, string fileName)
            {
                _action = action;
                _respondent = resp;
                _respId = respId;

                _fileName = fileName;

            }

            /// <summary>
            /// Obtient l'ID du répondant à traiter.
            /// </summary>
            /// <returns>L'ID du répondant.</returns>
            public int GetRespId()
            {
                return _respId;
            }

            /// <summary>
            /// Retourne l'instance du submit action lié à cette instance.
            /// </summary>
            /// <returns>La référence de l'action Submit lié à ce travailleur.</returns>
            public ISubmitAction GetSubmitAction()
            {
                return _action;
            }

            /// <summary>
            /// Cette méthode contient le gros du travail à faire sur le fil d'exécution.
            /// </summary>
            public void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                try
                {
                    if (_action._stopped) return;

                    var db = ServicesContainer.GetService<IbrandDatabase>();
                    if (!db.GetAnswersMap(_respId).ContainsKey(QUESTION_VIMEO_ID))
                    {
                        // On envoie vers VIMEO maintenant.
                        /*var uploadVideo = new UploadVideoToVimeo(_fileName, int.Parse(_respondent.Id), false);
                        uploadVideo.Init(_key, _secretKey, _token, _secretToken);
                        uploadVideo.DoWork();

                        var vimeoId = uploadVideo.VideoId;
                        if (uploadVideo.Success && vimeoId != null)
                        {
                            // Enregistrement de la question Viméo
                            var vimeoA = new Answer(QuestionText.Type, QUESTION_VIMEO_ID, 1, vimeoId);
                            db.DeleteAnswer(_respId, vimeoA);
                            db.InsertAnswer(_respId, vimeoA);

                            _respondent.Answers.Add(vimeoA);

                            _action.UiSynchro?.AddUiMessage("Video transformed, thumbnail created, VIMEO sent.", 0);

                            ThirdWorkerStep = new SendFilesToPrintstationWorker(_action, _respondent, _respId);
                            _pool?.AddWorker(ThirdWorkerStep);
                        }
                        else
                        {
                            log.Error("Error while VIMEO sending. Was not successful.");
                            _action.UiSynchro?.AddUiMessage("Error while VIMEO sending. Was not successful.", 1);
                        }*/
                        /*
                        using (var file = new VimeoDotNet.Net.BinaryContent(_fileName))
                        {
                            var client = new VimeoDotNet.VimeoClient("5e8984e458692a57658d11e3626a4eda");

                            // act
                            var completedRequest = client.UploadEntireFile(file);

                            if (completedRequest.IsVerifiedComplete)
                            {
                                var videoClip = completedRequest.ClipUri;

                                var start_index = videoClip.LastIndexOf('/') + 1;
                                var lenght = videoClip.LastIndexOf(':') - start_index;

                                var vimeoId = videoClip.Substring(start_index, lenght);
                                // Enregistrement de la question Viméo
                                var vimeoA = new Answer(QuestionText.Type, QUESTION_VIMEO_ID, 1, vimeoId);
                                db.DeleteAnswer(_respId, vimeoA);
                                db.InsertAnswer(_respId, vimeoA);

                                _respondent.Answers.Add(vimeoA);

                                _action.UiSynchro?.AddUiMessage("Video transformed, thumbnail created, VIMEO sent.", 0);

                                db.SetRespondentReadyToSync(_respId);
                                _action.SynchEvents?.NewItemEvent.Set();
                            }
                            else
                            {
                                log.Error("Error while VIMEO sending. Was not successful.");
                                _action.UiSynchro?.AddUiMessage("Error while VIMEO sending. Was not successful.", 1);
                            }
                        }
                        */
                    }
                    else
                    {
                        // S'il y a déjà une réponse.
                        _action.UiSynchro?.AddUiMessage("Video already sent.", 0);

                        db.SetRespondentReadyToSync(_respId);
                        _action.NewRespondentEventHandle?.Set();
                    }
                }
                catch (System.Exception ex)
                {
                    log.Fatal("Error while T1DFCSubmitAction_WorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                    _action.UiSynchro?.AddUiMessage("Error while T1DFCSubmitAction_WorkerStep2#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }
        }
    }
}
