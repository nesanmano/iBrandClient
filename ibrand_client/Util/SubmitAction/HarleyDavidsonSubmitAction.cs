using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    public class HarleyDavidsonSubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string CONTRAST = "contrast";
        private const string SOUND_OFFSET = "sound_offset";
        private const string MAX_INTRO_TIME = "max_intro_time";
        private const string MESSAGE_FADE_START = "message_fade_start";
        private const string MESSAGE_FADE_DURATION = "message_fade_duration";
        private const string BLEND_OUTRO_START = "blend_outro_start";
        private const string BLEND_OUTRO_DURATION = "blend_outro_duration";
        private const string THUMBNAIL_TIME = "thumbnail_time";

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_harley", 1);
            AddSubmitActionWorkerPool("upload_harley", 1);
        }

        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var id in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var resp = db.GetRespondent(id);
                resp.Answers = db.GetAnswers(id);

                var worker = new HarleyDavidsonSubmitActionWorker(this, resp, id, null);

                worker.DoWork(ct);
                worker.SecondWorkerStep?.DoWork(ct);
                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new HarleyDavidsonSubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }

        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey(CONTRAST))
            {
                _configData.Add(CONTRAST, "1.5");
            }
            if (!_configData.ContainsKey(SOUND_OFFSET))
            {
                _configData.Add(SOUND_OFFSET, "1.0");
            }
            if (!_configData.ContainsKey(MAX_INTRO_TIME))
            {
                _configData.Add(MAX_INTRO_TIME, "10.0");
            }
            if (!_configData.ContainsKey(MESSAGE_FADE_START))
            {
                _configData.Add(MESSAGE_FADE_START, "7.0");
            }
            if (!_configData.ContainsKey(MESSAGE_FADE_DURATION))
            {
                _configData.Add(MESSAGE_FADE_DURATION, "1.0");
            }
            if (!_configData.ContainsKey(BLEND_OUTRO_START))
            {
                _configData.Add(BLEND_OUTRO_START, "9.0");
            }
            if (!_configData.ContainsKey(BLEND_OUTRO_DURATION))
            {
                _configData.Add(BLEND_OUTRO_DURATION, "1.0");
            }
            if (!_configData.ContainsKey(THUMBNAIL_TIME))
            {
                _configData.Add(THUMBNAIL_TIME, "8.0");
            }
        }

        public static void CreateHarleyVideoVideo(string stringffmpegExecutable, string videoOut, string videoIn, string outroFile, string soundtrack, 
            string overlayLogo, string overlayMessage, double contrast, double soundOffset, double maxIntroTime, 
            double messageFadeStart, double messageFadeDuration,
            double blendOutroStart, double blendOutroDuration)
        {
            var sb = new StringBuilder();
            sb.Append($"-i \"{videoIn}\"");
            sb.Append($" -i \"{outroFile}\"");
            sb.Append($" -itsoffset {StringUtil.FormatFloat(2, soundOffset)} -i \"{soundtrack}\"");
            sb.Append($" -i \"{overlayLogo}\"");
            sb.Append($" -loop 1 -i \"{overlayMessage}\"");
            sb.Append(" -r 30 -filter_complex \"");
            sb.Append($"[0:v]eq=contrast={StringUtil.FormatFloat(2, contrast)}:saturation=0,trim=duration={StringUtil.FormatFloat(2, maxIntroTime)}[desat];");
            sb.Append("[desat][3:v]overlay[logo1];");
            sb.Append($"[4:v]fade=t=in:st={StringUtil.FormatFloat(2, messageFadeStart)}:d={StringUtil.FormatFloat(2, messageFadeDuration)}:alpha=1,trim=duration={StringUtil.FormatFloat(2, maxIntroTime)}[logo2];");
            sb.Append("[logo1][logo2]overlay[part_one];");
            sb.Append($"[1:v]format=pix_fmts=yuva420p,fade=t=in:st=0:d={StringUtil.FormatFloat(2, blendOutroDuration)}:alpha=1,setpts=PTS+{StringUtil.FormatFloat(2, blendOutroStart)}/TB[part_two];");
            sb.Append("[part_one][part_two]overlay=format=yuv420[v_out]");
            sb.Append($"\" -map \"[v_out]\" -map 2:a -shortest \"{videoOut}\"");

            LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
            if (!File.Exists(videoOut)) throw new System.Exception("Harley Davidson Video creation failed: final file failed.");
        }

        // ReSharper disable once InconsistentNaming
        private class HarleyDavidsonSubmitActionWorker : SubmitActionWorkerAbstract<HarleyDavidsonSubmitAction>
        {
            private readonly SubmitActionWorkerPool _pool;
            private const int QUESTION_VIDEO = 4;
            private const int QUESTION_VIMEOID = 3;
            private const int QUESTION_PHOTO_THUMBNAIL = 2;

            public HarleyDavidsonSubmitActionWorkerStep2 SecondWorkerStep { get; private set; }

            public HarleyDavidsonSubmitActionWorker(HarleyDavidsonSubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
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

                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    string videoFileName;

                    if (_respondent.GetAnswerText(QUESTION_VIDEO, 1).IndexOf("C:") != -1)
                    {
                        videoFileName = _respondent.GetAnswerText(QUESTION_VIDEO, 1);
                    }
                    else {
                        videoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + _respondent.GetAnswerText(QUESTION_VIDEO, 1);
                    }

                    if (StringUtil.GetFileName(videoFileName).IndexOf("_t", StringComparison.Ordinal) == -1)
                    {
                        if (!File.Exists(videoFileName)) throw new System.Exception("File does not exists: " + videoFileName);

                        var transformedVideoFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + "_t.mp4";

                        var baseDir = _action.GetTerminalDirectory;

                        var overlay = baseDir + "overlay.png";
                        if (!File.Exists(overlay)) throw new System.Exception("File does not exists: " + overlay);
                        var overlay_white = baseDir + "overlay_logoblanc.png";
                        if (!File.Exists(overlay_white)) throw new System.Exception("File does not exists: " + overlay_white);
                        var videoOutro = baseDir + "outro.mp4";
                        if (!File.Exists(videoOutro)) throw new System.Exception("File does not exists: " + videoOutro);
                        var soundtrack = baseDir + "sound.aac";
                        if (!File.Exists(soundtrack)) throw new System.Exception("File does not exists: " + soundtrack);
                        var contrast = double.Parse(_action._configData[CONTRAST], CultureInfo.InvariantCulture);
                        var soundOffset = double.Parse(_action._configData[SOUND_OFFSET], CultureInfo.InvariantCulture);
                        var maxIntroTime = double.Parse(_action._configData[MAX_INTRO_TIME], CultureInfo.InvariantCulture);
                        var messageFadeStart = double.Parse(_action._configData[MESSAGE_FADE_START], CultureInfo.InvariantCulture);
                        var messageFadeDuration = double.Parse(_action._configData[MESSAGE_FADE_DURATION], CultureInfo.InvariantCulture);
                        var blendOutroStart = double.Parse(_action._configData[BLEND_OUTRO_START], CultureInfo.InvariantCulture);
                        var blendOutroDuration = double.Parse(_action._configData[BLEND_OUTRO_DURATION], CultureInfo.InvariantCulture);
                        var thumbnailTime = float.Parse(_action._configData[THUMBNAIL_TIME], CultureInfo.InvariantCulture);

                        CreateHarleyVideoVideo(config.FfmpegLocation, transformedVideoFileName, videoFileName,
                            videoOutro, soundtrack, overlay_white, overlay, contrast, soundOffset, maxIntroTime,
                            messageFadeStart, messageFadeDuration, blendOutroStart, blendOutroDuration);

                        if (!File.Exists(transformedVideoFileName))
                        {
                            throw new System.Exception("Video File not created for respondent " + _respId);
                        }

//                        var videoAnswer = new Answer(QuestionTypeEnum.Text, QUESTION_VIDEO, 1, transformedVideoFileName);
//                        db.DeleteAnswer(_respId, videoAnswer);
//                        db.InsertAnswer(_respId, videoAnswer);

                        /* Extraction de la trame de présentation. */
                        var frameFileName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail) + GenerateUniqueId.GetId() + ".jpg";

                        // Création du vidéo, et de l'image de présentation.
                        FfmpegUtil.ExtractSingleFrame(config.FfmpegLocation, transformedVideoFileName, thumbnailTime, frameFileName);

                        if (!File.Exists(frameFileName))
                        {
                            throw new System.Exception("Frame File not created for respondent " + _respId);
                        }

//                        var photoAnswer = new Answer(QuestionTypeEnum.PhotoText, QUESTION_PHOTO_THUMBNAIL, 1, frameFileName);
//                        db.DeleteAnswer(_respId, photoAnswer);
//                        db.InsertAnswer(_respId, photoAnswer);
//
//                        _respondent.Answers.Add(videoAnswer);
//                        _respondent.Answers.Add(photoAnswer);
//
//                        videoFileName = transformedVideoFileName;
                    }

                    SecondWorkerStep = new HarleyDavidsonSubmitActionWorkerStep2(_action, _respondent, _respId, videoFileName, QUESTION_VIMEOID);

                    // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                    _pool?.AddWorker(SecondWorkerStep);
                }
                catch (System.Exception ex)
                {
                    var str = "Error while doing HarleyDavidsonSubmitActionWorker submit action for respondent: " + _respId;
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
        }

        // ReSharper disable once InconsistentNaming
        class HarleyDavidsonSubmitActionWorkerStep2 : ISubmitActionWorker
        {
            private static readonly Logger log2 = LogManager.GetCurrentClassLogger();

            private readonly HarleyDavidsonSubmitAction _action;
            private readonly Respondent _respondent;
            private readonly int _respId;
            private readonly int _vimeoQuestion;

            private readonly string _fileName;

            /// <summary>
            /// Constructeur principal
            /// </summary>
            /// <param name="action">L'initiateur</param>
            /// <param name="resp">Le répondant à créer</param>
            /// <param name="respId">L'identifiant.</param>
            /// <param name="fileName">Le fichier à envoyer</param>
            /// <param name="vimeoQUestionNo">Le numéro de question de la question vimeo</param>
            public HarleyDavidsonSubmitActionWorkerStep2(HarleyDavidsonSubmitAction action, Respondent resp, int respId,
                string fileName, int vimeoQUestionNo)
            {
                _action = action;
                _respondent = resp;
                _respId = respId;
                _fileName = fileName;
                _vimeoQuestion = vimeoQUestionNo;
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
                    if (!db.GetAnswersMap(_respId).ContainsKey(new QuestionIdentifier(_vimeoQuestion)))
                    {
                        log2.Error("filename :" + _fileName);
                        /*
                        using (var file = new BinaryContent(_fileName))
                        {
                            var client = new VimeoClient("5e8984e458692a57658d11e3626a4eda");

                            // act
                            var completedRequest = client.UploadEntireFile(file);

                            if (completedRequest.IsVerifiedComplete)
                            {
                                var videoClip = completedRequest.ClipUri;

                                var startIndex = videoClip.LastIndexOf('/') + 1;
                                var lenght = videoClip.LastIndexOf(':') - startIndex;

                                var vimeoId = videoClip.Substring(startIndex, lenght);
                                // Enregistrement de la question Viméo
                                var vimeoA = new Answer(QuestionText.Type, _vimeoQuestion, 1, vimeoId);
                                db.DeleteAnswer(_respId, vimeoA);
                                db.InsertAnswer(_respId, vimeoA);

                                _respondent.Answers.Add(vimeoA);

                                _action.UiSynchro?.AddUiMessage("Video transformed, thumbnail created, VIMEO sent.", 0);

                                db.SetRespondentReadyToSync(_respId);
                                _action.SynchEvents?.NewItemEvent.Set();
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
                        db.SetRespondentReadyToSync(_respId);
                        _action.NewRespondentEventHandle?.Set();
                    }
                }
                catch (System.Exception ex)
                {
                    log2.Fatal("Error while HarleyDavidsonSubmitActionWorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while HarleyDavidsonSubmitActionWorkerStep2#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }
        }
    }
}
