using System;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    // ReSharper disable once InconsistentNaming
    public class T1DFCSubmitAction : AbstractSubmitActionSendFilesToPrintstation
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
            AddSubmitActionWorkerPool("SendFilesToPrintstationSubmitAction_t1", 3);
        }

        protected override void ValidateConfigData()
        {
            base.ValidateConfigData();

            _vimeoConsumerKey = _configData["consumer_key"];
            _vimeoConsumerSecret = _configData["consumer_secret"];
            _vimeoToken = _configData["token"];
            _vimeoTokenSecret = _configData["token_secret"];

            if (_vimeoConsumerKey == null || _vimeoConsumerSecret == null || _vimeoToken == null || _vimeoTokenSecret == null)
            {
                throw new System.Exception("VIMEO information is lacking from config.txt file in projet directory.");
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

                var temp = new T1DFCSubmitActionWorker(this, null, respId, null);
                temp.DoWork(ct);

                temp.SecondWorkerStep?.DoWork(ct);

                temp.SecondWorkerStep?.ThirdWorkerStep?.DoWork(ct);

                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new T1DFCSubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }

        // ReSharper disable once InconsistentNaming
        private class T1DFCSubmitActionWorker : SubmitActionWorkerAbstract<T1DFCSubmitAction>
        {
            private const int QUESTION_VIDEO = 2;
            private const int QUESTION_CUT_VIDEO = 5;

            private readonly SubmitActionWorkerPool _pool;

            public T1DFCSubmitActionWorkerStep2 SecondWorkerStep { get; private set; }

            public T1DFCSubmitActionWorker(T1DFCSubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
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
                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    var respondent = db.GetRespondent(_respId);
                    respondent.Answers = db.GetAnswers(_respId);

                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    var videoFileName = respondent.GetAnswerText(QUESTION_VIDEO, 1);

                    string slowVideoFileName;

                    if (StringUtil.GetFileName(videoFileName).IndexOf("_slow", StringComparison.Ordinal) == -1 || !File.Exists(videoFileName))
                    {
                        var cutVideoFileName = respondent.GetAnswerText(QUESTION_CUT_VIDEO, 1);

                        throw new System.Exception("Obsolete.");
                        
                        // var videoMetadata = FfmpegUtil.GetVideoFileMetadata(null, new FileInfo(videoFileName));
                        //
                        // slowVideoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + GenerateUniqueId.GetId() + "_slow.mp4";
                        //
                        // var overlay = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(respondent.ProjectId, respondent.QuestionnaireId,
                        // DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + "overlay.png";
                        //
                        // const double SCALE = 0.1;
                        // //FfmpegUtil.SlowCropVideo(config.FfmpegLocation, cutVideoFileName, slowVideoFileName, SCALE, videoMetadata.VideoFps * SCALE, null);
                        // FfmpegUtil.OverlaySlowVideo(config.FfmpegLocation, cutVideoFileName, slowVideoFileName, videoMetadata.GetMainVideoStreamData().FrameRate.DoubleValue * SCALE, overlay, SCALE);
                        //
//                        var videoAnswer = new Answer(QuestionTypeEnum.Text, QUESTION_VIDEO, 1, slowVideoFileName);
//                        db.DeleteAnswer(_respId, videoAnswer);
//                        db.InsertAnswer(_respId, videoAnswer);
                    }
                    else
                    {
                        slowVideoFileName = videoFileName;
                    }

                    if (File.Exists(slowVideoFileName))
                    {
                        SecondWorkerStep = new T1DFCSubmitActionWorkerStep2(_action, respondent, _respId, slowVideoFileName, _action._submitActionWorkerPools[2]);

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

        // ReSharper disable once InconsistentNaming
        class T1DFCSubmitActionWorkerStep2 : ISubmitActionWorker
        {
            private static readonly Logger log2 = LogManager.GetCurrentClassLogger();
            private const int QUESTION_VIMEO_ID = 4;

            private readonly T1DFCSubmitAction _action;
            private readonly Respondent _respondent;
            private readonly int _respId;

            private readonly string _fileName;

            private readonly SubmitActionWorkerPool _pool;

            public SendFilesToPrintstationWorker ThirdWorkerStep { get; private set; }

            /// <summary>
            /// Constructeur principal
            /// </summary>
            /// <param name="action">L'initiateur</param>
            /// <param name="resp">Le répondant à créer</param>
            /// <param name="respId">L'identifiant.</param>
            /// <param name="fileName"></param>
            /// <param name="thirdPool"></param>
            public T1DFCSubmitActionWorkerStep2(T1DFCSubmitAction action, Respondent resp, int respId, string fileName, SubmitActionWorkerPool thirdPool)
            {
                _action = action;
                _respondent = resp;
                _respId = respId;
                _fileName = fileName;
                _pool = thirdPool;

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
                    if (!db.GetAnswersMap(_respId).ContainsKey(new QuestionIdentifier(QUESTION_VIMEO_ID)))
                    {
                        /*
                        using (var file = new VimeoDotNet.Net.BinaryContent(_fileName))
                        {
                            var client = new VimeoDotNet.VimeoClient("70746e347e7d942d3a919567dc7947b5");
                            //VimeoDotNet.VimeoClient client = new VimeoDotNet.VimeoClient("5e8984e458692a57658d11e3626a4eda");
                            // act
                            var completedRequest = client.UploadEntireFile(file);

                            if (completedRequest.IsVerifiedComplete)
                            {
                                var videoClip = completedRequest.ClipUri;

                                log2.Error("VIMEO ID "+ videoClip);

                                var startIndex = videoClip.LastIndexOf('/') + 1;
                                var lenght = videoClip.Length - startIndex;
                                //int start_index = videoClip.LastIndexOf('/') + 1;
                                //int lenght = videoClip.LastIndexOf(':') - start_index;*

                                var vimeoId = videoClip.Substring(startIndex, lenght);
                                log2.Error("VIMEO ID 2" + videoClip);
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
                                log2.Error("Error while VIMEO sending. Was not successful.");
                                _action.UiSynchro?.AddUiMessage("Error while VIMEO sending. Was not successful.", 1);
                            }
                        }
                */
                    }
                    else
                    {
                        // S'il y a déjà une réponse.
                        _action.UiSynchro?.AddUiMessage("Video already sent.", 0);

                        ThirdWorkerStep = new SendFilesToPrintstationWorker(_action, _respondent, _respId);
                        _pool?.AddWorker(ThirdWorkerStep);
                        /*db.SetRespondentReadyToSync(_respId);
                        _action.SynchEvents?.NewItemEvent.Set();*/
                    }
                }
                catch (System.Exception ex)
                {
                    log2.Fatal("Error while T1DFCSubmitAction_WorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                    _action.UiSynchro?.AddUiMessage("Error while T1DFCSubmitAction_WorkerStep2#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }
        }
    }
}
