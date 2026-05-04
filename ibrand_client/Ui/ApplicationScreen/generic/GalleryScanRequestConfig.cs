using System.Collections.Generic;
using Newtonsoft.Json;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    public class GalleryScanRequestConfig : IJsonConfig
    {
        public string DefaultConfigFileName => "scan_request_config.json";
        
        [JsonProperty("terminal_id")]
        public int TerminalId { get; set; }
        
        [JsonRequired]
        [JsonProperty("questionnaire_id")]
        public int QuestionnaireId { get; set; }

        [JsonRequired]
        [JsonProperty("dynamic_data_filters")]
        public List<string> DataFilters { get; set; } = new List<string> { "roaming", "ride" };

        /// <summary>
        /// Pour tests unitaires. Utilise un groupe d'image fixe pour peupler la gallerie. Faux par défaut.
        /// </summary>
        [JsonProperty("use_test_converter")]
        public bool UseTestConverter { get; set; }
    }
}