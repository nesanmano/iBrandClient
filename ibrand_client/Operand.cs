using System;
using System.Collections.Generic;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.Question;

namespace NomadLogic.iBrandDesktop
{
    public class Operand
    {
        private readonly bool _notChoice;

        public Operand(string questionNo, string choiceNo)
        {
            QuestionNo = questionNo;
            QuestionNoInt = int.Parse(questionNo);

            if (choiceNo.IndexOf("!", StringComparison.Ordinal) != -1)
            {
                ChoiceNo = int.Parse(choiceNo.Substring(1, choiceNo.Length));
                _notChoice = true;
            }
        }

        public string QuestionNo { get; }

        public int QuestionNoInt { get; }

        public int ChoiceNo { get; }
        
        public bool Evaluate(Question question)
        {
            if (question.No != QuestionNoInt) return false;

            var answers = new List<Answer>();
            question.GetAnswers(answers);

            var nbrAnswer = answers.Count;

            for (var i = 0; i < nbrAnswer; ++i)
            {
                var answer = answers[i];

                switch (question)
                {
                    case QuestionCombo _ when answer.ChoiceNo != ChoiceNo:
                        continue;
                    case QuestionRadio _:
                    case QuestionCheck _:
                    {
                        if (answer.ChoiceNo == ChoiceNo)
                        {
                            return !_notChoice;
                        }

                        return _notChoice;
                    }
                }
            }

            return false;
        }
    }
}
