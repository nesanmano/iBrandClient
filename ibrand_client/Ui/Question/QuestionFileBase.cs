using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    /// <summary>
    /// Cette classe de question encapsule les méthode communes pour une question qui représente un fichier sur disque.
    /// Normalement, ce type de question contient du texte qui se traduit par un chemin absolu sur disque.
    /// </summary>
    public abstract class QuestionFileBase : Question
    {
        public override Type GetDefaultChoiceClassType()
        {
            return typeof(ChoiceText);
        }

        public override void GetAnswers(List<Answer> answers)
        {
            answers.AddRange(Choices.Where(c => c.IsAnswered()).Select(
                c => new Answer{QuestionType = (QuestionTypeEnum)QuestionTypeInt, QuestionNo = No, ChoiceNo = c.No, Response = c.GetAnswerText()}));
        }
        
        public override string ReportFormat(List<Answer> answers)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var str in answers.Select(a => StringUtil.GetFileName(a.Response)))
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
    }
}