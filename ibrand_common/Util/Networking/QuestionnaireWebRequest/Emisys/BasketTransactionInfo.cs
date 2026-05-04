using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class BasketTransactionInfo
    {
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        [JsonProperty(PropertyName = "pos_id")]
        public int PointOfServiceId { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string BasketSignature { get; set; }

        [JsonProperty(PropertyName = "terminal_id")]
        public string TerminalId { get; set; }

        [JsonProperty(PropertyName = "cards")]
        public IList<CardPaymentInfo> CardPayments { get; set; }
    }
}