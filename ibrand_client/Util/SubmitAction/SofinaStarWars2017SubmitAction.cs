using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;
using NomadLogic.Multimedia.XmlTransformation.image;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe d'action pour le projet SOF-2017-01.
    /// Combine Transformation d'image et envoi vers printstation.
    /// </summary>
    public class SofinaStarWars2017SubmitAction : AbstractSubmitActionSendFilesToPrintstation, ISubmitActionPictureTransformation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        protected override void AdditionalInitOperations()
        {
            if (PictureTransformationController != null)
            {
                PictureTransformationController.Dispose();
                PictureTransformationController = null;
            }

            PictureTransformationController = new TransformationsController();
            //PictureTransformationController.Initialize(_project.ProjectDir + QuestionnaireId + Path.DirectorySeparatorChar + "picture_transformation_config.xml");
        }

        /// <inheritdoc />
        public virtual IList<Bitmap> TransformPictures(IRespondent resp, bool preview)
        {
            return PictureTransformationController.TransformPictures(resp, preview);
        }

        /// <inheritdoc />
        public virtual TransformationsController PictureTransformationController { get; set; }

        /// <inheritdoc />
        public string PictureTransformationConfigFile => _configData["picture_transformation"];

        /// <inheritdoc />
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation", 1);
            AddSubmitActionWorkerPool("send_print", 1);
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

                var worker = new SofinaStarWars2017SubmitActionWorker(this, resp, id, null);

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

        /// <inheritdoc />
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new SofinaStarWars2017SubmitActionWorker(this, resp, resp.Id, _submitActionWorkerPools[1]);
        }

        // ReSharper disable once InconsistentNaming
        private class SofinaStarWars2017SubmitActionWorker : SubmitActionWorkerAbstract<SofinaStarWars2017SubmitAction>
        {
            private readonly SubmitActionWorkerPool _secondPool;

            public SendFilesToPrintstationWorker[] SecondWorkerStep { get; private set; }

            public SofinaStarWars2017SubmitActionWorker(SofinaStarWars2017SubmitAction action, Respondent resp,
                int respId, SubmitActionWorkerPool secondPool) : base(action, resp, respId)
            {
                // Rien de plus
                _secondPool = secondPool;
            }

            public override void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                try
                {
                    var controller = new TransformationsController();
                    //controller.Initialize(_action._project.ProjectDir + _respondent.QuestionnaireId + Path.DirectorySeparatorChar + "picture_transformation_config.xml");
                    var transformations = controller.GetTransformations(_respondent, false);
                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    foreach (var t in transformations)
                    {
                        var picture = _respondent.GetAnswerText(int.Parse(t.OutputNameQuestionNo));
                        if (string.IsNullOrEmpty(picture))
                            picture = GenerateUniqueId.GetId();

                        var fileName = StringUtil.RemoveExtension(StringUtil.GetFileName(picture)) + "." + t.OutputFormat;
                        var transformedPicture = DirectoryUtil.GetLocalIbrandTerminalDataDir(_respondent.ProjectId, _respondent.QuestionnaireId, _respondent.TerminalId,
                                                     DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                     DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + fileName;



                        // On fait uniquement la transformation si le fichier n'existe pas dans le répertoire d'images transformées.
                        if (!File.Exists(transformedPicture))
                        {
                            var transformedPictureBitmap = t.TransformImage();
                            if (t.OutputFormat.ToLower().Equals("png"))
                            {
                                transformedPictureBitmap.Save(ImageFormat.Png, 9, transformedPicture);
                            }
                            else
                            {
                                transformedPictureBitmap.Save(ImageFormat.Jpeg, 90, transformedPicture);
                            }

                            var answerPicture = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = int.Parse(t.OutputNameQuestionNo), Response = transformedPicture};
                            db.DeleteAnswer(_respId, answerPicture);
                            db.InsertAnswer(_respId, answerPicture);
                        }
                    }

                    SecondWorkerStep = new [] {
                            new SendFilesToPrintstationWorker(_action, _respondent, _respId)
                        };

                    _secondPool?.AddWorker(SecondWorkerStep[0]);
                }
                catch (System.Exception ex)
                {
                    log.Fatal(ex, "Error while Transforming pictures");

                    _action.UiSynchro?.AddUiMessage("Error while Transforming pictures", 4);
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
