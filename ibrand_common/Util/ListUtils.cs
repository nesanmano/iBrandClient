using System.Linq;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Classe d'extensions et de méthode utilitaires pour les listes génériques. Implémente la méthode LINQ "Sort" sur des IList.
    /// Voir ici pour une discussion sur ces extensions et leur pertinence.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [PublicAPI]
    public static class ListUtils
    {
        public static void Sort<T>(this IList<T> list)
        {
            if (list is List<T> list1)
            {
                list1.Sort();
            }
            else
            {
                var copy = new List<T>(list);
                copy.Sort();
                Copy(copy, 0, list, 0, list.Count);
            }
        }

        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T> list1)
            {
                list1.Sort(comparison);
            }
            else
            {
                var copy = new List<T>(list);
                copy.Sort(comparison);
                Copy(copy, 0, list, 0, list.Count);
            }
        }

        public static void Sort<T>(this IList<T> list, IComparer<T> comparer)
        {
            if (list is List<T> list1)
            {
                list1.Sort(comparer);
            }
            else
            {
                var copy = new List<T>(list);
                copy.Sort(comparer);
                Copy(copy, 0, list, 0, list.Count);
            }
        }

        public static void Sort<T>(this IList<T> list, int index, int count,
            IComparer<T> comparer)
        {
            if (list is List<T> list1)
            {
                list1.Sort(index, count, comparer);
            }
            else
            {
                var range = new List<T>(count);
                for (var i = 0; i < count; i++)
                {
                    range.Add(list[index + i]);
                }
                range.Sort(comparer);
                Copy(range, 0, list, index, count);
            }
        }

        private static void Copy<T>(IList<T> sourceList, int sourceIndex,
            IList<T> destinationList, int destinationIndex, int count)
        {
            for (var i = 0; i < count; i++)
            {
                destinationList[destinationIndex + i] = sourceList[sourceIndex + i];
            }
        }

        private static bool NullAndCountCheck<T>(IList<T> arg1, IList<T> arg2)
        {
            // Check null et de nombre d'items.
            if (arg1 == null && arg2 == null) return true;
            if (arg1 == null) return false;
            if (arg2 == null) return false;
            return arg1.Count == arg2.Count;
        }

        private static int NullAndCountCompare<T>(IList<T> arg1, IList<T> arg2)
        {
            // Check null et de nombre d'items.
            if (arg1 == null && arg2 == null) return 0;
            if (arg1 == null) return -1;
            if (arg2 == null) return 1;
            return arg1.Count - arg2.Count;
        }

        /// <summary>
        /// Indique si les listes sont équivalentes, soit les éléments sont égaux, mais pas nécessairement dans le même ordre.
        /// Pour souci d'efficacité, on utilise le compararateur naturel.
        /// </summary>
        /// <typeparam name="T">Le type interne de la liste. Doir être comparable avec lui-même.</typeparam>
        /// <param name="arg1">Première liste</param>
        /// <param name="arg2">Deuxième liste</param>
        /// <returns>TRIE si les éléments sont égaux, mais pas nécessairement dans le même ordre.</returns>
        public static bool EquivalentToOther<T>(IList<T> arg1, IList<T> arg2) where T : IComparable<T>
        {
            if (!NullAndCountCheck(arg1, arg2)) return false;

            var sorted1 = new List<T>(arg1);
            sorted1.Sort();

            var sorted2 = new List<T>(arg2);
            sorted2.Sort();

            return !sorted1.Where((t, i) => t.CompareTo(sorted2[i]) == 0).Any();
        }

        /// <summary>
        /// Indique si les listes sont équivalentes, soit les éléments sont égaux, mais pas nécessairement dans le même ordre.
        /// Pour souci d'efficacité, on utilise un compareur.
        /// </summary>
        /// <typeparam name="T">Le type interne de la liste. Doir être comparable avec lui-même.</typeparam>
        /// <param name="arg1">Première liste</param>
        /// <param name="arg2">Deuxième liste</param>
        /// <param name="comparer">Le comparateur des éléments.</param>
        /// <returns>TRIE si les éléments sont égaux, mais pas nécessairement dans le même ordre.</returns>
        public static bool EquivalentToOther<T>(IList<T> arg1, IList<T> arg2, IComparer<T> comparer)
        {
            if (comparer == null) return false;
            if (!NullAndCountCheck(arg1, arg2)) return false;

            var sorted1 = new List<T>(arg1);
            sorted1.Sort(comparer);

            var sorted2 = new List<T>(arg2);
            sorted2.Sort(comparer);

            return !sorted1.Where((t, i) => comparer.Compare(t, sorted2[i]) == 0).Any();
        }

        /// <summary>
        /// Indique si les listes sont égales, soit les éléments sont égaux, dans le même ordre.
        /// </summary>
        /// <typeparam name="T">Le type interne de la liste</typeparam>
        /// <param name="arg1">Première liste</param>
        /// <param name="arg2">Deuxième liste</param>
        /// <returns>TRIE si les éléments sont égaux et dans le même ordre.</returns>
        public static bool EqualToOther<T>(IList<T> arg1, IList<T> arg2) where T : IEquatable<T>
        {
            return NullAndCountCheck(arg1, arg2) && arg1.SequenceEqual(arg2);
        }

        /// <summary>
        /// Compare les éléments de deux listes, et dès qu'une liste ne contient pas le même nombre d,élément ou qu'un élément au même index n'est pas
        /// strictement égal à son vis-à-vis, on retourne la valeur comparative.
        /// Une valeur de zéro implique que les liste sont "égales" sur tous leurs éléments, dans le même ordre.
        /// </summary>
        /// <typeparam name="T">Le type comparable.</typeparam>
        /// <param name="arg1">La première liste.</param>
        /// <param name="arg2">La deuxième liste.</param>
        public static int Compare<T>(IList<T> arg1, IList<T> arg2) where T : IComparable<T>
        {
            var comp = NullAndCountCompare(arg1, arg2);
            if (0 != comp) return comp;

            for (var i = 0; i < arg1.Count; i++)
            {
                comp = arg1[i].CompareTo(arg2[i]);
                if (comp != 0) return comp;
            }

            return 0;
        }
    }
}