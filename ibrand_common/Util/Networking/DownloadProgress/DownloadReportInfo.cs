namespace NomadLogic.Common.Util.Networking.DownloadProgress
{
    public class DownloadReportInfo
    {
        /// <summary>
        /// Comme le nombre de Bytes peut être très élevé (plus qu'un int peut contenir, cette valeur représente
        /// le pourcentage de complétion des octets à télécharger.
        /// </summary>
        public int BytesOverallProgress { get; set; } = 0;

        /// <summary>
        /// Nombre de fichiers à télécharger.
        /// </summary>
        public int TotalNumFilestoDownload { get; set; } = 0;

        public string Message { get; set; }

        /// <summary>
        /// Nombre de fichiers complètement téléchargés.
        /// </summary>
        public int FilesOverallProgress { get; set; } = 0;

        public int Error { get; set; } = 0;  // default 0 means execution of task are OK and teh system is responsive.
    }
}
