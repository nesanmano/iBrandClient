using System;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    /// <summary>
    /// Classe générique de SubmitAction pour gérer une situation de CameraRing (File Bundle)
    /// </summary>
    public class CreateFileBundleSubmitAction : AbstractSubmitActionSendFilesToPrintstation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        internal const string QUESTION_BUNDLE_NUMBER = "question_bundle_no";
        internal const string QUESTION_VIDEO_NUMBER = "question_video_no";
        internal const string QUESTION_FIRST_PICTURE_NUMBER = "question_first_picture_no";
        internal const string QUESTION_THUMBNAIL_NUMBER = "question_thumbnail_no";
        internal const string THUMBNAIL_TIME = "thumbnail_time";
        internal const string NORMALIZE_PICTURES = "normalize_pictures";
        internal const string NORMALIZATION_CROP_SIZE = "normalize_crop_size";
        internal const string SEND_TO_PRINTSTATION = "send_to_printstation";

        /// <inheritdoc/>
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("Work on File Bundle", 1);
            AddSubmitActionWorkerPool("Send to Printstations", 1);
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
                respondent.DynamicHeaders = db.GetDynamicHeaders(respId);

                var temp = new BundleCameraRingFilesWorker(this, respondent, respId, null);
                temp.DoWork(ct);

                // On lance la deuxième étape ici. NextWorkerStep aura alors été créé à l'interne lors du DoWork.
                if (ct != null && ct.Value.IsCancellationRequested) break;
                temp.SecondWorkerStep?.DoWork(ct);

                UiSynchro?.AddProgressStep(1);
            }
        }

        /// <inheritdoc />
        public override bool ReSubmitRespondent(int respondentId)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondent = db.GetRespondent(respondentId);

            respondent.Answers = db.GetAnswers(respondentId);
            respondent.DynamicHeaders = db.GetDynamicHeaders(respondentId);

            var ok = ProcessRespondent(respondent, respondentId);
            return ok;
        }

        /// <inheritdoc />
        public override bool SubmitRespondent(Respondent respondent)
        {
            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();
                var respId = db.InsertRespondent(respondent, false);

                var ok = ProcessRespondent(respondent, respId);

                return ok;
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Remplace la création du répondant, car il faut gérer sur quel pool il est envoyé.
        /// </summary>
        /// <param name="resp">Le répondant</param>
        /// <param name="respId">Son ID numréqiue.</param>
        /// <returns></returns>
        protected virtual bool ProcessRespondent(Respondent resp, int respId)
        {
            var temp = new BundleCameraRingFilesWorker(this, resp, respId, _submitActionWorkerPools[1]);
            return _submitActionWorkerPools[0].AddWorker(temp);
        }

        /// <inheritdoc />
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void ValidateConfigData()
        {
            base.ValidateConfigData();
            if (!_configData.ContainsKey(QUESTION_BUNDLE_NUMBER))
            {
                throw new System.Exception("Missing critical configuration (question_bundle_no) in config file: " + QUESTION_BUNDLE_NUMBER);
            }
            if (!_configData.ContainsKey(NORMALIZE_PICTURES))
            {
                _configData.Add(NORMALIZE_PICTURES, "false");
            }
            if (!_configData.ContainsKey(QUESTION_VIDEO_NUMBER))
            {
                _configData.Add(QUESTION_VIDEO_NUMBER, "-1");
            }
            if (!_configData.ContainsKey(QUESTION_THUMBNAIL_NUMBER))
            {
                _configData.Add(QUESTION_THUMBNAIL_NUMBER, "-1");
            }
            if (!_configData.ContainsKey(THUMBNAIL_TIME))
            {
                _configData.Add(THUMBNAIL_TIME, "3.0");
            }
            if (!_configData.ContainsKey(SEND_TO_PRINTSTATION))
            {
                _configData.Add(SEND_TO_PRINTSTATION, "false");
            }
        }
    }
}
