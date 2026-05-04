using System.Windows.Forms;
using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Base pour une définition de base d'un contrôle WinForm destiné à un écran de questionnaire.
    /// </summary>
    /// <typeparam name="T">Une sous-classe de Control</typeparam>
    public abstract class ScreenUiFieldAbstract<T> : ScreenFieldAbstract where T : Control
    {

        /// <summary>
        /// Référence vers l'instance de l'élément UI.
        /// </summary>
        [JsonIgnore]
        public T ControlInstance { get; set; }

        /// <summary>
        /// Indique si cette définition authorise une réponse vide.
        /// </summary>
        public bool AllowEmpty { get; set; }
    }
}