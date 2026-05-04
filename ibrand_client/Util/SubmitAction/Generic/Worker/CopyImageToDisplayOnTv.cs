using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    public class CopyImageToDisplayOnTv : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly CopyImageToDisplayOnTvAction _action;
        private readonly int _respId;

        public CopyImageToDisplayOnTv(CopyImageToDisplayOnTvAction action, Respondent resp, int respId)
        {
            _respondent = resp;
            _action = action;
            _respId = respId;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            var answer = _respondent.GetAnswer(_action.ImageQuestion);
            var db = ServicesContainer.GetService<IbrandDatabase>();

            if (answer == null)
            {
                log.Debug("No answer. SKipping.");
                db.SetRespondentReadyToSync(_respId);
                return;
            }

            if (!File.Exists(answer.Response))
            {
                log.Error("Invalid file to copy: " + answer.Response);
                db.SetRespondentReadyToSync(_respId);
                return;
            }

            var newFilePath = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() +
                              StringUtil.GetFileName(answer.Response);

            try
            {
                File.Copy(answer.Response, newFilePath, true);
                db.SetRespondentReadyToSync(_respId);
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        public int GetRespId()
        {
            return _respId;
        }
    }
}