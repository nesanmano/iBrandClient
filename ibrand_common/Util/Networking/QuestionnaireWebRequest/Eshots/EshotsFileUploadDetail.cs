using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsFileUploadDetail
    {
        /// <summary>
        /// Date de création du fichier en format UTC ("yyyy-MM-dd hh:mm:ss")
        /// </summary>
        [JsonProperty(PropertyName = "createDtm")]
        public string CreationDate { get; set; }

        /// <summary>
        /// Contenu binaire du fichier à envoyer. DOIT être encodé en base64, sinon le traitement d'envoi HTTP va échouer.
        /// </summary>
        [JsonProperty(PropertyName = "binary")]
        public string FileContent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }
    }
}