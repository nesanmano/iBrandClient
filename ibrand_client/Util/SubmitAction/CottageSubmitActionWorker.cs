using System;
using System.Drawing.Imaging;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe d'exécution pour le traitement de l'image.
    /// </summary>
    class CottageSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly CottageSubmitAction _action;
        private readonly int _respIdD;

        public CottageSubmitActionWorker(CottageSubmitAction action, Respondent resp, int respId)
        {
            _respondent = resp;
            _action = action;
            _respIdD = respId;
        }

        private void OnUploadFinished(bool success, int respondentId)
        {
            //Sync respondent
            if (success)
            {
                _action.UiSynchro?.AddUiMessage("Photo uploaded on FTP.", 0);

                var db = ServicesContainer.GetService<IbrandDatabase>();

                db.SetRespondentReadyToSync(respondentId);

                _action.NewRespondentEventHandle?.Set();

                //Finish
                _action.OnWorkFinished(Convert.ToInt32(_respondent.Id));
            }
            else
            {
                _action.UiSynchro?.AddUiMessage("Could NOT upload video on FTP.", 2);

                //Finish
                _action.OnWorkFinished(respondentId);
            }
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                var transformation = _action.PictureTransformationController.GetTransformations(_respondent, false);
                var picture = _respondent.GetAnswerText(int.Parse(transformation[0].OutputNameQuestionNo));
                var transformedPictureBitmap = _action.PictureTransformationController.TransformPictures(_respondent, false);

                var fileName = StringUtil.GetFileName(picture);

                var transformedPicture = //configuration.TransformedPicturesDir + 
                    fileName;
                transformedPictureBitmap[0].Save(transformedPicture, ImageFormat.Jpeg);

                //var answerPicture = new Answer(QuestionTypeEnum.Photo, int.Parse(transformation[0].OutputNameQuestionNo), 1, transformedPicture);

                var db = ServicesContainer.GetService<IbrandDatabase>();

                //db.DeleteAnswer(_respIdD, answerPicture);
                //db.InsertAnswer(_respIdD, answerPicture);

                if (_action._configData["ftp_adress"] != null)
                {
                    var uploadPhoto = new Ftp(_action._configData["ftp_adress"], "cottageftp", "springshow2014", null, transformedPicture, _respondent);
                    uploadPhoto.OnUploadFinished += OnUploadFinished;
                    _action.UiSynchro?.AddUiMessage("Uploading video on FTP...", 0);
                    uploadPhoto.DoWork();
                }
                db.SetRespondentReadyToSync(_respIdD);
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while Transforming pictures");

                _action.UiSynchro?.AddUiMessage("Error while Transforming pictures", 4);
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        public int GetRespId()
        {
            return _respIdD;
        }
    }
}