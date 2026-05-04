using System;

namespace NomadLogic.Common.Util
{
    public interface IEventLogger
    {
        /// <summary>
        /// Écrit un événement dans la zone à cet effet et dans les fichiers journaux au besoin.
        /// <param name="message">Le message à envoyer dans les journaux.</param>
        /// </summary>
        void LogEvent(string message);

        /// <summary>
        /// Écrit un événement dans la zone à cet effet et dans les fichiers journaux au besoin.
        /// <param name="message">Le message à envoyer dans les journaux.</param>
        /// <param name="e">Référence vers l'erreur à journaliser.</param>
        /// </summary>
        void LogEvent(string message, Exception e);
    }
}
