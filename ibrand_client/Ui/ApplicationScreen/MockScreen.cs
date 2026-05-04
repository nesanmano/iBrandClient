using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Synchro.SynchFinishers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    /// <summary>
    /// Une implémentation vide d'un écran, utilisé pour fin de tests ou pour certaines situation où un objet de ce type est requis pour fin de signature de méthode.
    /// Idéalement, ces situation devraient être rares, car il s'agit plutôt d'un mauvais design de méthodes.
    /// N'est pas une forme Windows, et donc n'est pas en mesure de faire quoique ce soit en terme de présentation visuelle.
    /// Il est possible d'assigner manuellement un "répondant" à une instance, qui sera utilisé comme répondant actuel dans certains cas comme source de réponses.
    /// Aucun événement ne sera jamaqis lancé, cette classe est une coquille vide.
    /// </summary>
    public class MockScreen : IAbstractScreen
    {
        public event ChangeToPseudoModalEvent OnChangeToPseudoModal;
        public event EventHandler OnFirstScreen;
        public event EventHandler OnClose;
        public event ChangePageEvent OnNext;
        public event ChangePageEvent OnBack;
        public event ChangePageEvent OnGoTo;
        public bool IsSaveOnThisPage { get; set; }
        public bool SkipSubmitActions { get; set; }
        public bool IsRespondentAlreadySaved { get; set; }
        public bool Visible { get; set; }

        public bool InvokeRequiredForm()
        {
            return false;
        }

        public Control ParentForm { get; set; }

        public void Recycle()
        {
            // Vide
        }

        public void SetActivated()
        {
            // Vide
        }

        public void SetActivatedFlag()
        {
            // vide
        }

        public void BringToFront()
        {
            // Vide
        }

        public void Invalidate()
        {
            // Vide
        }

        public bool ChangeLanguage(string language)
        {
            return false; // Ne change jamais de langue.
        }

        public void InvokeForm(ChangeScreenDelegate changeScreenDelegate, int index)
        {
            // Vide
        }

        public void Dispose()
        {
            // Vide
        }

        public void InvokeForm(Action showFirstScreenDelegate)
        {
            // vide.
        }

        public void FlagForClosing()
        {
            // vide
        }

        public void InvokeRefresh()
        {
            // vide
        }
        
        public IRespondent GetCurrentRespondentWithAnswers()
        {
            return MockRespondent;
        }

        /// <summary>
        /// Un "faux" répondant, qui sera utilisé comme source de données par ce mock object. Il ne sera jamais modifié à l'interne.
        /// </summary>
        public IRespondent MockRespondent { get; set; }

        public bool SaveAnswerToQuestion(QuestionIdentifier qi, string responseValue)
        {
            return true; // Ne fait aucune modification au "répondant" sous-jacent.
        }

        public bool SaveAnswerToQuestion(Answer answer)
        {
            return true; // Ne fait aucune modification au "répondant" sous-jacent.
        }

        public bool SaveAnswerToQuestionCheck(int questionNo, int choiceNo, bool check)
        {
            return true; // Ne fait aucune modification au "répondant" sous-jacent.
        }

        public bool SaveAnswerToQuestionCheck(QuestionIdentifier qi, bool check)
        {
            return true; // Ne fait aucune modification au "répondant" sous-jacent.
        }

        public bool SaveDynamicHeader(DynamicHeader dh)
        {
            return true; // Ne fait aucune modification au "répondant" sous-jacent.
        }

        public bool SaveDynamicHeaderCollection(ICollection<DynamicHeader> dhc)
        {
            return true; // Ne fait aucune modification au "répondant" sous-jacent.
        }

        public bool ClearDynamicHeaderCollection(params string[] dataKeyPurge)
        {
            return true; // Ne fait aucune modification au "répondant" sous-jacent.
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, DataValue dv)
        {
            throw new NotImplementedException();
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, List<DataValue> dvList)
        {
            throw new NotImplementedException();
        }

        public Questionnaire GetCurrentQuestionnaire()
        {
            throw new NotImplementedException();
        }

        public bool Init()
        {
            throw new NotImplementedException();
        }
    }
}
