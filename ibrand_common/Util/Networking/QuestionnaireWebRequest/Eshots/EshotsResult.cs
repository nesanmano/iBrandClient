using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    /// <summary>
    /// Toutes les requêtes vers l'API de Eshots sont basée sur le fait qu'elles peuvent échouer. Si c'est le cas, il y aura
    /// des informations des erreurs avec des Strings.
    /// Toutes les interactions donnent également un message.
    /// </summary>
    public abstract class EshotsResult
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public IList<string> Errors { get; set; }
    }
}