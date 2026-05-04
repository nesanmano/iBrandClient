using System;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.iBrand
{
    /// <summary>
    /// Réponse en format JSON de la commande pour une version condensée des répondants et de leurs réponses.
    /// </summary>
    [PublicAPI]
    public class IBrandCondensedQuestionnaireAnswer
    {
        /// Numéro unique du répondant sur le serveur, auquel la réponse est associée.
        public int Numero { get; set; }

        /// Version string de la date, en format "yyyy-MM-dd"
        [JsonProperty(PropertyName = "Date")]
        public string DateString { get; set; }

        /// Version string de l'heure, en format "HH:mm:ss"
        [JsonProperty(PropertyName = "Heure")]
        public string HeureString { get; set; }

        /// Version complète de DateTime. Donne DateTime.MinValue si les valeurs sont invalide. n'est instancié qu'une fois.
        [JsonIgnore]
        public DateTime Date {
            get
            {
                if (_parsedDate.HasValue) return _parsedDate.Value;

                if (string.IsNullOrWhiteSpace(DateString) || string.IsNullOrWhiteSpace(HeureString))
                {
                    _parsedDate = DateTime.MinValue;
                    return _parsedDate.Value;
                }

                _parsedDate = DateTime.ParseExact(DateString + " " + HeureString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                return _parsedDate.Value;
            }
        }

        private DateTime? _parsedDate;

        /// Numéro de question associé à la réponse.
        [JsonProperty(PropertyName = "qNo")]
        public int QuestionNo { get; set; }

        /// Numéro de choix associé à la réponse.
        [JsonProperty(PropertyName = "cNo")]
        public int ChoixNo { get; set; } = 1;
        
        /// Valeur textuelle de la réponse. Peut être vide ou null.
        public string Reponse { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}