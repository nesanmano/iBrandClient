using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsEventDaysResultInfo
    {
        /// <summary>
        /// Les champs de cette requête ne seront pas tous nécéssairement remplis. Les propriétés critiques le seront: eventId, name et la liste de jours.
        /// </summary>
        [JsonProperty(PropertyName = "events")]
        public IList<EshotsEvent> Events { get; set; }
    }
}