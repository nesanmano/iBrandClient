namespace NomadLogic.Common.Respondent
{
    /// Défini des classes capablent de retourner le répondant temporaire qui est en train d'être remplit.
    public interface ICurrentRespondentSource
    {
        /// Retourne le répondant qui est en train d'être construit dans une interface usager.
        IRespondent GetValidatedCurrentRespondent();
    }
}