using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Classe pour lire, stocker et accéder des clés/valeurs à la façon Java.
    /// Voir ici pour le code original:
    /// https://stackoverflow.com/questions/485659/can-net-load-and-parse-a-properties-file-equivalent-to-java-properties-class
    /// Autheur: Nick Rimmer
    /// </summary>
    [PublicAPI]
    public class PropertiesHolder
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Dictionary<string, string> _list;
        private string _filename;

        public PropertiesHolder(string filepath)
        {
            Reload(filepath);
        }

        public PropertiesHolder(FileInfo file)
        {
            Reload(file);
        }
        
        public string GetValue(string field, string defValue)
        {
            var tmp = GetValue(field);
            return tmp ?? defValue;
        }

        public string GetValue(string field)
        {
            return _list.ContainsKey(field) ? _list[field] : null;
        }

        public void SetValue(string field, object value)
        {
            if (!_list.ContainsKey(field))
                _list.Add(field, value.ToString());
            else
                _list[field] = value.ToString();
        }

        public void Save()
        {
            Save(_filename);
        }

        public void Save(string filename)
        {
            _filename = filename;

            if (!File.Exists(filename))
                File.Create(filename);

            using (var file = new StreamWriter(filename))
            {
                foreach (var prop in _list.Keys.ToArray())
                    if (!string.IsNullOrWhiteSpace(_list[prop]))
                        file.WriteLine(prop + "=" + _list[prop]);
            }
        }

        public void Reload()
        {
            Reload(_filename);
        }

        public void Reload(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                log.Warn("Cannot open property file because file name is invalid.");
                return;
            }

            _filename = filename;
            _list = new Dictionary<string, string>();

            if (File.Exists(filename))
                LoadFromFile(filename);
            else
                File.Create(filename);
        }

        private void Reload(FileInfo filename)
        {
            Reload(filename?.FullName);
        }

        private void LoadFromFile(string file)
        {
            foreach (var line in File.ReadAllLines(file))
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("'") || !line.Contains('=')) continue;
                var index = line.IndexOf('=');
                var key = line.Substring(0, index).Trim();
                var value = line.Substring(index + 1).Trim();

                if (value.StartsWith("\"") && value.EndsWith("\"") ||
                    value.StartsWith("'") && value.EndsWith("'"))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                try
                {
                    //ignore duplicates
                    _list.Add(key, value);
                }
                catch
                {
                    // Die nice
                }
            }
        }

        /// <summary>
        /// S'assure que le dictionnaire de propriétés contiennent des valeurs non-vides pour chaque clés
        /// passée en paramètre.
        /// </summary>
        /// <param name="props"></param>
        /// <returns>TRUE si et seulement si chaque élément possède une valeur non vide.</returns>
        public bool EnsureExistingValues(params string[] props)
        {
            return props.All(p => !string.IsNullOrWhiteSpace(GetValue(p)));
        }
    }
}