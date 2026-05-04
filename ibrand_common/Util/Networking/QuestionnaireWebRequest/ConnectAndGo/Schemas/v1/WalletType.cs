using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class WalletType : BaseDateAndI18N
    {
        //     "id": 1000001,
        //     "name": "Papier", 
        //     "name_i18n": List [],
        //     "issuing": "MIFARE",
        //     "created_at": 1588686313 },
        
        [JsonProperty("issuing")]
        public string IssuedBy { get; set; }
    }
}