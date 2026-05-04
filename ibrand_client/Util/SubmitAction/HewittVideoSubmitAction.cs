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
    public class HewittVideoSubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string RESPONDENT_SIZE = "respondent_size";
        private const string DURATION_FADE_IN_RESP = "duration_fade_in_resp";
        private const string DURATION_TRANSITION_OUTRO = "duration_transition_outro";
        private const string DURATION_INTRO = "duration_intro";
        private const string DURATION_RESPONDANT_VIDEO = "duration_respondant_video";
        private const string TRANSITION_INTRO_START = "transition_intro_start";

        private const string FILE_INTRO = "file_intro";
        private const string FILE_OUTRO = "file_outro";
        private const string FILE_LOGO = "file_logo";
        private const string FILE_SOUND = "file_sound";
        private const string FILE_TRANSITION = "file_transition";

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_startrek", 1);
            AddSubmitActionWorkerPool("upload_startrek", 1);
        }

        /// <inheritdoc />
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

                var worker = new HewittVideoSubmitActionWorker(this, resp, id, null);

                worker.DoWork(ct);
                worker.SecondWorkerStep?.DoWork(ct);
                UiSynchro?.AddProgressStep(1);
            }
        }

        /// <inheritdoc />
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new HewittVideoSubmitActionWorker(this, resp,resp.Id, _submitActionWorkerPools[1]);
        }

        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey(RESPONDENT_SIZE))
            {
                _configData.Add(RESPONDENT_SIZE, "720x406");
            }
            if (!_configData.ContainsKey(DURATION_FADE_IN_RESP))
            {
                _configData.Add(DURATION_FADE_IN_RESP, "2.00");
            }
            if (!_configData.ContainsKey(DURATION_TRANSITION_OUTRO))
            {
                _configData.Add(DURATION_TRANSITION_OUTRO, "1.00");
            }
            if (!_configData.ContainsKey(DURATION_INTRO))
            {
                _configData.Add(DURATION_INTRO, "11.00");
            }
            if (!_configData.ContainsKey(DURATION_RESPONDANT_VIDEO))
            {
                _configData.Add(DURATION_RESPONDANT_VIDEO, "15.00");
            }
            if (!_configData.ContainsKey(TRANSITION_INTRO_START))
            {
                _configData.Add(TRANSITION_INTRO_START, "8.00");
            }

            // fichiers

            if (!_configData.ContainsKey(FILE_INTRO))
            {
                _configData.Add(FILE_INTRO, "introHewitt_720x406.mp4");
            }
            if (!_configData.ContainsKey(FILE_LOGO))
            {
                _configData.Add(FILE_LOGO, "overlay_720x406.png");
            }
            if (!_configData.ContainsKey(FILE_OUTRO))
            {
                _configData.Add(FILE_OUTRO, "outroHewitt_720x406.mp4");
            }
            if (!_configData.ContainsKey(FILE_SOUND))
            {
                _configData.Add(FILE_SOUND, "sound.aac");
            }
            if (!_configData.ContainsKey(FILE_TRANSITION))
            {
                _configData.Add(FILE_TRANSITION, "transitionHewitt_720x406.mov");
            }
        }

        public string RespondentSize
        {
            get { return _configData[RESPONDENT_SIZE]; }
            set { _configData[RESPONDENT_SIZE] = value; }
        }
        public float DurationFadeInRespondantVideo
        {
            get { return float.Parse(_configData[DURATION_FADE_IN_RESP], CultureInfo.InvariantCulture); }
            set { _configData[DURATION_FADE_IN_RESP] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public float DurationTransitionOutro
        {
            get { return float.Parse(_configData[DURATION_TRANSITION_OUTRO], CultureInfo.InvariantCulture); }
            set { _configData[DURATION_TRANSITION_OUTRO] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public float DurationIntroVideo
        {
            get { return float.Parse(_configData[DURATION_INTRO], CultureInfo.InvariantCulture); }
            set { _configData[DURATION_INTRO] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public float DurationRespondantVideo
        {
            get { return float.Parse(_configData[DURATION_RESPONDANT_VIDEO], CultureInfo.InvariantCulture); }
            set { _configData[DURATION_RESPONDANT_VIDEO] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public float TransitionIntroStart
        {
            get { return float.Parse(_configData[TRANSITION_INTRO_START], CultureInfo.InvariantCulture); }
            set { _configData[TRANSITION_INTRO_START] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public static void CreateHewittVideo(string videoOut, string videoIntro, string videoOutro, string videoTransition,
            string logoOverlay, string videoRespondant, double startCutRespondant,
            float transitionIntroStart, float durationRespondantVideo, float durationIntroVideo,
            float durationTransitionOutro, float durationFadeInRespondantVideo, string respondantVideoSize,
            string soundtrack, string stringffmpegExecutable)

        {

            var startOutro = StringUtil.FormatFloat(2, durationRespondantVideo + durationIntroVideo - durationTransitionOutro);
            var durationOutroTrans = StringUtil.FormatFloat(2, durationTransitionOutro);
            var respCutStart = StringUtil.FormatFloat(2, startCutRespondant);

            /* Étape 1 => vidéo répondant*/
            var respFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".mp4");
            {
                var sb = new StringBuilder();
                sb.Append($"-i \"{videoRespondant}\"");
                sb.Append($" -i \"{logoOverlay}\"");
                sb.Append(" -r 30 -filter_complex \"");
                sb.Append($"[0:v]scale={respondantVideoSize},format=gray,fade=t=in:st={respCutStart}:d={StringUtil.FormatFloat(2, durationFadeInRespondantVideo)}:c=white,trim=start={respCutStart}:duration={StringUtil.FormatFloat(2, durationRespondantVideo)},setpts=PTS-STARTPTS[desat];");
                sb.Append("[desat][1:v]overlay[logo1]");
                sb.Append($"\" -map \"[logo1]\" \"{respFile}\"");

                LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
                if (!File.Exists(respFile)) throw new System.Exception($"Hewitt Video creation failed: respondent video file failed. \n{sb}");
            }
            
            /* Étape 2 => Assemblage*/
            var videoFinal = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".mp4");
            {
                var sb = new StringBuilder();
                sb.Append($"-i \"{videoIntro}\"");
                sb.Append($" -i \"{videoOutro}\"");
                sb.Append($" -i \"{videoTransition}\"");
                sb.Append($" -i \"{respFile}\"");
                sb.Append(" -r 30 -filter_complex \"");
                sb.Append($"[2:v]setpts=PTS+{transitionIntroStart}/TB[ckout];");
                sb.Append("[0:v][3:v]concat=n=2,fps=fps=30[main];");
                sb.Append("[main][ckout]overlay[main_trans];");
                sb.Append($"[1:v]fps=fps=30,format=pix_fmts=yuva420p,fade=t=in:st=0:d={durationOutroTrans}:alpha=1,setpts=PTS+{startOutro}/TB[outro];");
                sb.Append("[main_trans][outro]overlay[final]");
                sb.Append($"\" -map \"[final]\" \"{videoFinal}\"");

                LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
                if (!File.Exists(videoFinal)) throw new System.Exception($"Hewitt Video creation failed: assembly failed. \n{sb}");
            }

            /* Étape 3 => son*/
            {
                var sb = new StringBuilder();
                sb.Append($"-i \"{videoFinal}\"");
                sb.Append($" -i \"{soundtrack}\"");
                sb.Append($" -filter_complex \"[1:0]apad\" -shortest \"{videoOut}\"");

                LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
                if (!File.Exists(videoOut)) throw new System.Exception($"Hewitt Video creation failed: final file failed. \n{sb}");
            }
        }

        // ReSharper disable once InconsistentNaming
        private class HewittVideoSubmitActionWorker : SubmitActionWorkerAbstract<HewittVideoSubmitAction>
        {
            private readonly SubmitActionWorkerPool _pool;
            private const int QUESTION_VIDEO = 11;
            private const int QUESTION_VIDEO_MOD = 15;
            private const int QUESTION_CUT_TIME = 12;
            private const int QUESTION_VIMEOID = 13;
            private const int QUESTION_PHOTO_THUMBNAIL = 10;

            public HewittVideoSubmitActionWorkerStep2 SecondWorkerStep { get; private set; }

            public HewittVideoSubmitActionWorker(HewittVideoSubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
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

                    var baseDir = _action.GetTerminalDirectory;

                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    var videoFileName = _respondent.GetAnswerText(QUESTION_VIDEO, 1);
                    var videoFileNameMod = _respondent.GetAnswerText(QUESTION_VIDEO_MOD, 1);
                    var videoCutTimeString = _respondent.GetAnswerText(QUESTION_CUT_TIME, 1);

                    if (!File.Exists(videoFileNameMod))
                    {
                        if (!File.Exists(videoFileName)) throw new System.Exception("Original video file does not exists.");

                        var logo = baseDir + _action._configData[FILE_LOGO];
                        var introV = baseDir + _action._configData[FILE_INTRO];
                        var outroV = baseDir + _action._configData[FILE_OUTRO];
                        var transitionV = baseDir + _action._configData[FILE_TRANSITION];
                        var soundtrack = baseDir + _action._configData[FILE_SOUND];

                        var cutTime = 0.0;

                        try
                        {
                            cutTime = double.Parse(videoCutTimeString, CultureInfo.InvariantCulture);
                        }
                        catch (System.Exception)
                        {
                            log.Warn("Could not read respondant time: {0}. Using zero instead.", videoCutTimeString);
                        }

                        cutTime = cutTime - 12.0f;
                        if (cutTime < 0.0f) cutTime = 0.0f;

                        /* Création du vidéo */

                        var outputName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + ".mp4";

                        CreateHewittVideo(outputName, introV, outroV, transitionV, logo, videoFileName, cutTime,
                            _action.TransitionIntroStart, _action.DurationRespondantVideo, _action.DurationIntroVideo,
                            _action.DurationTransitionOutro, _action.DurationFadeInRespondantVideo, _action.RespondentSize,
                            soundtrack, config.FfmpegLocation);

                        if (!File.Exists(outputName))
                        {
                            throw new System.Exception("Video File not created for respondent " + _respId);
                        }

                        /* Extraction de la trame de présentation. */
                        var frameFileName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail) + GenerateUniqueId.GetId() + ".jpg";

                        // Création du vidéo, et de l'image de présentation.
                        FfmpegUtil.ExtractSingleFrame(config.FfmpegLocation, outputName, 17.0f, frameFileName);

                        if (!File.Exists(frameFileName))
                        {
                            throw new System.Exception("Frame File not created for respondent " + _respId);
                        }

                        /* Les fichiers sont ok, on met les résultats dans la BD. */
                        /*var videoAnswer = new Answer(QuestionTypeEnum.Text, QUESTION_VIDEO_MOD, 1, outputName);
                        db.DeleteAnswer(_respId, videoAnswer);
                        db.InsertAnswer(_respId, videoAnswer);

                        var photoAnswer = new Answer(QuestionTypeEnum.PhotoText, QUESTION_PHOTO_THUMBNAIL, 1, frameFileName);
                        db.DeleteAnswer(_respId, photoAnswer);
                        db.InsertAnswer(_respId, photoAnswer);

                        _respondent.Answers.Add(videoAnswer);
                        _respondent.Answers.Add(photoAnswer);

                        videoFileNameMod = outputName;*/
                    }

                    SecondWorkerStep = new HewittVideoSubmitActionWorkerStep2(_action, _respondent, _respId, videoFileNameMod, QUESTION_VIMEOID);

                    // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                    _pool?.AddWorker(SecondWorkerStep);
                }
                catch (System.Exception ex)
                {
                    var str = "Error while doing HewittVideoSubmitAction submit action for respondent: " + _respId;
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
        class HewittVideoSubmitActionWorkerStep2 : ISubmitActionWorker
        {
            private static readonly Logger log2 = LogManager.GetCurrentClassLogger();

            private readonly HewittVideoSubmitAction _action;
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
            public HewittVideoSubmitActionWorkerStep2(HewittVideoSubmitAction action, Respondent resp, int respId,
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
                    log2.Fatal("Error while HewittVideoSubmitActionWorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while HewittVideoSubmitActionWorkerStep2#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }
        }
    }
}
