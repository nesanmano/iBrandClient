namespace NomadLogic.Common.Ui.NomadScreen
{
    /// <summary>
    /// Indique qu'une instance retourne un texte spécifique selon un code de langue.
    /// TODO: Cette interface ne contient pas la définition de changement de langue (mais devrait)
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface IHasI18nText
    {
        string CurrentLanguage { get; }

        string GetCurrentText();
    }
}