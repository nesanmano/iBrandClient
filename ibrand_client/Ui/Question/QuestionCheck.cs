using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    public class QuestionCheck : Question
    {
        public override Type GetDefaultChoiceClassType()
        {
            return typeof(ChoiceCheck);
        }

        public override QuestionTypeEnum QuestionType => QuestionTypeEnum.Check;
        
        public override bool IsAnswered()
        {
            // Une question check est toujours "répondue": elle est soit vari soit fausse.
            return true;
        }

        public override void GetAnswers(List<Answer> answers)
        {
            answers.AddRange(Choices.Where(c => c is ChoiceCheck cc && cc.IsChecked()).Select(
                c => new Answer{QuestionType = (QuestionTypeEnum)QuestionTypeInt, QuestionNo = No, ChoiceNo = c.No}));
        }

        public override bool HasChoiceText()
        {
            return false;
        }

        public override string ReportFormat(List<Answer> answers)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var cIndex in answers.Select(a => a.ChoiceNo))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }

                var c = GetChoice(cIndex);
                var text = c.GetText();
                sb.Append(text ?? cIndex.ToString());
            }

            return sb.ToString();
        }
    }
}
