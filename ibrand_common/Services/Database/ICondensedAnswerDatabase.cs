using System;
using System.Collections.Generic;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Services.Database
{
    public interface ICondensedAnswerDatabase
    {
        DateTimeOffset GetHighestDateFromCondensedAnswers(int questionnaireId, bool lowest = false);
        int InsertCondensedAnswersData(int localCurrentQuestionaireId, int eRespondentServerId, DateTime eCreationDate, int aQuestionNoInt, int aChoiceNoInt, string aResponse);
        IList<CondensedAnswer> GetCondensedAnswers(int questionnaireId, DateTimeOffset minCutOff, QuestionIdentifier qi = null, bool? inactive = null, bool? hasLocalFile = null, uint? limit = null);

        IList<CondensedAnswer> GetCondensedAnswersFromRespondent(int serverRespondentId);
        int UpdateCondensedAnswerLocalFile(int caRespondentNumber, QuestionIdentifier getQuestionIdentifier, string localFileData);
        List<Answer> GetAnswersFromCondensedTable(int respNumber);

    }
}