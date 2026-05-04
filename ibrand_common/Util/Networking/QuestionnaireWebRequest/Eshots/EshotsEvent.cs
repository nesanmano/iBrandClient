using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    [PublicAPI]
    public class EshotsEvent
    {
        [JsonProperty(PropertyName = "address1")]
        public string AddressMain { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string AddressSecond { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "subCategory")]
        public string SubCategory { get; set; }

        [JsonProperty(PropertyName = "city")] public string City { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "state")] public string State { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "eventId")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "name")] public string EventName { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// La structure de sérialisation d'un "jour" est un simple dictionnaire.
        /// Mais on peut analyser plus longtemps cette structure. C'est pourquoi on peut "extraire"
        /// les jours définis via les méthodes "ExtractEventDays" et son opération contraire "ConvertEventDays"
        /// contenues dans cette classe.
        /// </summary>
        [JsonProperty(PropertyName = "eventDays")]
        public IDictionary<string, int> EventDays { get; set; }

        /// <summary>
        /// Fait une liste des descriptifs de l'événement.
        /// </summary>
        [JsonIgnore]
        public string FullDescription {
            get {
                var sb = new StringBuilder();

                sb.Append("Name: ");
                sb.AppendLine(EventName);

                if (Category != null)
                {
                    sb.Append("\tCategory: ");
                    sb.AppendLine(Category);
                }

                if (Description != null)
                {
                    sb.Append("\tDescription: ");
                    sb.Append(Description);
                    if (SubCategory != null)
                    {
                        sb.Append(" (Sub: ");
                        sb.Append(SubCategory);
                        sb.Append(")");
                    }
                    sb.AppendLine();
                }

                if (AddressMain != null || AddressSecond != null || City != null || PostalCode != null || Country != null || State != null)
                {
                    sb.AppendLine("\tAddress:");
                    sb.AppendLine($"\t\tAddr (Main): {AddressMain}");
                    if (AddressSecond != null) sb.AppendLine($"\t\tAddr (Sec): {AddressSecond}");
                    sb.AppendLine($"\t\tCity: {City}");
                    sb.AppendLine($"\t\tPostal Code: {PostalCode}");
                    sb.AppendLine($"\t\tState: {State}");
                    sb.AppendLine($"\t\tCountry: {Country}");
                }

                return sb.ToString();
            }
        }

    /// <summary>
        /// Retourne une liste de jours d'événements basée sur cette instance.
        /// </summary>
        /// <returns>Une liste des jours d'événeemnts en une format plus complet.</returns>
        public IList<EshotsEventDay> ExtractEventDays()
        {
            return ExtractEventDays(EventDays);
        }

        /// <summary>
        /// Retourne une liste de jours d'événements transformés via un dictionnaire.
        /// </summary>
        /// <param name="originalList">Le dictionnaire des jours encodés via ("jour":"identifiant")</param>
        /// <returns>Une liste des jours d'événeemnts en une format plus complet.</returns>
        public static IList<EshotsEventDay> ExtractEventDays(IDictionary<string, int> originalList)
        {
            return originalList?.Select(item => new EshotsEventDay
            {
                EventDayCode = item.Key,
                EventDayId = item.Value
            }).ToList();
        }

        /// <summary>
        /// Retourne un dictionnaire de jours d'événements transformés via une liste d'objets.
        /// </summary>
        /// <param name="originalList">La liste des objets.</param>
        /// <returns>Un dictionnaire des jours encodés via ("jour":"identifiant").</returns>
        public static IDictionary<string, int> ConvertEventDays(IList<EshotsEventDay> originalList)
        {
            return originalList?.ToDictionary(d => d.EventDayCode, d => d.EventDayId);
        }

        public override string ToString()
        {
            return $"{EventName} - ({EventId})";
        }
    }
}