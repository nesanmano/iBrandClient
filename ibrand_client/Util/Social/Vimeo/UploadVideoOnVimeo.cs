using System;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    [Obsolete]
    public class UploadVideoOnVimeo
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static object _lock;

        private readonly Respondent _respondent;
        private readonly EventWaitHandle _syncEvents;

        public event OnUploadOnVimeoFinishedDelegate OnFinished;

        public UploadVideoOnVimeo(Respondent respondent, EventWaitHandle syncEvents)
        {
            _respondent = respondent;
            _syncEvents = syncEvents;

            if (_lock == null)
                _lock = new object();
        }

        public void DoWork()
        {
            try
            {
                var answerVideo = _respondent.Answers[0];//Question video
                var videoFlvName = answerVideo.Response;

                lock (_lock)
                {

                    var uploadVideo = new UploadVideoToVimeo(videoFlvName, _respondent.Id, false);
                    uploadVideo.OnUploadFinished += OnUploadFinished;

                    uploadVideo.DoWork();
                }

            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while UploadToVimeo Do work");
                
                //Finish
                OnFinished?.Invoke(Convert.ToInt32(_respondent.Id));
            }
        }

        private void OnUploadFinished(bool success, int respondentId, string videoId)
        {
            //Sync respondent
            if (success)
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();

                //Add video id answer
                //var videoIdAnswer = new Answer{QuestionType = QuestionTypeEnum.Vimeo, QuestionNo = 3, Response = videoId};
                //db.InsertAnswer(respondentId, videoIdAnswer);
                //db.SetRespondentReadyToSync(respondentId);

                _syncEvents?.Set();

                //Finish
                OnFinished?.Invoke(respondentId);
            }
            else
            {
                //Finish
                OnFinished?.Invoke(respondentId);
            }
        }
    }
}
