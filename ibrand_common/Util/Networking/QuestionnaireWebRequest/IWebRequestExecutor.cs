using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    [PublicAPI]
    public interface IWebRequestExecutor : IDisposable
    {
        /// <summary>
        /// Initialize l'exécuteur web.
        /// </summary>
        /// <returns></returns>
        Task<bool> InitializeExecutor();

        /// <summary>
        /// Le nombre de milisecondes maximales pour chaque requête.
        /// </summary>
        int Timeout { get; set; }
    }
}
