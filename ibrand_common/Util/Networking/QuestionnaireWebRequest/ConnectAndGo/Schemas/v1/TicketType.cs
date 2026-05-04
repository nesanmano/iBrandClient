using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class TicketType : BaseDateAndI18N
    {
        /*
         "ticket_type": OrderedMap { 
            "id": 1000004, 
            "name": "Admission 1 jour", 
            "name_i18n": "{}", 
            "validity": OrderedMap {<TicketValidity>},
            "status": "ACTIVE",
            "issuing": "WALLET", 
            "created_at": 1588756990 }, */
        [JsonProperty("validity")]
        public TicketValidity TicketValidities { get; set; }
        [JsonProperty("status")]
        public TicketStatusEnum Status { get; set; }
        [JsonProperty("issuing")]
        public string IssuedBy { get; set; }
    }
}