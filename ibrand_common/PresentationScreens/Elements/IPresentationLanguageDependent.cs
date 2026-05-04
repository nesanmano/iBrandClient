namespace NomadLogic.Common.PresentationScreens.Elements
{
    /// <summary>
    /// Propriétés et méthodes de base pour des éléments qui dépendent de la langue d'affichage
    /// </summary>
    public interface IPresentationLanguageDependent
    {
        /// <summary>
        /// Le code de langue, en format ISO. Exemple: "en", "ca-FR"
        /// </summary>
        string LanguageCode { get; set; }
    }
}