using System;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Classe pour exécuter une action très particulière sur une zone de clic dans un écran.
    /// </summary>
    public struct CustomClickAction
    {
        public CustomClickAction(Rectangle clickZone, Action action)
        {
            ClickZone = clickZone;
            ToExecute = action;
        }

        /// <summary>
        /// La zone qui réagit aux clicks.
        /// </summary>
        public Rectangle ClickZone { get; }
        /// <summary>
        /// L'action à exécuter de façon synchrone.
        /// </summary>
        public Action ToExecute { get; }
    }
}
