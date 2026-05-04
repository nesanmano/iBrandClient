using NomadLogic.Common.Services.Configuration;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Envoi d'un fichier vers le serveur de eShots: version vidéo.
    /// </summary>
    public class SendImageToEshotsWorker : SendMultimediaToEshotsWorkerBase
    {
        public SendImageToEshotsWorker(SendImageToEshotsSubmitAction action, Respondent resp, int respId) : base(action, resp, respId) {}

        protected override void TransformFile(Respondent resp, IbrandDatabase db, NomadConfiguration config)
        {
            // Rien par défaut.
        }
    }
}