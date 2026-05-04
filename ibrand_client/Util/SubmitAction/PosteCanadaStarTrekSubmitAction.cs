using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    public class PosteCanadaStarTrekSubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string RESPONDENT_SIZE = "respondent_size";
        private const string RESPONDENT_PAD = "respondent_pad";
        private const string INTRO_START = "transition_intro_start";
        private const string INTRO_DURATION = "transition_intro_duration";
        private const string OUTRO_START = "transition_outro_start";
        private const string OUTRO_DURATION = "transition_outro_duration";
        private const string INTRO_TOTAL_DURATION = "intro_total_duration";
        private const string OUTRO_TOTAL_DURATION = "outro_total_duration";

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_startrek", 1);
            AddSubmitActionWorkerPool("upload_startrek", 1);
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

                var worker = new PosteCanadaStarTrekSubmitActionWorker(this, resp, id, null);

                worker.DoWork(ct);
                worker.SecondWorkerStep?.DoWork(ct);
                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new PosteCanadaStarTrekSubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }

        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey(RESPONDENT_SIZE))
            {
                _configData.Add(RESPONDENT_SIZE, "324x487"); //"1280x1920");
            }
            if (!_configData.ContainsKey(RESPONDENT_PAD))
            {
                _configData.Add(RESPONDENT_PAD, "720:540:228:0"); //"2840:2130:900:0");
            }
            if (!_configData.ContainsKey(INTRO_START))
            {
                _configData.Add(INTRO_START, "0.25");
            }
            if (!_configData.ContainsKey(INTRO_DURATION))
            {
                _configData.Add(INTRO_DURATION, "2.25");
            }
            if (!_configData.ContainsKey(OUTRO_START))
            {
                _configData.Add(OUTRO_START, "0.25");
            }
            if (!_configData.ContainsKey(OUTRO_DURATION))
            {
                _configData.Add(OUTRO_DURATION, "2.25");
            }
            if (!_configData.ContainsKey(INTRO_TOTAL_DURATION))
            {
                _configData.Add(INTRO_TOTAL_DURATION, "5.0");
            }
            if (!_configData.ContainsKey(OUTRO_TOTAL_DURATION))
            {
                _configData.Add(OUTRO_TOTAL_DURATION, "6.0");
            }
        }

        public string RespondentSize
        {
            get { return _configData[RESPONDENT_SIZE]; }
            set { _configData[RESPONDENT_SIZE] = value; }
        }
        public string RespondentPad
        {
            get { return _configData[RESPONDENT_PAD]; }
            set { _configData[RESPONDENT_PAD] = value; }
        }
        public float IntroStart
        {
            get { return float.Parse(_configData[INTRO_START], CultureInfo.InvariantCulture); }
            set { _configData[INTRO_START] = value.ToString(CultureInfo.InvariantCulture); }
        }
        public float IntroDuration
        {
            get { return float.Parse(_configData[INTRO_DURATION], CultureInfo.InvariantCulture); }
            set { _configData[INTRO_DURATION] = value.ToString(CultureInfo.InvariantCulture); }
        }
        public float IntroTotalDuration
        {
            get { return float.Parse(_configData[INTRO_TOTAL_DURATION], CultureInfo.InvariantCulture); }
            set { _configData[INTRO_TOTAL_DURATION] = value.ToString(CultureInfo.InvariantCulture); }
        }
        public float OutroSTart
        {
            get { return float.Parse(_configData[OUTRO_START], CultureInfo.InvariantCulture); }
            set { _configData[OUTRO_START] = value.ToString(CultureInfo.InvariantCulture); }
        }
        public float OutroDuration
        {
            get { return float.Parse(_configData[OUTRO_DURATION], CultureInfo.InvariantCulture); }
            set { _configData[OUTRO_DURATION] = value.ToString(CultureInfo.InvariantCulture); }
        }
        public float OutroTotalDuration
        {
            get { return float.Parse(_configData[OUTRO_TOTAL_DURATION], CultureInfo.InvariantCulture); }
            set { _configData[OUTRO_TOTAL_DURATION] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public static void CreateStarTrekVideo(string videoOut, string bkg1, string bkg2, string respondentPicture,
            FfmpegChromaKeyInfo chromaInfo, string videoTransIntro, string videoTransOutro, string grainFile,
            string respondentPictureSize, string respondentPicturePadding, string logoOverlay,
            float introTransitionStart, float introTransitionDuration, 
            float outroTransitionStart, float outroTransitionDuration,
            float introTotalDuration, float outroTotalDuration,
            string soundtrack, string stringffmpegExecutable)

        {
            /* Étape 1 => répondant*/
            var keyFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
            {
                var keyFile2 = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                var sb = new StringBuilder();
                sb.Append($"-i \"{respondentPicture}\" -filter_complex \"");
                sb.Append($"[rgb]chromakey={chromaInfo.HexadecimalRbgCode()}:{StringUtil.FormatFloat(2, chromaInfo.Similarity)}:{StringUtil.FormatFloat(2, chromaInfo.Blend)}[ckout];");
                sb.Append($"[0:v]scale={respondentPictureSize}[resize];");
                sb.Append($"[resize]pad={respondentPicturePadding}:{chromaInfo.HexadecimalRbgCode()}[pad];");
                sb.Append($"[pad]format=pix_fmts=rgb24[rgb]\" -map \"[ckout]\" \"{keyFile2}\"");

                LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
                if (!File.Exists(keyFile2)) throw new System.Exception("Star Trek Video creation failed: respondent keying file failed.");

                using (var respondent = FileUtil.LoadBitmap(keyFile2))
                {
                    using (var grain = FileUtil.LoadBitmap(grainFile))
                    {
                        using (var modResp = respondent.AddOverlayWithOriginalMask(grain))
                        {
                            modResp.Save(ImageFormat.Png, 9, keyFile);
                        }
                    }
                }
                if (!File.Exists(keyFile)) throw new System.Exception("Star Trek Video creation failed: respondent keying file failed (2).");
            }
            
            /* Étape 2 => intro*/
            var introFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".mp4");
            {
                var start = StringUtil.FormatFloat(2, introTransitionStart);
                var duration = StringUtil.FormatFloat(2, introTransitionDuration);
                var startDecrease = StringUtil.FormatFloat(2, introTransitionStart + introTransitionDuration);

                var sb = new StringBuilder();
                sb.Append($"-loop 1 -i \"{keyFile}\"");
                sb.Append($" -i \"{videoTransIntro}\"");
                sb.Append($" -loop 1 -i \"{bkg1}\"");
                sb.Append($" -loop 1 -i \"{logoOverlay}\"");
                sb.Append(" -r 30 -filter_complex \"[0:v]alphaextract[alf];[1:v][alf]alphamerge[fg];");
                sb.Append($"[fg]fade=t=in:st={start}:d={duration}:alpha=1,fade=t=out:st={startDecrease}:d={duration}:alpha=1[faded];");
                sb.Append($"[0:v]fade=t=out:st={startDecrease}:d=0.00:alpha=1,trim=duration={introTotalDuration}[subject_faded];");
                sb.Append("[2:v][subject_faded]overlay=shortest=1[out1];[out1][faded]overlay=shortest=1[v];[v][3:v]overlay=shortest=1[final]");
                sb.Append($"\" -map \"[final]\" \"{introFile}\"");

                LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
                if (!File.Exists(introFile)) throw new System.Exception("Star Trek Video creation failed: intro file failed.");
            }

            /* Étape 3 => outro*/
            var outroFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".mp4");
            {
                var start = StringUtil.FormatFloat(2, outroTransitionStart);
                var duration = StringUtil.FormatFloat(2, outroTransitionDuration);
                var startDecrease = StringUtil.FormatFloat(2, outroTransitionStart + outroTransitionDuration);

                var sb = new StringBuilder();
                sb.Append($"-loop 1 -i \"{keyFile}\"");
                sb.Append($" -i \"{videoTransOutro}\"");
                sb.Append($" -loop 1 -i \"{bkg2}\"");
                sb.Append($" -loop 1 -i \"{logoOverlay}\"");
                sb.Append(" -r 30 -filter_complex \"[0:v]alphaextract[alf];[1:v][alf]alphamerge[fg];");
                sb.Append($"[fg]fade=t=in:st={start}:d={duration}:alpha=1,fade=t=out:st={startDecrease}:d={duration}:alpha=1[faded];");
                sb.Append($"[0:v]fade=t=in:st={startDecrease}:d=0.00:alpha=1,trim=duration={outroTotalDuration}[subject_faded];");
                sb.Append("[2:v][subject_faded]overlay=shortest=1[out1];[out1][faded]overlay=shortest=1[v];[v][3:v]overlay=shortest=1[final]");
                sb.Append($"\" -map \"[final]\" \"{outroFile}\"");

                LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
                if (!File.Exists(outroFile)) throw new System.Exception("Star Trek Video creation failed: outro file failed.");
            }

            /* Étape 4 => fichier final*/
            {
                var sb = new StringBuilder();
                sb.Append($"-i \"{introFile}\"");
                sb.Append($" -i \"{outroFile}\"");
                sb.Append($" -i \"{soundtrack}\"");
                sb.Append($" -filter_complex \"[0:v][1:v]concat[final]\" -map \"[final]\" -map 2:a \"{videoOut}\"");

                LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
                if (!File.Exists(videoOut)) throw new System.Exception("Star Trek Video creation failed: final file failed.");
            }
        }

        // ReSharper disable once InconsistentNaming
        private class PosteCanadaStarTrekSubmitActionWorker : SubmitActionWorkerAbstract<PosteCanadaStarTrekSubmitAction>
        {
            private readonly SubmitActionWorkerPool _pool;
            private const int QUESTION_VIDEO = 3;
            private const int QUESTION_VIMEOID = 4;
            private const int QUESTION_PHOTO_THUMBNAIL = 2;
            private const int QUESTION_ORIGINAL_PICTURE = 1;

            public PosteCanadaStarTrekSubmitActionWorkerStep2 SecondWorkerStep { get; private set; }

            public PosteCanadaStarTrekSubmitActionWorker(PosteCanadaStarTrekSubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
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

                    var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(_action.ProjectId, _action.QuestionnaireId, _action.TerminalId);

                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    var videoFileName = _respondent.GetAnswerText(QUESTION_VIDEO, 1);

                    if (videoFileName == null || !File.Exists(videoFileName))
                    {
                        /* Modification de la photo du répondant */
                        var oriPicture = _respondent.GetAnswerText(QUESTION_ORIGINAL_PICTURE, 1);
                        var chroma = JsonUtil.ReadJsonFile<FfmpegChromaKeyInfo>(new FileInfo(FfmpegChromaKeyInfo.DefaultConfigFileLocation));
                        var bkg1 = baseDir + "bkg_a.jpg";
                        var bkg2 = baseDir + "bkg_b.jpg";
                        var grain = baseDir + "grain overlay.png";
                        var logo = baseDir + "overlay_logo.png";
                        var videoIntro = baseDir + "intro.mp4";
                        var videoOutro = baseDir + "outro.mp4";
                        var soundtrack = baseDir + "sound.mp3";

                        /* Création du vidéo */

                        videoFileName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.ProjectId, _action.QuestionnaireId, _action.TerminalId,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + ".mp4";

                        CreateStarTrekVideo(videoFileName, bkg1, bkg2, oriPicture, chroma,
                            videoIntro, videoOutro, grain, _action.RespondentSize, _action.RespondentPad, logo,
                            _action.IntroStart, _action.IntroDuration, _action.OutroSTart, _action.OutroDuration,
                            _action.IntroTotalDuration, _action.OutroTotalDuration,
                            soundtrack, config.FfmpegLocation);

                        if (!File.Exists(videoFileName))
                        {
                            throw new System.Exception("Video File not created for respondent " + _respId);
                        }

                        /* Extraction de la trame de présentation. */
                        var frameFileName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.ProjectId, _action.QuestionnaireId, _action.TerminalId,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail) + GenerateUniqueId.GetId() + ".jpg";

                        // Création du vidéo, et de l'image de présentation.
                        FfmpegUtil.ExtractSingleFrame(config.FfmpegLocation, videoFileName, 0.0f, frameFileName);

                        if (!File.Exists(frameFileName))
                        {
                            throw new System.Exception("Frame File not created for respondent " + _respId);
                        }

                        /* Les fichiers sont ok, on met les résultats dans la BD. */
//                        var videoAnswer = new Answer(QuestionTypeEnum.Video, QUESTION_VIDEO, 1, videoFileName);
//                        db.DeleteAnswer(_respId, videoAnswer);
//                        db.InsertAnswer(_respId, videoAnswer);
//
//                        var photoAnswer = new Answer(QuestionTypeEnum.PhotoText, QUESTION_PHOTO_THUMBNAIL, 1, frameFileName);
//                        db.DeleteAnswer(_respId, photoAnswer);
//                        db.InsertAnswer(_respId, photoAnswer);
//
//                        _respondent.Answers.Add(videoAnswer);
//                        _respondent.Answers.Add(photoAnswer);
                    }

                    SecondWorkerStep = new PosteCanadaStarTrekSubmitActionWorkerStep2(_action, _respondent, _respId, videoFileName, QUESTION_VIMEOID);

                    // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                    _pool?.AddWorker(SecondWorkerStep);
                }
                catch (System.Exception ex)
                {
                    var str = "Error while doing PosteCanadaStarTrekSubmitAction submit action for respondent: " + _respId;
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
        class PosteCanadaStarTrekSubmitActionWorkerStep2 : ISubmitActionWorker
        {
            private static readonly Logger log2 = LogManager.GetCurrentClassLogger();

            private readonly PosteCanadaStarTrekSubmitAction _action;
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
            public PosteCanadaStarTrekSubmitActionWorkerStep2(PosteCanadaStarTrekSubmitAction action, Respondent resp, int respId,
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
                    log2.Fatal("Error while PosteCanadaStarTrekSubmitActionWorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while PosteCanadaStarTrekSubmitActionWorkerStep2#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }
        }
    }
}
