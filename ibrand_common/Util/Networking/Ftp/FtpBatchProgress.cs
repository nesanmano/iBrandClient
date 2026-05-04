using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    [PublicAPI]
    public class FtpBatchProgress
    {
        public FtpBatchProgress() { }

        public FtpBatchProgress(Dictionary<string, FtpTransferProgress> data)
        {
            Transfers = data?.Values.ToList() ?? new List<FtpTransferProgress>();
        }

        public List<FtpTransferProgress> Transfers { get; set; } = new List<FtpTransferProgress>();
        public TimeSpan ElapsedTime { get; set; }
        public double Completion { get; set; }

        public override string ToString()
        {
            return $"Completion : {Completion*100:F2}%, Elapsed: {ElapsedTime.ToStringHourMinSec(true, true)}, File List details :\n\t{StringUtil.FormatEnumerable(Transfers, true, 1)}";
        }
    }

    public class FtpTransferProgress
    {
        public bool IsDownload { get; set; }
        public double PercentComplete { get; set; }
        public string Path { get; set; }
        public int Speed { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return $"Name:'{FileName}', Direction:{(IsDownload ? "down" : "up")}, Completion:{PercentComplete:F2}%, Speed:{Speed} Bps";
        }
    }
}