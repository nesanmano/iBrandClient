namespace NomadLogic.Common.Respondent
{
    /// <summary>
    /// Interface privilégiée pour indiquer qu'une instance peut obtenir le répondant courant.
    /// </summary>
    public interface IObtainCurrentFullRespondent
    {
        IRespondent GetCurrentRespondentWithAnswers();
    }
}