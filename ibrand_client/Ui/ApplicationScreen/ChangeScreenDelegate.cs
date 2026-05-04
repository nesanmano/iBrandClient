namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    /// <summary>
    /// Ce délégué est utilisé pour tous les changements de page, car ces changements utilisent principalement un seul argument.
    /// </summary>
    /// <param name="nbrPages">Le nombre de pages à changer, selon la méthiode.</param>
    public delegate void ChangeScreenDelegate(int nbrPages);
}