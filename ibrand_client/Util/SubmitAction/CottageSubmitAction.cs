using System;
using NLog;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    public class CottageSubmitAction : AbstractSubmitActionPictureTransformation
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformationAndFtp", 1);
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new CottageSubmitActionWorker(this, resp, resp.Id);
        }

        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey("ftp_address")) throw new SystemException("WebFacadeConfig file does not contain an ftp adress.");
        }
    }
}
