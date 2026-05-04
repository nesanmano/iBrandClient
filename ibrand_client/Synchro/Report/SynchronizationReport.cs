using System;
using NomadLogic.Common.Services;

namespace NomadLogic.iBrandDesktop.Synchro.Report
{
    public class SynchronizationReport : IProgressDeepCopy<SynchronizationReport>
    {
        public DateTime ReportTime { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorNumber { get; set; }
        public string Message { get; set; }
        public int RemainToSync { get; set; }
        public int TotalSync { get; set; }
        public ulong TotalData { get; set; }
        public ProgressBarData CurrentUpload { get; set; }

        public bool IsEmpty()
        {
            return ErrorMessage == null && Message == null && RemainToSync == 0 && CurrentUpload == null;
        }

        public SynchronizationReport Copy()
        {
            return new SynchronizationReport
            {
                Message = Message,
                CurrentUpload = CurrentUpload,
                ErrorMessage = ErrorMessage,
                ErrorNumber = ErrorNumber,
                RemainToSync = RemainToSync,
                ReportTime = ReportTime,
                TotalData = TotalData,
                TotalSync = TotalSync
            };
        }
    }

    public class ProgressBarData
    {
        public int Min { get; set; }
        public int Max { get; set; } = 100;
        public int Val { get; set; }
        public int Step { get; set; } = 10;
    }
}