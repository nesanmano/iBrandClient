using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Serveur.Structures
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestRespondentDataIdentificationStrategyEnum
    {
        Answer,
        ExternalIdentifier,
        Token
    }
}