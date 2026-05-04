using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Ui.NomadScreen
{
    public interface ISaveToQuestionnaire
    {
        bool Save(ISaveLocalRespondentData saveTo, string activeLanguage);
    }
}