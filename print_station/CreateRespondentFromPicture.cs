using Canon.Eos.Framework;
using NLog;
using System.IO;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Camera;
using NomadLogic.Common.Util.Camera.Canon;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.PrintStation
{
    class CreateRespondentFromPicture : IPictureListener
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly object _lock = new object();

        private readonly QuestionnaireUtil _questionnaireUtil;
        private readonly CameraControllerCanon _canonController;

        public CreateRespondentFromPicture(QuestionnaireUtil questionnaireUtil)
        {
            _questionnaireUtil = questionnaireUtil;

            _canonController = CameraControllerCanon.GetInstance();
            if (!_canonController.Init())
            {
                log.Error("Could not initialize Camera Controller for Canon cameras.");
                return;
            }

            var cameras = _canonController.GetCurrentEosCameras();
            log.Trace("Initial cameras collected.");

            _canonController.RegisterForNewPictureEvent(this);
            _canonController.PictureSaveDirectory = new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir());
            _canonController.NewCameraEvent += NewCameraDetected;

            foreach (var eosCamera in cameras)
            {
                var files = eosCamera.Value.GetAllImagesSub();
                foreach(var fileEntry in files)
                {
                    _canonController.SaveFileOnCamera(fileEntry.Reference, eosCamera.Value);
                }
            }
        }

        private void NewCameraDetected(EosCamera camera)
        {
            var files = camera.GetAllImagesSub();
            foreach (var fileEntry in files)
            {
                _canonController.SaveFileOnCamera(fileEntry.Reference, camera);
            }
        }

        public void PictureRequestCompleted(IPictureRequest request) { }

        public void PictureRequestCancelled(IPictureRequest request) { }

        public void OnForcedUnregister() { }

        public void PictureTaken(string filePath, string[] cameraId)
        {
            var fileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_questionnaireUtil.Questionnaire.Terminal,
                               DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none)
                                          + GenerateUniqueId.GetId() + ".jpg";
            File.Move(filePath, fileName);

            SaveAnswerToQuestion(1, 1, fileName);

            _questionnaireUtil.QuestionnaireController.SaveQuestionnaire(true);
        }

        public void Dispose()
        {
            log.Trace("Closing sequence started.");
            lock (_lock)
            {
                _canonController.UnRegisterForNewPictureEvent(this); 
                _canonController.Dispose();
            }
        }

        private void SaveAnswerToQuestion(int questionNo, int choiceNo, string responseValue)
        {
            var questionnaire = _questionnaireUtil.Questionnaire;

            var question = questionnaire.GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailiable question no = {0}", questionNo);
                return;
            }

            var tmp = question.GetChoice(choiceNo);
            if (tmp == null)
            {
                log.Warn("Could not save answer. Unavailiable choice no = {0}", choiceNo);
                return;
            }

            try
            {
                var choiceText = (ChoiceText)tmp;

                var answer = new Answer{QuestionNo = question.No, ChoiceNo = choiceText.No, Response = responseValue };

                choiceText.SetAnswer(answer);
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not save answer. Text={0}", responseValue);
            }
        }


    }
}
