using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    [UsedImplicitly]
    public class StampedeCalgary2016VideoSubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const int QUESTION_PICTURE = 1;
        private const int QUESTION_VIDEO = 2;
        private const int QUESTION_VIMEOID = 4;
        private const int QUESTION_CUT = 3;
        private static readonly int[] QUESTIONS_CONTACT_PRINCIPAUX = { QUESTION_VIMEOID + 2, QUESTION_VIMEOID + 3 };
        private static readonly int[][] QUESTIONS_CONTACT_SECONDAIRES =
        {
            new [] { QUESTION_VIMEOID + 4, QUESTION_VIMEOID + 5 },
            new [] { QUESTION_VIMEOID + 6, QUESTION_VIMEOID + 7 }
        };

        private const string THUMBNAIL_TIME = "thumbnail_time";
        private const string FRAMERATE = "framerate";
        private const string SECTION_TIME = "section_time";
        private const string INTERMISSION_TIME = "intermission_time";
        private const string SCALE = "scale";
        private const string FADE_TIME = "fade_time";
        private const string SECOND_SECTION_TIME = "second_section_time";
        private const string SLOW_MULTIPLIER = "slow_multiplier";

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_stampede", 1);
            AddSubmitActionWorkerPool("upload_stampede", 2);
            AddSubmitActionWorkerPool("split_stampede", 2);
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

                var worker = new StampedeCalgary2016VideoSubmitActionWorker(this, resp, id, null, null);

                worker.DoWork(ct);

                var worker2 = worker.SecondWorkerStep;

                if (worker2 != null)
                {
                    worker2.DoWork(ct);

                    var worker3 = worker2.ThirdWorkerStep;
                    worker3?.DoWork(ct);
                }

                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new StampedeCalgary2016VideoSubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1], _submitActionWorkerPools[2]);
        }

        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey(THUMBNAIL_TIME))
            {
                _configData.Add(THUMBNAIL_TIME, "5.0");
            }

            if (!_configData.ContainsKey(FRAMERATE))
            {
                _configData.Add(FRAMERATE, "30.0");
            }

            if (!_configData.ContainsKey(SECTION_TIME))
            {
                _configData.Add(SECTION_TIME, "4.0");
            }

            if (!_configData.ContainsKey(INTERMISSION_TIME))
            {
                _configData.Add(INTERMISSION_TIME, "2.0");
            }

            if (!_configData.ContainsKey(SCALE))
            {
                _configData.Add(SCALE, "1280x720");
            }

            if (!_configData.ContainsKey(FADE_TIME))
            {
                _configData.Add(FADE_TIME, "0.5");
            }

            if (!_configData.ContainsKey(SECOND_SECTION_TIME))
            {
                _configData.Add(SECOND_SECTION_TIME, "3.0");
            }

            if (!_configData.ContainsKey(SLOW_MULTIPLIER))
            {
                _configData.Add(SLOW_MULTIPLIER, "2.5");
            }
        }

        public static void CreateStampede2016Video(string stringffmpegExecutable, string videoOut, string videoIn, string introOutroFile, string bkgVideo,
            string soundtrack, float cutTime, string overlayFile, float sectionTime, float intermissionTime, float fadeTime, float secondSectionTime, 
            float timesSlowed, float framerate, FfmpegChromaKeyInfo chromaSettings, string intermissionFile, Size respVideoSize)
        {
            var tmpMidFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".mp4");
            var tmpFinal = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".mp4");
            var frameRateStr = StringUtil.FormatFloat(2, framerate);

            var sb = new StringBuilder();
            sb.Append($"-i \"{videoIn}\"");
            sb.Append($" -i \"{bkgVideo}\"");
            sb.Append(" -r 60 -filter_complex \"");
            sb.Append($"[0:v]trim=start={StringUtil.FormatFloat(2, cutTime - (sectionTime/2))}:duration={StringUtil.FormatFloat(2, sectionTime)},setpts=PTS-STARTPTS,scale={respVideoSize.Width}:{respVideoSize.Height}[trim];");
            sb.Append($"[trim]chromakey={chromaSettings.HexadecimalRbgCode()}:{StringUtil.FormatFloat(2, chromaSettings.Similarity)}:{StringUtil.FormatFloat(2, chromaSettings.Blend)}[ckout];");
            sb.Append("[1:v][ckout]overlay=shortest=1[v_out]");
            sb.Append($"\" -map \"[v_out]\" \"{tmpMidFile}\"");

            LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
            if (!File.Exists(tmpMidFile)) throw new System.Exception(@"Stampede Video creation failed: mid video failed.\r\tCommand line- " + sb);

            var fadeTimeStr = StringUtil.FormatFloat(2, fadeTime);
            var intermissionTimeStr = StringUtil.FormatFloat(2, intermissionTime);
            var interMinusDurationTimestr = StringUtil.FormatFloat(2, intermissionTime - fadeTime);

            sb.Clear();
            sb.Append($"-i \"{tmpMidFile}\"");
            sb.Append($" -i \"{tmpMidFile}\"");
            sb.Append($" -loop 1 -i \"{introOutroFile}\"");
            sb.Append($" -loop 1 -i \"{introOutroFile}\"");
            sb.Append($" -loop 1 -i \"{intermissionFile}\"");
            sb.Append($" -loop 1 -i \"{overlayFile}\"");
            sb.Append($" -r {frameRateStr} -filter_complex \"");
            sb.Append($"[2:v]fade=t=out:st={interMinusDurationTimestr}:d={fadeTimeStr},trim=duration={intermissionTimeStr},setsar=sar=1[intro];");
            sb.Append($"[3:v]fade=t=in:st=0.0:d={fadeTimeStr},trim=duration={intermissionTimeStr},setsar=sar=1[outro];");
            sb.Append($"[4:v]fade=t=in:st=0.0:d={fadeTimeStr},trim=duration={intermissionTimeStr},fade=t=out:st={interMinusDurationTimestr}:d={fadeTimeStr},setsar=sar=1[inter];");
            sb.Append($"[1:v]trim=start={StringUtil.FormatFloat(2, (sectionTime - secondSectionTime)/2)}:duration={StringUtil.FormatFloat(2, secondSectionTime)},setpts={StringUtil.FormatFloat(2, timesSlowed)}*(PTS-STARTPTS),");
            sb.Append($"fade=t=in:st=0.0:d={fadeTimeStr},fade =t=out:st={StringUtil.FormatFloat(2, timesSlowed * secondSectionTime)}:d={fadeTimeStr},setsar=sar=1[slowed];");
            sb.Append($"[0:v]fade=t=in:st=0.0:d={fadeTimeStr},fade=t=out:st={StringUtil.FormatFloat(2, sectionTime - fadeTime)}:d={fadeTimeStr},setsar=sar=1[main_fade];");
            sb.Append("[main_fade][inter][slowed]concat=n=3[middle];");
            sb.Append("[middle][5:0]overlay=shortest=1[middle_overlay];");
            sb.Append("[intro][middle_overlay][outro]concat=n=3,format=pix_fmts=yuv420p[v_out]");
            sb.Append($"\" -map \"[v_out]\" \"{tmpFinal}\"");

            LaunchProcessUtil.LaunchProcess(stringffmpegExecutable, sb.ToString());
            if (!File.Exists(tmpFinal)) throw new System.Exception("Stampede Video creation failed: final (no sound) failed.\r\tCommand line- " + sb);

            FfmpegUtil.AddPaddedSoundtrack(stringffmpegExecutable, videoOut, tmpFinal,  soundtrack);
            if (!File.Exists(tmpFinal)) throw new System.Exception("Stampede Video creation failed: final failed.");
        }

        // ReSharper disable once InconsistentNaming
        private class StampedeCalgary2016VideoSubmitActionWorker : SubmitActionWorkerAbstract<StampedeCalgary2016VideoSubmitAction>
        {
            private readonly SubmitActionWorkerPool _secondPool;
            private readonly SubmitActionWorkerPool _thirdPool;

            public StampedeCalgary2016VideoSubmitActionWorkerStep2 SecondWorkerStep { get; private set; }

            public StampedeCalgary2016VideoSubmitActionWorker(StampedeCalgary2016VideoSubmitAction action, Respondent resp, 
                int respId, SubmitActionWorkerPool secondPool, SubmitActionWorkerPool thirdPool) : base(action, resp, respId)
            {
                // Rien de plus
                _secondPool = secondPool;
                _thirdPool = thirdPool;
            }

            protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                if (_action._stopped) return;

                try
                {
                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    string videoFileName;

                    var respVideoAnswer = _respondent.GetAnswerText(QUESTION_VIDEO, 1);

                    if (respVideoAnswer.IndexOf("C:", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        videoFileName = respVideoAnswer;
                    }
                    else
                    {
                        videoFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + respVideoAnswer;
                    }

                    if (StringUtil.GetFileName(videoFileName).IndexOf("_t", StringComparison.Ordinal) == -1)
                    {
                        if (!File.Exists(videoFileName)) throw new System.Exception("File does not exists: " + videoFileName);

                        var transformedVideoFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + "_t.mp4";

                        var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(_action.TerminalInfo);

                        var introOutro = baseDir + "intro-outro_720.jpg";
                        if (!File.Exists(introOutro)) throw new System.Exception("File does not exists: " + introOutro);
                        var bkgVideo = baseDir + "bkg_720.mp4";
                        if (!File.Exists(bkgVideo)) throw new System.Exception("File does not exists: " + bkgVideo);
                        var soundtrack = baseDir + "sound.aac";
                        if (!File.Exists(soundtrack)) throw new System.Exception("File does not exists: " + soundtrack);
                        var middleFile = baseDir + "middle_720.jpg";
                        if (!File.Exists(middleFile)) throw new System.Exception("File does not exists: " + middleFile);
                        var overlayFile = baseDir + "overlay_720.png";
                        if (!File.Exists(overlayFile)) throw new System.Exception("File does not exists: " + overlayFile);

                        var thumbnailTime = float.Parse(_action._configData[THUMBNAIL_TIME], CultureInfo.InvariantCulture);
                        var framerate = float.Parse(_action._configData[FRAMERATE], CultureInfo.InvariantCulture);
                        var sectionTime = float.Parse(_action._configData[SECTION_TIME], CultureInfo.InvariantCulture);
                        var intermissionTime = float.Parse(_action._configData[INTERMISSION_TIME], CultureInfo.InvariantCulture);
                        var fadeTime = float.Parse(_action._configData[FADE_TIME], CultureInfo.InvariantCulture);
                        var secondSectionTime = float.Parse(_action._configData[SECOND_SECTION_TIME], CultureInfo.InvariantCulture);
                        var timesSlowed = float.Parse(_action._configData[SLOW_MULTIPLIER], CultureInfo.InvariantCulture);

                        var cutTime = float.Parse(_respondent.GetAnswerText(QUESTION_CUT, 1), CultureInfo.InvariantCulture);

                        var chroma = ChromaKeyManager.GetDefaultChromaConfigFfmpeg();

                        var points = StringUtil.GetDataFromDelimitedString(_action._configData[SCALE], 'x');

                        CreateStampede2016Video(config.FfmpegLocation, transformedVideoFileName, videoFileName, introOutro,
                            bkgVideo, soundtrack, cutTime, overlayFile, sectionTime, intermissionTime, fadeTime, secondSectionTime, timesSlowed,
                            framerate, chroma, middleFile, new Size(points[0], points[1]));

                        if (!File.Exists(transformedVideoFileName))
                        {
                            throw new System.Exception("Video File not created for respondent " + _respId);
                        }

                        //On copie le fichier pour ShowPictureOnTv
                        File.Copy(transformedVideoFileName,  DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() + StringUtil.GetFileName(transformedVideoFileName));

//                        var videoAnswer = new Answer(QuestionTypeEnum.Video, QUESTION_VIDEO, 1, transformedVideoFileName);
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

//                        var photoAnswer = new Answer(QuestionTypeEnum.PhotoText, QUESTION_PICTURE, 1, frameFileName);
//                        db.DeleteAnswer(_respId, photoAnswer);
//                        db.InsertAnswer(_respId, photoAnswer);
//                        
//                        _respondent.Answers[0] = videoAnswer; // remplace la réponse de vidéo.
//                        _respondent.Answers.Add(photoAnswer);
//
//                        videoFileName = transformedVideoFileName;
                    }

                    SecondWorkerStep = new StampedeCalgary2016VideoSubmitActionWorkerStep2(_action, _respondent, _respId, videoFileName, QUESTION_VIMEOID, 
                        _thirdPool);

                    // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin. Si ce bassin existe.
                    _secondPool?.AddWorker(SecondWorkerStep);
                }
                catch (System.Exception ex)
                {
                    var str = "Error while doing submit action for respondent: " + _respId;
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
            
        }

        // ReSharper disable once InconsistentNaming
        class StampedeCalgary2016VideoSubmitActionWorkerStep2 : ISubmitActionWorker
        {

            private readonly StampedeCalgary2016VideoSubmitAction _action;
            private readonly Respondent _respondent;
            private readonly int _respId;
            private readonly int _vimeoQuestion;
            private readonly string _fileName;

            private readonly SubmitActionWorkerPool _pool;
            public StampedeCalgary2016VideoSubmitActionWorkerStep3 ThirdWorkerStep { get; private set; }

            /// <summary>
            /// Constructeur principal
            /// </summary>
            /// <param name="action">L'initiateur</param>
            /// <param name="resp">Le répondant à créer</param>
            /// <param name="respId">L'identifiant.</param>
            /// <param name="fileName">Le fichier à envoyer</param>
            /// <param name="vimeoQUestionNo">Le numéro de question de la question vimeo</param>
            /// <param name="pool">Troisième pool</param>
            public StampedeCalgary2016VideoSubmitActionWorkerStep2(StampedeCalgary2016VideoSubmitAction action, Respondent resp, int respId,
                string fileName, int vimeoQUestionNo, SubmitActionWorkerPool pool)
            {
                _action = action;
                _respondent = resp;
                _respId = respId;
                _fileName = fileName;
                _vimeoQuestion = vimeoQUestionNo;
                _pool = pool;
            }
            
            public int GetRespId()
            {
                return _respId;
            }
            
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
                        log.Error("filename :" + _fileName);
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

                                ThirdWorkerStep = new StampedeCalgary2016VideoSubmitActionWorkerStep3(_action, _respondent, _respId);

                                // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin. Si ce bassin existe.
                                _pool?.AddWorker(ThirdWorkerStep);
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

                        ThirdWorkerStep = new StampedeCalgary2016VideoSubmitActionWorkerStep3(_action, _respondent, _respId);

                        // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin. Si ce bassin existe.
                        _pool?.AddWorker(ThirdWorkerStep);
                    }
                }
                catch (System.Exception ex)
                {
                    log.Fatal("Error while StampedeCalgary2016VideoSubmitActionWorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while StampedeCalgary2016VideoSubmitActionWorkerStep2#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }
        }

        private class StampedeCalgary2016VideoSubmitActionWorkerStep3 : ISubmitActionWorker
        {
            private readonly StampedeCalgary2016VideoSubmitAction _action;
            private readonly Respondent _respondent;
            private readonly int _respId;

            public StampedeCalgary2016VideoSubmitActionWorkerStep3(StampedeCalgary2016VideoSubmitAction action,
                Respondent resp, int respId)
            {
                _action = action;
                _respondent = resp;
                _respId = respId;
            }

            public void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                try
                {
                    if (_action._stopped) return;

                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    var nouveauRepondants = new List<Respondent>();

                    foreach (var extraDataId in QUESTIONS_CONTACT_SECONDAIRES)
                    {
                        var qEmail = _respondent.Answers.FirstOrDefault(a => a.QuestionNo == extraDataId[0]);
                        var qSms = _respondent.Answers.FirstOrDefault(a => a.QuestionNo == extraDataId[1]);

                        if (qEmail == null && qSms == null) continue;

                        // On créé le nouveau répondant.
                        var newResp = new Respondent(_respondent) { Answers = new List<Answer>() };
                        // On assigne les réponses dont le numéro est plus bas que celui de la question de courriel principale.
                        var tempList = new List<Answer>();
                        tempList.AddRange(_respondent.Answers.FindAll(a => a.QuestionNo < QUESTIONS_CONTACT_PRINCIPAUX[0]));
                        foreach(var answer in tempList)
                        {
                            newResp.Answers.Add(new Answer(answer));
                        }
                        // On assigne de nouvelles réponses au question courriel et SMS principale
//                        if (qEmail != null) newResp.Answers.Add(new Answer(QuestionTypeEnum.Email, QUESTIONS_CONTACT_PRINCIPAUX[0], 1, qEmail.Response));
//                        if (qSms != null) newResp.Answers.Add(new Answer(QuestionTypeEnum.Text, QUESTIONS_CONTACT_PRINCIPAUX[1], 1, qSms.Response));

                        nouveauRepondants.Add(newResp);
                    }

                    // Une fois les nouveaux répondants créés, on les enregistre dans la DB et on les flag comme étant prêts eux-aussi
                    // On le fait après la boucle de création au cas où une erreur se produit et que l'on a ajouté des répondants dans la bd pour rien.
                    foreach (var resp in nouveauRepondants)
                    {
                        db.InsertRespondent(resp, true);
                    }

                    db.SetRespondentReadyToSync(_respId);

                    _action.UiSynchro?.AddUiMessage("Respondent Stampede action finished.", 4);
                }
                catch (System.Exception ex)
                {
                    log.Fatal("Error while StampedeCalgary2016VideoSubmitActionWorkerStep3#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while StampedeCalgary2016VideoSubmitActionWorkerStep3#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }

            public int GetRespId()
            {
                return _respId;
            }

            public ISubmitAction GetSubmitAction()
            {
                return _action;
            }
        }
    }
}
