using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    public class EventTemplate : BaseDateAndI18N
    {
        /*
         *  "id": 1000001, 
            "name": "Billets par date",
            "name_i18n": List [],
            "event_type": "EVENT", 
            "created_at": 1594589984
         */
        [JsonProperty("event_type")]
        public EventType EventType { get; set; }
    }
}