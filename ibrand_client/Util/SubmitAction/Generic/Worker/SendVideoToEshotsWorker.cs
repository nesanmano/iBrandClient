using NomadLogic.Common.Services.Configuration;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Envoi d'un fichier vers le serveur de eShots: version vidéo.
    /// </summary>
    public class SendVideoToEshotsWorker : SendMultimediaToEshotsWorkerBase
    {
        public SendVideoToEshotsWorker(SendVideoToEshotsSubmitAction action, Respondent resp, int respId) : base(action, resp, respId) {}

        public SubmitVideoTransformationSetting VideoCreationSettings { get; set; }

        public SubmitActionCreateVideoUtil.ExecVideoCreationCommand VideoCreationMethod { get; set; }
        
        protected override void TransformFile(Respondent resp, IbrandDatabase db, NomadConfiguration config)
        {
            if (VideoCreationSettings != null && VideoCreationMethod != null)
            {
                // On fait une transformation vidéo ici. Contrairement auc Submit Action Worker de création vidéo, il n'y a pas
                // d'envoi vers Viméo, de back-up ou d'envoi FTP: juste la création du vidéo et de la manipulation de réponses qui en découle.
                // Ces opérations sont discartés si on détecte que le vidéo n'a pas besoin d'être re-créé.
                SubmitActionCreateVideoUtil.ExecuteTransformation(VideoCreationSettings, resp, db, config, VideoCreationMethod);
            }
        }
    }
}