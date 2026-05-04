using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    /// <summary>
    /// Représente un mapping d'une propriété à un jeton de questionnaire.
    /// De façon implicite, on va tenter de convertir le résultat de la recherche par jeton (un "string") au type de la propriété détectée.
    /// </summary>
    [PublicAPI]
    public class PropertyQuestionnaireMapping
    {
        /// <summary>
        /// Nom de la propriété de l'objet à laquelle il faut assigner la valeur.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Si applicable, utilise les jetons via questionnaire pour aller chercher la réponse dans un questionnaire.
        /// </summary>
        public string TokenValue { get; set; }

        /// <summary>
        /// Ce string est utilisé quand la propriété est une COLLECTION générique (Pare xemple, une liste de produits).
        /// La valeur sera d'abord passé à travers l'interpréteur de jeton.
        /// Puis, la valeur modifiée sera ensuite utilisée pour créer un nouvel objet, car il s'agit d'une définition Json. (Un produit)
        /// Et finalement, sera injecté dans la collection.
        /// Faites attentions à ne pas trop imbriquer d'objets de cette façon. Cela complexifie BEAUCOUP la structure Json...
        /// </summary>
        public string JsonValue { get; set; }
    }
}