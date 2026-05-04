using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Report
{
    public class ReportItemRespondent<T> : IReportItem
    {
        public ReportItemRespondent([NotNull] string propertyName, [NotNull] Func<IRespondent> respondentMethod)
        {
            _propName = propertyName;
            _getRespMtehod = respondentMethod;

            _propInfo = typeof(IRespondent).GetProperties().FirstOrDefault(p => p.Name.Equals(_propName));
            if (_propInfo == null) throw new System.Exception("Unknown property for type IRespondent: " + _propName);
        }

        public Func<string, T> ConversionMethod { get; set; }

        private readonly string _propName;
        private readonly Func<IRespondent> _getRespMtehod;
        private readonly PropertyInfo _propInfo;

        public string HeaderDefinition()
        {
            return _propName;
        }

        public object GenerateData()
        {
            var data = _propInfo.GetValue(_getRespMtehod.Invoke());
            return ConversionMethod == null ? data : ConversionMethod.Invoke(data?.ToString());
        }
    }
}