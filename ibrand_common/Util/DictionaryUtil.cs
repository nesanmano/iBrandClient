using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Méthode pour, sécuritairement, accéder ou manipuler des dictionnaires dans C#.
    /// </summary>
    [PublicAPI]
    public static class DictionaryUtil
    {
        /// <summary>
        /// Vérifie si dans un dictionnaire une clé existe. Si cette clé existe bel et bien, on retourne sa valeur. Sinon,
        /// plutôt que de ne rien retourner, on retourne le tout premier élément de ce dictionnaire.
        /// Ce genre de vérification se fait, par exemple, dans un contexte d'internationalisation, où on cherche une langue
        /// précise ou l'on se rabat sur la première existante si aucune n'existe.
        /// Si le dictionnaire est nul ou s'il ne contient rien, on retourne la valeur par défaut de la clé. Fonctionne mieux
        /// avec un dictionnaire où le type de valeur est nullable.
        /// </summary>
        /// <typeparam name="TK">Le type de clé.</typeparam>
        /// <typeparam name="TV">Le type de valeur.</typeparam>
        /// <param name="dico">Le dictionnaire sur lequel effectuer la recherche.</param>
        /// <param name="key">La clé recherchée.</param>
        /// <returns>Si présente, la valeur associée à la clé. Sinon, la première valeur du dictionnaire,
        /// sans tenir compte de la clé. Si le dictionnaire est vide ou nul, retourne la valeur par défaut du type de la valeur.</returns>
        public static TV GetDefaultValue<TK, TV>(this IDictionary<TK, TV> dico, TK key)
        {
            if (dico == null || !dico.Any()) return default;
            return dico.ContainsKey(key) ? dico[key] : dico.First().Value;
        }

        /// <summary>
        /// Vérifie si dans un dictionnaire une clé existe. Si cette clé existe bel et bien, on retourne sa valeur. Sinon,
        /// plutôt que de ne rien retourner, on retourne le tout premier élément de ce dictionnaire.
        /// Ce genre de vérification se fait, par exemple, dans un contexte d'internationalisation, où on cherche une langue
        /// précise ou l'on se rabat sur la première existante si aucune n'existe.
        /// Si le dictionnaire est nul ou s'il ne contient rien, on retourne la valeur par défaut de la clé. Fonctionne mieux
        /// avec un dictionnaire où le type de valeur est nullable.
        /// </summary>
        /// <typeparam name="TK">Le type de clé.</typeparam>
        /// <typeparam name="TV">Le type de valeur.</typeparam>
        /// <param name="dico">Le dictionnaire sur lequel effectuer la recherche.</param>
        /// <param name="key">La clé recherchée.</param>
        /// <returns>Si présente, la valeur associée à la clé. Sinon, la première valeur du dictionnaire,
        /// sans tenir compte de la clé. Si le dictionnaire est vide ou nul, retourne la valeur par défaut du type de la valeur.</returns>
        public static TV GetDefaultValue<TK, TV>(this Dictionary<TK, TV> dico, TK key)
        {
            return GetDefaultValue((IDictionary<TK, TV>)dico, key);
        }

        /// <summary>
        /// Vérifie si dans un dictionnaire (en lecture seule) une clé existe. Si cette clé existe bel et bien, on retourne sa valeur. Sinon,
        /// plutôt que de ne rien retourner, on retourne le tout premier élément de ce dictionnaire.
        /// Ce genre de vérification se fait, par exemple, dans un contexte d'internationalisation, où on cherche une langue
        /// précise ou l'on se rabat sur la première existante si aucune n'existe.
        /// Si le dictionnaire est nul ou s'il ne contient rien, on retourne la valeur par défaut de la clé. Fonctionne mieux
        /// avec un dictionnaire où le type de valeur est nullable.
        /// </summary>
        /// <typeparam name="TK">Le type de clé.</typeparam>
        /// <typeparam name="TV">Le type de valeur.</typeparam>
        /// <param name="dico">Le dictionnaire sur lequel effectuer la recherche.</param>
        /// <param name="key">La clé recherchée.</param>
        /// <returns>Si présente, la valeur associée à la clé. Sinon, la première valeur du dictionnaire,
        /// sans tenir compte de la clé. Si le dictionnaire est vide ou nul, retourne la valeur par défaut du type de la valeur.</returns>
        public static TV GetDefaultValue<TK, TV>(this IReadOnlyDictionary<TK, TV> dico, TK key)
        {
            if (dico == null || !dico.Any()) return default;
            return dico.ContainsKey(key) ? dico[key] : dico.First().Value;
        }
    }
}