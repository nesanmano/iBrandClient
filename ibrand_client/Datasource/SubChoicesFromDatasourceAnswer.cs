using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Datasource
{
    public class SubChoicesFromDatasourceAnswer
    {
        private readonly AbstractDatasource _datasource;
        private readonly Questionnaire _questionnaire;
        private readonly string _fieldName;
        private readonly ChoiceCombo _choice;

        private int _dependsOnQuestionNo;
        private int _dependsOnChoiceNo;
        private string _dependsOnFieldName;

        public SubChoicesFromDatasourceAnswer(Questionnaire questionnaire, AbstractDatasource datasource, string fieldName, ChoiceCombo choice)
        {
            _questionnaire = questionnaire;
            _datasource = datasource;
            _fieldName = fieldName;
            _choice = choice;
        }
        
        public void SetDependsOn(int questionNo, int choiceNo, string dependsOnFieldName)
        {
            _dependsOnQuestionNo = questionNo;
            _dependsOnChoiceNo = choiceNo;
            _dependsOnFieldName = dependsOnFieldName;
        }

        public void AddSubChoices()
        {

            if (_datasource != null && _questionnaire != null)
            {
                var question = _questionnaire.GetQuestion(_dependsOnQuestionNo);

                var choice = question.GetChoice(_dependsOnChoiceNo);
                var endexedFieldAnswer = choice.GetAnswerText();

                var values = _datasource.GetFieldValues(endexedFieldAnswer, _dependsOnFieldName, _fieldName);
                _choice.AddSubChoices(_questionnaire.LanguageCurrent, values);
            }

        }
    }
}
