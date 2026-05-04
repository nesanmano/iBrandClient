using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    /// <summary>
    /// Représente une assignation directe via nos jetons de transformation d'un questionnaire.
    /// </summary>
    [PublicAPI]
    public class ParameterDefinitionValue : ParameterDefinition
    {
        /// <summary>
        /// Dans le cas où il s'agit d'une valeur simple que l'on peut chercher dans le questionnaire ou le répondant, on utilise un
        /// Jeton de valeur. Voir StringUtil.
        /// </summary>
        public string TokenValue { get; set; }

        public override object CreateParameterValue(IRespondent resp, Dictionary<QuestionIdentifier, Answer> dicAnswers)
        {
            var strResult = StringUtil.CreateStringFromTokenizedPatternRespondent(TokenValue, dicAnswers, null, resp);
            if (!IsEnum) return strResult;
            var enumType = ClassUtil.GetEnumType(NameType);
            var enumVal = Enum.Parse(enumType, strResult, true);
            return enumVal;

        }
    }
}