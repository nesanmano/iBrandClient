namespace NomadLogic.Common.Ui.NomadScreen
{
    /// Contient une méthode pour permettre d'être initialisée après le constructeur, dans un cycle de vie contrôlé par
    /// une instance externe.
    public interface IInitialisable
    {
        bool Init();
    }
}