using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Test.Common.Respondent
{
    [PublicAPI]
    public class MockRespondent : IRespondent
    {
        public Answer GetAnswer(int questionNo, int choiceNo = 1)
        {
            return MockAnswer.FirstOrDefault(a => a.QuestionNo == questionNo && a.ChoiceNo == choiceNo);
        }

        public Answer GetAnswer(QuestionIdentifier qi)
        {
            return qi == null ? null : GetAnswer(qi.QuestionNo, qi.ChoiceNo);
        }

        public string GetAnswerText(int questionNo, int choiceNo = 1)
        {
            return MockAnswer.FirstOrDefault(a => a.QuestionNo == questionNo && a.ChoiceNo == choiceNo )?.Response;
        }

        public string GetAnswerText(QuestionIdentifier qi)
        {
            return qi == null ? null : GetAnswerText(qi.QuestionNo, qi.ChoiceNo);
        }

        public List<Answer> MockAnswer { get; set; } = new List<Answer>();

        public string Language { get; set; }

        public string QuestionnaireDirectory { get; set; }

        public string TerminalDirectory { get; }

        public string GetContextDirectory()
        {
            throw new System.NotImplementedException();
        }

        public int EventId { get; set; }

        public string Date { get; set; }

        public int ProjectId { get; set; }

        public int QuestionnaireId { get; set; }

        public int TerminalId { get; set; }

        public int Id { get; set; } = -1;

        public string TimeZone { get; set; }

        public bool IsSynchronized { get; set; }
        public List<DynamicHeader> DynamicHeaders { get; set; } = new List<DynamicHeader>();
        public List<Answer> Answers { get; set; }
        public string ExternalUniqueIdentifier { get; set; }
    }
}