using System;
using Newtonsoft.Json;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Condition
{
    /// <summary>
    /// Interface pour évaluer une instance via les réponses d'un répondant, ou le répondant lui-même s'il y a lieu.
    /// </summary>
    [JsonConverter(typeof(ConditionJsonConverter))]
    public interface ICondition : IComparable<ICondition>
    {
        /// <summary>
        /// Indique si la condition exprimée par l'instance est vrai pour ce répondant.
        /// </summary>
        /// <param name="respondent"></param>
        /// <returns></returns>
        bool Evaluate(IRespondent respondent);
    }
}
