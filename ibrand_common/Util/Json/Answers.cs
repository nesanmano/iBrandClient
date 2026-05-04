using System.Collections.Generic;
using System.Runtime.Serialization;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Util.Json
{
    [DataContract]
    public class Answers
    {
        [DataMember(Name = "answers")]
        public List<Answer> Values { get; set; } = new List<Answer>();
    }
}
