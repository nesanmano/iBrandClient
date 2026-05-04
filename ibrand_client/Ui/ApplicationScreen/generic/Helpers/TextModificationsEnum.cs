using System;
using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Liste de modifications fréquentes sur le texte avant validation et enregistrement dans une question.
    /// Voir: https://stackoverflow.com/questions/43143175/c-sharp-json-net-render-flags-enum-as-string-array
    /// </summary>
    [Flags]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum TextModificationsEnum
    {
        /// <summary>
        /// Enlève les "whitespace" superflus dans la chaîne.
        /// </summary>
        Trim,
        /// <summary>
        /// Tout en minuscule.
        /// </summary>
        ToLower,
        /// <summary>
        /// Tout en majuscule.
        /// </summary>
        ToUpper
    }
}