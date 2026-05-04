using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{

    /// <summary>
    /// Classe pour afficher des contrôles supplémentaires, où activer des actions précises quand certaines options de partages
    /// ont été activées et/ou complétées.
    /// On utilise le concepts de "flags" pour la comparaison, par comparaison de Bits. On ne supporte pas la comparaison "OU EXCLUSIF".
    /// </summary>
    public class ShareScreenOptionTrigger {
        /// Si non-nul, affiche une image à l'écran quand les conditions sont réunies.
        /// Requiert une valeur valide à "ClickZone".
        public string ImageFile { get; set; }
        /// Référence interne du Bitmap chargé en mémoire.
        internal Bitmap Bitmap { get; set; }
        /// Zone d'affichage de l'image (si non-nulle) et de clic (si une action de clic est définie).
        public Rectangle? ClickZone { get; set; }
        /// La combinaison de drapeaux sociaux à vérifie. Comment elle est vérifiée dépend de la valeur de "TriggerOr".
        public ShareSocialOption Trigger { get; set; }
        /// Action, nul par défaut, à invoquer dès que les conditions sont réunies.
        public Action TriggerAction { get; set; }
        /// Action, nul par défaut, à invoquer quand les conditions sont réunies et que l'on clique sur la zone définie par "ClickZone".
        public Action ClicAction { get; set; }
        /// Chaque action ne s'exécute qu'une fois par répondant. Préviens des boucles infinies.
        internal bool HasTriggered { get; set; }
        /// Faux par défaut. Indique, si vrai, que l'on doit comparer les drapeaux avec la logique "OU", plutôt que "ET".
        public bool TriggerOr { get; set; }

        /// <summary>
        /// Vérifie si les options déclenchées satisfassent les conditions de cette instance.
        /// On compare selon les propriétés au moment de l'invocation.
        /// </summary>
        /// <param name="toCompare">Valeur à comparer.</param>
        /// <returns></returns>
        public bool CheckFlags(ShareSocialOption toCompare) {
            if (TriggerOr) { return (Trigger & toCompare) != 0; } // OR (qui est en fait un NAND, l'inverse logique de AND)
            return Trigger == toCompare; // AND
        }
    }

}