using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Datasource
{
    public class DatasourceInfo
    {
        // ReSharper disable once NotAccessedField.Local
        private string _id;

        public DatasourceInfo(string id, string name, string src, string datasourceType)
        {
            _id = id;
            Name = name;
            FileSrc = src;
            FileName = StringUtil.GetFileName(src);
            DatasourceType = datasourceType;
        }

        public DatasourceInfo(string name, string fileName, string datasourceType)
        {
            _id = null;
            Name = name;
            FileSrc = null;
            FileName = fileName;
            DatasourceType = datasourceType;
        }

        public DatasourceInfo(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string FileName { get; }

        public string DatasourceType { get; }

        public string FileSrc { get; }
    }
}
