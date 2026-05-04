using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public class PostInitCardInfo
    {
        /// Represente le resultat de la requête
        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "nl_firstname")]
        public string Firstname { get; set; }

        [JsonProperty(PropertyName = "nl_lastname")]
        public string Lastname { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstnameEmisys { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastnameEmisys { get; set; }

        [JsonProperty(PropertyName = "nl_zipcode")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "nl_sms")]
        public string Sms { get; set; }

        [JsonProperty(PropertyName = "nl_legal_age")]
        public string IsLegalAge { get; set; }

        [JsonProperty(PropertyName = "nl_contest_rules")]
        public string IsAgreeContestRules { get; set; }

        [JsonProperty(PropertyName = "nl_td_client")]
        public string IsTdClient { get; set; }

        [JsonProperty(PropertyName = "nl_lifestyle")]
        public int LifeStyleCode { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}
