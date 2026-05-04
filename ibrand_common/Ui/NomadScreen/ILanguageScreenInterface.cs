using System;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Ui.NomadScreen
{
    /// <summary>
    /// Interface pour les écrans qui peuvent provoquer un changement de langue.
    /// </summary>
    [PublicAPI]
    public interface ILanguageScreenInterface: IObtainCurrentFullRespondent
    {
        /// <summary>
        /// Événement à écouter pour savoir si l'écran provoque un changement de langue. 
        /// </summary>
        event LanguageSelectionEventHandler OnLanguageSelection;
        
        /// <summary>
        /// Méthode utilisée pour provoquer le changement de langue.
        /// </summary>
        /// <param name="lg">Le code de langue qui doit être utilisé.</param>
        void LanguageSelection(string lg);

        /// <summary>
        /// Indique quelle est la question de langue dans le questionnaire. Null par défaut, ce qui implique aucune
        /// sélection de langue.
        /// </summary>
        [Obsolete]
        int? QuestionLanguageNo { get; set; }
        
        /// <summary>
        /// Le sous-choix qui est associé au français avec la question de langue. Par défaut, 2
        /// </summary>
        [Obsolete]
        int QuestionLanguageChoiceNoFr { get; set; }
        
        /// <summary>
        /// Le sous-choix qui est associé à l'anglais avec la question de langue. Par défaut, 1
        /// </summary>
        [Obsolete]
        int QuestionLanguageChoiceNoEn { get; set; }
        
        /// <summary>
        /// Indique que le type de question qui gère la langue est une question radio, au lieu d'une question combo. 
        /// Faux par défaut (on assume combo par défaut).
        /// </summary>
        [Obsolete]
        bool IsQuestionLanguageRadio { get; set; }
    }
}