using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.Common.Util.DateAndTime
{
    /// <summary>
    /// Cette classe est la base pour calculer une date à partir de plusieurs éléments, de façon dynamique.
    /// Il est normalement difficile de configurer une date de façon non-programmatique. Cette classe et ses enfants se veulent une façon
    /// de configurer ce calcul via configuration texte (avec format JSON habituellement), et d'être facilement intégrable via une approche
    /// d'inversion de contrôle.
    /// </summary>
    [JsonConverter(typeof(DateTimeCalculatorJsonConverter))]
    public abstract class DatetimeCalculator
    {
        /// <summary>
        /// Si on veut être mis au courant (et si la classe d'implémentation le permet), on peut être notifié quand
        /// la date calculée change. La méthode <see cref="CalculateDate"/> permet de lire la dernière date calculée.
        /// </summary>
        public event EventHandler<DateTimeOffset> DateHasUpdated;

        protected void TriggerEventDateUpdate(DateTimeOffset dt)
        {
            DateHasUpdated?.Invoke(this, dt);
        }

        /// <summary>
        /// Calcule un point temporel à partir de l'état actuel, et le retourne selon un type de date précis (Local, UTC, ou inconnu).
        /// Par défaut, retourne les valeurs sous forme UTC.
        /// Ne provoque pas d'événement, ne fait que retourner la présente valeur depuis le dernier changement.
        /// </summary>
        public abstract DateTimeOffset CalculateDate(DateTimeKind dateType = DateTimeKind.Utc);

        public override string ToString()
        {
            return CalculateDate().ToString(DateUtil.UtcTimezoneFormatString);
        }
    }

    /// <summary>
    /// Forme la plus basique d'un calculateur. Il stocke une seule date, et n'en change pas automatiquement.
    /// Par défaut, la valeur DateTime minimale est utilisée.
    /// </summary>
    public class DateTimeCalculatorFixed : DatetimeCalculator
    {
        private DateTime _fixed = DateTime.MinValue;

        public DateTime FixedDate
        {
            get => _fixed;
            set
            {
                if (_fixed == value) return;
                _fixed = value;
                TriggerEventDateUpdate(_fixed);
            }
        }
        
        public override DateTimeOffset CalculateDate(DateTimeKind dateType = DateTimeKind.Utc)
        {
            if (dateType == DateTimeKind.Unspecified || FixedDate.Kind == dateType)
            {
                return FixedDate;
            }

            return dateType == DateTimeKind.Local ? FixedDate.ToLocalTime() : FixedDate.ToUniversalTime();
        }
    }

    /// <summary>
    /// Classe pour désérialiser les classes d'implémentation de ICondition
    /// </summary>
    public class DateTimeCalculatorJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new Exception("Cannot do Write operation with this converter.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            DatetimeCalculator cond;

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.

            /* DateTimeCalculatorFixed */
            if (jsonObject.Properties().Any(p => p.Name.Equals("FixedDate")))
            {
                cond = new DateTimeCalculatorFixed();
            }

            /* DateTimeCalculatorFromNow */
            else if (jsonObject.Properties().Any(p => p.Name.Equals("ReduceByTimespan")))
            {
                cond = new DateTimeCalculatorFromNow();
            }
            
            else
            {
                throw new Exception("Unknown subclass of DatetimeCalculator. Add this to the code.");
            }
            
            serializer.Populate(jsonObject.CreateReader(), cond);
            return cond;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(DatetimeCalculator).IsAssignableFrom(objectType);
        }
    }
}