using System.Collections.Generic;
using System.Linq;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Report
{
    public class ReportTupleGeneratorIbrandTerminal : ReportTupleGeneratorAbstract
    {
        private readonly List<IReportItem> _reportItems = new List<IReportItem>();
        private SortedList<int, IList<Answer>> _fullAnswers;
        private IList<Respondent> _respondents;
        private int _currentRespondentIndex;
        private IList<object> _currentTuple;
        private readonly bool _useNotSynced;
        private readonly TerminalInfo _terminal;

        public ReportTupleGeneratorIbrandTerminal(IbrandDatabase db, TerminalInfo terminal, Questionnaire questionnaire, bool metadataGeneration = false, bool useNotSynched = false)
        {
            _db = db;
            _questionnaire = questionnaire;
            _terminal = terminal;
            _metadataGeneration = metadataGeneration;
            _useNotSynced = useNotSynched;
            
            _reportItems.Add(new ReportItemRespondent<string>("Id", GetCurrentRespondent));
            _reportItems.Add(new ReportItemRespondent<string>("Date", GetCurrentRespondent)
            {
                ConversionMethod = s =>
                {
                    var dt = DateUtil.FromJavaTimeToDateTime(s);
                    return dt.ToString(DateUtil.UtcTimezoneFormatString);
                }
            });
            _reportItems.Add(new ReportItemRespondent<string>("Language", GetCurrentRespondent));
            if (_metadataGeneration)
            {
                _reportItems.Add(new ReportItemRespondent<string>("TimeZone", GetCurrentRespondent));
                _reportItems.Add(new ReportItemRespondent<bool>("IsSynchronized", GetCurrentRespondent));
            }

            // On itère dans les questions du questionnaire maintenant.
            foreach (var qId in questionnaire.GetQuestionIdList())
            {
                _reportItems.Add(new ReportItemQuestion(qId, questionnaire.GetQuestion(qId), GetAnswersFromCurrentRespondent));
            }
        }

        public override int ColumnNumber => _reportItems.Count;
        
        public override IList<string> GenerateHeaderData()
        {
            return _reportItems.Select(ri => ri.HeaderDefinition()).ToList();
        }

        public override IList<object> CurrentTuple()
        {
            return _currentTuple;
        }

        public override bool CreateNextTuple()
        {
            if (_respondents == null)
            {
                _currentRespondentIndex = 0;
                _fullAnswers = _db.GetAnswersByTerminal(_terminal, _useNotSynced);
                _respondents = _db.GetRespondentsByTerminal(_terminal, _useNotSynced);
            }

            if (_respondents == null || _respondents.Count == _currentRespondentIndex)
            {
                _currentTuple = null;
                return false;
            }

            _currentTuple = new List<object>();
            foreach (var ri in _reportItems)
            {
                _currentTuple.Add(ri.GenerateData());
            }

            _currentRespondentIndex++;

            return true;
        }

        private IRespondent GetCurrentRespondent()
        {
            return _respondents[_currentRespondentIndex];
        }

        private IList<Answer> GetAnswersFromCurrentRespondent()
        {
            _fullAnswers.TryGetValue(GetCurrentRespondent().Id, out var answers);
            return answers;
        }
    }
}
