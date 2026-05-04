using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.PrintStation.Contest
{
    /// Représente une période où l'on peut gagner dans un concours.
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    [PublicAPI]
    public class ContestPeriod : IEquatable<ContestPeriod>, IComparable<ContestPeriod>
    {
        public override int GetHashCode()
        {
            unchecked
            {
                return (StartTime.GetHashCode() * 397) ^ EndTime.GetHashCode();
            }
        }

        /// Date de départ, inclusif.
        public DateTime StartTime { get; set; }

        /// Date de fin, exclusif.
        public DateTime EndTime { get; set; }

        /// Nom de la période. N'est pas utilisé dans AUCUNE méthode de comparaison, incluant égalité.
        public string Name { get; set; }

        /// Retourne une autre instance avec des références différentes (sauf si immutabe, comme le nom).
        public ContestPeriod Clone()
        {
            var clone = new ContestPeriod
            {
                StartTime = new DateTime(StartTime.Ticks),
                EndTime = new DateTime(EndTime.Ticks),
                Name = Name
            };

            return clone;
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as ContestPeriod);
        }

        public bool Equals(ContestPeriod other)
        {
            return other != null &&
                   StartTime == other.StartTime &&
                   EndTime == other.EndTime;
        }

        public static bool operator ==(ContestPeriod period1, ContestPeriod period2)
        {
            return EqualityComparer<ContestPeriod>.Default.Equals(period1, period2);
        }

        public static bool operator !=(ContestPeriod period1, ContestPeriod period2)
        {
            return !(period1 == period2);
        }

        /// Indique qu'une période se situe avant une autre. Sa fin est avant le début de l'autre.
        /// Si un des deux argument est invalide, et s'ils se croisent (overlap), retourne faux.
        public static bool operator <(ContestPeriod period1, ContestPeriod period2)
        {
            if (!period1.IsValid() || !period2.IsValid()) return false;
            if (period1.Overlap(period2)) return false;

            return period1.EndTime < period2.StartTime;
        }

        /// Indique qu'une période se situe après une autre. Sdébut est après la fin de l'autre..
        public static bool operator >(ContestPeriod period1, ContestPeriod period2)
        {
            if (!period1.IsValid() || !period2.IsValid()) return false;
            if (period1.Overlap(period2)) return false;

            return period1.StartTime >= period2.EndTime;
        }

        /// <summary>
        /// Indique si la période est valide.
        /// Une période est valide si et seulement si:
        /// 1- Le début et la fin ne sont pas égaux.
        /// 2- Le début est avant la fin.
        /// </summary>
        /// <returns>TRUE si valide.</returns>
        public bool IsValid()
        {
            return StartTime != EndTime && StartTime < EndTime;
        }

        /// <summary>
        /// Retourne la durée entre le début et la fin.
        /// </summary>
        /// <returns>La durée de la période.</returns>
        public TimeSpan Span()
        {
            return EndTime.Subtract(StartTime);
        }

        /// <summary>
        /// Indique si cette période n'est pas disjointe avec une autre période.
        /// Une période invalide n'est jamais cojointe avec une autre.
        /// </summary>
        /// <param name="other">L'autre période à vérifier.</param>
        /// <returns></returns>
        public bool Overlap(ContestPeriod other)
        {
            if (!IsValid() || !other.IsValid()) return false;

            return (StartTime < other.EndTime && StartTime >= other.StartTime) || (EndTime > other.StartTime && EndTime < other.EndTime);
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        public string ToString(string dateFormat)
        {
            return ToStringFormat(dateFormat);
        }

        public string ToString(string dateFormat, IFormatProvider culture)
        {
            return ToStringFormat(dateFormat, culture);
        }

        /// ToString avec options de formatage.
        public string ToStringFormat(string dateFormat = "F", IFormatProvider culture = null)
        {
            if (culture == null)
            {
                culture = CultureInfo.InvariantCulture;
            }
            
            var sb = new StringBuilder();
            
            if (Name != null)
            {
                sb.Append(Name);
                sb.Append(": ");
            }
            
            sb.Append(StartTime.ToString(dateFormat, culture));
            sb.Append(" to ");
            sb.Append(EndTime.ToString(dateFormat, culture));

            return sb.ToString();
        }

        public int CompareTo(ContestPeriod other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var startTimeComparison = StartTime.CompareTo(other.StartTime);
            return startTimeComparison != 0 ? startTimeComparison : EndTime.CompareTo(other.EndTime);
        }

        /// Teste si la date donnée est située dans la période.
        public bool InPeriod(DateTime date)
        {
            return StartTime <= date && EndTime > date;
        }

        /// <summary>
        /// Génère un nom de fichier unique selon le contenu de la période.
        /// Des période égales génèrent des noms de fichiers égaux.
        /// </summary>
        /// <returns>Un nom de fichier de sauvegarde unique selon l'état de la période.</returns>
        public string GetFileName()
        {
            var sb = new StringBuilder();

            sb.Append(StartTime.Ticks.ToString().PadLeft(20, '0'));
            sb.Append('_');
            sb.Append(EndTime.Ticks.ToString().PadLeft(20, '0'));
            sb.Append(".json");

            return sb.ToString();
        }

    }
}