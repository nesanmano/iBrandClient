using System;
using NLog;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Filtre pour déterminer qu'un fichier est valide selon une date venant du serveur. Cette date doit être strictement égale ou
    /// supérieure à la date en paramètre. La valeur numérique est sous forme de "Ticks"
    /// Normalement cette information existe sous la forme d'un "long" qui est mis en préfixe dans le nom du fichier, suivi d'un '_'.
    /// Le format de sauvegarde est "{ticks_date}_{identifiant répondant serveur}_{nom fichier + extension}"
    /// </summary>
    public class ComparableFileInfoFilterServerDate : ComparableFileInfoFilterBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override bool Filter(ComparableFileInfo cfi)
        {
            if (cfi?.FileInfoRef == null) return false;

            var splits = cfi.FileInfoRef.Name.Split('_');
            if (splits.Length < 3)
            {
                return false;
            }

            if (!long.TryParse(splits[0], out var val))
            {
                return false;
            }

            var minDate = MinimalDate?.CalculateDate().Ticks ?? 0;
            var result = val > minDate;

            if (!result) return false;

            if (!log.IsTraceEnabled) return true;

            var minDateStr = new DateTime(minDate, DateTimeKind.Utc).ToLocalTime().ToString(DateUtil.UtcTimezoneFormatString);
            var fileDateStr = new DateTime(val, DateTimeKind.Utc).ToLocalTime().ToString(DateUtil.UtcTimezoneFormatString);
            log.Trace("File filter positive for following file ({0}) with Filter Date ({1}) vs File Date ({2})", cfi.FileInfoRef.Name, minDateStr, fileDateStr);

            return true;
        }

        public  DatetimeCalculator MinimalDate { get; set; } = new DateTimeCalculatorFromNow();
    }
}