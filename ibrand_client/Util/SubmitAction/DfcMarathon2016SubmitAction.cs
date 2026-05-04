using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    public class DfcMarathon2016SubmitAction : AbstractSubmitActionSendFilesToPrintstation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_dfc", 1);
            AddSubmitActionWorkerPool("upload_dfc", 1);
            AddSubmitActionWorkerPool("SendFilesToPrintstationSubmitAction_dfc", 3);
        }
        
        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var temp in respondentList.TakeWhile(respId => ct == null || !ct.Value.IsCancellationRequested).Select(
                respId => new DfcMarathon2016SubmitActionWorker(this, null, respId, null)))
            {
                temp.DoWork(ct);
                temp.SecondWorkerStep?.DoWork(ct);
                temp.SecondWorkerStep?.ThirdWorkerStep?.DoWork(ct);
                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new DfcMarathon2016SubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }

        protected override void ValidateConfigData()
        {
            base.ValidateConfigData();

            // 70, 215, 576, 865

            if (_configData.ContainsKey("x"))
                PictureInfoTable[0] = int.Parse(_configData["x"]);
            else
                PictureInfoTable[0] = 70;

            if (_configData.ContainsKey("y"))
                PictureInfoTable[1] = int.Parse(_configData["y"]);
            else
                PictureInfoTable[1] = 215;

            if (_configData.ContainsKey("w"))
                PictureInfoTable[2] = int.Parse(_configData["w"]);
            else
                PictureInfoTable[2] = 576;

            if (_configData.ContainsKey("h"))
                PictureInfoTable[3] = int.Parse(_configData["h"]);
            else
                PictureInfoTable[3] = 865;
        }

        public int[] PictureInfoTable { get; } = new int[4];

        // ReSharper disable once InconsistentNaming
        private class DfcMarathon2016SubmitActionWorker : SubmitActionWorkerAbstract<DfcMarathon2016SubmitAction>
        {

            private readonly SubmitActionWorkerPool _pool;
            private const int QUESTION_VIDEO = 3;
            private const int QUESTION_PHOTO_THUMBNAIL = 2;
            private const int QUESTION_LOCATION = 10;
            private const int QUESTION_ORIGINAL_PICTURE = 1;

            public DfcMarathon2016SubmitActionWorkerStep2 SecondWorkerStep { get; private set; }

            public DfcMarathon2016SubmitActionWorker(DfcMarathon2016SubmitAction action, Respondent resp, int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
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
                    var location = int.Parse(_respondent.GetAnswerText(QUESTION_LOCATION, 1));

                    if (videoFileName == null || !File.Exists(videoFileName))
                    {
                        /* Sélection du overlay selon la question de location */
                        var overlayFile = baseDir;
                        switch (location)
                        {
                            case 1:
                                overlayFile += "blue_nose.png";
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                overlayFile += "multisport_canada.png";
                                break;
                            case 11:
                                overlayFile += "niagara.png";
                                break;
                            case 12:
                                overlayFile += "county.png";
                                break;
                            case 13:
                                overlayFile += "pei.png";
                                break;
                            case 14:
                                overlayFile += "legs_litteracy.png";
                                break;
                            case 15:
                            case 16:
                                overlayFile += "gthl.png";
                                break;
                            default:
                                throw new System.Exception("No association with overlay file with combo choice number: " + location);
                        }

                        /* Modification de la photo du répondant */
                        var oriPicture = _respondent.GetAnswerText(QUESTION_ORIGINAL_PICTURE, 1);
                        var keyedPicture = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                        
                        if (!ChromaKeyManager.ChromaKey(oriPicture, keyedPicture))
                        {
                            throw new System.Exception("Unable to key picture: " + oriPicture);
                        }

                        var overlayRespondent = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");

                        using (var bmpResp = FileUtil.LoadBitmap(keyedPicture))
                        {
                            bmpResp.SetResolution(300, 300);
                            using (var canevas = new Bitmap(1080, 1080, PixelFormat.Format32bppArgb))
                            {
                                canevas.SetResolution(300, 300);
                                using (var g = Graphics.FromImage(canevas))
                                {
                                    var tab = _action.PictureInfoTable;
                                    g.DrawImage(bmpResp, new Rectangle(tab[0],tab[1],tab[2],tab[3]),
                                        new Rectangle(0, 0, bmpResp.Width, bmpResp.Height), GraphicsUnit.Pixel);
                                }

                                canevas.Save(ImageFormat.Png, 9, overlayRespondent);
                            }
                        }

                        /* Création du vidéo */

                        var videoBase = baseDir + "base_movie.mp4";
                        var roadOverlay = baseDir + "road_overlay.png";
                        videoFileName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + ".mp4";
                        
                        FfmpegUtil.OverlayOnVideo(config.FfmpegLocation, videoBase, videoFileName, new [] {roadOverlay, overlayRespondent, overlayFile});

                        if (!File.Exists(videoFileName))
                        {
                            throw new System.Exception("Video File not created for respondent " + _respId);
                        }

                        /* Extraction de la trame de présentation. */
                        var frameFileName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail) + GenerateUniqueId.GetId() + ".jpg";

                        // Création du vidéo, et de l'image de présentation.
                        FfmpegUtil.ExtractSingleFrame(config.FfmpegLocation, videoFileName, 6.0f, frameFileName);
                        
                        if (!File.Exists(frameFileName))
                        {
                            throw new System.Exception("Frame File not created for respondent " + _respId);
                        }

                        /* Les fichiers sont ok, on met les résultats dans la BD. */
//                        var videoAnswer = new Answer(QuestionTypeEnum.Text, QUESTION_VIDEO, 1, videoFileName);
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

                    SecondWorkerStep = new DfcMarathon2016SubmitActionWorkerStep2(_action, _respondent, _respId, videoFileName, _action._submitActionWorkerPools[2]);

                    // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                    _pool?.AddWorker(SecondWorkerStep);
                }
                catch (System.Exception ex)
                {
                    var str = "Error while doing DFC submit action for respondent: " + _respId;
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
        class DfcMarathon2016SubmitActionWorkerStep2 : ISubmitActionWorker
        {
            private static readonly Logger log2 = LogManager.GetCurrentClassLogger();
            private readonly QuestionIdentifier QUESTION_VIMEO_ID = new QuestionIdentifier(4);

            private readonly DfcMarathon2016SubmitAction _action;
            private readonly Respondent _respondent;
            private readonly int _respId;

            private readonly string _fileName;

            private readonly SubmitActionWorkerPool _pool;

            internal SendFilesToPrintstationWorker ThirdWorkerStep { get; private set; }

            /// <summary>
            /// Constructeur principal
            /// </summary>
            /// <param name="action">L'initiateur</param>
            /// <param name="resp">Le répondant à créer</param>
            /// <param name="respId">L'identifiant.</param>
            /// <param name="fileName">Le fichier à envoyer</param>
            /// <param name="thirdPool">Le bassin d'exécution suivant.</param>
            public DfcMarathon2016SubmitActionWorkerStep2(DfcMarathon2016SubmitAction action, Respondent resp, int respId, string fileName, SubmitActionWorkerPool thirdPool)
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
                    if (!db.GetAnswersMap(_respId).ContainsKey(QUESTION_VIMEO_ID))
                    {
                        /*
                        using (var file = new VimeoDotNet.Net.BinaryContent(_fileName))
                        {
                            var client = new VimeoDotNet.VimeoClient("5e8984e458692a57658d11e3626a4eda");

                            // act
                            var completedRequest = client.UploadEntireFile(file);

                            if (completedRequest.IsVerifiedComplete)
                            {
                                var videoClip = completedRequest.ClipUri;

                                var startIndex = videoClip.LastIndexOf('/') + 1;
                                var lenght = videoClip.LastIndexOf(':') - startIndex;

                                var vimeoId = videoClip.Substring(startIndex, lenght);
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
                    log2.Fatal("Error while DfcMarathon2016SubmitActionWorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while DfcMarathon2016SubmitActionWorkerStep2#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }
        }
    }
}
