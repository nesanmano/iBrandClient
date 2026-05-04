using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Zone de réponse avec option de désigner un code de langue lors de sa sélection ou exécution déférée.
    /// </summary>
    public class LanguageZone : AnswerZone
    {
        /// <summary>
        /// Le code de langue à assigner au questionnaire (et écrans).
        /// </summary>
        [JsonRequired]
        public string LanguageCode { get; set; }
    }
}