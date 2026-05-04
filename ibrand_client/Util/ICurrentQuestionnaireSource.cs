namespace NomadLogic.iBrandDesktop.Util
{
    /// <summary>
    /// Interface indiquant que la classe peut retourner le questionnaire courant.
    /// </summary>
    public interface ICurrentQuestionnaireSource
    {
        Questionnaire GetCurrentQuestionnaire();
    }
}