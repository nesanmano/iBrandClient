using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NomadLogic.iBrandDesktop.Exception;

namespace NomadLogic.iBrandDesktop.Datasource
{
    public class InMemoryDatasource : AbstractDatasource
    {
        private string _entryCharacters;
        private bool _inEntry;

        private List<Field> _fields;

        private Field _indexedField;

        private Hashtable _indexedFieldToEntry;

        public InMemoryDatasource(DatasourceInfo datasourceInfo, Project project)
            : base(datasourceInfo, project)
        {
        }

        public override void Load()
        {
            ReadFile();
        }

        public override void Close()
        {
            //Do Nothing
        }

        protected void ReadFile()
        {
            StreamReader stream = null;

            try
            {
                var fileName = _datasourceInfo.FileName;

                if (!File.Exists(fileName))
                {
                    throw new NoFileException("Can't find datasource file :" + _datasourceInfo.FileName);
                }

                stream = new StreamReader(fileName, Encoding.GetEncoding(1252));

                var settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment,
                    IgnoreWhitespace = true,
                    IgnoreComments = true
                };


                var xmlReader = XmlReader.Create(stream, settings);

                ParseXml(xmlReader);
            }
            finally
            {
                stream?.Close();
            }
        }

        private void ParseXml(XmlReader xmlReader)
        {
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        StartElement(xmlReader);
                        break;
                    case XmlNodeType.EndElement:
                        EndElement(xmlReader);
                        break;
                    case XmlNodeType.Text:
                        if (_inEntry)
                            _entryCharacters = xmlReader.Value;
                        break;
                }
            }
        }

        public override bool HasValue(string fieldName, string value)
        {
            return _indexedFieldToEntry.ContainsKey(value);
        }

        public override List<string> GetFieldValues(string indexedFieldValue, string indexedFieldName, string fieldName)
        {
            var values = new List<string>();

            var entries = Entries;
            var indexedFieldIndex = GetFieldIndex(indexedFieldName);
            var fieldIndex = GetFieldIndex(fieldName);
            foreach (var entry in entries)
            {
                if (entry.GetValue(indexedFieldIndex).Equals(indexedFieldValue))
                {
                    values.Add(entry.GetValue(fieldIndex));
                }
            }

            return values;
        }

        public override List<string> GetValues(string fieldName)
        {
            var values = new List<string>();

            var entries = Entries;
            var fieldIndex = GetFieldIndex(fieldName);
            foreach (var entry in entries)
            {
                if (!values.Contains(entry.GetValue(fieldIndex)))
                    values.Add(entry.GetValue(fieldIndex));
            }

            return values;
        }

        public override string GetFieldValue(string indexedFieldValue, string indexedFieldName, string fieldName)
        {
            string value = null;

            if (_indexedField == null) return null;
            var entry = (Entry)_indexedFieldToEntry[indexedFieldValue];

            if (entry == null) return null;
            var index = GetFieldIndex(fieldName);
            if (index != -1)
                value = entry.GetValue(index);

            return value;
        }

        private List<Entry> Entries { get; set; }

        private int GetFieldIndex(string fieldName)
        {
            return _fields.IndexOf(new Field(fieldName));
        }

        protected void StartElement(XmlReader xmlReader)
        {
            if (xmlReader.Name.Equals("field"))
            {
                if (_fields == null)
                    _fields = new List<Field>();

                var name = xmlReader.GetAttribute("name");

                var field = new Field(_fields.Count, name);
                _fields.Add(field);

                if (xmlReader.GetAttribute("index") != null)
                {
                    _indexedField = field;
                }
            }
            else if (xmlReader.Name.Equals("entry"))
            {
                _inEntry = true;
            }
        }

        protected void EndElement(XmlReader xmlReader)
        {
            if (!xmlReader.Name.Equals("entry")) return;
            var entry = new Entry(_fields.Count);

            var caracs = _entryCharacters.ToCharArray();

            var nbrCaracs = caracs.Length;
            var index = 0;
            var nbrEntry = 0;

            for (var i = 0; i < nbrCaracs; ++i)
            {
                var c = caracs[i];

                if (c != ',') continue;
                entry.AddValue(nbrEntry, new string(caracs, index, (i - index)));
                index = i + 1;
                ++nbrEntry;
            }

            entry.AddValue(nbrEntry, new string(caracs, index, (nbrCaracs - index)));

            if (_indexedField != null)
            {
                if (_indexedFieldToEntry == null)
                    _indexedFieldToEntry = new Hashtable();

                if (!_indexedFieldToEntry.Contains(entry.GetValue(_indexedField.No)))
                    _indexedFieldToEntry.Add(entry.GetValue(_indexedField.No), entry);
            }


            if (Entries == null)
                Entries = new List<Entry>();

            Entries.Add(entry);
            _inEntry = false;
        }

        
    }
}
