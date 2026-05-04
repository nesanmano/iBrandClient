using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery
{
    /// <summary>
    /// Cette classe sauvegarde ou créer une réponse de sélection de gallerie comme simple indice.
    /// Par exemple, on dit que le 4ième élément sélectionné donne la réponse texte "4" (si aucun offset).
    /// </summary>
    public class SaveGallerySelectionAsIndex : ISaveToQuestionnaireFormatter<ComparableFileInfo>
    {
        /// <summary>
        /// Le numéro de question où placer la réponse.
        /// </summary>
        [JsonRequired]
        public int QuestionNo { get; set; }

        /// <summary>
        /// Au besoin, le numéro de choix.
        /// </summary>
        public int ChoiceNo { get; set; } = 1;

        /// <summary>
        /// Au besoin, le type énumératif de la question.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public QuestionTypeEnum QuestionType { get; set; } = QuestionTypeEnum.Text;

        /// <summary>
        /// Si on doit ajouter un "offset" à l'indice choisi. L'indice choisit est toujours basé-zéro, donc si on doit utiliser des valeurs
        /// qui commencent par 1, on doit mettre cette valeur à 1.
        /// Par défaut, on prend l'indice naturel (basé zéro).
        /// </summary>
        public int IndexOffset { get; set; } = 0;

        public bool Save(ISaveLocalRespondentData saveTo, int index, ComparableFileInfo elem)
        {
            return QuestionType == QuestionTypeEnum.Combo
                ? saveTo.SaveAnswerToQuestion(new Answer {QuestionNo = QuestionNo, ChoiceNo = ChoiceNo, Response = elem.FileInfoRef.FullName, QuestionType = QuestionType})
                : saveTo.SaveAnswerToQuestion(new Answer {QuestionNo = QuestionNo, ChoiceNo = ChoiceNo, Response = (index + IndexOffset).ToString()});
        }

        public IList<Answer> CreateAnswers(int index, ComparableFileInfo elem)
        {
            var res = new Answer{QuestionType = QuestionType, QuestionNo = QuestionNo, ChoiceNo = ChoiceNo};

            switch (QuestionType)
            {
                case QuestionTypeEnum.Combo:
                    res.ChoiceNo = index + 1 + IndexOffset;
                    break;
                case QuestionTypeEnum.Text:
                    res.Response = (index + IndexOffset).ToString();
                    break;
                default:
                    throw new System.Exception("Question type is not supported for index conversion.");
            }

            return new List<Answer> {res};
        }
    }
}