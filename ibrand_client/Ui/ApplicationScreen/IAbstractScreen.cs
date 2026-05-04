using System;
using System.Threading;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.iBrandDesktop.Synchro.SynchFinishers;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public interface IAbstractScreen : ISaveLocalRespondentData, ICurrentQuestionnaireSource, IObtainCurrentFullRespondent, IInitialisable
    {
        event ChangeToPseudoModalEvent OnChangeToPseudoModal;
        event EventHandler OnFirstScreen;
        event EventHandler OnClose;
        event ChangePageEvent OnNext;
        event ChangePageEvent OnBack;
        event ChangePageEvent OnGoTo;

        bool IsSaveOnThisPage { get; set; }
        bool SkipSubmitActions { get; set; }
        bool IsRespondentAlreadySaved { get; }
        bool Visible { get; set; }
        bool InvokeRequiredForm();
        Control ParentForm { get; set; }
        void Recycle();
        void SetActivated();
        void SetActivatedFlag();
        void BringToFront();
        void Invalidate();
        bool ChangeLanguage(string language);
        void InvokeForm(ChangeScreenDelegate changeScreenDelegate, int index);
        void Dispose();
        void InvokeForm(Action showFirstScreenDelegate);
        void FlagForClosing();
        /// Provoque le rafaîchissement de l'écran de façon thread-safe du UI.
        void InvokeRefresh();
    }
}