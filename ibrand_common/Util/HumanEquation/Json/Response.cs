using System.Runtime.Serialization;

namespace NomadLogic.Common.Util.HumanEquation.Json
{
    [DataContract]
    public abstract class Response
    {
        public Response()
        {
        }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        /*[DataMember(Name = "data")]
        public virtual ResponseData Data { get; set; }*/
    }
}
