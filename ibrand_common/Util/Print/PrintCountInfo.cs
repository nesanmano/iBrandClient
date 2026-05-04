using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Print
{
    /// <summary>
    /// Classe pour gérer les valeur du compteur d'impression.
    /// </summary>
    [DataContract]
    public class PrintCountInfo
    {
        /// <summary>
        /// Compteur pour le nombre d'actions d'impressions données sans réinitialisation.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int PrintActionCounter { get; set; }

        /// <summary>
        /// Limite supérieure pour avertir l'usager que son papier est sur le point de finir.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int PrintActionCounterThreshold { get; set; } = 370;

        /// <summary>
        /// Indique si on affiche l'avertissement quand le nombre d'impression dépasse la limite supérieure..
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool PrintActionCounterShowWarning { get; set; }
    }
}