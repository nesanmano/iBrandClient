using System;
using System.Text;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.DateAndTime
{
    /// <summary>
    /// Classe statique pour la manipulation de dates et de temps.
    /// Contient des méthodes de conversion de timestamp Java à C#
    /// </summary>
    [PublicAPI]
    public static class DateUtil
    {
        /// <summary>
        /// Date de base pour le EPOCH de Windows C#.
        /// </summary>
        public static readonly DateTime WindowsEpoch = new DateTime(1601, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// Date de base pour le EPOCH de Java.
        /// </summary>
        public static readonly DateTime JavaEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// La différence entre le EPOCH de Java et le EPOCH de Windows, en nombre de Ticks.
        /// </summary>
        public static readonly long EpochDiffTicks;

        /// <summary>
        /// Format avec timezone, compatible UTC. Si date locale, utilise le format 'K' pour afficher la zone temporelle, soit "+00:00" ou "-00:00"
        /// </summary>
        public static readonly string UtcTimezoneFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";

        /// <summary>
        /// Format exact de date pour le format ISO 8601.
        /// </summary>
        public static readonly string Iso8601FormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK";

        static DateUtil()
        {
            EpochDiffTicks = (JavaEpoch.ToFileTimeUtc() - WindowsEpoch.ToFileTimeUtc())
                            / TimeSpan.TicksPerMillisecond;
        }
        
        /// <summary>
        /// Vérifie si une date est déjà en format UTC avant de faire une tentative de conversion. Prévient une double conversion.
        /// Voir ici pour plus d'information: https://stackoverflow.com/questions/1820915/how-can-i-format-datetime-to-web-utc-format
        /// </summary>
        /// <param name="inTime">L'instance à vérifier/convertir</param>
        /// <returns>Le résultat de la conversion/vérification.</returns>
        public static DateTime SafeUniversal(this DateTime inTime)
        {
            return (DateTimeKind.Unspecified == inTime.Kind)
                ? new DateTime(inTime.Ticks, DateTimeKind.Utc)
                : inTime.ToUniversalTime();
        }
        
        /// <summary>
        /// Obtient le temps en TimeStamp de format Java.
        /// Utiliser "ToDateTime" pour convertir cette valeur en DateTime.
        /// </summary>
        /// <returns></returns>
        public static string GetJavaTimeStringFromNow()
        {
            return Convert.ToString(GetJavaTimeFromNow());
        }
        
        /// <summary>
        /// Obtient le temps en TimeStamp de format Java.
        /// Utiliser "ToDateTime" pour convertir cette valeur en DateTime.
        /// </summary>
        /// <returns></returns>
        public static long GetJavaTimeFromNow()
        {
            var nowInUtc = DateTime.UtcNow;
            return (nowInUtc - JavaEpoch).Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Obtient la valeur entière d'une date en format Java EPOCH, sous forme de String.
        /// </summary>
        /// <param name="date">La date C# à transformer.</param>
        /// <returns>Une valeur entière de cette transformation.</returns>
        public static string GetStringJavaTimeFromDateTime(DateTime date)
        {
            return Convert.ToString(GetJavaTimeFromDateTime(date));
        }

        /// <summary>
        /// Obtient la valeur entière d'une date en format Java EPOCH.
        /// </summary>
        /// <param name="date">La date C# à transformer.</param>
        /// <returns>Une valeur entière de cette transformation.</returns>
        public static long GetJavaTimeFromDateTime(DateTime date)
        {
            return (date - JavaEpoch).Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Convertit une valeur de JAVA EPOCH en DateTime
        /// </summary>
        /// <param name="javaTime">La valeur entière de la date JAVA.</param>
        /// <returns>Le DateTime convertit.</returns>
        public static DateTime FromJavaTimeToDateTime(long javaTime)
        {
            long val = (javaTime + EpochDiffTicks) * TimeSpan.TicksPerMillisecond;
            var d = DateTime.FromFileTime(val).ToLocalTime();
            return d;
        }

        /// <summary>
        /// Convertit une valeur de JAVA EPOCH en DateTime. Si la chaîne est inconvertible en long, une exeption sera lancée.
        /// </summary>
        /// <param name="javaTimeStr">La valeur de la date JAVA, en string.</param>
        /// <returns>Le DateTime convertit.</returns>
        public static DateTime FromJavaTimeToDateTime(string javaTimeStr)
        {
            if (!long.TryParse(javaTimeStr, out var javaTime)) throw new Exception("Cannot transform string into long: " + javaTimeStr);
            return FromJavaTimeToDateTime(javaTime);
        }

        /// <summary>
        /// Méthode d'extension pour formatter un TimeSPan en format simple de "hh:mm:ss", avec "ss" optionel.
        /// </summary>
        /// <param name="span">La référence de TimeSpan</param>
        /// <param name="useSeconds">Indique s'il faut afficher les secondes.</param>
        /// <param name="useMiliseconds">Indique s'il faut afficher les milisecondes. Faux par défaut.</param>
        /// <returns>Un string formatté.</returns>
        public static string ToStringHourMinSec(this TimeSpan span, bool useSeconds = true, bool useMiliseconds = false)
        {
            var sb = new StringBuilder();

            sb.Append(span.Hours);
            sb.Append(":");
            sb.Append(span.Minutes);
            if (!useSeconds) return sb.ToString();
            sb.Append(":");
            sb.Append(span.Seconds);
            if (!useMiliseconds) return sb.ToString();
            sb.Append(".");
            sb.Append($"{span.Milliseconds:000}");

            return sb.ToString();
        }

        /// <summary>
        /// Méthode d'extension pour formatter un TimeSpan en format de lecture.
        /// Affiche quelque chose genre " 1h 24m 23s 345ms", en n'affichant pas les valeurs de zéro pour économiser de l'espace.
        /// Cet affichage n'est pas un format ISO.
        /// </summary>
        /// <param name="span">La référence de TimeSpan</param>
        /// <param name="useMiliSeconds"></param>
        /// <returns>Un string formatté.</returns>
        public static string ToMinimumReadableTime(this TimeSpan span, bool useMiliSeconds = true)
        {
            var sb = new StringBuilder();

            if (span.Hours != 0)
            {
                sb.Append(span.Hours);
                sb.Append("h");
            }

            if (span.Minutes != 0)
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(span.Minutes);
                sb.Append("m");
            }
            if (span.Seconds != 0)
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(span.Seconds);
                sb.Append("s");
            }

            if (!useMiliSeconds) return sb.ToString();

            if (sb.Length != 0) sb.Append(' ');
            sb.Append($"{span.Milliseconds:000}ms");

            return sb.ToString();
        }

        /// <summary>
        /// Convertie une date UNIX en format numérique vers un format texte de cette date.
        /// Nécessite aussi un nom complet de la Timezone pour convertir cette valeur, car la date est contenues en UTC.
        /// On peut trouver le nom des zones avec un peu d'aide. Voir ici:
        /// https://stackoverflow.com/questions/7908343/list-of-timezone-ids-for-use-with-findtimezonebyid-in-c
        /// </summary>
        /// <param name="dateOff">La valeur UNIX EPOCH de la date.</param>
        /// <param name="timeZoneId">Le nom </param>
        /// <returns>Un tuple avec la date, puis l'heure, en format canadien.</returns>
        public static (string, string) ExtractStringDateAndTime(long dateOff, string timeZoneId)
        {
            var dateTimeOff = DateTimeOffset.FromUnixTimeMilliseconds(dateOff);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var modTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOff.UtcDateTime, timeZoneInfo);

            return (modTime.ToString("yyyy-MM-dd"), modTime.ToString("HH:mm:ss"));
        }
    }
}
