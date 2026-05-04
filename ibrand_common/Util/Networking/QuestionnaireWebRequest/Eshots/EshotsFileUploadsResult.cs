using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsFileUploadsResult : EshotsResult
    {
        /// <summary>
        /// Lien direct vers le fichier sur le web.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Juste le nom du fichier, sans chemin.
        /// </summary>
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// L'identifiant numérique du fichier à l'interne du système de Eshots.
        /// </summary>
        [JsonProperty(PropertyName = "mediaId")]
        public long MediaId { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}