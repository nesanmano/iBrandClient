using System.Linq;
using NomadLogic.iBrandDesktop.Datasource;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    /// <summary>
    /// Cette classe abstraite représente une question qui contient du texte qui peut être valider en utilisant un REGEX ou d'autres
    /// propriétés spécifique de validation texte.
    /// </summary>
    public abstract class QuestionAnswersValidationBase : Question
    {
        protected int _answersPattern = -1;
        protected string _pattern;

        protected AbstractDatasource _datasource; //datasource validation
        protected string _fieldName;

        public void SetDatasourceValidation(AbstractDatasource datasource, string fieldName)
        {
            _datasource = datasource;
            _fieldName = fieldName;
        }

        public override bool ValidateDatasourceField()
        {
            if (_datasource == null || _fieldName == null) return true;

            return Choices.All(c =>
            {
                var text = c.GetAnswerText();
                return (text.Length == 0 && !Mandatory) || _datasource.HasValue(_fieldName, text);
            });
        }

        public string Pattern => _pattern;

        public void SetAnswersPattern(int pattern)
        {
                _answersPattern = pattern;
                _pattern = PatternMatcher.GetPattern(_answersPattern);
        }

        public override bool ValidateAnswersPattern()
        {
            if (_answersPattern == -1) return true;

            return Choices.All(c =>
            {
                var text = c.GetAnswerText();
                return (text.Length == 0 && !Mandatory) || PatternMatcher.Match(_answersPattern, text);
            });
        }

        public override string AnswersPattern => _pattern;
    }
}
