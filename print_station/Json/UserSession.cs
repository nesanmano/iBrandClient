using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace NomadLogic.PrintStation.Json
{
    [DataContract]
    public class UserSession
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }
        [DataMember(Name = "wallId")]
        public int WallID { get; set; }
        [DataMember(Name = "userSessionId")]
        public String UserSessionID { get; set; }

        public UserSession()
        {

        }

        public UserSession(int id, int wall_id, String user_session_id)
        {
            this.ID = id;
            this.WallID = wall_id;
            this.UserSessionID = user_session_id;

        }
    }
}
