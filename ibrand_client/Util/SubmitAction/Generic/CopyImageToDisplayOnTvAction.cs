using System;
using NLog;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    
    public class CopyImageToDisplayOnTvAction : AbstractSubmitAction
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("SendToDisplay", 1);
        }

        protected new void ValidateConfigData()
        {
            if (ImageQuestion == -1) throw new SystemException("No image question source");
        }

        public int ImageQuestion
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("image_question"))
                {
                    return int.Parse(_configData["image_question"]);
                }

                return -1;
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new CopyImageToDisplayOnTv(this, resp, resp.Id);
        }
    }
}
