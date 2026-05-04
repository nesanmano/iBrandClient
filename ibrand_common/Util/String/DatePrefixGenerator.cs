using System;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.String
{
    public class DatePrefixGenerator
    {
        /// <summary>
        /// Le format de la date à introduire comme préfixe. Si null ou vide, on va omettre ce préfixe et le diviseur.
        /// Par défaut: yyyyMMdd (ou une version compacte de la date seulement en ordre international.
        /// </summary>
        public string DatePrefixFormat { get; set; } = "yyyyMMdd";

        /// <summary>
        /// caractère entre le préfixe et le reste du code.
        /// </summary>
        public string DividerString { get; set; } = "_";
        
        /// <summary>
        /// Créer une version préfixée de la chaîne en paramètre.
        /// Si le format est vide ou null, on retourne la chaîne sans modification (no-op).
        /// </summary>
        public string ModifyString([NotNull] string code)
        {
            if (string.IsNullOrWhiteSpace(DatePrefixFormat)) return code;
            var prefix = DateTime.Today.ToString(DatePrefixFormat);
            return $"{prefix}{DividerString}{code}";
        }
    }

    
}