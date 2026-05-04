using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    /// <summary>
    /// Structure de la liste des campagnes et événements tels que retournés par la requête d'événements de Eshots.
    /// </summary>
    [PublicAPI]
    public class EshotsEventsResultInfo : EshotsResult
    {
        [JsonProperty(PropertyName = "campaigns")]
        public IList<EshotsCampaign> Campaigns { get; set; }
    }
}