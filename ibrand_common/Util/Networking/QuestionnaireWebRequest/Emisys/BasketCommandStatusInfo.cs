using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class BasketCommandStatusInfo
    {
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        /// <summary>
        /// Uniquement utilisé si erreur.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "basket_status"),
         JsonConverter(typeof(StringEnumConverter))]
        public BasketStatusEnum Status { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "customer_birthday")]
        public string CustomerBirthday { get; set; }

        [JsonProperty(PropertyName = "customer_country")]
        public string CustomerCountry { get; set; }

        [JsonProperty(PropertyName = "customer_email")]
        public string CustomerEmail { get; set; }

        [JsonProperty(PropertyName = "customer_zipcode")]
        public string CustomerZip { get; set; }

        [JsonProperty(PropertyName = "date_confirmed"),
         JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? DateConfirmed { get; set; }

        [JsonProperty(PropertyName = "date_created"),
         JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? DateCreated { get; set; }

        [JsonProperty(PropertyName = "date_payment"),
         JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? DatePayment { get; set; }

        [JsonProperty(PropertyName = "date_updated"),
         JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? DateUpdated { get; set; }

        [JsonProperty(PropertyName = "email_reminder_date")]
        public string EmailRemainderDate { get; set; }

        [JsonProperty(PropertyName = "fees")]
        public float Fees { get; set; }

        [JsonProperty(PropertyName = "fees_transaction")]
        public float FeesTransaction { get; set; }

    }
}