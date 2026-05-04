namespace NomadLogic.iBrandDesktop.Datasource
{
    public class DatasourceAnswer
    {
        private readonly AbstractDatasource _datasource;
        private readonly Questionnaire _questionnaire;
        private readonly string _fieldName;
        private readonly int _choiceNo;

        private int _dependsOnQuestionNo;
        private int _dependsOnChoiceNo;
        private string _dependsOnFieldName;

        public DatasourceAnswer(Questionnaire questionnaire, AbstractDatasource datasource, string fieldName, int choiceNo)
        {
            _questionnaire = questionnaire;
            _datasource = datasource;
            _fieldName = fieldName;
            _choiceNo = choiceNo;
        }

        public int ChoiceNo => _choiceNo;

        public void SetDependsOn(int questionNo, int choiceNo, string dependsOnFieldName)
        {
            _dependsOnQuestionNo = questionNo;
            _dependsOnChoiceNo = choiceNo;
            _dependsOnFieldName = dependsOnFieldName;
        }

        public string GetAnswer()
        {
            var answer = "";

            if (_datasource != null && _questionnaire != null)
            {
                var question = _questionnaire.GetQuestion(_dependsOnQuestionNo);

                var choice = question.GetChoice(_dependsOnChoiceNo);
                var endexedFieldAnswer = choice.GetAnswerText();

                answer = _datasource.GetFieldValue(endexedFieldAnswer, _dependsOnFieldName, _fieldName);
            }

            return answer;
        }
    }
}
