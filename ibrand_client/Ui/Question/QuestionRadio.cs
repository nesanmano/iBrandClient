using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.Choice;

//using OpenNETCF.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    public class QuestionRadio : Question
    {
        public override Type GetDefaultChoiceClassType()
        {
            return typeof(ChoiceRadio);
        }

        public override QuestionTypeEnum QuestionType => QuestionTypeEnum.Radio;
        
        public override bool IsAnswered()
        {
            return Choices.Any(c => c is ChoiceRadio cb && cb.IsSelected());
        }

        public override void GetAnswers(List<Answer> answers)
        {
            answers.AddRange(Choices.Where(c => c is ChoiceRadio cr && cr.IsSelected()).Select(
                c => new Answer{QuestionType = (QuestionTypeEnum)QuestionTypeInt, QuestionNo = No, ChoiceNo = c.No}));
        }


        public void OnRadioButtonClicked(object sender, EventArgs e)
        {
            // On désélectionne tous les autre choix. C'est la particularité d'un choix radio: ils sont tous mutuellement exclusifs.
            foreach(var c in Choices.Where(c => c is ChoiceRadio cr && cr.GetRadioButton() != sender))
            {
                c.ResetAnswer();
            }
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
                sb.Append(c.GetText());
            }

            return sb.ToString();
        }
    }
}
