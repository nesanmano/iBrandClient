using System.Threading;
using JetBrains.Annotations;
using Nito.AsyncEx;
using NomadLogic.Common.Serveur;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Interface de base pour les SubmitAction.
    /// </summary>
    [PublicAPI]
    public interface ISubmitAction
    {
        int ProjectId { get; }
        int QuestionnaireId { get; }
        int TerminalId { get; }
        TerminalInfo TerminalInfo { get; }
        ScreenConfig ScreenConfiguration { get; set; }

        AsyncAutoResetEvent NewRespondentEventHandle { get; }

        event OnCompleteStopDelegate OnCompleteStopReached;
        event OnRespondentFinishedDelegate OnRespondentFinish;
        event OnSavedDelegate OnRespondentSaved;

        void Init([NotNull] TerminalInfo tInfo, AsyncAutoResetEvent newRespondentHandle);
        bool IsStopped();
        bool ReSubmitRespondent(int respondentId);
        void StartWorkerPools();
        void StopProcessing();
        bool SubmitRespondent(Respondent respondent);
        void TransformNotReadyToSyncRespondents(CancellationToken? ct);
        void OnWorkFinished(int respondantId);
    }
}