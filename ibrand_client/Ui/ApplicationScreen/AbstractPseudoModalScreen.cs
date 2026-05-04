using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    /// <summary>
    /// Extension de la classe AbstractScreen pour utiliser les méthodes de contrôle pour un écran qui ne fait pas partit de la séquence habituelle
    /// d'un photobooth.
    /// <br/>Un écran pseudomodal est un écran qui remplace le concept de dialogue modal, afin d'être facilement réutilisable dans le photobooth sans avoir à la recréer
    /// à chaque fois. Quand un écran veut lancer un écran pseudomodal, il doit dire quel écran (associé à un tag), cet écran est ensuite affiché et une fois terminé on revient
    /// à l'écran qui l'a appellé.
    /// </summary>
    public abstract class AbstractPseudoModalScreen : AbstractScreen
    {
        public event ChangePageEvent OnReturn;
        protected int _lastIndex;

        /// <summary>
        /// Construteur pour satisfaire l'écran abstrait de base.
        /// </summary>
        /// <param name="size">La taille de l'écran.</param>
        /// <param name="screenConf">AbstractConfiguration de projet PhotoBooth</param>
        protected AbstractPseudoModalScreen(Size size, ScreenConfig screenConf) : base(size, screenConf) { }
        
        /// <summary>
        /// Ferme la fenêtre pseudomodale et revient au dernier écran du questionnaire.
        /// </summary>
        protected void ReturnToLastScreen()
        {
            if (OnReturn == null) return;
            BeforeOnReturnEvent();
            _activated = false;
            OnReturn?.Invoke(_lastIndex);
            _lastIndex = -1;
        }

        /// <summary>
        /// Indique la fenêtre à étée choisie pour être affichée à l'écran.
        /// </summary>
        /// <param name="lastIndex"></param>
        public void ShowPseudoModal(int lastIndex)
        {
            BeforeShowPseudoModal();
            _lastIndex = lastIndex;
            _activated = true;
        }

        /// <summary>
        /// Méthode à implémenter pour les opérations d'initialisation de la fenêtre.
        /// </summary>
        protected abstract void BeforeShowPseudoModal();

        /// <summary>
        /// Méthode à implémenter pour les opérations à effectuer avant que la fenêtre ne soit fermée et que l'on revienne au questionnaire.
        /// </summary>
        protected abstract void BeforeOnReturnEvent();
    }
}
