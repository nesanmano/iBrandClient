using System;
using System.Text;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Génère une séquence de caractères hexadécimals basé sur l'heure actuelle. Donne un chiffre pseudo-unique. Pseudo dans le sens ou
    /// aucun vérification n'est faite pour s'assurer de son unicité. C'est juste astronomiquement improbable que le même numéro ressorte
    /// dans un même contexte de travail.
    /// <br/>Peut également générer des valeur d'entier double (long).
    /// </summary>
    public class GenerateUniqueId
    {
        private static int _sessionCount;
        private static long _lastTimeVal;
        private static readonly Random randomSource = new Random();

        private const long MaxRandomLen = 2176782336L; // 36 ** 6

        /*
        * The session identifier must be unique within the typical
        * lifespan of a Session; the value can roll over after that.  3
        * characters: (this means a roll over after over a day, which is
        * much larger than a typical lifespan)
        */
        private const long MaxSessionLifespanTics = 46656; // 36 ** 3

        /*
         * Millisecons between different tics.  So this means that the
         * 3-character time string has a new value every 2 seconds:
         */
        public static long _tic_difference = 2000;

        public static long NextLong()
        {
            var buffer = new byte[sizeof(long)];
            randomSource.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Génère un code hexadécimal.
        /// </summary>
        /// <returns>Une chaîne avec un code hexadecimal, tout en minuscule.</returns>
        public static string GetId()
        {
            var sessionId = new StringBuilder();

            // Random value.
            var n = NextLong();
            if (n < 0)
            {
                n = -n;
            }

            n %= MaxRandomLen;

            // Add maxLen to pad the leading characters with '0'; remove
            // first digit with substring.
            n += MaxRandomLen;
            
            sessionId.Append(Convert.ToString(n, 16)
                             .Substring(1));

            var utcNow = DateTime.UtcNow;
            var baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            var timeStamp = (utcNow - baseTime).Ticks / 10000;

            var timeVal = (timeStamp / _tic_difference);

            // Cut.
            timeVal %= MaxSessionLifespanTics;

            // Padding, see above.
            timeVal += MaxSessionLifespanTics;

            sessionId.Append(Convert.ToString(timeVal, 16)
                             .Substring(1));

            /*
             * Make the string unique: append the session count since last
             * time flip.
             */

            // Count sessions only within tics.  So the 'real' session
            // count isn't exposed to the public.
            if (_lastTimeVal != timeVal)
            {
                _lastTimeVal = timeVal;
                _sessionCount = 0;
            }
            sessionId.Append(Convert.ToString(++_sessionCount,
                                           16));

            return sessionId.ToString();
        }
    }
}
