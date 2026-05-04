using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    public class SendImageToEshotsSubmitAction : SendMultimediaToEshotsSubmitActionBase
    {
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            var worker = new SendImageToEshotsWorker(this, resp, resp.Id);
            
            // TODO Peut-être mettre des options de transformation d'image via Image Magik dans le futur, au besoin.
            // Mais comme ces transformations peuvent se faire dans les écrans, il n'y en a pas vraiment le besoin.

            return worker;
        }
    }
}
