using System;
using System.Collections.Generic;
using NomadLogic.Common.Util.Excel;

namespace NomadLogic.Common.Services.ActivityLogger
{
    public class ActivityReportItem : IExcelItem, IExcelHeaderGroup
    {
        private static readonly IReadOnlyList<string> HEADERS = new List<string>
        {
            "Id","DateTimeCreated","ActionType","TerminalId","Details"
        }.AsReadOnly();

        public IList<string> ExportDataLine()
        {
            return new List<string>
            {
                Id.ToString(),
                DateTimeCreated.ToString("yyyy-MM-dd HH:mm:ss"),
                ActionType.ToString(),
                TerminalId.ToString(),
                Details
            };
        }

        public IReadOnlyList<string> ExportHeadersLine() => HEADERS;

        public ActivityReportItem(int id, DateTime dateTime, ActivityLoggerActionType actionTypeRead, int termIdRead, string details)
        {
            Id = id;
            DateTimeCreated = dateTime;
            ActionType = actionTypeRead;
            TerminalId = termIdRead;
            Details = details;
        }

        public string Details { get; set; }

        public int TerminalId { get; set; }

        public ActivityLoggerActionType ActionType { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int Id { get; set; }
        
        
    }
}