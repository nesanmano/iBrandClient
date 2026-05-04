using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    [PublicAPI]
    public class AccessControlLog : BaseCreationDate
    {
        /*
            "id": 201, 
            "gate_id": 2, 
            "access_point_id": "4",
            "zone_from": null,
            "zone_to": OrderedMap { 
                "id": 2, 
                "name": "main", 
                "name_i18n": "{}", 
                "created_at": 1601497603 
            },

            "kill_switch": "NO", 
            "validation_type": "NONE", 
            "bypass_zone_validation": false,
            "update_zone_current": true, 
            "wallet_id": 85301,
            "wallet_type": OrderedMap { 
                "id": 1000001,
                "name": "Papier", 
                "name_i18n": List [],
                "issuing": "MIFARE",
                "created_at": 1588686313 
            }, 

            "ticket": OrderedMap { 
                "id": 108634, 
                "ticket_type": OrderedMap { 
                    "id": 1000004, 
                    "name": "Admission 1 jour", 
                    "name_i18n": "{}", 
                    "validity": OrderedMap {
                        "id": 1000005, 
                        "name": "Admission 1 journée",
                        "name_i18n": "{}",
                        "created_at": 1588756958 
                    },
                    "status": "ACTIVE",
                    "issuing": "WALLET", 
                    "created_at": 1588756990 
                }, 
                "event": OrderedMap {
                    "id": 1000001,
                    "event_template": OrderedMap {
                        "id": 1000001, 
                        "name": "Billets par date",
                        "name_i18n": List [],
                        "event_type": "EVENT", 
                        "created_at": 1594589984
                    },
                    "start_datetime": "2020-09-30T17:37:39.000000Z",
                    "end_datetime": "2023-07-13T18:00:00.000000Z",
                    "created_at": 1594590066
                },
                "transaction": OrderedMap {
                    "id": 1036651, 
                    "transaction_id": "SAC-36599",
                    "created_at": 1601497326
                },
                "product": OrderedMap {
                    "id": 1000534,
                    "name": "1 jour 1,37m et plus / 54'' et plus 2021",
                    "name_i18n": "{}", 
                    "image": "https://cng-everest.s3.amazonaws.com/sac/products/GFd8FDTQsHR4Fj3paEa26Iht4gK9ekM4SfHJlUOy.jpeg",
                    "created_at": 1599302409
                },
                "created_at": 1601482927 
            },

            "account": OrderedMap {
                "id": 1133019, 
                "firstname": "Jazmyne",
                "lastname": "Grimes", 
                "email": "alec66@shanahan.org",
                "image": "https://cng-everest.s3.amazonaws.com/sac/accounts/1133019_e84fa5c92e4da10b83747bcc504bdd81fe4adec0.jpeg", 
                "created_at": 1600967459
            }, 
            "validation_result": OrderedMap {
                "id": 16,
                "name": "NOT_ENOUGH_ADDONS",
                "name_i18n": List [],
                "created_at": 1599075520 
            }, 
            "gate_created_at": 1601599165, 
            "created_at": 1601599166,
            "created_by": 2
         */
        
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("gate_id")]
        public int GateId { get; set; }
        [JsonProperty("access_point_id")]
        public int AccessPointId { get; set; }
        [JsonProperty("zone_from")]
        public Zone ZoneOrigin { get; set; }
        [JsonProperty("zone_to")]
        public List<Zone> ZoneTargets { get; set; }
        [JsonProperty("kill_switch")]
        public string KillSwitch { get; set; }
        [JsonProperty("validation_type")]
        public ValidationTypeEnum ValidationType { get; set; }
        [JsonProperty("bypass_zone_validation")]
        public bool ZoneBupassValidation { get; set; }
        [JsonProperty("update_zone_current")]
        public bool ZoneUpdateCurrent { get; set; }
        [JsonProperty("wallet_id")]
        public int WalletId { get; set; }
        [JsonProperty("wallet_type")]
        public WalletType WalletType { get; set; }
        [JsonProperty("ticket")]
        public Ticket Ticket { get; set; }
        [JsonProperty("event")]
        public List<Event> Events { get; set; }
        [JsonProperty("account")]
        public Account Account { get; set; }
        [JsonProperty("validation_result")]
        public ValidationResult ValidationResult { get; set; }
        [JsonProperty("gate_created_at")]
        public long GateCreationDate { get; set; }
        [JsonProperty("created_by")]
        public long CreationAuthor { get; set; }
    }
}