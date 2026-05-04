using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    public class QuestionText : QuestionAnswersValidationBase
    {
        public override Type GetDefaultChoiceClassType()
        {
            return typeof(ChoiceText);
        }

        public override QuestionTypeEnum QuestionType => QuestionTypeEnum.Text;
        
        public override string ReportFormat(List<Answer> answers)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var str in answers.Select(a => a.Response))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }

                sb.Append(str);
            }

            return sb.ToString();
        }

        public override void GetAnswers(List<Answer> answers)
        {
            answers.AddRange(Choices.Where(c => c.IsAnswered()).Select(
                c => new Answer{QuestionType = (QuestionTypeEnum)QuestionTypeInt, QuestionNo = No, ChoiceNo = c.No, Response = c.GetAnswerText()}));
        }
    }
}
