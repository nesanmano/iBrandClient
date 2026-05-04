using System;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Services.Database
{
    public class CondensedAnswer
    {
        public int QuestionnaireId { get; set; }
        public int RespondentNumber { get; set; }
        public int ChoiceNo { get; set; }
        public bool Inactive { get; set; }
        public string LocalFile { get; set; }
        public string Response { get; set; }
        public DateTime LocalInsertedTime { get; set; }
        public int QuestionNo { get; set; }

        public QuestionIdentifier GetQuestionIdentifier()
        {
            return new QuestionIdentifier(QuestionNo, ChoiceNo);
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}