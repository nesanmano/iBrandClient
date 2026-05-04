using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Classe pour envelopper une expression régulière compilée, et quelques méthode utilitaire pour analyser des châine contre ce regex.
    /// </summary>
    [PublicAPI]
    public class RegexWrapper
    {
        private string _regexString;
        private Regex _regexValidation;
        
        public string RegexDefinition
        {
            get => _regexString;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _regexString = null;
                    _regexValidation = null;
                }
                else
                {
                    if (value.Trim().Equals(_regexString)) return;

                    _regexString = value.Trim();
                    _regexValidation = new Regex(_regexString, RegexOptions.Compiled);
                }
            }
        }
        
        /// <summary>
        /// Indique si la totalité de la chaîne est valide.
        /// Si on a spécifié aucun regex (mauvaise configuration), on retourne toujours faux.
        /// Si l'agument est null, on retourne faux. Un argument vide pourrait être valid par contre, selon le regex.
        /// </summary>
        public bool ExactMatch(string str)
        {
            if (_regexValidation == null) return false;
            if (str == null) return false;

            var rez = _regexValidation.Match(str);
            return rez.Success && rez.Index == 0 && rez.Length == str.Length;
        }

        /// <summary>
        /// Retourne l'ensemble des "Match" trouvées sur la chaîne.
        /// SI null, c'est que l'instance n'était pas initialisée correctement ou que l'argument était null.
        /// </summary>
        public MatchCollection Matches(string str)
        {
            if (_regexValidation == null) return null;
            return str == null ? null : _regexValidation.Matches(str);
        }
    }
}