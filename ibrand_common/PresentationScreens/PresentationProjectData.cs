using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.PresentationScreens
{
    /// <summary>
    /// Classe qui englobe les informations sur un projet spécifique.
    /// </summary>
    [DataContract]
    [PublicAPI]
    public class PresentationProjectData : IPresentationElement
    {
        /// <summary>
        /// Identifiant numérique d'un projet.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ProjectId { get; set; }

        /// <summary>
        /// Code de projet interne. Par exemple: NOM-2016-01
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// Identifiant numérique du questionnaire.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int QuestionnaireId { get; set; }

        /// <summary>
        /// Le nom du fichier qui contient l'information pour créer le questionnaire.
        /// Le chemin est assumé comme relatif au répertoire du questionnaire.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string QuestionnaireFileName { get; set; }

        #region Implementation of IPresentation

        /// <inheritdoc />
        public void Dispose() {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool Initialize() {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}