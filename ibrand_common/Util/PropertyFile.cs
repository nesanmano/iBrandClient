using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Cette classe représente une façon aisée de lire et d'accéder à un fichier de propriétés.
    /// </summary>
    [Obsolete] // TODO Utiliser des configuration JSON à la place.
    public class PropertyFile
    {
        private readonly FileInfo _file;
        private readonly Dictionary<string, string> _dico;

        public PropertyFile(string path) : this(new FileInfo(path)) { }

        public PropertyFile(FileInfo file)
        {
            if (file == null || !file.Exists)
            {
                throw new FileNotFoundException("File does not exists: " + file);
            }

            _file = file;
            _dico = new Dictionary<string, string>();

            ReadProperties();
        }

        public void ReadProperties()
        {
            _dico.Clear();
            foreach (var line in File.ReadAllLines(_file.FullName))
            {
                var tab = line.Split('=');
                var rest = string.Concat(tab.Skip(1).ToArray());
                _dico.Add(tab[0], rest);
            }
        }

        public string GetProperty(string key)
        {
            if (_dico.ContainsKey(key))
            {
                return _dico[key];
            }

            return null;
        }

        /// <summary>
        /// Retourner une valeur double de la propriétée voulue.
        /// </summary>
        /// <param name="key">Le code de la clé.</param>
        /// <returns>LA valeur double. NaN si la clé n'existe pas.</returns>
        public double GetPropertyDouble(string key)
        {
            if (_dico.ContainsKey(key))
            {
                return double.Parse(_dico[key]);
            }

            return double.NaN;
        }
    }
}
