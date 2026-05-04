using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.PresentationScreens
{
    /// <summary>
    /// Représente une structure qui contient toutes les informations nécessaires pour créer à partir d'un questionnaire
    /// une série d'écrans de présentation utilisable par iBrand Desktop et le PrintStation pour utilisation par un usager.
    /// Ces écrans sont utilisé pour présenter de l'information sur le questionnaire, collecter les réponses pour le questionnaire
    /// et à certains endroits faire du traitement sur les réponses (comme des transformations d'images par exemple).
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class PresentationData : IPresentationElement
    {
        /// <summary>
        /// Information sur le projet.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public PresentationProjectData PresentationProjectData { get; set; }

        /// <summary>
        /// La liste d'écrans de présentation.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<PresentationScreenData> Screens { get; set; }

        #region Implementation of IDisposable

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
