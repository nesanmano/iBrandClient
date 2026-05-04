using System;
using NLog;

namespace NomadLogic.Common.Util.DateAndTime
{
    /// <summary>
    /// Cette variante de calcul de date se fait toujours à partir du moment présent, en format local, puis applique des
    /// modificateurs vers le bas (soustraction). On obtient donc toujours une date antérieur car la réduction se fait sur une valeur absolue (TimeSpan).
    /// Il n'y a pas d'événement lancé: le recalcul de la date se fait à chaque appel de la fonction de date.
    /// </summary>
    public class DateTimeCalculatorFromNow : DatetimeCalculator
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private DateTime _lastCalculatedTime = DateTime.MinValue;

        /// <summary>
        /// Indique qu'il faut partir le calcul de la date en enlevant au préalable la composante de l'heure.
        /// On garde l'information de l'heure par défaut.
        /// </summary>
        public bool StartFromDateOnly { get; set; }

        /// <summary>
        /// Soustrait cette valeur de la date actuelle.
        /// Par défaut, zéro (no-op).
        /// </summary>
        public TimeSpan ReduceByTimespan { get; set; } = TimeSpan.Zero;

        public override DateTimeOffset CalculateDate(DateTimeKind dateType = DateTimeKind.Utc)
        {
            var n = dateType == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
            if (StartFromDateOnly)
            {
                n = n.Date;
            }

            var offset = n - ReduceByTimespan;

            if (_lastCalculatedTime >= offset) return offset;

            // Journalise la date calculée si elle change. Avec cette classe, la date calculée PEUT changer rarement, ou à chaque fois.
            _lastCalculatedTime = offset;
            if (!log.IsDebugEnabled) return offset;

            var localTrans = dateType != DateTimeKind.Local ? $", (local={_lastCalculatedTime.ToLocalTime().ToString(DateUtil.UtcTimezoneFormatString)})" : "";
            var typ = dateType.ToString();
            var dateStr = offset.ToString(DateUtil.UtcTimezoneFormatString);
            log.Debug($"DateCalculator: New calculated date ({typ}): {dateStr}{localTrans}");

            return offset;
        }
    }
}