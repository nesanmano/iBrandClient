using System;
using System.Text;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload
{
    /// <summary>
    /// Classe de rapport pour les événement "IProgress".
    /// </summary>
    public class RespondentBundleDownloadReport : IProgressDeepCopy<RespondentBundleDownloadReport>
    {
        public string Message { get; set; }
        public string MessageError { get; set; }
        public int TotalFilesToDownLoad { get; set; }
        public int TotalFilesDownloaded { get; set; }
        public long TotalBytesDownloaded { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsFinished { get; set; }
        
        public RespondentBundleDownloadReport Copy()
        {
            return new RespondentBundleDownloadReport
            {
                IsFinished = IsFinished,
                Message = Message,
                StartTime = StartTime,
                MessageError = MessageError,
                TotalBytesDownloaded = TotalBytesDownloaded,
                TotalFilesDownloaded = TotalFilesDownloaded,
                TotalFilesToDownLoad = TotalFilesToDownLoad
            };
        }

        public string ToStringQuickPreview()
        {
            var sb = new StringBuilder();

            if (Message != null)
            {
                sb.AppendLine($"Report event: {Message}");
            }

            if (MessageError != null)
            {
                sb.AppendLine($"Report Error!: {Message}");
            }

            if (IsFinished)
            {
                var time = DateTime.Now - StartTime;
                sb.AppendLine("Download for event finished. Time elapsed.: " + time.ToMinimumReadableTime());
            }

            // Le reste des infos:
            if (TotalFilesToDownLoad != 0)
            {
                var data = $"Download report: {TotalFilesToDownLoad:000}/{TotalFilesDownloaded:000}, Downloaded: {MathUtil.ByteNumberReadable(TotalBytesDownloaded)}";
                sb.AppendLine(data);
            }

            return sb.ToString();
        }
    }
}