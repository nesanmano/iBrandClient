using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas
{
    [JsonConverter(typeof(StringEnumConverter))]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ValidationTypeEnum
    {
        NONE
    }
}