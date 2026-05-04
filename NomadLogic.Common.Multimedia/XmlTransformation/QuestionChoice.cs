using System;

namespace NomadLogic.Multimedia.XmlTransformation
{
    public class QuestionChoice
    {
        private readonly int _questionNo;
        private readonly int _choiceNo;

        [Obsolete("Should be using a base class systeme for this feauture.")]
        public QuestionChoice(int questionNo, int choiceNo)
        {
            _questionNo = questionNo;
            _choiceNo = choiceNo;
        }

        public int GetChoiceNo()
        {
            return _choiceNo;
        }

        public int GetQuestionNo()
        {
            return _questionNo;
        }
    }
}
