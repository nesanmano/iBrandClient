using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Datasource
{
    public abstract class AbstractDatasource
    {
        public static string _bdType = "database";
        public static string _inMemoryType = "in_memory";

        protected DatasourceInfo _datasourceInfo;

        protected Project _project;

        protected AbstractDatasource(DatasourceInfo datasourceInfo, Project project)
        {
            _datasourceInfo = datasourceInfo;
            _project = project;
        }

        public abstract bool HasValue(string fieldName, string value);
        public abstract string GetFieldValue(string indexedFieldValue, string indexedFieldName, string fieldName);
        public abstract List<string> GetFieldValues(string indexedFieldValue, string indexedFieldName, string fieldName);
        public abstract List<string> GetValues(string fieldName);

        public abstract void Close();
        
        public DatasourceInfo DatasourceInfo => _datasourceInfo;

        public abstract void Load();
        
        public override bool Equals(object o)
        {
            var datasource = o as AbstractDatasource;
            return datasource != null && datasource.DatasourceInfo.Equals(DatasourceInfo);
        }

        public override int GetHashCode()
        {
            return DatasourceInfo.GetHashCode();
        }

        public class Field
        {
            public Field(int no, string name)
            {
                No = no;
                Name = name;
            }

            public Field(string name)
            {
                Name = name;
                No = -1;
            }

            public int No { get; }

            public string Name { get; }

            public override bool Equals(object o)
            {
                var field = o as Field;
                if (field == null) return false;
                var f = field;
                return f.Name.Equals(Name);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }

        public class Entry
        {
            readonly string[] _values;

            public Entry(int nbrValue)
            {
                _values = new string[nbrValue];
            }

            public string GetValue(int index)
            {
                return _values[index];
            }

            public void AddValue(int index, string value)
            {
                _values[index] = value;
            }
        }
    }
}
