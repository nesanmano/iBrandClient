using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Structure pour décrire comment un zone peut faire changer d'écran un questionnaire.
    /// </summary>
    public class SectionSkipScreenData
    {
        /// La zone de réaction.
        public Rectangle Zone { get; set; }
        /// La nombre de page à sauter. Ne peut pas être zéro (équivaut à un no-op). Peut être négatif (Back)
        /// DÉPRÉCIÉ: Utiliser les options de changements par options.
        [Obsolete]
        public int PageSkip { get; set; }

        /// <summary>
        /// Options de changement de page avec conditions.
        /// </summary>
        public ScreenChangeOptions PageChangeOptions { get; set; }

        /// Selon l'écran, indique que l'on ne veut pas faire de validation.
        /// Utilisé pour afficher un écran qui revient immédiatement à l'appellant habituellement.
        public bool SkipValidation { get; set; }

        /// Exécute une action totalement personnalisée quand on choisi cette option.
        [Obsolete]
        public Action CustomActionOnActivation { get; set; }
    }
}
