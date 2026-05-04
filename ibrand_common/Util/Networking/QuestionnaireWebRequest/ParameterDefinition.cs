using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    [PublicAPI]
    [JsonConverter(typeof(ParameterDefinitionConverter))]
    public abstract class ParameterDefinition
    {
        /// <summary>
        /// Nom du paramètre
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type du paramètre (au long, incluant le namespace).
        /// Les valeurs suivantes sont des jokers pour les types par valeur:
        /// [string, int, float, double, bool]
        /// </summary>
        public string NameType { get; set; }

        /// <summary>
        /// Indique si un type est détecté comme énumération. Sa valeur ne sera crée comme pour les autre dans cette situation.
        /// </summary>
        [JsonIgnore]
        public bool IsEnum { get; set; }

        /// <summary>
        /// Créer l'objet à passer en paramètre lors de l'invocation de la méthode par réflexion.
        /// </summary>
        /// <param name="resp">Le répondant source actuel.</param>
        /// <param name="dicAnswers">Le dictionnaire de réponses actuel.</param>
        /// <returns>L'objet paramètre.</returns>
        public abstract object CreateParameterValue(IRespondent resp, Dictionary<QuestionIdentifier, Answer> dicAnswers);
    }
}