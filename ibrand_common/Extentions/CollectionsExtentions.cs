using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NomadLogic.Common.Extentions
{
    /// <summary>
    /// Extensions pour ajouter des fonctionalités aux listes.
    /// </summary>
    public static class CollectionsExtentions
    {
        /// <summary>
        /// Pour mélanger les éléments d'une liste. Ne fait que brasser l'ordre des éléments sans nouvelles instanciations.
        /// Voir: https://stackoverflow.com/questions/273313/randomize-a-listt
        /// </summary>
        /// <typeparam name="T">Le type générique.</typeparam>
        /// <param name="list">Référence vers la liste</param>
        /// <param name="seed">La source aléatoire. Optionel­.</param>
        public static void Shuffle<T>(this IList<T> list, long? seed = null)
        {
            var n = list.Count;
            var rng = seed.HasValue ? new Random((int)seed.Value) : new Random();
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>
        /// Vide un file concurente. Ceci n'est pas une opération O(1) à cause des mécaniques anti-blocage.
        /// Voir ici: https://social.msdn.microsoft.com/Forums/en-US/accf4254-ee81-4059-9251-619bc6bbeadf/clear-a-concurrentqueue?forum=rx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out _))
            {
                // do nothing
            }
        }
    }
}