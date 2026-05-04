using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Serveur.TerminalStructure;

namespace NomadLogic.Common.Serveur
{
    /// <summary>
    /// Cette classe représente les informations critiques pour préparer un questionnaire et les autres modules critiques à l'exécution du terminal.
    /// En gros, il n'est pas une information de terminal venant du serveur (celle-ci est contenue dans la classe <see cref="TerminalInfo"/> qui est
    /// chargé avant), mais un fichier qui qui à cette application comment se préparer et se charger pour saisir et traiter les informations pour ce terminal.
    /// </summary>
    public class TerminalIbrandDefinition
    {
        [JsonProperty("sync_mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TerminalServerSyncMode SynchronizationMode { get; set; } = TerminalServerSyncMode.CreateAnswersOnly;

        [JsonProperty("languages")]
        public string[] Languages { get; set; }

        [JsonProperty("submit_action_class")]
        public string SubmitActionClass { get; set; }
        
        [JsonProperty("loadLastRespondent")]
        public bool LoadLastRespondent { get; set; }
        
        [JsonProperty("questions")]
        public TerminalQuestion[] Questions { get; set; }
        
        [JsonProperty("fullscreen")]
        public bool IsFullSCreen { get; set; }

        [JsonProperty("loaderclass")]
        public string LoaderClass { get; set; }

        [JsonProperty("xangleconfig")]
        public string XangleConfig { get; set; }

        [JsonProperty("has_barcode_scanner")]
        public bool InitBarcodeScanner { get; set; }

        [JsonProperty("dynamic_sync_exclusion_list")]
        public List<string> DynamicHeaderSyncExclusionList { get; set; } = new List<string>();

        /// <summary>
        /// Optionel: La classe de finaliseur pour la dernière étape (envoi vers serveur ou finition locale).
        /// </summary>
        [JsonProperty("sync_class")]
        public string SyncFinisherClassName { get; set; }
    }
}
