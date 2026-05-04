using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop;
using NomadLogic.iBrandDesktop.Synchro.SynchFinishers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.Test.iBrand.Ui.ApplicationScreen
{
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
            throw new NotImplementedException();
        }

        public Control ParentForm { get; set; }
        public void Recycle()
        {
            throw new NotImplementedException();
        }

        public void SetActivated()
        {
            throw new NotImplementedException();
        }

        public void SetActivatedFlag()
        {
            throw new NotImplementedException();
        }

        public void BringToFront()
        {
            throw new NotImplementedException();
        }

        public void Invalidate()
        {
            throw new NotImplementedException();
        }

        public bool ChangeLanguage(string language)
        {
            throw new NotImplementedException();
        }

        public void InvokeForm(ChangeScreenDelegate changeScreenDelegate, int index)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void InvokeForm(Action showFirstScreenDelegate)
        {
            throw new NotImplementedException();
        }

        public void FlagForClosing()
        {
            throw new NotImplementedException();
        }

        public void InvokeRefresh()
        {
            throw new NotImplementedException();
        }
        
        public IRespondent GetCurrentRespondentWithAnswers()
        {
            return MockRespondent;
        }

        public IRespondent MockRespondent { get; set; }

        public bool SaveAnswerToQuestion(QuestionIdentifier qi, string responseValue = null)
        {
            MockAnswersSaved.Add(new Answer
            {
                QuestionIdentifier = qi, Response = responseValue
            });
            return true;
        }

        public bool SaveAnswerToQuestion(int questionNo, int choiceNo, Answer answer)
        {
            throw new NotImplementedException();
        }

        public bool SaveAnswerToQuestion(Answer answer)
        {
            throw new NotImplementedException();
        }

        public bool SaveAnswerToQuestion(int questionNo, int choiceNo, int subChoiceNo)
        {
            throw new NotImplementedException();
        }

        public bool SaveAnswerToQuestionCheck(int questionNo, int choiceNo, bool check)
        {
            throw new NotImplementedException();
        }

        public bool SaveAnswerToQuestionCheck(QuestionIdentifier qi, bool check)
        {
            throw new NotImplementedException();
        }

        public bool SaveDynamicHeader(DynamicHeader dh)
        {
            throw new NotImplementedException();
        }

        public bool SaveDynamicHeaderCollection(ICollection<DynamicHeader> dhc)
        {
            throw new NotImplementedException();
        }

        public bool ClearDynamicHeaderCollection(params string[] dataKeyPurge)
        {
            throw new NotImplementedException();
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, DataValue dv)
        {
            throw new NotImplementedException();
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, List<DataValue> dvList)
        {
            throw new NotImplementedException();
        }

        public List<Answer> MockAnswersSaved { get; set; } = new List<Answer>();

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
