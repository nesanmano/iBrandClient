using System.Collections.Generic;
using JetBrains.Annotations;
using MCollections;

namespace NomadLogic.Common.Util
{
    [PublicAPI]
    public interface IElementMonitor<T>
    {
        /// <summary>
        /// Donne une nouvelle liste d'éléments pour remplacer l'ancienne.
        /// Assume que la liste est triée ou que les éléments ne doivent pas changer d'index.
        /// </summary>
        /// <param name="updatedElements">La nouvelle liste d'éléments.</param>
        void UpdateElements([NotNull] IndexedSet<T> updatedElements);
    }
}