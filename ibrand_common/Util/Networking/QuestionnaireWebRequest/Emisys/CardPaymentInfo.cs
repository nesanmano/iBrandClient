using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class CardPaymentInfo
    {
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        /// <summary>
        /// Code hexadécimal de la carte.
        /// </summary>
        [JsonProperty(PropertyName = "card_uid")]
        public string CardUid { get; set; }

        /// <summary>
        /// Le montant du paiement
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public float Amount { get; set; }
    }
}