using System.Windows.Forms;
using Newtonsoft.Json;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.DatabaseExporter
{
    public class DatabaseExtractorConfig : IJsonConfig
    {
        public string DefaultConfigFileName { get; } = "database_exporter_config.json";
        public bool DestroyAll { get; set; }
        public bool DestroyData { get; set; }
        public bool DestroyFiles { get; set; }
        public bool DestroyLogs { get; set; }
        public bool DestroyUnsynched { get; set; }
        public bool IncludeMetadata { get; set; }
        public bool GenerateHeaders { get; set; }
        public int ReportType { get; set; } = -1;
        public string LastSelectedExportDir { get; set; }
    }
}