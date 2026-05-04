namespace NomadLogic.Common.PresentationScreens
{
    /// <summary>
    /// Interface de base pour un écran de présentation.
    /// </summary>
    public interface IPresentationScreen : IPresentationElement {
        /// <summary>
        /// Indique que l'écran devient actif et va réagir aux interaction avec l'usager.
        /// </summary>
        void Activate();
        /// <summary>
        /// L'écran devient inactif et ne modifiera pas son état suite à une interaction avec l'usager.
        /// </summary>
        void DeActivate();
        /// <summary>
        /// L'usager change et donc on remet à zéro certaines informations concernant l'usager actuel.
        /// </summary>
        void Recycle();
    }
}