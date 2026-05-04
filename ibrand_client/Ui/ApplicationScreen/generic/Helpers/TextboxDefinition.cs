using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    public class TextboxDefinition : ScreenUiFieldAbstract<TextBox>
    {
        /// <summary>
        /// Si non nul, le texte candidat pour la réponse doit être validé par ce validateur.
        /// </summary>
        public ITextValidation Validation { get; set; }

        /// <summary>
        /// Options (via enum drapeau) pour de légère options sur le texte.
        /// Par défaut, on fait un "Trim".
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TextModificationsEnum Modifications { get; set; } = TextModificationsEnum.Trim;
    }
}