namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    /// <summary>
    /// Interface pour les éléments visuels qui réagissent sur un changement de langue.
    /// Si ce changement n'implique que des éléments d'images, l'interface "IImageInitialisable" est suffisante.
    /// </summary>
    public interface IChangeLanguage
    {
        void ChangeLanguage(AbstractScreen screen, string languageCode);
    }
}