using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    class FoodNetworkGifAndSendPsSubmitAction : AbstractSubmitActionSendFilesToPrintstation
    {

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("Work on File Bundle", 1);
            AddSubmitActionWorkerPool("Send to Printstations", 1);
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

                var worker = new FoodNetworkGifAndSendPSSubmitActionWorker(this, resp, id, null);

                worker.DoWork(ct);

                var worker2 = worker.SecondWorkerStep;
                if (worker2 != null)
                {
                    foreach (var subWorker in worker2)
                    {
                        subWorker.DoWork(ct);
                    }
                }

                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new FoodNetworkGifAndSendPSSubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }


        // ReSharper disable once InconsistentNaming
        private class FoodNetworkGifAndSendPSSubmitActionWorker : SubmitActionWorkerAbstract<FoodNetworkGifAndSendPsSubmitAction>
        {
            private readonly SubmitActionWorkerPool _pool;

            public SendFilesToPrintstationWorker[] SecondWorkerStep { get; private set; }

            public FoodNetworkGifAndSendPSSubmitActionWorker(FoodNetworkGifAndSendPsSubmitAction action, Respondent resp,
                int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
            {

                _pool = secondPool;
            }

            public override void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                if (_action._stopped) return;

                try
                {
                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    _respondent.Answers = db.GetAnswers(_respondent.Id);

                    const int questionBundleNo = 4;
                    const int questionFirstPictureNo = 1;
                    const int questionGifNo = 2;



                    var respondent = db.GetRespondent(_respId);
                    respondent.Answers = db.GetAnswers(_respId);


                    var bundleQuestionResponse = respondent.GetAnswerText(questionBundleNo);//Question photo bundle.

                    if (bundleQuestionResponse.IndexOf(".zip") == -1)
                    {

                        #region Préparation répertoire d'images
                        if (!Directory.Exists(bundleQuestionResponse))
                        {
                            log.Warn("Bundle directory does not exists: " + bundleQuestionResponse);
                            return;
                        }

                        var di = new DirectoryInfo(bundleQuestionResponse);

                        var orderedPictureList = di.EnumerateFiles("*.jpg").ToList();

                        // Erreur fatale. Pas d'images à traiter.
                        if (!orderedPictureList.Any())
                        {
                            log.Error(
                                "Respondent has no pictures in directory. Impossible to use this respondent. Discarding it.");
                            db.SetRespondentReadyToSync(_respId);
                            _action.NewRespondentEventHandle?.Set();
                            _action.OnWorkFinished(_respId);
                            return;
                        }

                        FileUtil.SortFileCollection(orderedPictureList);
                        #endregion

                        List<FileInfo> picturesToUse = new DirectoryInfo(bundleQuestionResponse).GetFiles().ToList();

                        #region première image
                        var firstPictFileName =
                            DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.none) +
                                GenerateUniqueId.GetId() + ".jpg";

                        File.Copy(picturesToUse.First().FullName, firstPictFileName);

                        var fistQuestionAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = questionFirstPictureNo, Response = firstPictFileName};
                        db.DeleteAnswer(_respId, fistQuestionAnswer);
                        db.InsertAnswer(_respId, fistQuestionAnswer);
                        #endregion

                        #region GIF
                        var gifFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                              DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                              DirectoryUtil.ProjectDataDirectoriesModEnum.gif) +
                                          GenerateUniqueId.GetId() + ".gif";

                        // Création du GIF
                        ImageUtil.CreateGif(picturesToUse, gifFileName, true, true, 1, (int)(1f / 3f * 1000), null, new Size(400, 400));

                        var gifQuestionAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = questionGifNo, Response = gifFileName};
                        db.DeleteAnswer(_respId, gifQuestionAnswer);
                        db.InsertAnswer(_respId, gifQuestionAnswer);
                        #endregion

                        var zippedFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                                 DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                 DirectoryUtil.ProjectDataDirectoriesModEnum.bundle) +
                                             GenerateUniqueId.GetId() + ".zip";


                        if (!File.Exists(zippedFileName))
                        {
                            using (var zip = new ZipFile())
                            {
                                zip.AddFiles(picturesToUse.Select(file => file.FullName), "");
                                zip.Save(zippedFileName);
                            }
                        }

                        var photoBundleAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoBundle, QuestionNo = questionBundleNo, Response = zippedFileName};
                        db.DeleteAnswer(_respId, photoBundleAnswer);
                        db.InsertAnswer(_respId, photoBundleAnswer);

                        FileUtil.DeleteFolderContent(bundleQuestionResponse);
                        _respondent.Answers.First(a => a.QuestionNo.Equals(questionBundleNo.ToString())).Response =
                            zippedFileName;
                    }



                    #region Envoi Printstation (chaînage)
                    if (bool.Parse(_action._configData[CreateFileBundleSubmitAction.SEND_TO_PRINTSTATION]))
                    {
                        // Si le bassin est nul, alors on fait des transformations synchrones, et on aura besoin d'avoir accès au
                        // deuxième travailleur de l'extérieur.

                        SecondWorkerStep = new[]
                       {
                            new SendFilesToPrintstationWorker(_action, _respondent, _respId)
                        };

                        // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                        _pool?.AddWorker(SecondWorkerStep[0]);
                    }
                    else
                    {
                        db.SetRespondentReadyToSync(_respId);
                        _action.NewRespondentEventHandle?.Set();
                        _action.OnWorkFinished(_respId);
                    }
                    #endregion

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

            protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
            {
                throw new NotImplementedException();
            }
        }
    }
}
