using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NomadLogic.Common.Respondent
{
    /// <summary>
    /// Les classes implémentant cette interface peuvent facilement écrire dans une couche de persistance les réponses et groupes dynamique d'un répondant.
    /// </summary>
    [PublicAPI]
    public interface ISaveLocalRespondentData
    {
        /// <summary>
        /// Ajoute une réponse texte à une question, via son identifiant.
        /// </summary>
        bool SaveAnswerToQuestion(QuestionIdentifier qi, string responseValue = null);
        
        /// <summary>
        /// Ajoute une réponse complète à une question.
        /// </summary>
        bool SaveAnswerToQuestion(Answer answer);

        /// <summary>
        /// Ajoute un état à une question de type booléen (Check box).
        /// </summary>
        [Obsolete("Utiliser la version avec identifiant.")]
        bool SaveAnswerToQuestionCheck(int questionNo, int choiceNo, bool check);
        
        /// <summary>
        /// Ajoute un état à une question de type booléen (Check box).
        /// </summary>
        bool SaveAnswerToQuestionCheck(QuestionIdentifier qi, bool check);
        
        /// <summary>
        /// Ajoute un groupe dynamique à ceux existants.
        /// </summary>
        bool SaveDynamicHeader(DynamicHeader dh);
        
        /// <summary>
        /// Ajoute une liste de groupes complets
        /// </summary>
        /// <param name="dhc"></param>
        /// <returns></returns>
        bool SaveDynamicHeaderCollection(ICollection<DynamicHeader> dhc);
        
        /// <summary>
        /// Enlève les groupes identifiés par une liste de clés.
        /// </summary>
        /// <param name="dataKeyPurge">La liste de clés.</param>
        /// <returns>True si aucun erreur n'a été détectée.</returns>
        bool ClearDynamicHeaderCollection(params string[] dataKeyPurge);
        
        /// <summary>
        /// Cet appel cherche le premier groupe qui possède la même clé, et y ajoute la valeur dynamique.
        /// Si aucun groupe n'existe, on le créer.
        /// </summary>
        /// <param name="dataKey">La clé de groupe.</param>
        /// <param name="dv">La valeur à ajouter.</param>
        /// <returns>True si aucune erreur n'a été détectée.</returns>
        bool AddToFirstMatchingKeyHeader(string dataKey, DataValue dv);

        /// <summary>
        /// Cet appel cherche le premier groupe qui possède la même clé, et y ajoute les valeurs dynamiques.
        /// Si aucun groupe n'existe, on le créer.
        /// </summary>
        /// <param name="dataKey">La clé de groupe.</param>
        /// <param name="dvList">La liste de valeurs à ajouter.</param>
        /// <returns>True si aucune erreur n'a été détectée.</returns>
        bool AddToFirstMatchingKeyHeader(string dataKey, List<DataValue> dvList);
    }
}