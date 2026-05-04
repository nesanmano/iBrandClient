using System;
using System.Globalization;
using NLog;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsEventDay : IEquatable<EshotsEventDay>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public string EventDayCode { get; set; }

        public int EventDayId { get; set; }

        public DateTime Date
        {
            get
            {
                if (!DateTime.TryParseExact(EventDayCode, "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedDate))
                {
                    log.Warn("Could not parse day code into date time: code=" + EventDayCode);
                }

                return parsedDate;
            }
        }

        public override string ToString()
        {
            return $"{Date.ToShortDateString()} - ({EventDayId})";
        }

        #region Equality methods

        public bool Equals(EshotsEventDay other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EventDayId == other.EventDayId
                && string.Equals(EventDayCode, other.EventDayCode);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EshotsEventDay) obj);
        }

        public static bool operator ==(EshotsEventDay left, EshotsEventDay right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EshotsEventDay left, EshotsEventDay right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EventDayCode != null ? EventDayCode.GetHashCode() : 0) * 397) ^ EventDayId;
            }
        }

        #endregion
    }
}