namespace NomadLogic.Common.Ui.NomadScreen.ScreenAction
{
    /// <summary>
    /// Implémentation de base d'une action d'écran.
    /// </summary>
    public abstract class AbstractScreenAction : IScreenAction
    {
        public bool IsCritical { get; set; }
        public bool ShowErrorMessage { get; set; }
        public string ErrorMessageFr { get; set; }
        public string ErrorMessageEn { get; set; }
    }
}