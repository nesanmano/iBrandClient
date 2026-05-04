using System.Runtime.Serialization;

namespace NomadLogic.Common.Util.HumanEquation.Json
{
    [DataContract]
    public class RequestTokenResponse : Response
    {
        public RequestTokenResponse() : base()
        {
        }

        [DataMember(Name = "data")]
        public Data ReturnData { get; set; }

        [DataContract]
        public class Data : ResponseData
        {
           
            public Data()
            {
            }

            [DataMember(Name = "token")]
            public string Token { get; set; }

            [DataMember(Name = "expiry")]
            public string Expiry { get; set; }
        }

    }
}
