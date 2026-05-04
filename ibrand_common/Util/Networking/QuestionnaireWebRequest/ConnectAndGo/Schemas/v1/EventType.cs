using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    [JsonConverter(typeof(StringEnumConverter))]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum EventType
    {
        EVENT
    }
}