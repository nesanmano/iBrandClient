using System;

namespace NomadLogic.Common.Services
{
    public interface IProgressDeepCopy<out T>
    {
        /// <summary>
        /// Faite une copie profonde de l'instance en une nouvelle référence, complètement distincte.
        /// </summary>
        /// <returns>La nouvelle instance.</returns>
        T Copy();
    }
}