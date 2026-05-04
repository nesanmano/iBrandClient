using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.Question;

namespace NomadLogic.iBrandDesktop.Report
{
    public class ReportItemQuestion : IReportItem
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly int _qId;
        private readonly Question _q;
        private readonly Func<IList<Answer>> _method;

        public ReportItemQuestion(int qId, [NotNull] Question question, [NotNull] Func<IList<Answer>> answerMethod)
        {
            _qId = qId;
            _q = question;
            _method = answerMethod;
        }

        public string HeaderDefinition()
        {
            return _q.GetText();
        }

        public object GenerateData()
        {
            var answers = _method?.Invoke()?.Where(a => a.QuestionNo == _qId).ToList() ?? new List<Answer>();
            return _q.ReportFormat(answers);
        }
    }
}