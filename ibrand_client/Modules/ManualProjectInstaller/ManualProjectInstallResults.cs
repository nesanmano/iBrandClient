using System.Collections.Generic;
using System.IO;

namespace NomadLogic.iBrandDesktop.Modules.ManualProjectInstaller
{
    public class ManualProjectInstallResults
    {
        public IList<ManualProjectInstallResult> Results { get; set; } = new List<ManualProjectInstallResult>();

        public int TotalJobs { get; set; }
        public int SuccessCount { get; set; }
        public int FailuresCount { get; set; }
    }

    public class ManualProjectInstallResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public System.Exception ErrorCause { get; set; }
        public IList<FileInfo> OriginalFiles { get; set; }
        public int FilesCount { get; set; }
        public long FilesTotalSize { get; set; }
        public FileInfo TerminalInstallDirectory { get; set; }
    }
}