using System.Threading;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using static NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses.SubmitActionCreateVideoUtil;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Ce worker est fait pour transformer un video à partir d'informations contenues dans le questionnaire, en invoquant une méthode de son action (l'implémentation de cette méthode
    /// détermine la nature de cette transformation, et donc est caché à ce Worker).
    /// Il est par la suite programmé pour passer le bâton à un Worker de type "SendToVimeo".
    /// </summary>
    public class CreateVideoWorker : SubmitActionWorkerAbstract<AbstractCreateVideoAndSendSubmitAction>
    {
        public CreateVideoWorker(AbstractCreateVideoAndSendSubmitAction action, Respondent resp, int respId,
            SubmitActionWorkerNextStepInfo nextSteps) : base(action, resp, respId, nextSteps)
        {

        }

        /// <inheritdoc />
        protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
        {
            return new SendVideoToVimeoWorker(_action, _respondent, _respId, next);
        }

        internal ExecVideoCreationCommand CreationMethod { get; set; }

        internal SubmitVideoTransformationSetting VideoCreationSettings { get; set; }

        /// <inheritdoc />
        protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();
            
            // Fait en premier pour enlever les références du répondant condensé le plus vite possible.
            if (_action.WebRequestDeleteUsedNumberQuestionNo.HasValue)
            {
                var qNo = _action.WebRequestDeleteUsedNumberQuestionNo.Value;
                var nbrStr = _respondent.GetAnswer(qNo)?.Response;
                if (nbrStr != null && int.TryParse(nbrStr, out var num))
                {
                    db.DeleteCondensedAnswers(num);
                }
            }
            
            ExecuteTransformation(VideoCreationSettings, _respondent, db, config, CreationMethod);

        }
    }
}