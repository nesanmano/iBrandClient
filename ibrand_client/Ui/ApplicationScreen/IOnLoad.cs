namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    /// <summary>
    /// Interface pour les éléments visuels qui s'initialise au même moment que l'écran de présentation. Implique souvent une association directe
    /// avec des éléments visuels de WinForm.
    /// </summary>
    public interface IOnLoad
    {
        bool OnLoad(AbstractScreen screen);
    }
}